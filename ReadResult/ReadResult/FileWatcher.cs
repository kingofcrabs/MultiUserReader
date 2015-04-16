using ManagedWinapi.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Automation;

namespace ReadResult
{
    class FileWatcher
    {
        
        string sFolder;
        public delegate void Created(string file);
        public event Created onCreated;
        private Timer timerFileSize = new Timer(1000);

        public FileWatcher(string folder)
        {
            sFolder = folder;
        }

        public void Start()
        {
            timerFileSize.Elapsed += timerFileSize_Elapsed;
            timerFileSize.Start();
            
        }

      
        private bool AcquisitionWindowVisible()
        {
            

            AutomationElement acquisitionWindow = AutomationElement.RootElement.FindFirst(TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, "Measurement in progress"));

            return acquisitionWindow != null;
        }

        private void timerFileSize_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool isAcquiring = AcquisitionWindowVisible();
            if (isAcquiring)
                return;

            timerFileSize.Stop();
            if (onCreated != null)
            {
                IEnumerable<string> files = Directory.EnumerateFiles(sFolder, "*.xml");
                if(files.Count() >0)
                    onCreated(files.First());
            }
         
        }
    }
}
