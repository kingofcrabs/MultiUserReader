using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
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
        public bool IsValidFolder { get; set; }
        public CheckRemoteFolder()
        {
            IsValidFolder = false;
            InitializeComponent();
            this.Loaded += CheckRemoteFolder_Loaded;
            txtHint.Text = "请确保工作目录中有定义文件！";
        }

        void CheckRemoteFolder_Loaded(object sender, RoutedEventArgs e)
        {
            //DialogResult = false;
            CopyRemoteFolder();
            var files = Directory.EnumerateFiles(GlobalVars.Instance.TempFolder, "*.xls");
            lstPlates.ItemsSource = files;
        }
        private void CopyRemoteFolder()
        {
            string tempFolder = GlobalVars.Instance.TempFolder;
            Directory.Delete(tempFolder,true);
            Directory.CreateDirectory(tempFolder);
            string remoteFolder = GlobalVars.Instance.RemoteFolder;
            SharedFolder sharedFolder = new SharedFolder();
            sharedFolder.CopyFolderFromRemote(remoteFolder);
        }
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
