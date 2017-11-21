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
            
            var files = Directory.EnumerateFiles(folder, "*.asc");
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
            List<string> content = File.ReadAllLines(sNewFile).ToList();
            Dictionary<int, double> wellID_Vals = new Dictionary<int,double>();
            content = content.Skip(1).ToList();
            for(int y = 0; y < content.Count; y++)
            {
                List<string> subStrs = content[y].Split('\t').ToList();
                subStrs = subStrs.Skip(1).ToList();
                for(int x = 0 ; x< subStrs.Count-1; x++)
                {
                    int ID = x * 8 + y + 1;
                    wellID_Vals.Add(ID, double.Parse(subStrs[x]));
                }
            }
            List<double> results = new List<double>();
            int maxID = wellID_Vals.Max(pair => pair.Key);
            for (int id = 1; id <= maxID; id++)
                results.Add(wellID_Vals[id]);
            return results;
        }
    }
}
