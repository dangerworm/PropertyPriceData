using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Csv;
using FileSplitter.Models;

namespace FileSplitter.Processors
{
    public class PostcodeLatLongProcessor : ProcessableLocationEntityProcessor<LocationData>
    {
        private const int LatLongDecimalPlaces = 2;

        public PostcodeLatLongProcessor(string inputDirectory, string inputFile, string outputDirectory, OutputType outputType)
            : base(inputDirectory, inputFile, outputDirectory, outputType)
        {
        }

        public IDictionary<string, LocationData[]> KeyedLocationData;

        public void Process()
        {
            var stream = File.OpenRead(InputPath);
            var csvOptions = new CsvOptions { HeaderMode = HeaderMode.HeaderPresent };
            
            Console.Write("Processing location data...");
            EntityData = CsvReader.ReadFromStream(stream, csvOptions)
                .Select(x => new LocationData(x.Values))
                .Where(x => x.Latitude != 0 && x.Longitude != 0)
                .ToArray();
            Console.WriteLine("done.");

            stream.Dispose();

            KeyedLocationData = CreateLocationFiles();
        }

        private IDictionary<string, LocationData[]> CreateLocationFiles()
        {
            Console.WriteLine();
            var keyedLocationData = CreateOutputFiles(x => x.PostcodeStart, "postcodes", false);
            Console.Write("done.");

            Console.WriteLine();
            var groupByFunction = (LocationData data) => $"{TruncateDouble(data.Latitude, LatLongDecimalPlaces)},{TruncateDouble(data.Longitude, LatLongDecimalPlaces)}";
            CreateOutputFiles(groupByFunction, "coordinates", !Directory.GetFiles(OutputDirectoryPath).Any());
            Console.Write("done.");

            Console.WriteLine();

            return keyedLocationData;
        }

        private IDictionary<string, LocationData[]> CreateOutputFiles(Func<LocationData, string> groupFunction, string outputType, bool doWrite, string filenamePrefix = "")
        {
            var message = $"Processing {outputType}...";
            Console.Write(message);

            var keyedLocationData = EntityData
                .GroupBy(groupFunction)
                .ToDictionary(x => $"{filenamePrefix}{x.Key}", y => y.ToArray());

            if (doWrite)
            {
                var locationDataList = keyedLocationData.ToArray();

                var percentageStep = Math.Floor(locationDataList.Length / 100.0);
                var counter = 0;
                foreach (var pair in locationDataList)
                {
                    if (counter++ % percentageStep == 0)
                    {
                        Console.CursorLeft = message.Length;
                        Console.Write($"{counter * 100 / locationDataList.Length}%");
                    }

                    CreateOutputFile(pair);
                }

                Console.CursorLeft = message.Length;
            }

            return keyedLocationData;
        }

        private void CreateOutputFile(KeyValuePair<string, LocationData[]> pair)
        {
            var (filename, locations) = pair;

            var outputPath = Path.Combine(OutputDirectoryPath, $"{filename}.{OutputType.ToString().ToLower()}");
            var writer = File.CreateText(outputPath);

            var locationData = locations.Select(x => x.AsStringArray()).ToArray();
            
            CsvWriter.Write(writer, new string[locationData.First().Length], locationData, ',', true);

            writer.Dispose();
        }
    }
}
