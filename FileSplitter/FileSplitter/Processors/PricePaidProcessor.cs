using Csv;
using FileSplitter.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileSplitter.Processors
{
    public class PricePaidProcessor : ProcessableLocationEntityProcessor<PricePaidData>
    {
        private const int LatLongDecimalPlaces = 3;
        private const string GoogleMapsBaseUrl = "https://www.google.co.uk/maps/place/";

        private readonly HttpClient _httpClient;
        private readonly int _numberOfRecordsToSkip;

        public PricePaidProcessor(
            string inputDirectory, 
            string inputFile, 
            string outputDirectory, 
            IDictionary<string, LocationData[]> keyedLocationData,
            OutputType outputType,
            int numberOfRecordsToSkip = 0)
            : base(inputDirectory, inputFile, outputDirectory, outputType)
        {
            _httpClient = new HttpClient();
            _numberOfRecordsToSkip = numberOfRecordsToSkip;

            KeyedLocationData = keyedLocationData;
        }

        public IDictionary<string, LocationData[]> KeyedLocationData;

        public void Process()
        {
            Console.Write("Counting lines in house price data file...");
            var reader = File.OpenText(InputPath);
            var numberOfLines = CountLines(reader);
            Console.WriteLine($"{numberOfLines:###,###,###,###}.");

            var logStep = GetLogStep(numberOfLines);

            const string message = "Processing price data...";
            Console.Write(message);
            var stopwatch = new Stopwatch();

            var csvOptions = new CsvOptions { HeaderMode = HeaderMode.HeaderAbsent };
            var counter = -1;
            var unknowns = 0;
            string line;
            while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
            {
                if (counter++ % logStep == 0)
                {
                    var remainingTime = counter == 0
                        ? TimeSpan.FromDays(1)
                        : TimeSpan.FromSeconds((stopwatch.Elapsed.TotalSeconds / (counter - _numberOfRecordsToSkip)) * (numberOfLines - counter));

                    Console.CursorLeft = message.Length;
                    Console.Write($"line {counter:###,###,###,###} of {numberOfLines:###,###,###,###} ({counter * 100.0 / numberOfLines:F1}%) " +
                        $"- {remainingTime.Days:00}d {remainingTime.Hours:00}h {remainingTime.Minutes:00}m {remainingTime.Seconds:00}s remaining");
                }

                if (counter <= _numberOfRecordsToSkip)
                {
                    continue;
                }

                if (!stopwatch.IsRunning)
                {
                    stopwatch.Start();
                }

                var pricePaidItem = CsvReader.ReadFromText(line, csvOptions)
                .Select(x => new PricePaidData(x.Values))
                .SingleOrDefault(x => x.RecordStatus != "Deleted" && x.PostcodeFull != string.Empty);

                if (pricePaidItem == null)
                {
                    continue;
                }

                if (pricePaidItem.PostcodeFull == "UNKNOWN POSTCODE")
                {
                    unknowns++; // 31,384 in pp-complete.csv from June 2022
                    continue;
                }

                AddLatLongDataFromKeyedLocationData(pricePaidItem);

                if (pricePaidItem.Latitude == 0 && pricePaidItem.Longitude == 0)
                {
                    continue;
                }

                AppendToFiles(pricePaidItem);
            }

            stopwatch.Stop();
            Console.CursorLeft = message.Length;
            Console.WriteLine("done.                                                               ");

            reader.Dispose();

            Console.WriteLine();
        }

        private static int CountLines(StreamReader reader)
        {
            var numberOfLines = 0;
            while (reader.ReadLine() != null)
            {
                if (numberOfLines % 1000000 == 0)
                {
                    Console.CursorLeft = 42;
                    Console.Write($"{numberOfLines:###,###,###,###}");
                }

                numberOfLines++;
            }

            Console.CursorLeft = 42;

            reader.BaseStream.Position = 0;
            reader.DiscardBufferedData();

            return numberOfLines;
        }

        private static double GetLogStep(int numberOfLines)
        {
            var divisor = 1000.0;
            double logStep;
            do
            {
                logStep = Math.Floor(numberOfLines / divisor);
                divisor *= 10;
            } while (logStep > 1000);

            return logStep;
        }

        private bool AddLatLongDataFromGoogleMaps(PricePaidData item)
        {
            // Takes 0.5 seconds per record. Way too slow.

            bool success = false;

            Task.Run(async () =>
            {
                var response = await _httpClient.GetAsync($"{GoogleMapsBaseUrl}{item.Paon} {item.Saon} {item.Street} {item.PostcodeFull}");
                var content = await response.Content.ReadAsStringAsync();

                var regex = new Regex(@"<meta content[^\s]*ll=(-?\d+\.-?\d+),(-?\d+\.-?\d+)"" [^\s]*>");
                var groups = regex.Matches(content);

                if (!groups.Any())
                {
                    success = false;
                    return;
                }

                item.Latitude = double.Parse(groups[0].Groups[1].Value);
                item.Longitude = double.Parse(groups[0].Groups[2].Value);

                success = true;
            }).Wait();

            return success;
        }

        private void AddLatLongDataFromKeyedLocationData(PricePaidData item)
        {
            if (!KeyedLocationData.ContainsKey(item.PostcodeStart))
            {
                return;
            }

            var location = KeyedLocationData[item.PostcodeStart].SingleOrDefault(x => x.PostcodeFull == item.PostcodeFull);
            if (location == null)
            {
                return;
            }

            item.Latitude = location.Latitude;
            item.Longitude = location.Longitude;
        }

        private void AppendToFiles(IProcessableLocationEntity item)
        {
            AppendToFile(item.PostcodeStart, item);
            // AppendToFile($"{TruncateDouble(item.Latitude, LatLongDecimalPlaces)},{TruncateDouble(item.Longitude, LatLongDecimalPlaces)}", item);
        }

        private void AppendToFile(string filename, IProcessableLocationEntity item)
        {
            var path = GetFilePath(filename);
            var writer = File.AppendText(path);

            if (OutputType == OutputType.Csv)
            {
                var csvColumns = item.AsStringArray();
                CsvWriter.Write(writer, new string[csvColumns.Length], new[] { csvColumns }, ',', true);
            }

            if (OutputType == OutputType.Json)
            {
                writer.WriteLine(item.AsJson());
            }

            writer.Dispose();
        }

        private string GetFilePath(string filename)
        {
            return Path.Combine(OutputDirectoryPath, $"{filename}.{OutputType.ToString().ToLower()}");
        }
    }
}
