using ManagedWinapi.Windows;
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
            var result = selectFolderForm.ShowDialog();
            if(result != System.Windows.Forms.DialogResult.OK)
            {
                this.Close();
                return;
            }
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


     

       

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AddTracer();
            lstboxPlates.SelectedIndex = 0;
            this.KeyDown += MainWindow_KeyDown;
            plateRender = new PlateRender(this);
            myCanvas.Children.Add(plateRender);
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
             
            switch(e.Key)
            {
                case Key.G:
                    OnStartAcq();
                    break;
                case Key.N:
                    if( Keyboard.Modifiers == ModifierKeys.Control)
                        OnNewPlate();
                    break;
                case Key.F1:
                    OnHelp();
                    break;
                default:
                    break;
            }
        }

        private void OnHelp()
        {
            Help helpForm = new Help();
            helpForm.ShowDialog();
        }

        private void OnStartAcq()
        {
            if (lstboxPlates.SelectedItem == null)
            {
                Trace.WriteLine("Please select a plate first!");
                return;
            }


            if (GlobalVars.Instance.StartButton == null)
            {
                List<string> windowInfos = new List<string>();
                SystemWindow[] windows = SystemWindow.AllToplevelWindows;
                SystemWindow icontrolWindow = null;
                for (int i = 0; i < windows.Length; i++)
                {
                    string sTitle = windows[i].Title;
                    if (sTitle != "")
                        windowInfos.Add(sTitle);
                    sTitle = sTitle.ToLower();
                    if (sTitle.Contains("tecan") && sTitle.Contains("control"))
                    {
                        icontrolWindow = windows[i];
                        GlobalVars.Instance.IControlWindow = icontrolWindow;
                        break;
                    }
                }

                if (icontrolWindow == null)
                {
                    string sInfoFile = @"c:\windowsInfo.txt";
                    File.WriteAllLines(sInfoFile, windowInfos);
                    Trace.WriteLine(string.Format("Cannot find icontrol! Windows information has been written to:{0}", sInfoFile));
                    return;
                }

                AutomationElement iControl = AutomationElement.FromHandle(icontrolWindow.HWnd);
                // Sample usage
                ShowWindow(iControl.Current.NativeWindowHandle, SW_SHOWMAXIMIZED);
                Thread.Sleep(200);
                AutomationElement startBtn = iControl.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Start"));
                GlobalVars.Instance.StartButton = startBtn;
            }
            bool bEnable = (bool)GlobalVars.Instance.StartButton.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty);
            if(!bEnable)
            {
                Trace.WriteLine("Cannot start acquisition, icontrol is not ready!");
                return;
            }
            Trace.WriteLine("Acquisition started.");
            Utility.BackupFiles();
            fileWatcher = new FileWatcher(GlobalVars.Instance.WorkingFolder);
            fileWatcher.onCreated += fileWatcher_onCreated;
            var click = GlobalVars.Instance.StartButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            click.Invoke();
            fileWatcher.Start();
        }

        private void OnNewPlate()
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
            this.Dispatcher.Invoke( new Action(()=>{
                  
                      chkboxBackGround.IsChecked = GlobalVars.Instance.PlatesInfo.CurrentPlateData.BackGround;
                      chkboxSampleVal.IsChecked = GlobalVars.Instance.PlatesInfo.CurrentPlateData.SampleVal;
                      //if (switchWindowState)
                      this.Height = this.Height + 1;
                      this.Refresh();
                      this.Height = this.Height - 1;
                      }
                  ));
          
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
