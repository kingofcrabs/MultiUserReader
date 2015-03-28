using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ReadResult
{
    public static class SerializeHelper
    {
        /// <summary>
        /// Serialize an instance of type T to a specified XML file
        /// </summary>
        /// <typeparam name="T">Type of object to be serialized</typeparam>
        /// <param name="xmlFileName">XML file name</param>
        /// <param name="objectGraph">object of type T</param>
        public static void Serialize<T>(string xmlFileName, T objectGraph)
        {
            if (string.IsNullOrEmpty(xmlFileName))
            {
                throw new ArgumentException("文件不存在！", "xmlFileName");
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                string sContent = "";
                using (var ms = new MemoryStream())
                {
                    using (var xw = XmlWriter.Create(ms,
                        new XmlWriterSettings()
                        {
                            Encoding = new UTF8Encoding(false),
                            Indent = true,
                            NewLineOnAttributes = true,
                        }))
                    {
                        serializer.Serialize(xw, objectGraph);
                        sContent = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
                File.WriteAllText(xmlFileName, sContent);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to serialize the object : " + ex.Message);
            }

        }

        /// <summary>
        /// Deserialize an XML file to an instance of type T
        /// </summary>
        /// <typeparam name="T">Type of object to be deserialized</typeparam>
        /// <param name="xmlFileName">XML file name</param>
        /// <returns>object of type T</returns>
        public static T Deserialize<T>(string xmlFileName)
        {

            if (!File.Exists(xmlFileName))
            {
                string errorMessage = string.Format("文件不存在！", xmlFileName);
                throw new ArgumentException(errorMessage, "xmlFileName");
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (Stream fs = new FileStream(xmlFileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;

                using (XmlReader reader = XmlReader.Create(fs, settings))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
        }


    }
}
