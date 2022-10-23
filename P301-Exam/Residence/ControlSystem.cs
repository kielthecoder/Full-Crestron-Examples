using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;

namespace Residence
{
    public class ControlSystem : CrestronControlSystem
    {
        private BasicTriListWithSmartObject _tpLobby;

        public ControlSystem() : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;
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
                _tpLobby.SigChange += _tpLobby_SigChange;

                if (_tpLobby.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                    ErrorLog.Error("Failed to register touchpanel: {0}", _tpLobby.RegistrationFailureReason);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void _tpLobby_SigChange(BasicTriList dev, SigEventArgs args)
        {

        }
    }
}