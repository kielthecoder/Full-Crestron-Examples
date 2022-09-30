using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;

namespace P201_Projector_Exam
{
    public class ControlSystem : CrestronControlSystem
    {
        private UI _ui;

        public ControlSystem()
            : base()
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
                // UI class manages all connected touchpanels
                _ui = new UI();

                // This is _NOT_ a mistake!  My original exam predates SmartGraphics, so an
                // XPanel executable was provided with the class materials.  It also ran on
                // a PRO2, but 2-series won't run S# programs.
                _ui.Add(new Xpanel(0x03, this));
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }
    }
}