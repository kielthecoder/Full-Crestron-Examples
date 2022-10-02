using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace P201_Projector_Exam
{
    internal class DvdControl
    {
        private UI _ui;
        private IROutputPort _port;

        public DvdControl(UI ui, IROutputPort port, string irFileName)
        {
            _ui = ui;
            _ui.Press += _ui_Press;
            _ui.Release += _ui_Release;

            _port = port;

            CrestronConsole.PrintLine("DVD: Loading {0}...", irFileName);
            _port.LoadIRDriver(irFileName);
        }

        private void _ui_Press(object dev, uint sig)
        {
            switch (sig)
            {
                case 16: // Previous
                    _port.Press("|<<SKIP");
                    _ui.SetFeedback(16, true);
                    break;
                case 17: // Rewind
                    _port.Press("REV");
                    _ui.SetFeedback(17, true);
                    break;
                case 18: // Menu
                    _port.Press("MENU");
                    _ui.SetFeedback(18, true);
                    break;
                case 19: // Forward
                    _port.Press("FWD");
                    _ui.SetFeedback(19, true);
                    break;
                case 20: // Next
                    _port.Press("SKIP>>|");
                    _ui.SetFeedback(20, true);
                    break;
                case 21: // Stop
                    _port.Press("STOP");
                    _ui.SetFeedback(21, true);
                    break;
                case 22: // Play
                    _port.Press("PLAY");
                    _ui.SetFeedback(22, true);
                    break;
                case 23: // Pause
                    _port.Press("PAUSE");
                    _ui.SetFeedback(23, true);
                    break;
                case 24: // Up
                    _port.Press("UP");
                    _ui.SetFeedback(24, true);
                    break;
                case 25: // Down
                    _port.Press("DOWN");
                    _ui.SetFeedback(25, true);
                    break;
                case 26: // Left
                    _port.Press("LEFT");
                    _ui.SetFeedback(26, true);
                    break;
                case 27: // Right
                    _port.Press("RIGHT");
                    _ui.SetFeedback(27, true);
                    break;
                case 28: // Enter
                    _port.Press("ENTER");
                    _ui.SetFeedback(28, true);
                    break;
            }
        }

        private void _ui_Release(object dev, uint sig)
        {
            switch (sig)
            {
                case 16: // Previous
                    _port.Release();
                    _ui.SetFeedback(16, false);
                    break;
                case 17: // Rewind
                    _port.Release();
                    _ui.SetFeedback(17, false);
                    break;
                case 18: // Menu
                    _port.Release();
                    _ui.SetFeedback(18, false);
                    break;
                case 19: // Forward
                    _port.Release();
                    _ui.SetFeedback(19, false);
                    break;
                case 20: // Next
                    _port.Release();
                    _ui.SetFeedback(20, false);
                    break;
                case 21: // Stop
                    _port.Release();
                    _ui.SetFeedback(21, false);
                    break;
                case 22: // Play
                    _port.Release();
                    _ui.SetFeedback(22, false);
                    break;
                case 23: // Pause
                    _port.Release();
                    _ui.SetFeedback(23, false);
                    break;
                case 24: // Up
                    _port.Release();
                    _ui.SetFeedback(24, false);
                    break;
                case 25: // Down
                    _port.Release();
                    _ui.SetFeedback(25, false);
                    break;
                case 26: // Left
                    _port.Release();
                    _ui.SetFeedback(26, false);
                    break;
                case 27: // Right
                    _port.Release();
                    _ui.SetFeedback(27, false);
                    break;
                case 28: // Enter
                    _port.Release();
                    _ui.SetFeedback(28, false);
                    break;
            }
        }
    }
}
