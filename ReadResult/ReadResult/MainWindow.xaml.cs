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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
     
        [DllImport("user32.dll")]
        static extern bool ShowWindow(int hWnd, int nCmdShow);

    
        public MainWindow()
        {
            InitializeComponent();
            CheckRemoteFolder selectFolderForm = new CheckRemoteFolder();
            selectFolderForm.ShowDialog();
            if (!selectFolderForm.IsValidFolder)
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
            log.Info("main form loaded");
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
            log.Info("start acquisition");
            if (lstboxPlates.SelectedItem == null)
            {
                log.Info("Please select a plate first!");
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
                    log.Error(string.Format("Cannot find icontrol! Windows information has been written to:{0}", sInfoFile));
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
                log.Info("Cannot start acquisition, icontrol is not ready!");
                return;
            }
            log.Info("Acquisition started.");
            try
            {
                Utility.BackupFiles();
            }
            catch(Exception ex)
            {
                log.Error("backup failed:" + ex.Message);
            }
            log.Info("backup files");
            fileWatcher = new FileWatcher(GlobalVars.Instance.WorkingFolder);
            fileWatcher.onCreated += fileWatcher_onCreated;
           
            var click = GlobalVars.Instance.StartButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
            click.Invoke();
            Thread.Sleep(10000);
            fileWatcher.Start();
        }

        private void OnNewPlate()
        {
            log.Info("new plate");
            QueryLabel queryForm = new QueryLabel();
            var res = queryForm.ShowDialog();
            if (!(bool)res)
                return;
            string s = queryForm.PlateFilePath;
            log.Info(string.Format("new plate: {0}", s));
            FileInfo fileInfo = new FileInfo(s);
            plateNames.Add(fileInfo.Name);
            GlobalVars.Instance.PlatesInfo.AddPlate(s);
            lstboxPlates.SelectedIndex = plateNames.Count - 1;
        }

        void fileWatcher_onCreated(string sNewFile)
        {
            log.Info(string.Format("found result file: {0}", sNewFile));
            try
            {
                var result = Utility.ReadFromFile(sNewFile);
                log.InfoFormat("read {0} samples.", result.Count);
                GlobalVars.Instance.PlatesInfo.CurrentPlateData.SetValues(result);
                UpdateCurrentPlateInfo(GlobalVars.Instance.PlatesInfo.CurrentPlateName);
                ExcelInterop.Write();
                fileWatcher.onCreated -= fileWatcher_onCreated;
                //Save2Remote();
                log.Info(string.Format("Result has been written to plate: {0}", GlobalVars.Instance.PlatesInfo.CurrentPlateName));
            }
            catch(Exception ex)
            {
                Trace.Write("Error happend: " + ex.Message);
            }
            //plateRender.Refresh();
        }

        //private void Save2Remote()
        //{
        //    var curPlateName = GlobalVars.Instance.PlatesInfo.CurrentPlateName;
        //    var curStage = GlobalVars.Instance.PlatesInfo.CurrentPlateData.Stage;
        //    if (curStage == AcquiredStage.SampleVal)
        //    {
        //        string workingFolder = GlobalVars.Instance.TempFolder;
        //        string sFileName = workingFolder + curPlateName;
        //        SharedFolder sharedFolder = new SharedFolder();
        //        sharedFolder.SaveACopyfileToServer(sFileName);
        //    }
        //}

   
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
             log.Info(string.Format("select plate: {0}", curPlateName));
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
