using NHotkey.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Xml;

namespace ReadResult
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<string> plateNames = new ObservableCollection<string>();
        private const int SW_SHOWMAXIMIZED = 3;
        TraceListener _textBoxListener;
        PlateRender plateRender;
        FileWatcher fileWatcher = null;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(int hWnd, int nCmdShow);

    
        public MainWindow()
        {
            InitializeComponent();
            SetWorkingFolder selectFolderForm = new SetWorkingFolder();
            selectFolderForm.ShowDialog();
            lstboxPlates.ItemsSource = plateNames;
            
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        #region trace
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Trace.Listeners.Clear();
        }


        private void AddTracer()
        {
            _textBoxListener = new TextBoxTraceListener(txtLog);
            _textBoxListener.Name = "Textbox";
            _textBoxListener.TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ThreadId;
            Trace.Listeners.Add(_textBoxListener);
        }
        #endregion


     

        public static AutomationElement GetWindowByName(string name)
        {
            AutomationElement root = AutomationElement.RootElement;
            var collection = root.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
            foreach (AutomationElement window in collection)
            {
                //Debug.WriteLine(window.Current.Name + window.Current.ClassName);
                if (window.Current.Name.Contains(name))
                {
                    return window;
                }
            }
            return null;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AddTracer();
            lstboxPlates.SelectedIndex = 0;
            HotkeyManager.Current.AddOrReplace("CreateNew", Key.N, ModifierKeys.Control , OnNewPlate);
            HotkeyManager.Current.AddOrReplace("StartAcq", Key.G, ModifierKeys.None, OnStartAcq);
            HotkeyManager.Current.AddOrReplace("F1", Key.F1, ModifierKeys.None, OnHelp);

          
            plateRender = new PlateRender(this);
            myCanvas.Children.Add(plateRender);
        }

        private void OnHelp(object sender, NHotkey.HotkeyEventArgs e)
        {
            Help helpForm = new Help();
            helpForm.ShowDialog();
        }

        private void OnStartAcq(object sender, NHotkey.HotkeyEventArgs e)
        {
            if (lstboxPlates.SelectedItem == null)
            {
                Trace.WriteLine("Please select a plate first!");
                return;
            }

            AutomationElement iControl = GetWindowByName("control");
            if (iControl == null)
            {
                Trace.WriteLine("Cannot find icontrol!");
                return;
            }

            // Sample usage
            ShowWindow(iControl.Current.NativeWindowHandle, SW_SHOWMAXIMIZED);
            Thread.Sleep(200);
            AutomationElement startBtn = iControl.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Start"));

            Trace.WriteLine("Acquisition started.");
            Utility.BackupFiles();
            fileWatcher = new FileWatcher(GlobalVars.Instance.WorkingFolder);
            fileWatcher.onCreated += fileWatcher_onCreated;
            fileWatcher.Start();

            var click = startBtn.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            click.Invoke();
        }

        private void OnNewPlate(object sender, NHotkey.HotkeyEventArgs e)
        {
            QueryLabel queryForm = new QueryLabel();
            var res = queryForm.ShowDialog();
            if (!(bool)res)
                return;
            string s = queryForm.PlateName;
            Trace.WriteLine(string.Format("new plate: {0}", s));
            plateNames.Add(s);
            GlobalVars.Instance.PlatesInfo.AddPlate(s);
            lstboxPlates.SelectedIndex = plateNames.Count - 1;
        }


        public XmlNode GetNode(XmlNodeList nodeList, string name)
        {
            foreach (XmlNode node in nodeList)
            {
                if (node.Name == name)
                    return node;
            }
            throw new Exception("Cannot find");
        }


        private List<double> ReadFromFile(string sNewFile)
        {
            List<double> vals = new List<double>();
            XmlDocument doc = new XmlDocument();
            doc.Load(sNewFile);    //加载Xml文件  
            XmlElement rootElem = doc.DocumentElement;   //获取根节点  
            XmlNode sectionNode = GetNode(rootElem.ChildNodes, "Section");
            XmlNode dataNode = GetNode(sectionNode.ChildNodes, "Data");
            foreach (XmlNode node in dataNode.ChildNodes)
            {
                string sVal = node.InnerText;
                vals.Add(double.Parse(sVal));
            }
            Trace.WriteLine(string.Format("Read {0} well values.", vals.Count));
            return vals;
        }
    

        void fileWatcher_onCreated(string sNewFile)
        {
            Trace.WriteLine(string.Format("found result file: {0}", sNewFile));
            try
            {
                var result = ReadFromFile(sNewFile);
                GlobalVars.Instance.PlatesInfo.CurrentPlateData.SetValues(result);
                UpdateCurrentPlateInfo(GlobalVars.Instance.PlatesInfo.CurrentPlateName);
                ExcelInterop.Write();
                Trace.WriteLine(string.Format("Result has been written to plate: {0}", GlobalVars.Instance.PlatesInfo.CurrentPlateName));
            }
            catch(Exception ex)
            {
                Trace.Write("Error happend: " + ex.Message);
            }
            //plateRender.Refresh();
        }

   
        private void lstboxPlates_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstboxPlates == null)
                return;

             if (lstboxPlates.SelectedIndex == -1)
                return;

             lstboxPlates.Focus();
             string curPlateName = lstboxPlates.SelectedItem.ToString();
             if (!GlobalVars.Instance.PlatesInfo.PlateNames.Contains(curPlateName))
                 return;
             Trace.WriteLine(string.Format("select plate: {0}", curPlateName));
             UpdateCurrentPlateInfo(curPlateName, false);
        }

        private void UpdateCurrentPlateInfo(string curPlateName,bool switchWindowState = true)
        {
            GlobalVars.Instance.PlatesInfo.CurrentPlateName = curPlateName;
            this.Dispatcher.Invoke( ()=>
                  {
                      chkboxBackGround.IsChecked = GlobalVars.Instance.PlatesInfo.CurrentPlateData.BackGround;
                      chkboxSampleVal.IsChecked = GlobalVars.Instance.PlatesInfo.CurrentPlateData.SampleVal;
                      //if (switchWindowState)
                      {
                          this.Height = this.Height + 1;
                          this.Refresh();
                          this.Height = this.Height - 1;
                          //this.WindowState = System.Windows.WindowState.Maximized;
                          //this.WindowState = System.Windows.WindowState.Normal;
                      }
                  });
          
        }
    }



    

    public static class ExtensionMethods
    {

        private static Action EmptyDelegate = delegate() { };



        public static void Refresh(this UIElement uiElement)
        {

            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);

        }

    }
}
