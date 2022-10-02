using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace P201_Projector_Exam
{
    internal class VcrControl
    {
        private UI _ui;
        private IROutputPort _port;

        public VcrControl(UI ui, IROutputPort port, string irFileName)
        {
            _ui = ui;
            _ui.Press += _ui_Press;
            _ui.Release += _ui_Release;

            _port = port;

            CrestronConsole.PrintLine("VCR: Loading {0}...", irFileName);
            _port.LoadIRDriver(irFileName);
        }

        private void _ui_Press(object dev, uint sig)
        {
            switch (sig)
            {
                case 11: // Rewind
                    _port.Press("REW");
                    _ui.SetFeedback(11, true);
                    break;
                case 12: // Forward
                    _port.Press("FFWD");
                    _ui.SetFeedback(12, true);
                    break;
                case 13: // Stop
                    _port.Press("STOP");
                    _ui.SetFeedback(13, true);
                    break;
                case 14: // Pause
                    _port.Press("PAUSE");
                    _ui.SetFeedback(14, true);
                    break;
                case 15: // Play
                    _port.Press("PLAY");
                    _ui.SetFeedback(15, true);
                    break;
            }
        }

        private void _ui_Release(object dev, uint sig)
        {
            switch (sig)
            {
                case 11: // Rewind
                    _port.Release();
                    _ui.SetFeedback(11, false);
                    break;
                case 12: // Forward
                    _port.Release();
                    _ui.SetFeedback(12, false);
                    break;
                case 13: // Stop
                    _port.Release();
                    _ui.SetFeedback(13, false);
                    break;
                case 14: // Pause
                    _port.Release();
                    _ui.SetFeedback(14, false);
                    break;
                case 15: // Play
                    _port.Release();
                    _ui.SetFeedback(15, false);
                    break;
            }
        }
    }
}
