using System;
using System.Threading;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;

namespace P201_Projector_Exam
{
    public class ControlSystem : CrestronControlSystem
    {
        private UI _ui;
        private ProjectorControl _proj;
        private ScreenControl _screen;
        private VcrControl _vcr;
        private DvdControl _dvd;
        private VolumeControl _volume;
        private SystemMacros _macros;
        private ProgrammerInfo _info;

        private System.Threading.Thread _dateRefresh;

        public bool SystemOn
        {
            get
            {
                if (_proj != null)
                    return _proj.PowerOn;

                return false;
            }
            set
            {
                if (_proj != null)
                {
                    _proj.PowerOn = value;
                    _ui.SetFeedback(3, _proj.PowerOn);
                }
            }
        }

        private int _src;
        public int CurrentSource
        {
            get
            {
                return _src;
            }
            set
            {
                _src = value;

                _ui.SetFeedback(40, SystemOn && _src == 1); // VCR
                _ui.SetFeedback(41, SystemOn && _src == 2); // DVD
                _ui.SetFeedback(42, SystemOn && _src == 3); // PC

                if (_proj != null)
                {
                    switch (_src)
                    {
                        case 1:
                            _proj.SetInput(1);
                            break;
                        case 2:
                            _proj.SetInput(2);
                            break;
                        case 3:
                            _proj.SetInput(5);
                            break;
                    }
                }
            }
        }

        public ControlSystem()
            : base()
        {
            try
            {
                Crestron.SimplSharpPro.CrestronThread.Thread.MaxNumberOfUserThreads = 20;

                CrestronEnvironment.ProgramStatusEventHandler += ProgramStatusChange;
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
                _proj = new ProjectorControl(_ui);
                
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
                    _vcr = new VcrControl(_ui, this.IROutputPorts[1], Path.Combine(Directory.GetApplicationDirectory(), "MITSUBISHI HSU-770 VHS VCR.ir"));
                    _dvd = new DvdControl(_ui, this.IROutputPorts[2], Path.Combine(Directory.GetApplicationDirectory(), "MITSUBISHI DD-2000.ir"));
                }
                else
                {
                    _vcr = null;
                    _dvd = null;
                }

                _volume = new VolumeControl(_ui, xp.UShortInput[1]);
                _macros = new SystemMacros(_ui, _proj, _screen, _volume);
                _info = new ProgrammerInfo(_ui);

                // This is _NOT_ a mistake!  My original exam predates SmartGraphics, so an
                // XPanel executable was provided with the class materials.  It also ran on
                // a PRO2, but 2-series won't run S# programs.
                _ui.Add(xp);

                _dateRefresh = new System.Threading.Thread(SerializeDate);
                _dateRefresh.Start();
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void ProgramStatusChange(eProgramStatusEventType type)
        {
            if (type == eProgramStatusEventType.Stopping)
            {
                if (_dateRefresh != null)
                    _dateRefresh.Abort();
            }
        }

        private void SerializeDate()
        {
            int pause = 1000;

            while (true)
            {
                // 10/02/2022
                _ui.SetSerial(1, DateTime.Today.ToShortDateString());

                if (DateTime.Now.Hour < 23)
                {
                    pause = 1800000; // 30 minutes
                }
                else // 11pm
                {
                    if (DateTime.Now.Minute < 59)
                    {
                        pause = 60000; // 1 minute
                    }
                    else // 11:59pm
                    {
                        pause = 5000; // 5 seconds
                    }
                }

                System.Threading.Thread.Sleep(pause);
            }
        }

        private void _ui_Press(object dev, uint sig)
        {
            switch (sig)
            {
                case 3: // Power toggle
                    SystemOn = !SystemOn;
                    CurrentSource = CurrentSource;
                    break;

                case 8: // Start Up
                    SystemOn = true;
                    CurrentSource = 1; // vcr
                    break;

                case 9: // Shut Down
                    SystemOn = false;
                    CurrentSource = 0; // nothing
                    break;

                case 40: // Source - VCR
                    CurrentSource = 1;
                    break;

                case 41: // Source - DVD
                    CurrentSource = 2;
                    break;

                case 42: // Source - PC
                    CurrentSource = 3;
                    break;
            }
        }
    }
}