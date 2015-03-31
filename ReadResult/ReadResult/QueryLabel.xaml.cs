using System;
using System.Collections.Generic;
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
        public QueryLabel()
        {
            InitializeComponent();
            this.Loaded += QueryLabel_Loaded;
            
        }

        void QueryLabel_Loaded(object sender, RoutedEventArgs e)
        {
            txtPath.Focus();
            var files = Directory.EnumerateFiles(GlobalVars.Instance.WorkingFolder, "*.xls");
            lstPlateNames.ItemsSource = files;
        }

        public string PlateName { get; set; }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string s = txtPath.Text;
            string wholePath = GlobalVars.Instance.WorkingFolder + "\\" + s + ".xls";
            if (lstPlateNames.SelectedItem != null)
            {
                wholePath = lstPlateNames.SelectedItem.ToString();
            }
            if(!File.Exists(wholePath))
            {
                SetInfo(string.Format("文件：{0}不存在！请重新输入。",wholePath));
                return;
            }
            FileInfo fileInfo = new FileInfo(wholePath);
            PlateName = fileInfo.Name;
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
            var files = Directory.EnumerateFiles(GlobalVars.Instance.WorkingFolder, "*.xls");
            List<string> names = files.Where(x => IsValid(x,plateName)).ToList();
            lstPlateNames.ItemsSource = names;
            if (names.Count > 0)
                lstPlateNames.SelectedIndex = 0;
        }

        private bool IsValid(string x,string prefix)
        {
            FileInfo fileInfo = new FileInfo(x);
            return fileInfo.Name.StartsWith(prefix);
        }
    }
}
