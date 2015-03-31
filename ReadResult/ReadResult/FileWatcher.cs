using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ReadResult
{
    class FileWatcher
    {
        Timer timerNewFile = null;
        
        DateTime timeStart;
        string sFolder;
        public delegate void Created(string file);
        public event Created onCreated;
        
        Int64 lastTimeSize = 0;
        Timer timerFileSize = null;
        string theNewOne;

        public FileWatcher(string folder)
        {
            sFolder = folder;
        }

        public void Start()
        {
            timerNewFile = new Timer(1000);
            timeStart = DateTime.Now;
            timerNewFile.Elapsed += timerNewFile_Elapsed;
            timerNewFile.Start();
            
        }

        void timerNewFile_Elapsed(object sender, ElapsedEventArgs e)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(sFolder, "*.xml");
            if (files.Count() > 0)
            {
                theNewOne = files.First();
                timerNewFile.Stop();
                lastTimeSize = (new FileInfo(theNewOne)).Length;
                timerFileSize = new Timer(1000);
                timerFileSize.Elapsed += timerFileSize_Elapsed;
                timerFileSize.Start();
            }
        }

        private void timerFileSize_Elapsed(object sender, ElapsedEventArgs e)
        {
            long currentSize = new FileInfo(theNewOne).Length;
            if(currentSize == lastTimeSize)
            {
                timerFileSize.Stop();
                timerNewFile.Elapsed -= timerFileSize_Elapsed;
                if (onCreated != null)
                {
                    onCreated(theNewOne);
                }
            }
            else
            {
                lastTimeSize = currentSize;
            }
        }

        private bool IsNewFile(string s)
        {
            FileInfo fileInfo = new FileInfo(s);
            double totalSeconds = fileInfo.CreationTime.Subtract(timeStart).TotalSeconds;
            return totalSeconds > 0;
            
        }
    }
}
