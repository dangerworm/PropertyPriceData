using System;
using System.Text.Json;
using System.Collections.Generic;

namespace FileSplitter.Models
{
    public class LocationData : IProcessableLocationEntity
    {
        public LocationData(IReadOnlyList<string> locationData)
        {
            PostcodeFull = locationData[1];

            Latitude = double.TryParse(locationData[2], out var latitude)
                ? latitude
                : 0;

            Longitude = double.TryParse(locationData[3], out var longitude)
                ? longitude
                : 0;
        }

        public string PostcodeFull { get; }

        public double Latitude { get; }

        public double Longitude { get; }

        public string PostcodeStart => $"{Postcode[0]} {Postcode[1][..1]}";

        public string[] Postcode => PostcodeFull.Split(" ");

        public string[] AsStringArray()
        {
            return Latitude == 0 || Longitude == 0
                ? Array.Empty<string>()
                : new[] { PostcodeFull, $"{Latitude:F10}", $"{Longitude:F10}" };
        }

        public string AsJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
