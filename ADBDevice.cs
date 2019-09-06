namespace NoXP.Scrcpy
{
    public class ADBDevice
    {
        public string Serial { get; }
        public string Name { get; }
        public string IpAddress { get; set; }



        public ADBDevice(string serial, string name)
        {
            this.Serial = serial;
            this.Name = name;
        }

        public override string ToString()
        {
            return string.Format("{0} '{1}'", Serial, Name);
        }
    }

}
