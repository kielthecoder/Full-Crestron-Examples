using System;

namespace P201_Projector_Exam
{
    internal class DvdControl
    {
        private UI _ui;

        public DvdControl(UI ui)
        {
            _ui = ui;
            _ui.Press += _ui_Press;
            _ui.Release += _ui_Release;
        }

        private void _ui_Press(object dev, uint sig)
        {
            switch (sig)
            {
                case 16: // Previous
                    _ui.SetFeedback(16, true);
                    break;
                case 17: // Rewind
                    _ui.SetFeedback(17, true);
                    break;
                case 18: // Menu
                    _ui.SetFeedback(18, true);
                    break;
                case 19: // Forward
                    _ui.SetFeedback(19, true);
                    break;
                case 20: // Next
                    _ui.SetFeedback(20, true);
                    break;
                case 21: // Stop
                    _ui.SetFeedback(21, true);
                    break;
                case 22: // Play
                    _ui.SetFeedback(22, true);
                    break;
                case 23: // Pause
                    _ui.SetFeedback(23, true);
                    break;
                case 24: // Up
                    _ui.SetFeedback(24, true);
                    break;
                case 25: // Down
                    _ui.SetFeedback(25, true);
                    break;
                case 26: // Left
                    _ui.SetFeedback(26, true);
                    break;
                case 27: // Right
                    _ui.SetFeedback(27, true);
                    break;
                case 28: // Enter
                    _ui.SetFeedback(28, true);
                    break;
            }
        }

        private void _ui_Release(object dev, uint sig)
        {
            switch (sig)
            {
                case 16: // Previous
                    _ui.SetFeedback(16, false);
                    break;
                case 17: // Rewind
                    _ui.SetFeedback(17, false);
                    break;
                case 18: // Menu
                    _ui.SetFeedback(18, false);
                    break;
                case 19: // Forward
                    _ui.SetFeedback(19, false);
                    break;
                case 20: // Next
                    _ui.SetFeedback(20, false);
                    break;
                case 21: // Stop
                    _ui.SetFeedback(21, false);
                    break;
                case 22: // Play
                    _ui.SetFeedback(22, false);
                    break;
                case 23: // Pause
                    _ui.SetFeedback(23, false);
                    break;
                case 24: // Up
                    _ui.SetFeedback(24, false);
                    break;
                case 25: // Down
                    _ui.SetFeedback(25, false);
                    break;
                case 26: // Left
                    _ui.SetFeedback(26, false);
                    break;
                case 27: // Right
                    _ui.SetFeedback(27, false);
                    break;
                case 28: // Enter
                    _ui.SetFeedback(28, false);
                    break;
            }
        }
    }
}
