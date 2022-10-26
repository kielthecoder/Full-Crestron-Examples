using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.EthernetCommunication;
using Crestron.SimplSharpPro.UI;

namespace CentralControl
{
    public class ControlSystem : CrestronControlSystem
    {
        private BasicTriListWithSmartObject _tpLobby;
        private Switch _dmSw;

        private BasicTriList _eiscBoardroom;
        
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
                // Fire alarm is wired to I/O port 1
                if (this.SupportsDigitalInput)
                {
                    this.DigitalInputPorts[1].StateChange += _fireAlarm_StateChange;

                    if (this.DigitalInputPorts[1].Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                        ErrorLog.Error("Error registering DigitalInput for fire alarm: {0}", this.DigitalInputPorts[1].DeviceRegistrationFailureReason);
                }
                else if (this.SupportsVersiport)
                {
                    this.VersiPorts[1].SetVersiportConfiguration(eVersiportConfiguration.DigitalInput);
                    this.VersiPorts[1].VersiportChange += _fireAlarm_VersiportChange;

                    if (this.VersiPorts[1].Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                        ErrorLog.Error("Error registering Versiport for fire alarm: {0}", this.VersiPorts[1].DeviceRegistrationFailureReason);
                }

                // TSW-1052 with SmartGraphics extenders
                _tpLobby = new Tsw1052(0x03, this);
                _tpLobby.LoadSmartObjects(Path.Combine(Directory.GetApplicationDirectory(), "Lobby_TSW-1052.sgd"));

                // Standard event handlers
                _tpLobby.OnlineStatusChange += _tpLobby_OnlineStatusChange;
                _tpLobby.SigChange += _tpLobby_SigChange;

                // SmartGraphics event handlers!  I wish there was a way to reference them by name, but
                // seems like you have to use the ID they're assigned in VTP.
                _tpLobby.SmartObjects[1].SigChange += _tpLobby_PasscodeKeypad;
                _tpLobby.SmartObjects[2].SigChange += _tpLobby_SelectionList;
                _tpLobby.SmartObjects[3].SigChange += _tpLobby_SourceList;
                _tpLobby.SmartObjects[4].SigChange += _tpLobby_DestinationList;
                _tpLobby.SmartObjects[5].SigChange += _tpLobby_Lobby1Controls;
                _tpLobby.SmartObjects[6].SigChange += _tpLobby_Lobby2Controls;
                _tpLobby.SmartObjects[7].SigChange += _tpLobby_ChangePasscode;

                if (_tpLobby.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    ErrorLog.Error("Failed to register touchpanel: {0}", _tpLobby.RegistrationFailureReason);

                // DM-MD8x8 (older style chassis)
                _dmSw = new DmMd8x8(0x10, this);

                if (_dmSw.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    ErrorLog.Error("Failed to register DM switch: {0}", _dmSw.RegistrationFailureReason);

                // This is very poorly documented!
                // You tell the DM cards which slot they occupy.
                // Think: slot 1 = outputs 1 & 2
                //        slot 2 = outputs 3 & 4
                //        slot 3 = outputs 5 & 6 etc.
                var slot3 = new Dmc4kCoHdSingle(3, _dmSw);

                // RMC-SCALERs can now be added to the Output after we've created the cards.
                // We use Outputs[5] and Outputs[6] because those are on slot 3.

                var rmc1 = new DmRmcScalerC(0x14, _dmSw.Outputs[5]);

                if (rmc1.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    ErrorLog.Error("Failed to register DM RMC-SCALER-C: {0}", rmc1.RegistrationFailureReason);

                var rmc2 = new DmRmcScalerC(0x15, _dmSw.Outputs[6]);

                if (rmc2.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    ErrorLog.Error("Failed to register DM RMC-SCALER-C: {0}", rmc2.RegistrationFailureReason);

                // Print a table of inputs and outputs so I know we didn't cause an exception already
                // Exceptions will be printed to the ErrorLog

                CrestronConsole.PrintLine("DM switcher inputs:");

                foreach (var input in _dmSw.Inputs)
                {
                    CrestronConsole.PrintLine("  Card {0} - {1}", input.Number, input.Card);

                    if (input.Endpoint != null)
                        CrestronConsole.PrintLine("    Endpoint: {0}", input.Endpoint);
                }

                CrestronConsole.PrintLine("DM switcher outputs:");

                foreach (var output in _dmSw.Outputs)
                {
                    CrestronConsole.PrintLine("  Card {0} - {1}", output.Number, output.Card);

                    if (output.Endpoint != null)
                        CrestronConsole.PrintLine("    Endpoint: {0}", output.Endpoint);
                }

                // Create the EISC connections to the other rooms.
                //  Since we're the central controller, we'll create these as EISCServer objects.

                _eiscBoardroom = new EISCServer(0xE1, this);
                _eiscBoardroom.SigChange += _eiscBoardroom_SigChange;

                if (_eiscBoardroom.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    ErrorLog.Error("Failed to register Boardroom EISC: {0}", _eiscBoardroom.RegistrationFailureReason);
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

        public void FireAlarmSet()
        {
            _tpLobby.BooleanInput[20].BoolValue = true;
        }

        public void FireAlarmReset()
        {
            _tpLobby.BooleanInput[20].BoolValue = false;
        }

        private void _fireAlarm_VersiportChange(Versiport input, VersiportEventArgs args)
        {
            if (args.Event == eVersiportEvent.DigitalInChange)
            {
                if (input.DigitalIn)
                    FireAlarmSet();
                else
                    FireAlarmReset();
            }
        }

        private void _fireAlarm_StateChange(DigitalInput input, DigitalInputEventArgs args)
        {
            if (args.State)
                FireAlarmSet();
            else
                FireAlarmReset();
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

        private void SerialPortDebug(string portName, string data)
        {
            CrestronConsole.PrintLine("{0} received: {1}", portName, data);
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

        private void _display1_SerialDataRx(ComPort port, ComPortSerialDataEventArgs args)
        {
            SerialPortDebug("display1", args.SerialData);
        }

        private void _display2_SerialDataRx(ComPort port, ComPortSerialDataEventArgs args)
        {
            SerialPortDebug("display2", args.SerialData);
        }

        private void _eiscBoardroom_SigChange(BasicTriList eisc, SigEventArgs args)
        {

        }
    }
}