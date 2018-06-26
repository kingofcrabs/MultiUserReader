using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InputPlateHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> allPlateFiles = new List<string>();
        ObservableCollection<string> selectedPlates = new ObservableCollection<string>();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.N:
                    OnNewPlate();
                    break;
                case Key.Delete:
                    OnDeleteSelected();
                    break;
                default:
                    break;
            }
        }

        private void OnDeleteSelected()
        {
            if(lstSelectedPlates.SelectedItem != null)
            {
                selectedPlates.Remove(lstSelectedPlates.SelectedItem.ToString());
            }
        }

        private void OnNewPlate()
        {
            var interestFiles = new ObservableCollection<string>(allPlateFiles);
            lstExistingPlates.ItemsSource = interestFiles;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string sFile = FolderHelper.GetExeParentFolder() + "files.txt";
            allPlateFiles = File.ReadLines(sFile).ToList();
            allPlateFiles = allPlateFiles.Where(x => !x.Contains("Vol")).ToList();
            lstSelectedPlates.ItemsSource = selectedPlates;
            OnNewPlate();
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateValidLabels(txtName.Text);
        }

        private void UpdateValidLabels(string plateName)
        {
            var interestFiles = new ObservableCollection<string>(allPlateFiles.Where(x => IsValid(x, plateName)));
            lstExistingPlates.ItemsSource = interestFiles;

            //lstPlateNames.ItemsSource = interestFiles;
            if (interestFiles.Count > 0)
                lstExistingPlates.SelectedIndex = 0;
        }

        private bool IsValid(string x, string prefix)
        {
            FileInfo fileInfo = new FileInfo(x);
            return fileInfo.Name.Contains(prefix);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if(lstExistingPlates.SelectedIndex != -1)
            {
                if(!selectedPlates.Contains(lstExistingPlates.SelectedItem.ToString()))
                {
                    selectedPlates.Add(lstExistingPlates.SelectedItem.ToString());
                }
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if(selectedPlates.Count == 0)
            {
                MessageBox.Show("请选择要测试的板子！");
                return;
            }
            string sFile = FolderHelper.GetExeParentFolder() + "selected.txt";
            File.WriteAllLines(sFile, selectedPlates);
            string sCnt = FolderHelper.GetExeParentFolder() + "cnt.txt";
            File.WriteAllText(sCnt, selectedPlates.Count.ToString());
            this.Close();
        }
    }

    public static class FolderHelper
    {
        public static string GetExeFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }

        static public string GetExeParentFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            int index = s.LastIndexOf("\\");
            return s.Substring(0, index) + "\\";
        }
    }
}
