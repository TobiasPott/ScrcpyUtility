using System;

namespace NoXP.Scrcpy
{

    public class ScrcpyArguments : ICloneable
    {
        public static ScrcpyArguments Global { get; } = new ScrcpyArguments();



        private bool _useCrop = false;
        private int _cropWidth = -1;
        private int _cropHeight = -1;
        private int _cropX = -1;
        private int _cropY = -1;

        public bool NoControl { get; set; }
        public bool TurnScreenOff { get; set; }

        public int MaxSize { get; set; } = 0;
        public int Bitrate { get; set; } = 0;
        //public string Serial { get; set; } = "";


        public ScrcpyArguments(int cropWidth = -1, int cropHeigth = -1, int cropX = -1, int cropY = -1)
        {
            // set crop argument values
            if (cropWidth != -1 && cropHeigth != -1
                && cropX != -1 && cropY != -1)
            {
                _useCrop = true;
                _cropWidth = cropWidth;
                _cropHeight = cropHeigth;
                _cropX = cropX;
                _cropY = cropY;
            }
        }

        public override string ToString()
        {
            string result = string.Empty;
            if (this.NoControl)
                // disable any remote input to the connected device
                result += Constants.SCRCPY_ARG_NOCONTROL;
            if (this.TurnScreenOff)
                // disable any remote input to the connected device
                result += Constants.SCRCPY_ARG_TURNSCREENOFF;

            if (_useCrop)
                // apply cropping to the projected display
                result += string.Format(Constants.SCRCPY_ARG_CROP, _cropWidth, _cropHeight, _cropX, _cropY);

            if (this.MaxSize > 0)
                // set max size 
                result += string.Format(Constants.SCRCPY_ARG_MAXSIE, this.MaxSize);

            if (this.Bitrate > 0)
                // set bit rate 
                result += string.Format(Constants.SCRCPY_ARG_BITRATE, this.Bitrate);

            //if (!string.IsNullOrEmpty(this.Serial))
            //    // set serial 
            //    result += string.Format(Constants.SCRCPY_ARG_SERIAL, this.Serial);

            return result;
        }

        public object Clone()
        {
            ScrcpyArguments clone = new ScrcpyArguments(this._cropWidth, this._cropHeight, this._cropX, this._cropY);
            clone.NoControl = this.NoControl;
            clone.TurnScreenOff = this.TurnScreenOff;
            clone.MaxSize = this.MaxSize;
            clone.Bitrate = this.Bitrate;
            //clone.Serial = this.Serial;
            return clone;
        }
    }

}
