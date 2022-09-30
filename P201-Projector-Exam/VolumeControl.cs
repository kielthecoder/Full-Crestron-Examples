using System;
using Crestron.SimplSharpPro;

namespace P201_Projector_Exam
{
    internal class VolumeControl
    {
        private UI _ui;
        private Sig _gauge;

        private bool _muteOn;

        public VolumeControl(UI ui, Sig gauge)
        {
            _ui = ui;
            _ui.Press += _ui_Press;
            _ui.Release += _ui_Release;

            _gauge = gauge;
        }

        private void _ui_Press(object dev, uint sig)
        {
            switch (sig)
            {
                case 29: // Volume Up
                    _ui.SetFeedback(29, true);
                    _gauge.CreateRamp(65535, 500); // 5s
                    SetMute(false);
                    break;
                case 30: // Volume Down
                    _ui.SetFeedback(30, true);
                    _gauge.CreateRamp(0, 500); // 5s
                    SetMute(false);
                    break;
                case 31: // Volume Mute
                    SetMute(!_muteOn);
                    break;
                case 32: // Preset 1
                    _ui.SetFeedback(32, true);
                    SetPreset(16383); // 25%
                    break;
                case 33: // Preset 2
                    _ui.SetFeedback(33, true);
                    SetPreset(32767); // 50%
                    break;
                case 34: // Preset 3
                    _ui.SetFeedback(34, true);
                    SetPreset(49151); // 75%
                    break;
            }
        }

        private void _ui_Release(object dev, uint sig)
        {
            switch (sig)
            {
                case 29: // Volume Up
                    _ui.SetFeedback(29, false);
                    _gauge.StopRamp();
                    break;
                case 30: // Volume Down
                    _ui.SetFeedback(30, false);
                    _gauge.StopRamp();
                    break;
                case 32: // Preset 1
                    _ui.SetFeedback(32, false);
                    break;
                case 33: // Preset 2
                    _ui.SetFeedback(33, false);
                    break;
                case 34: // Preset 3
                    _ui.SetFeedback(34, false);
                    break;
            }
        }

        public void SetMute(bool mute)
        {
            _muteOn = mute;
            _ui.SetFeedback(31, _muteOn);
        }

        public void SetPreset(ushort value)
        {
            SetMute(false);
            _gauge.CreateRamp(value, 400);
        }
    }
}
