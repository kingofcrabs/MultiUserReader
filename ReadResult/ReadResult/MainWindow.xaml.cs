using HDLibrary.Wpf.Input;
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
        ObservableCollection<string> userNames = new ObservableCollection<string>();
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
            lstboxPlates.ItemsSource = userNames;
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
            
            HotKeyHost hotKeyHost = new HotKeyHost((HwndSource)HwndSource.FromVisual(App.Current.MainWindow));
            hotKeyHost.AddHotKey(new CustomHotKey(this,"CreateNew", Key.N, ModifierKeys.Control, true));
            hotKeyHost.AddHotKey(new CustomHotKey(this,"StartAcq", Key.G,ModifierKeys.None, true));
            plateRender = new PlateRender(this);
            myCanvas.Children.Add(plateRender);
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

        internal void OnStartAcquisition()
        {
            if(lstboxPlates.SelectedItem == null)
            {
                Trace.WriteLine("Please select a plate first!");
                return;
            }

            AutomationElement iControl = GetWindowByName("control");
            if(iControl ==null)
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

    

        void fileWatcher_onCreated(string sNewFile)
        {
            Trace.WriteLine(string.Format("found result file: {0}", sNewFile));
            var result = ReadFromFile(sNewFile);
            GlobalVars.Instance.PlatesInfo.CurrentPlateData.SetValues(result);
            UpdateCurrentPlateInfo(GlobalVars.Instance.PlatesInfo.CurrentPlateName);
            
            //plateRender.Refresh();
        }

        public void OnNewPlate()
        {
            QueryLabel queryForm = new QueryLabel();
            var res = queryForm.ShowDialog();
            if (!(bool)res)
                return;
            string s = queryForm.PlateName;
            Trace.WriteLine(string.Format("new plate: {0}", s));
            userNames.Add(s);
            GlobalVars.Instance.PlatesInfo.AddPlate(s);
            lstboxPlates.SelectedIndex = userNames.Count - 1;
        }

        private void lstboxPlates_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstboxPlates == null)
                return;

             if (lstboxPlates.SelectedIndex == -1)
                return;
             lstboxPlates.Focus();
             string curPlateName = lstboxPlates.SelectedItem.ToString();
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



    [Serializable]
    public class CustomHotKey : HotKey
    {

        MainWindow hostWindow = null;
        public CustomHotKey(MainWindow window, string name, Key key, ModifierKeys modifiers, bool enabled)
            : base(key, modifiers, enabled)
        {
            hostWindow = window;
            Name = name;
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged(name);
                }
            }
        }

        protected override void OnHotKeyPress()
        {
            //MessageBox.Show(string.Format("'{0}' has been pressed ({1})", Name, this));
            if(Key == System.Windows.Input.Key.N && Modifiers == ModifierKeys.Control)
            {
                //Ctrl + N
                hostWindow.OnNewPlate();
            }
            else if (Key == System.Windows.Input.Key.G)
            {
                hostWindow.OnStartAcquisition();
            }
            base.OnHotKeyPress();
        }


        protected CustomHotKey(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
            Name = info.GetString("Name");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Name", Name);
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
