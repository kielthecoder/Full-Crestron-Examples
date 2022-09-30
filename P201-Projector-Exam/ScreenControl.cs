using System;
using System.Threading;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace P201_Projector_Exam
{
    internal class ScreenControl
    {
        private UI _ui;
        private Relay _up;
        private Relay _down;

        private int _travel;
        private int _duration;
        private Thread _pulse;
        
        public ScreenControl(UI ui, Relay upRelay, Relay downRelay, int seconds)
        {
            _ui = ui;
            _ui.Press += _ui_Press;

            _up = upRelay;
            _down = downRelay;
            _travel = 0;                // 0 = stopped, 1 = up, 2 = down
            _duration = seconds * 1000; // Convert to milliseconds for Thread.Sleep
        }

        private void _ui_Press(object dev, uint sig)
        {
            switch (sig)
            {
                case 1:
                    ScreenUp();
                    break;
                case 2:
                    ScreenDown();
                    break;
            }
        }

        private void Pulse(object obj)
        {
            var relay = (Relay)obj;

            CrestronConsole.Print("Pulsing relay...");
            relay.Close();
            
            Thread.Sleep(_duration);
            
            relay.Open();
            CrestronConsole.PrintLine("done.");

            // Stop moving
            _travel = 0;
            _ui.SetFeedback(1, false);
            _ui.SetFeedback(2, false);
        }

        public void ScreenUp()
        {
            // Stop moving if we're traveling downward
            if (_travel == 2)
            {
                _pulse.Abort();
                _down.Open();
                _ui.SetFeedback(2, false);
                _travel = 0;
            }

            if (_travel == 0)
            {
                // Indicate we're traveling upward
                _travel = 1;
                _ui.SetFeedback(1, true);

                // Pulse relay for 20 seconds
                _pulse = new Thread(Pulse);
                _pulse.Start(_up);
            }
        }

        public void ScreenDown()
        {
            // Stop moving if we're traveling upward
            if (_travel == 1)
            {
                _pulse.Abort();
                _up.Open();
                _ui.SetFeedback(1, false);
                _travel = 0;
            }

            if (_travel == 0)
            {
                // Indicate we're traveling downward
                _travel = 2;
                _ui.SetFeedback(2, true);

                // Pulse relay for 20 seconds
                _pulse = new Thread(Pulse);
                _pulse.Start(_down);
            }
        }
    }
}
