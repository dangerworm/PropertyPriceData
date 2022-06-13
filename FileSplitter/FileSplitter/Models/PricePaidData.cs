using System;
using System.Collections.Generic;
using System.Text.Json;

namespace FileSplitter.Models
{
    public class PricePaidData : IProcessableLocationEntity
    {
        public PricePaidData(IReadOnlyList<string> locationData)
        {
            if (int.TryParse(locationData[1], out var price))
            {
                Price = price;
            }

            if (DateTime.TryParse(locationData[2], out var dateOfTransfer))
            {
                DateOfTransfer = dateOfTransfer;
            }

            PostcodeFull = locationData[3].Contains(' ')
                ? locationData[3]
                : "UNKNOWN POSTCODE";

            PropertyType = locationData[4] switch
            {
                "F" => "Flat/Maisonette/Townhouse",
                "D" => "Detached",
                "S" => "Semi-detached",
                "T" => "Terraced",
                _ => "Other"
            };

            OldOrNew = locationData[5] switch
            {
                "Y" => "New build",
                "N" => "Established",
                _ => "Unknown"
            };

            Duration = locationData[6] switch
            {
                "F" => "Freehold",
                "L" => "Leasehold",
                _ => "Unknown"
            };

            Paon = locationData[7];

            Saon = locationData[8];

            Street = locationData[9];

            Locality = locationData[10];

            TownOrCity = locationData[11];

            District = locationData[12];

            County = locationData[13];

            PpdCategoryType = locationData[14] switch
            {
                "A" => "Standard",
                "B" => "Additional",
                _ => "Unknown"
            };

            RecordStatus = locationData[15] switch
            {
                "A" => "Addition",
                "C" => "Changed",
                "D" => "Deleted",
                _ => "Unknown"
            };
        }

        public int Price { get; }

        public DateTime? DateOfTransfer { get; }

        public string PostcodeFull { get; }

        public string PropertyType { get; }

        public string OldOrNew { get; }

        public string Duration { get; }

        public string Paon { get; }

        public string Saon { get; }

        public string Street { get; }

        public string Locality { get; }

        public string TownOrCity { get; }

        public string District { get; }

        public string County { get; }

        public string PpdCategoryType { get; }

        public string RecordStatus { get; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string PostcodeStart => $"{Postcode[0]} {Postcode[1][..1]}";

        public string[] Postcode => PostcodeFull.Split(" ");

        public string[] AsStringArray()
        {
            return new[]
            {
                $"{Price:#}",
                $"{DateOfTransfer:u}",
                PostcodeFull,
                PropertyType,
                OldOrNew,
                Duration,
                Paon,
                Saon,
                Street,
                Locality,
                TownOrCity,
                District,
                County,
                PpdCategoryType,
                RecordStatus,
                $"{Latitude:F10}",
                $"{Longitude:F10}"
            };
        }

        public string AsJson()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(this, options);
        }
    }
}
