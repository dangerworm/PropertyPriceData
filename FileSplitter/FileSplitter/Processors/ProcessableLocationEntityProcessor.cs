using System;
using System.Collections.Generic;
using System.IO;
using FileSplitter.Models;

namespace FileSplitter.Processors
{
    public class ProcessableLocationEntityProcessor<T> where T : IProcessableLocationEntity
    {
        protected readonly string InputPath;
        protected readonly string OutputDirectoryPath;
        protected readonly OutputType OutputType;

        protected IReadOnlyCollection<T> EntityData;

        public ProcessableLocationEntityProcessor(
            string inputDirectory, 
            string inputFile, 
            string outputDirectory,
            OutputType outputType)
        {
            InputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, inputDirectory, inputFile);
            OutputDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, "..", "..", "..", "..", "..", outputDirectory);
            OutputType = outputType;

            if (!Directory.Exists(OutputDirectoryPath))
            {
                Directory.CreateDirectory(OutputDirectoryPath);
            }
        }

        protected static string TruncateDouble(double value, int decimalPlaces)
        {
            var formatString = string.Concat("{0:F", decimalPlaces, "}");
            var decimalPointMovement = Math.Pow(10, decimalPlaces);
            var adjustedCoordinate = Math.Truncate(value * decimalPointMovement) / decimalPointMovement;

            return string.Format(formatString, adjustedCoordinate);
        }
    }
}
