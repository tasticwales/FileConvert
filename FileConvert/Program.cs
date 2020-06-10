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

            Enums.ConvertTo conversionType = Enums.ConvertTo.NotSelected;

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
                        conversionType = Enums.ConvertTo.Json;
                        break;

                    case "/toxml":
                        conversionType = Enums.ConvertTo.Xml;
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
                if (conversionType != Enums.ConvertTo.NotSelected)
                {
                    if (filePath != "")
                    {
                        if (conversionType == Enums.ConvertTo.Xml)
                        {
                            convertedBody = await serviceXmlUtilities.Csv2Xml(filePath, ',');
                        }
                        else if (conversionType == Enums.ConvertTo.Json)
                        {
                            convertedBody = await serviceJsonUtilities.Csv2Json(filePath);
                        }
                    }
                    else if (connectionString != "" && readCommand != "")
                    {
                        string[] raw = await serviceDbExample.GetFromDB(connectionString, readCommand);

                        if (conversionType == Enums.ConvertTo.Xml)
                        {
                            convertedBody = serviceXmlUtilities.ProcessCsvContent(raw, ',');
                        }
                        else if (conversionType == Enums.ConvertTo.Json)
                        {
                            convertedBody = serviceJsonUtilities.ProcessJsonContent(raw);
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
