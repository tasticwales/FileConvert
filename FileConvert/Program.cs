using FileConvert.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FileConvert
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // really simple / basic method of getting passed arguements
            var arguments = new Dictionary<string, string>();
            foreach (var item in Environment.GetCommandLineArgs())
            {
                try
                {
                    var parts = item.Split('=');
                    if (parts.Length == 2)
                    {
                        arguments.Add(parts[0], parts[1]);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("No arguements provided.  Please use fileconvert /help=true for details of use");
                    return;
                }
            }

            if(arguments.Count < 1)
            {
                Console.WriteLine("No arguements provided.  Please use fileconvert /help=true for details of use");
                return;
            }

            string filePath = string.Empty;
            string connectionString = string.Empty;
            string readCommand = string.Empty;

            Enums.ConversionType conversionType = Enums.ConversionType.NotSelected;

            // use passed aruements to setup what we are going to do
            foreach (KeyValuePair<string, string> entry in arguments)
            {
                switch(entry.Key.ToLower())
                {
                    case "/help":
                        if(entry.Value.ToLower() == "true")
                        {
                            Console.WriteLine("Usage:");
                            Console.WriteLine("fileconvert /f = [source file] /tojson|toxml = true");
                            Console.WriteLine("Example: fileconvert /f='c:\test.csv' /tojson=true");
                            Console.WriteLine();
                            Console.WriteLine("fileconvert /connectionstring = [database connection string] /readcommand = [stored procedure] /tojson|toxml = true");
                            Console.WriteLine("Example: fileconvert /connectionstring='Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=testdb' /readcommand='getcsvlist' /tojson=true");
                            return;
                        }
                        break;

                    case "/f":
                        filePath = entry.Value;

                        if (!File.Exists(filePath))
                        {
                            Console.WriteLine("Unable to open file provided - please ensure the file exists and you have access to it");
                            return;
                        }
                        break;

                    case "/tojson":
                        conversionType = Enums.ConversionType.ToJson;
                        break;

                    case "/toxml":
                        conversionType = Enums.ConversionType.ToXml;
                        break;

                    case "/fromxml":
                        conversionType = Enums.ConversionType.FromXml;
                        break;

                    case "/connectionstring":
                        connectionString = entry.Value;
                        break;

                    case "/readcommand":
                        readCommand = entry.Value;
                        break;
                }
            }

            // setup dependency injection
            var collection = new ServiceCollection();
            collection.AddScoped<IXmlUtilities, XmlUtilities>();
            collection.AddScoped<IJsonUtilities, JsonUtilities>();
            collection.AddScoped<IDbExample, DbExample>();

            var serviceProvider = collection.BuildServiceProvider();
            var serviceXmlUtilities = serviceProvider.GetService<IXmlUtilities>();
            var serviceJsonUtilities = serviceProvider.GetService<IJsonUtilities>();
            var serviceDbExample = serviceProvider.GetService<IDbExample>();

            // this holds the converted file for further processin etc outside the scope of this utility
            string convertedBody = string.Empty;

            try
            {
                if (conversionType != Enums.ConversionType.NotSelected)
                {
                    if (filePath != "")
                    {
                        switch (conversionType)
                        {
                            case Enums.ConversionType.ToXml:
                                convertedBody = await serviceXmlUtilities.Csv2Xml(filePath, ',');
                                break;

                            case Enums.ConversionType.ToJson:
                                convertedBody = await serviceJsonUtilities.Csv2Json(filePath);
                                break;

                            case Enums.ConversionType.FromXml:
                                convertedBody = serviceXmlUtilities.Xml2Csv(filePath, ',');
                                break;
                        }
                    }
                    else if (connectionString != "" && readCommand != "")
                    {
                        // really, really simple example of how content could be retrieved from database and then common
                        // routines used for the conversion process.  This example simply demonstrates the use of a respository
                        // to acces the database rather than providing a fully robust solution.
                        string[] raw = await serviceDbExample.GetFromDB(connectionString, readCommand);

                        switch (conversionType)
                        {
                            case Enums.ConversionType.ToXml:
                                convertedBody = serviceXmlUtilities.ProcessCsvContent(raw, ',');
                                break;

                            case Enums.ConversionType.ToJson:
                                convertedBody = serviceJsonUtilities.ProcessJsonContent(raw);
                                break;

                            case Enums.ConversionType.FromXml:
                                convertedBody = serviceXmlUtilities.ProcessXmlContent(string.Join("",raw), ',');
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }
}
