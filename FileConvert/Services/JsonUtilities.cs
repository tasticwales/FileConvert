using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FileConvert.Services
{
    public interface IJsonUtilities
    {
        Task<string> Csv2Json(string sourceFileName);

        string ProcessJsonContent(string[] lines);
    }



    public class JsonUtilities : IJsonUtilities
    {
        public async  Task<string> Csv2Json(string sourceFileName)
        {
            string json = string.Empty;
            string csv = string.Empty;

            if (!string.IsNullOrEmpty(sourceFileName))
            {
                if (File.Exists(sourceFileName))
                {

                    using (StreamReader reader = new StreamReader(sourceFileName))
                    {
                        csv = await reader.ReadToEndAsync();
                    }

                    string[] lines = csv.Split(new string[] { "\n" }, System.StringSplitOptions.None);
                    json = ProcessJsonContent(lines);
                }
                else
                {
                    throw new Exception("Source file not found");
                }
            }
            else
            {
                throw new Exception("Source file not provided");
            }

            return json;
        }


        public string ProcessJsonContent(string[] lines)
        {
            string json = string.Empty;

            if (lines.Length > 1)
            {
                // parse headers
                string[] headers = lines[0].Split(',');

                StringBuilder sbjson = new StringBuilder();
                sbjson.Clear();
                sbjson.Append("[");

                string headerName = "";
                string temp = "";

                // parse data
                for (int i = 1; i < lines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]) && !string.IsNullOrEmpty(lines[i]))
                    {
                        sbjson.Append("{");

                        string[] data = lines[i].Split(',');

                        for (int h = 0; h < headers.Length; h++)
                        {
                            if (headers[h].Contains('_'))
                            {
                                string[] parts = headers[h].Split('_');

                                headerName = parts[1];

                                if (temp != parts[0] && temp != "")
                                {
                                    sbjson.Append("}");
                                }
                                else if (temp != parts[0])
                                {
                                    temp = parts[0];
                                    sbjson.Append(temp + ": {");
                                }
                            }
                            else
                            {
                                if (temp != "")
                                {
                                    sbjson.Append("}");
                                    temp = "";
                                }

                                headerName = headers[h];
                            }



                            sbjson.Append(
                                $"\"{headerName.Trim()}\": \"{data[h].Trim()}\"" + (h < headers.Length - 1 ? "," : null)
                            );
                        }

                        if (temp != "")
                        {
                            sbjson.Append("}");
                            temp = "";
                        }

                        sbjson.Append("}" + (i < lines.Length - 2 ? "," : null));
                    }
                }

                sbjson.Append("]");

                json = sbjson.ToString();
            }

            return json;
        }
    }
}
