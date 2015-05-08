using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ReadResult
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
           
            if (e.Args.Length > 0)
            {
                string workingFolder = GlobalVars.Instance.WorkingFolder;
                IEnumerable<string> files = Directory.EnumerateFiles(workingFolder, "*.xml");
                if(files.Count() == 0)
                {
                    MessageBox.Show("无法找到输出文件");
                    return;
                }
                string firstFile = "";
                if (files.Count() > 0)
                {
                    firstFile = files.First();
                }
                var vals = Utility.ReadFromFile(firstFile);
                Utility.BackupFiles();
                Clipboard.Clear();
                string content = "";
                string nextLine = "\r\n";
                for (int i = 0; i < 12; i++ )
                {
                    content += (i+1).ToString() + "\t";
                }
                content += nextLine;
                int curIndex = 0;
                for (int r = 0; r < 8; r++)
                {
                    for (int col = 0; col < 12; col++)
                    {
                        double v = Math.Round(vals[curIndex], 4);
                        content += v.ToString() + "\t";
                        curIndex++;
                    }
                    content += nextLine;
                }
                Clipboard.SetText(content);
                this.Shutdown();
                return;
            }
            base.OnStartup(e);
        }
       
    }
}
