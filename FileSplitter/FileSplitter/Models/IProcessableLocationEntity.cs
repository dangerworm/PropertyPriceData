namespace FileSplitter.Models
{
    public interface IProcessableLocationEntity
    {
        public string PostcodeStart { get; }
        
        public string[] Postcode { get; }

        public string PostcodeFull { get; }
         
        public double Latitude { get; }

        public double Longitude { get; }

        public string[] AsStringArray();
        
        public string AsJson();
    }
}
