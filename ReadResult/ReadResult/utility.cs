using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ReadResult
{
    class Utility
    {
        public static void BackupFiles()
        {
            string folder = GlobalVars.Instance.WorkingFolder;
            string dstFolder = folder + "backup\\";
            if (!Directory.Exists(dstFolder))
                Directory.CreateDirectory(dstFolder);
            
            var files = Directory.EnumerateFiles(folder, "*.xml");
            foreach(string sFile in files)
            {
                File.Delete(sFile);
            }
        }


        private static XmlNode GetNode(XmlNodeList nodeList, string name)
        {
            foreach (XmlNode node in nodeList)
            {
                if (node.Name == name)
                    return node;
            }
            throw new Exception("Cannot find");
        }


        public static List<double> ReadFromFile(string sNewFile)
        {
            List<double> vals = new List<double>();
            XmlDocument doc = new XmlDocument();
            doc.Load(sNewFile);    //加载Xml文件  
            XmlElement rootElem = doc.DocumentElement;   //获取根节点  
            XmlNode sectionNode = GetNode(rootElem.ChildNodes, "Section");
            XmlNode dataNode = GetNode(sectionNode.ChildNodes, "Data");
            foreach (XmlNode node in dataNode.ChildNodes)
            {
                string sVal = node.InnerText;
                vals.Add(double.Parse(sVal));
            }
            //log.Info(string.Format("Read {0} well values.", vals.Count));
            return vals;
        }
    }
}
