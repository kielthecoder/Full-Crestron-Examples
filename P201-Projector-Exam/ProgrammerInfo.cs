using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P201_Projector_Exam
{
    internal class ProgrammerInfo
    {
        private UI _ui;

        private uint[] _sigs = { 61, 62, 63, 64, 65, 66, 67, 68, 69, 70 };

        public ProgrammerInfo(UI ui)
        {
            _ui = ui;
            _ui.Press += _ui_Press;
            _ui.Release += _ui_Release;
        }

        private void _ui_Press(object dev, uint sig)
        {
            if (_sigs.Contains(sig))
            {
                _ui.SetFeedback(sig, true);

                switch (sig)
                {
                    case 61:
                        _ui.SetSerial(3, "Kiel Lofstrand");
                        break;
                    case 62:
                        _ui.SetSerial(3, "Providea Conferencing");
                        break;
                    case 63:
                        _ui.SetSerial(3, "1297 Flynn Rd");
                        break;
                    case 64:
                        _ui.SetSerial(3, "Suite 100");
                        break;
                    case 65:
                        _ui.SetSerial(3, "Camarillo, CA");
                        break;
                    case 66:
                        _ui.SetSerial(3, "93012-8015");
                        break;
                    case 67:
                        _ui.SetSerial(3, "klofstrand@provideallc.com");
                        break;
                    case 68:
                        _ui.SetSerial(3, "805-616-5995");
                        break;
                    case 69:
                        _ui.SetSerial(3, "Denver, CO");
                        break;
                    case 70:
                        _ui.SetSerial(3, "Dec 19 - Dec 21");
                        break;
                }
            }
        }

        private void _ui_Release(object dev, uint sig)
        {
            if (_sigs.Contains(sig))
            {
                _ui.SetFeedback(sig, false);
            }
        }
    }
}
