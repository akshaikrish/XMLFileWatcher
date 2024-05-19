using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using NLog;

namespace FileWatcher
{
    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static IConfigurationRoot _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appSettings.json")
                    .Build();
        

        private static readonly HashSet<string> ProcessedFiles = new HashSet<string>();

        static void Main(string[] args)
        {
            // Initializing NLog configuration
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "filewatcher.log" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            NLog.LogManager.Configuration = config;

            // Loading configuration from appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            _configuration = builder.Build();

            // Retrieving input and output folder paths from the configuration
            string inputFolder = _configuration["InputFolder"] ?? "C:\\DefaultInputFolder";
            string outputFolder = _configuration["OutputFolder"] ?? "C:\\DefaultOutputFolder";

            // Checking if the specified input and output folders exist
            if (!Directory.Exists(inputFolder) || !Directory.Exists(outputFolder))
            {
                // Logging error if input or output folder does not exist
                Logger.Error("Input or output folder does not exist.");
                return;
            }

            // Initializing and configuring the FileSystemWatcher
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                Path = inputFolder,
                Filter = "*.xml",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
            };

            // Subscribing to the Created event
            watcher.Created += (sender, e) => 
            {
                OnNewFileDetected(e.FullPath, outputFolder);
                Console.WriteLine("Press 'q' to quit the program.");
            };
            watcher.EnableRaisingEvents = true;

            // Logging information about the folder being watched
            Logger.Info("Watching folder: " + inputFolder);

            // Looping to keep the program running and checking for user input to quit
            while (true)
            {
                Console.WriteLine("Press 'q' to quit the program.");
                if (Console.ReadKey().KeyChar == 'q')
                {
                    break;
                }
            }
        }

        // Event handler for processing new files detected in the input folder
        private static void OnNewFileDetected(string xmlFilePath, string outputFolder)
        {
            string pdfFilePath = Path.ChangeExtension(xmlFilePath, ".pdf");

            // Skipping if the file has already been processed
            if (ProcessedFiles.Contains(xmlFilePath))
            {
                Logger.Info($"{xmlFilePath} is already processed");
                return;
            }

            // Retrying to check for the existence of the corresponding PDF file
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                if (File.Exists(pdfFilePath))
                {
                    ProcessFiles(xmlFilePath, pdfFilePath, outputFolder);
                    ProcessedFiles.Add(xmlFilePath);
                    return;
                }
                System.Threading.Thread.Sleep(1000); // Waiting for 1 second before retrying
            }

            // Logging error if the corresponding PDF file is not found after 3 attempts
            Logger.Error($"PDF file corresponding to {Path.GetFileName(xmlFilePath)} not found after 3 attempts.");
        }

        // Method for processing the XML and PDF files and moving them to the output folder
        private static void ProcessFiles(string xmlFilePath, string pdfFilePath, string outputFolder)
        {
            try
            {
                // Loading the XML file
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlFilePath);

                // Extracting necessary information from the XML file
                #pragma warning disable CS8600
                string mrn = doc.SelectSingleNode("//mrn")?.InnerText;
                string documentType = doc.SelectSingleNode("//documentType")?.InnerText;
                string encounterDateStr = doc.SelectSingleNode("//encounterDate")?.InnerText;
                #pragma warning restore CS8600

                // Validating the extracted information
                if (mrn == null || documentType == null || encounterDateStr == null)
                {
                    // Logging error if required fields are missing in the XML file
                    Logger.Error("Required fields are missing in the XML file.");
                    return;
                }

                // Validating MRN format
                if (!Regex.IsMatch(mrn, @"^\d{7}[A-Za-z]$"))
                {
                    // Logging error if MRN format is invalid
                    Logger.Error("MRN format is invalid.");
                    return;
                }

                // Parsing the encounter date
                DateTime encounterDate = DateTime.ParseExact(encounterDateStr, "yyyyMMddHHmmss", null);
                string formattedDate = encounterDate.ToString("dd-MM-yyyy");

                // Generating the new file name and path
                string newFileName = $"{mrn}_{documentType}_{formattedDate}.pdf";
                string newFilePath = Path.Combine(outputFolder, newFileName);

                // Checking if the file already exists in the output folder
                if (File.Exists(newFilePath))
                {
                    Logger.Info($"The existing file '{newFileName}' is being replaced with a new version.");
                }

                // Copying the PDF file to the output folder and overwriting if it already exists
                File.Copy(pdfFilePath, newFilePath, true);
                File.Delete(xmlFilePath);
                File.Delete(pdfFilePath);

                // Logging information about the processed and moved file
                Logger.Info($"Processed and moved file: {newFileName}");
            }
            catch (Exception ex)
            {
                // Logging error if an exception occurs during file processing
                Logger.Error(ex, "Error processing files.");
            }
        }
        
    }
}
