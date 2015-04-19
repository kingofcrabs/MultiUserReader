using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
