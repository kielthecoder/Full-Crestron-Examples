using System;
using System.Threading;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace P201_Projector_Exam
{
    internal class SystemMacros
    {
        private UI _ui;
        private ProjectorControl _proj;
        private ScreenControl _screen;
        private VolumeControl _volume;

        public SystemMacros(UI ui, ProjectorControl proj, ScreenControl screen, VolumeControl volume)
        {
            _ui = ui;
            _ui.Press += _ui_Press;
            _ui.Release += _ui_Release;

            _proj = proj;
            _screen = screen;
            _volume = volume;
        }

        private void _ui_Press(object dev, uint sig)
        {
            switch (sig)
            {
                case 8: // Start Up
                    CrestronConsole.PrintLine("StartUp Macro running");
                    _ui.SetFeedback(8, true);

                    if (_proj != null)
                        _proj.TurnOn();

                    if (_screen != null)
                        _screen.ScreenDown();

                    if (_volume != null)
                        _volume.SetPreset(49151); // 75%

                    break;
                case 9: // Shut Down
                    CrestronConsole.PrintLine("ShutDown Macro running");
                    _ui.SetFeedback(9, true);

                    if (_proj != null)
                        _proj.TurnOff();

                    if (_screen != null)
                        _screen.ScreenUp();

                    if (_volume != null)
                        _volume.SetMute(true);

                    break;
            }
        }

        private void _ui_Release(object dev, uint sig)
        {
            switch (sig)
            {
                case 8: // Start Up
                    _ui.SetFeedback(8, false);
                    break;
                case 9: // Shut Down
                    _ui.SetFeedback(9, false);
                    break;
            }
        }
    }
}
