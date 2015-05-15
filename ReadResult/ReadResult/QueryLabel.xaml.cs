using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for QueryLabel.xaml
    /// </summary>
    public partial class QueryLabel : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        ObservableCollection<string> interestFiles = new ObservableCollection<string>();
        public QueryLabel()
        {
            InitializeComponent();
            this.Loaded += QueryLabel_Loaded;
        }

        void QueryLabel_Loaded(object sender, RoutedEventArgs e)
        {
            log.Info("enum files");
            txtPath.Focus();
            interestFiles = new ObservableCollection<string>(GlobalVars.Instance.Files);
            lstPlateNames.ItemsSource = interestFiles;
        }

        private void SetDummyFiles()
        {
            for(int i = 0; i< 20; i++)
            {
                GlobalVars.Instance.Files.Add(string.Format("just test{0}.csv", i + 1));
            }
        }

        public string PlateFilePath { get; set; }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            log.Info("confirm");
            string s = txtPath.Text;
          
            if(lstPlateNames.SelectedItem == null)
            {
                SetInfo("请选择需要读数的板子！");
                return;
            }
            string wholePath = lstPlateNames.SelectedItem.ToString();
            FileInfo fileInfo = new FileInfo(wholePath);
            PlateFilePath = wholePath;
            if (GlobalVars.Instance.PlatesInfo.PlateNames.Contains(fileInfo.Name))
            {
                SetInfo(string.Format("板子:{0}已经存在，请重新选择！", PlateFilePath));
                return;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void SetInfo(string s)
        {
            txtHint.Text = s;
            txtHint.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void txtPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValidLabels(txtPath.Text);
        }

        private void UpdateValidLabels(string plateName)
        {
            interestFiles = new ObservableCollection<string>(GlobalVars.Instance.Files.Where(x => IsValid(x, plateName)));
            lstPlateNames.ItemsSource = interestFiles;
            if (interestFiles.Count > 0)
                lstPlateNames.SelectedIndex = 0;
        }

        private bool IsValid(string x,string prefix)
        {
            FileInfo fileInfo = new FileInfo(x);
            return fileInfo.Name.Contains(prefix);
        }

        private void btnRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            log.Info("remove some items");
            List<string> selectedFileNames = new List<string>();
            foreach(string fileName in lstPlateNames.SelectedItems)
            {
                selectedFileNames.Add(fileName);
            }
            for(int i = 0; i< selectedFileNames.Count; i++)
            {
                interestFiles.Remove(selectedFileNames[i]);
            }
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            log.Info("restore items");
            txtPath.Text = "";
            interestFiles.Clear();
            foreach (string sFile in GlobalVars.Instance.Files)
                interestFiles.Add(sFile);
        }
    }
}
