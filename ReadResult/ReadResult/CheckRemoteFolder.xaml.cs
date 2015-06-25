using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ReadResult
{
    /// <summary>
    /// Interaction logic for CheckRemoteFolder.xaml
    /// </summary>
    public partial class CheckRemoteFolder : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsValidFolder { get; set; }
        public CheckRemoteFolder()
        {
            IsValidFolder = false;
            InitializeComponent();
            this.Loaded += CheckRemoteFolder_Loaded;
            btnConfirm.IsEnabled = false;
            txtHint.Text = "请确保工作目录中有定义文件！\r\n";
        }
        void EnumFiles()
        {
            string root = GlobalVars.Instance.WorkingFolder;
            var dirs = Directory.EnumerateDirectories(root);
            dirs = dirs.Where(x => IsValidGroup(x));
            IEnumerable<string> validYears = GetValidYears(dirs);
            Print("valid years", validYears);

            IEnumerable<string> validMonths = GetValidMonths(validYears);
            Print("valid months", validMonths);

            List<string> allMonthsSub = new List<string>();
            foreach (string s in validMonths)
            {
                allMonthsSub.AddRange(Directory.EnumerateDirectories(s));
            }
            Print("valid months sub", allMonthsSub);

            List<string> latestFolders = new List<string>();
            foreach (string s in allMonthsSub)
            {
                latestFolders.AddRange(GetLatestThree(s));
            }
            Print("latest folders", latestFolders);

            List<string> allFiles = new List<string>();
            foreach (string s in latestFolders)
            {
                allFiles.AddRange(Directory.EnumerateFiles(s, "*.xls"));
            }
            allFiles = allFiles.Where(x => IsOD(x)).ToList();
            Print("allFiles", allFiles);
            GlobalVars.Instance.Files = allFiles;
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
               new Action(delegate()
                   {
                       lstPlates.ItemsSource = GlobalVars.Instance.Files;
                       btnConfirm.IsEnabled = true;
                   }));
            
        }
        void CheckRemoteFolder_Loaded(object sender, RoutedEventArgs e)
        {
            log.Info("begin load!");
            txtHint.Text += "begin load!\r\n";
            try
            {
                AsyncLoad();
            }
            catch(Exception ex)
            {
                txtHint.Text = "枚举文件失败: " + ex.Message;
            }
            //var files = Directory.EnumerateFiles(GlobalVars.Instance.WorkingFolder, "*.xls");
        }

        private void AsyncLoad()
        {
            ThreadStart start = new ThreadStart(EnumFiles);
            new Thread(start).Start();
        }


        private bool IsOD(string x)
        {
            x = x.ToLower();
            return x.Contains("_od");
        }

        private void Print(string desc, IEnumerable<string> contents)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(delegate()
                    {
                        txtHint.Text += desc+"\r\n";
                        foreach (string s in contents)
                        {
                            txtHint.Text += s + "\r\n";
                            //log.Info(s);
                        }
                        txtHint.Text += "\r\n";
                    }));
            log.Info(desc);
           
        }

        private IEnumerable<string> GetLatestThree(string s)
        {
            var dirs = Directory.EnumerateDirectories(s).ToList();
            dirs.Sort();
            dirs.Reverse();
            return dirs.Take(3);
        }

        private IEnumerable<string> GetValidMonths(IEnumerable<string> validYears)
        {
            var now = DateTime.Now;
            int curMonth = now.Month;
            List<string> validSubs = new List<string>();
            foreach (string s in validYears)
            {
                var subDirs = Directory.EnumerateDirectories(s);

                foreach (string subDir in subDirs)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(subDir);
                    string fileName = dirInfo.Name;
                    if (fileName.Contains("年") && fileName.Contains("月"))
                    {
                        int yearIndex = fileName.IndexOf("年");
                        int monthIndex = fileName.IndexOf("月");
                        var subStr = fileName.Substring(yearIndex + 1, monthIndex - yearIndex - 1);
                        int thisMonth = 0;
                        int.TryParse(subStr, out thisMonth);
                        if (thisMonth == curMonth)
                        {
                            validSubs.Add(subDir);
                        }
                    }

                }
            }
            return validSubs;
        }


        private IEnumerable<string> GetValidYears(IEnumerable<string> dirs)
        {
            int curYear = DateTime.Now.Year;
            List<string> validSubs = new List<string>();
            foreach (string s in dirs)
            {
                var subDirs = Directory.EnumerateDirectories(s);
                foreach (string subDir in subDirs)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(subDir);
                    string fileName = dirInfo.Name;
                    fileName = fileName.Replace("年", "");
                    int thisOneYear = 0;
                    bool bok = int.TryParse(fileName, out thisOneYear);
                    if (thisOneYear == curYear)
                    {
                        validSubs.Add(subDir);
                    }
                }
            }
            return validSubs;

        }

        private bool IsValidGroup(string x)
        {
            return x.Contains("A组") || x.Contains("B组") || x.Contains("D组");
        }
        //private void CopyRemoteFolder()
        //{
        //    string tempFolder = GlobalVars.Instance.TempFolder;
        //    if(Directory.Exists(tempFolder))
        //        Directory.Delete(tempFolder,true);
        //    Directory.CreateDirectory(tempFolder);
        //    string remoteFolder = GlobalVars.Instance.RemoteFolder;
        //    SharedFolder sharedFolder = new SharedFolder();
        //    sharedFolder.CopyFolderFromRemote(remoteFolder);
        //}
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if(lstPlates.Items.Count == 0)
            {
                SetInfo("没有找到定义文件！");
                return;
            }
            IsValidFolder = true;
            this.Close();
        }
        private void SetInfo(string s)
        {
            txtHint.Text = s;
            txtHint.Foreground = new SolidColorBrush(Colors.Red);
        }
    }
}
