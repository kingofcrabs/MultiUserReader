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
        private Timer timerFileSize = null;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public FileWatcher(string folder)
        {
            sFolder = folder;
        }

        public void Start()
        {
            timerFileSize = new Timer(1000);
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
            Trace.WriteLine("acquire finished!");
            IEnumerable<string> files = Directory.EnumerateFiles(sFolder, "*.xml");
            string firstFile = "";
            if (files.Count() > 0)
            {
                firstFile = files.First();
            }

            timerFileSize.Stop();
            timerFileSize.Elapsed -= timerFileSize_Elapsed;
            log.Info("stopped");
           
            if (onCreated != null)
            {
                onCreated(firstFile);
            }
         
        }
    }
}
