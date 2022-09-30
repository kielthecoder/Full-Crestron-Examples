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
        private ScreenControl _screen;
        private VcrControl _vcr;
        private DvdControl _dvd;
        private VolumeControl _volume;
        private SystemMacros _macros;

        private bool _systemOn;
        private int _src;

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
                var xp = new Xpanel(0x03, this);

                // UI class manages all connected touchpanels
                _ui = new UI();
                _ui.Press += _ui_Press;

                // Different UI elements hook into UI manager
                
                if (this.SupportsRelay)
                {
                    // Screen relays must be latched for 20 seconds
                    _screen = new ScreenControl(_ui, this.RelayPorts[1], this.RelayPorts[2], 20);
                }
                else
                {
                    _screen = null;
                }

                if (this.SupportsIROut)
                {
                    _vcr = new VcrControl(_ui);
                    _dvd = new DvdControl(_ui);
                }
                else
                {
                    _vcr = null;
                    _dvd = null;
                }

                _volume = new VolumeControl(_ui, xp.UShortInput[1]);
                _macros = new SystemMacros(_ui, _screen, _volume);

                // This is _NOT_ a mistake!  My original exam predates SmartGraphics, so an
                // XPanel executable was provided with the class materials.  It also ran on
                // a PRO2, but 2-series won't run S# programs.
                _ui.Add(xp);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void _ui_Press(object dev, uint sig)
        {
            switch (sig)
            {
                case 3: // Power toggle
                    _systemOn = !_systemOn;
                    _ui.SetFeedback(3, _systemOn);
                    _ui_SelectSource(_src);
                    break;

                case 8: // Start Up
                    _systemOn = true;
                    _src = 1;
                    _ui.SetFeedback(3, _systemOn);
                    _ui_SelectSource(1); // vcr
                    break;

                case 9: // Shut Down
                    _systemOn = false;
                    _src = 0;
                    _ui.SetFeedback(3, _systemOn);
                    _ui_SelectSource(0); // nothing
                    break;

                case 40: // Source - VCR
                    _ui_SelectSource(1);
                    break;

                case 41: // Source - DVD
                    _ui_SelectSource(2);
                    break;

                case 42: // Source - PC
                    _ui_SelectSource(3);
                    break;
            }
        }

        private void _ui_SelectSource(int src)
        {
            _src = src;

            _ui.SetFeedback(40, _systemOn && _src == 1); // VCR
            _ui.SetFeedback(41, _systemOn && _src == 2); // DVD
            _ui.SetFeedback(42, _systemOn && _src == 3); // PC
        }
    }
}