using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;

namespace CentralControl
{
    public class ControlSystem : CrestronControlSystem
    {
        private BasicTriListWithSmartObject _tpLobby;
        
        private Dictionary<int, uint> _sbpgJoin;
        private int _sbpgCurrent;

        private string _passcodeEntry;
        private string _userPasscode;

        public ControlSystem() : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                // Map subpage selection items to visibility join numbers
                _sbpgJoin = new Dictionary<int, uint>();
                _sbpgJoin[1] = 80;
                _sbpgJoin[2] = 81;
                _sbpgJoin[3] = 82;
                _sbpgJoin[4] = 83;
                _sbpgJoin[5] = 84;

                // Default passcode
                _userPasscode = "12345";
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        public override void InitializeSystem()
        {
            try
            {
                _tpLobby = new Tsw1052(0x03, this);
                _tpLobby.LoadSmartObjects(Path.Combine(Directory.GetApplicationDirectory(), "Lobby_TSW-1052.sgd"));

                _tpLobby.OnlineStatusChange += _tpLobby_OnlineStatusChange;
                _tpLobby.SigChange += _tpLobby_SigChange;
                _tpLobby.SmartObjects[1].SigChange += _tpLobby_PasscodeKeypad;
                _tpLobby.SmartObjects[2].SigChange += _tpLobby_SelectionList;
                _tpLobby.SmartObjects[3].SigChange += _tpLobby_SourceList;
                _tpLobby.SmartObjects[4].SigChange += _tpLobby_DestinationList;
                _tpLobby.SmartObjects[5].SigChange += _tpLobby_Lobby1Controls;
                _tpLobby.SmartObjects[6].SigChange += _tpLobby_Lobby2Controls;
                _tpLobby.SmartObjects[7].SigChange += _tpLobby_ChangePasscode;

                if (_tpLobby.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    ErrorLog.Error("Failed to register touchpanel: {0}", _tpLobby.RegistrationFailureReason);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void ShowSubpage(int sbpgNext)
        {
            if (sbpgNext != _sbpgCurrent)
            {
                if (_sbpgCurrent != 0)
                    _tpLobby.BooleanInput[_sbpgJoin[_sbpgCurrent]].BoolValue = false;

                _sbpgCurrent = sbpgNext;

                switch (_sbpgCurrent)
                {
                    case 1: // Help
                        break;
                    case 2: // Routing
                        break;
                    case 3: // Display Control
                        break;
                    case 4: // Change Password
                        _tpLobby.StringInput[1].StringValue = "Enter new passcode:";
                        _passcodeEntry = String.Empty;
                        break;
                    case 5: // Shutdown
                        break;
                }

                if (_sbpgCurrent != 0)
                    _tpLobby.BooleanInput[_sbpgJoin[_sbpgCurrent]].BoolValue = true;
            }
        }

        private void HideAllSubpages()
        {
            foreach (var k in _sbpgJoin.Keys)
                _tpLobby.BooleanInput[_sbpgJoin[k]].BoolValue = false;
        }

        public void LockPanel()
        {
            HideAllSubpages();

            _tpLobby.BooleanInput[50].BoolValue = true;
            _tpLobby.StringInput[1].StringValue = "Enter passcode to unlock panel";

            _passcodeEntry = String.Empty;
        }

        public void UnlockPanel()
        {
            _tpLobby.BooleanInput[50].BoolValue = false;
        }

        private void _tpLobby_OnlineStatusChange(GenericBase dev, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
                LockPanel();
        }

        private void _tpLobby_SigChange(BasicTriList dev, SigEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool)
            {
                switch (args.Sig.Number)
                {
                    case 35: // Lock Panel
                        if (args.Sig.BoolValue)
                            LockPanel();
                        break;
                    case 36: // Power Off Displays (and also Lock Panel)
                        if (args.Sig.BoolValue)
                            LockPanel();
                        break;
                }
            }
        }

        private void SmartObjectDebug(string objName, Sig sig)
        {
            CrestronConsole.Print("{0}: '{1}' ", objName, sig.Name);

            switch (sig.Type)
            {
                case eSigType.Bool:
                    CrestronConsole.PrintLine("= {0}", sig.BoolValue);
                    break;
                case eSigType.UShort:
                    CrestronConsole.PrintLine("= {0}", sig.UShortValue);
                    break;
                case eSigType.String:
                    CrestronConsole.PrintLine("= '{0}'", sig.StringValue);
                    break;
            }
        }

        private void _tpLobby_PasscodeKeypad(GenericBase dev, SmartObjectEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool)
            {
                if (args.Sig.BoolValue)
                {
                    if (args.Sig.Name == "Misc_1") // Reset
                    {
                        LockPanel();
                    }
                    else if (args.Sig.Name == "Misc_2") // Unlock
                    {
                        if (_passcodeEntry == _userPasscode)
                        {
                            _tpLobby.StringInput[1].StringValue = "CORRECT";

                            UnlockPanel();
                            ShowSubpage(2); // Routing
                        }
                        else
                        {
                            _tpLobby.StringInput[1].StringValue = "INCORRECT";
                        }

                        _passcodeEntry = String.Empty;
                    }
                    else // 0 - 9
                    {
                        if (_passcodeEntry.Length < 20)
                            _passcodeEntry += args.Sig.Name;

                        var stars = String.Empty;

                        for (int i = 0; i < _passcodeEntry.Length; i++)
                            stars += "*";

                        _tpLobby.StringInput[1].StringValue = stars;
                    }
                }
            }
        }

        private void _tpLobby_ChangePasscode(GenericBase dev, SmartObjectEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool)
            {
                if (args.Sig.BoolValue)
                {
                    if (args.Sig.Name == "Misc_1") // Cancel
                    {
                        _tpLobby.StringInput[1].StringValue = "Enter new passcode:";
                        _passcodeEntry = String.Empty;
                    }
                    else if (args.Sig.Name == "Misc_2") // Accept
                    {
                        _tpLobby.StringInput[1].StringValue = "Passcode changed!";

                        _userPasscode = _passcodeEntry;
                        _passcodeEntry = String.Empty;
                    }
                    else // 0 - 9
                    {
                        if (_passcodeEntry.Length < 20)
                            _passcodeEntry += args.Sig.Name;

                        _tpLobby.StringInput[1].StringValue = _passcodeEntry;
                    }
                }
            }
        }

        private void _tpLobby_SelectionList(GenericBase dev, SmartObjectEventArgs args)
        {
            if (args.Sig.Name == "Item Clicked")
                ShowSubpage((int)args.Sig.UShortValue);
        }

        private void _tpLobby_SourceList(GenericBase dev, SmartObjectEventArgs args)
        {
            SmartObjectDebug("SourceList", args.Sig);
        }

        private void _tpLobby_DestinationList(GenericBase dev, SmartObjectEventArgs args)
        {
            SmartObjectDebug("DestinationList", args.Sig);
        }

        private void _tpLobby_Lobby1Controls(GenericBase dev, SmartObjectEventArgs args)
        {
            SmartObjectDebug("Lobby1Controls", args.Sig);
        }

        private void _tpLobby_Lobby2Controls(GenericBase dev, SmartObjectEventArgs args)
        {
            SmartObjectDebug("Lobby2Controls", args.Sig);
        }
    }
}