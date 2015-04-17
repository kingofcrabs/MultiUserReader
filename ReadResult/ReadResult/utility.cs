﻿using System;
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
            string folder = GlobalVars.Instance.TempFolder;
            string dstFolder = folder + "backup\\";
            if (!Directory.Exists(dstFolder))
                Directory.CreateDirectory(dstFolder);
            
            var files = Directory.EnumerateFiles(folder, "*.xml");
            foreach(string sFile in files)
            {
                FileInfo fileInfo = new FileInfo(sFile);
                string dstFullPath = dstFolder + fileInfo.Name;
                if (File.Exists(dstFullPath))
                    File.Delete(dstFullPath);
                File.Move(sFile, dstFullPath);
            }
        }
    }
}
