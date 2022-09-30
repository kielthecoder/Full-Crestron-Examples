using System;

namespace P201_Projector_Exam
{
    internal class VcrControl
    {
        private UI _ui;

        public VcrControl(UI ui)
        {
            _ui = ui;
            _ui.Press += _ui_Press;
            _ui.Release += _ui_Release;
        }

        private void _ui_Press(object dev, uint sig)
        {
            switch (sig)
            {
                case 11: // Rewind
                    _ui.SetFeedback(11, true);
                    break;
                case 12: // Forward
                    _ui.SetFeedback(12, true);
                    break;
                case 13: // Stop
                    _ui.SetFeedback(13, true);
                    break;
                case 14: // Pause
                    _ui.SetFeedback(14, true);
                    break;
                case 15: // Play
                    _ui.SetFeedback(15, true);
                    break;
            }
        }

        private void _ui_Release(object dev, uint sig)
        {
            switch (sig)
            {
                case 11: // Rewind
                    _ui.SetFeedback(11, false);
                    break;
                case 12: // Forward
                    _ui.SetFeedback(12, false);
                    break;
                case 13: // Stop
                    _ui.SetFeedback(13, false);
                    break;
                case 14: // Pause
                    _ui.SetFeedback(14, false);
                    break;
                case 15: // Play
                    _ui.SetFeedback(15, false);
                    break;
            }
        }
    }
}
