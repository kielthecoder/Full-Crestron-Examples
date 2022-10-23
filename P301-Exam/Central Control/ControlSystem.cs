using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;
namespace Central_Control
{
    public class ControlSystem : CrestronControlSystem
    {
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
                // TODO
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }
    }
}