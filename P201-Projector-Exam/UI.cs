using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;


namespace P201_Projector_Exam
{
    internal class UI
    {
        private List<BasicTriList> _panels;

        public event EventHandler<uint> Press;
        public event EventHandler<uint> Release;

        public UI()
        {
            _panels = new List<BasicTriList>();

            Press += CommonPress;
            Release += CommonRelease;
        }

        public void Add(BasicTriList tp)
        {
            tp.SigChange += _ui_SigChange;

            if (tp.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
            {
                _panels.Add(tp);
            }
            else
            {
                ErrorLog.Error("Unable to register touchpanel at ID {0:X2}!", tp.ID);
            }
        }

        public void SetFeedback(uint sig, bool value)
        {
            foreach (var tp in _panels)
            {
                tp.BooleanInput[sig].BoolValue = value;
            }
        }

        public void SetAnalog(uint sig, ushort value)
        {
            foreach (var tp in _panels)
            {
                tp.UShortInput[sig].UShortValue = value;
            }
        }

        public void SetSerial(uint sig, string value)
        {
            foreach (var tp in _panels)
            {
                tp.StringInput[sig].StringValue = value;
            }
        }

        private void _ui_SigChange(BasicTriList dev, SigEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    if (args.Sig.BoolValue)
                    {
                        if (Press != null)
                            Press(dev, args.Sig.Number);
                    }
                    else
                    {
                        if (Release != null)
                            Release(dev, args.Sig.Number);
                    }

                    break;
            }
        }

        private void CommonPress(object sender, uint sig)
        {
            switch (sig)
            {
                case 1: // Screen Up
                    break;
                case 2: // Screen Down
                    break;
                case 3: // Power Toggle
                    break;
                case 8: // Start Up Macro
                    break;
                case 9: // Shut Down Macro
                    break;
                case 29: // Volume Up
                    break;
                case 30: // Volume Down
                    break;
                case 31: // Volume Mute
                    break;
                case 32: // Preset 1
                    break;
                case 33: // Preset 2
                    break;
                case 40: // Source - VCR
                    break;
                case 41: // Source - DVD
                    break;
                case 42: // Source - PC
                    break;
            }
        }

        private void CommonRelease(object sender, uint sig)
        {
            // TODO
        }
    }
}
