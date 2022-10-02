using System;

namespace P201_Projector_Exam
{
    internal class ProjectorControl
    {
        private UI _ui;

        private bool _powerOn;
        public bool PowerOn
        {
            get
            {
                return _powerOn;
            }
            set
            {
                if (_powerOn != value)
                {
                    if (value)
                        TurnOn();
                    else
                        TurnOff();
                }
            }
        }

        public ProjectorControl(UI ui)
        {
            _ui = ui;
        }

        public void TurnOn()
        {
            _ui.SetSerial(2, "\x0200PON\x03");
            _powerOn = true;
        }

        public void TurnOff()
        {
            _ui.SetSerial(2, "\x0200POF\x03");
            _powerOn = false;
        }

        public void SetInput(int input)
        {
            _ui.SetSerial(2, String.Format("\x0200IN{0}\x03", input));
        }
    }
}
