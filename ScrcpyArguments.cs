namespace NoXP.Scrcpy
{

    public class ScrcpyArguments
    {

        private bool _useCrop = false;
        private int _cropWidth = 0;
        private int _cropHeight = 0;
        private int _cropX = 0;
        private int _cropY = 0;

        private bool _useMaxSize = false;
        private int _maxSize = 0;

        private bool _useBitrate = false;
        private int _bitrate = 0;

        private bool _useSerial = false;
        private string _serial = string.Empty;
        public bool NoControl { get; set; }
        public bool TurnScreenOff { get; set; }


        public ScrcpyArguments(int cropWidth = -1, int cropHeigth = -1, int cropX = -1, int cropY = -1, int maxSize = -1, int bitrate = -1, string serial = "")
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
            // set max size argument
            if (maxSize != -1)
            {
                _useMaxSize = true;
                _maxSize = maxSize;
            }
            // set bitrate argument
            if (bitrate != -1)
            {
                _useBitrate = true;
                _bitrate = bitrate;
            }
            // set bitrate argument
            if (string.IsNullOrEmpty(serial))
            {
                _useSerial = true;
                _serial = serial;
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

            if (_useMaxSize)
                // set max size 
                result += string.Format(Constants.SCRCPY_ARG_MAXSIE, _maxSize);

            if (_useBitrate)
                // set bit rate 
                result += string.Format(Constants.SCRCPY_ARG_BITRATE, _bitrate);

            if (_useSerial)
                // set serial 
                result += string.Format(Constants.SCRCPY_ARG_SERIAL, _serial);

            return result;
        }

    }

}
