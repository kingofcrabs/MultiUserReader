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
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string s = txtPath.Text;
            string wholePath = GlobalVars.Instance.WorkingFolder + s + ".txt";
            if(!File.Exists(wholePath))
            {
                SetInfo(string.Format("文件：{0}不存在！请重新输入。",wholePath));
                return;
            }
            this.Close();
        }

        private void SetInfo(string s)
        {
            txtHint.Text = s;
            txtHint.Foreground = new SolidColorBrush(Colors.Red);
        }
    }
}
