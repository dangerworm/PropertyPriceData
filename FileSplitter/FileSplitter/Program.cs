using System;
using FileSplitter.Processors;

namespace FileSplitter
{
    public class Program
    {
        private const string InputDirectory = "Input";
        private const string LocationOutputDirectory = "location_data";
        private const string HouseDataOutputDirectory = "house_price_data";

        static void Main(string[] args)
        {
            // ukpostcodes.csv taken from https://www.freemaptools.com/download-uk-postcode-lat-lng.htm
            var latLongProcessor = new PostcodeLatLongProcessor(
                InputDirectory, 
                "ukpostcodes.csv", 
                LocationOutputDirectory, 
                OutputType.Csv);

            latLongProcessor.Process();

            Console.WriteLine();

            // pp-complete.csv taken from https://www.gov.uk/government/statistical-data-sets/price-paid-data-downloads#single-file
            var pricePaidProcessor = new PricePaidProcessor(
                InputDirectory, 
                "pp-complete.csv", 
                HouseDataOutputDirectory, 
                latLongProcessor.KeyedLocationData, 
                OutputType.Json);

            pricePaidProcessor.Process();
        }
    }
}
