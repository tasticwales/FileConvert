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
    public interface IXmlUtilities
    {
        Task<string> Csv2Xml(string sourceFileName, char fieldSplitCharacter);

        string ProcessCsvContent(string[] lines, char fieldSplitCharacter);

        string Xml2Csv(string sourceFileName, char fieldSplitCharacter);

        string ProcessXmlContent(string xmlContent, char fieldSplitCharacter);
    }



    public class XmlUtilities : IXmlUtilities
    {
        public async Task<string> Csv2Xml(string sourceFileName, char fieldSplitCharacter)
        {
            string xml = string.Empty;

            if (!string.IsNullOrEmpty(sourceFileName))
            {
                if (File.Exists(sourceFileName))
                {
                    string[] lines = await File.ReadAllLinesAsync(sourceFileName);
                    xml = ProcessCsvContent(lines, fieldSplitCharacter);
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

            return xml;
        }

        public string Xml2Csv(string sourceFileName, char fieldSplitCharacter)
        {
            string Csv = string.Empty;


            if (!string.IsNullOrEmpty(sourceFileName))
            {
                if (File.Exists(sourceFileName))
                {
                    string xmlContent = File.ReadAllText(sourceFileName);
                    Csv = ProcessXmlContent(xmlContent, fieldSplitCharacter);
                }
            }

            return Csv;
        }


        public string ProcessXmlContent(string xmlContent, char fieldSplitCharacter)
        {
            List<string> headerRow = new List<string>();
            List<string> rows = new List<string>(); ;

            XDocument doc = XDocument.Parse(xmlContent);

            foreach (XElement node in doc.Descendants("ROOT"))
            {
                foreach (XElement innerNode in node.Descendants("row"))
                {
                    string thisRow = string.Empty;
                    string thisHeaderRow = string.Empty;
                    headerRow.Clear();

                    foreach (XElement e in innerNode.Elements())
                    {
                        if (e.HasElements)
                        {
                            foreach (XElement x in e.Elements())
                            {
                                thisRow += x.Value + fieldSplitCharacter;
                                thisHeaderRow += e.Name.ToString() + "_" + x.Name.ToString() + fieldSplitCharacter;
                            }
                        }
                        else
                        {
                            thisRow += e.Value + fieldSplitCharacter;
                            thisHeaderRow += e.Name.ToString() + fieldSplitCharacter;
                        }
                    }

                    rows.Add(thisRow.Trim(fieldSplitCharacter));
                    headerRow.Insert(0, thisHeaderRow.Trim(fieldSplitCharacter));
                }
            }

            return string.Join(Environment.NewLine, headerRow.Concat(rows));
        }




        public string ProcessCsvContent(string[] lines, char fieldSplitCharacter)
        {
            StringBuilder sb = new StringBuilder();
            string[] headers = lines[0].Split(fieldSplitCharacter);
            var data = lines.Skip(1);
            string elementName = "";
            string headerName = "";

            XmlWriter writer = XmlWriter.Create(sb);
            writer.WriteStartElement("ROOT");

            foreach (var d in data)
            {
                writer.WriteStartElement("row");

                string[] cols = d.Split(fieldSplitCharacter);

                for (int i = 0; i < headers.Length; i++)
                {
                    if (headers[i].Contains('_'))
                    {
                        string[] parts = headers[i].Split('_');

                        if (elementName != parts[0])
                        {
                            writer.WriteStartElement(parts[0]);
                            elementName = parts[0];
                        }

                        headerName = parts[1];
                    }
                    else
                    {
                        if (elementName != "")
                        {
                            writer.WriteEndElement();
                        }

                        elementName = "";
                        headerName = headers[i];
                    }

                    writer.WriteElementString(headerName, cols[i]);
                }

                if (elementName != "")
                {
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Flush();

            return sb.ToString();
        }


    }
}
