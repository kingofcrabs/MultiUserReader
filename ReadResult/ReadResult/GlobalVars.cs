using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReadResult
{
    public class GlobalVars
    {
      
        private static GlobalVars vars = null;
        public PlatesInfo PlatesInfo { get; set; }
        public static GlobalVars Instance
        {
            get
            {
                if (vars == null)
                    vars = new GlobalVars();
                return vars;
            }
        }   
        public GlobalVars()
        {
            PlatesInfo = new PlatesInfo();
            Files = new List<string>();
         }

        //public string TempFolder
        //{
        //    get
        //    {
        //        return ConfigurationManager.AppSettings["tempFolder"];
        //    }
        //}

        //public string RemoteFolder
        //{
        //    get
        //    {
        //        return ConfigurationManager.AppSettings["remoteFolder"];
        //    }
        //}

        public System.Windows.Automation.AutomationElement StartButton { get; set; }

        public ManagedWinapi.Windows.SystemWindow IControlWindow { get; set; }

        public string WorkingFolder 
        { 
            get
            {
                return ConfigurationManager.AppSettings["workingFolder"];
            }
        }

        public List<string> Files { get; set; }
    }

   


    public class PlatesInfo
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        Dictionary<string, PlateData> plateVals = new Dictionary<string, PlateData>();
        
        public string CurrentPlateName { get; set; }

        public PlatesInfo()
        {
            //AddDummyData();
        }

      
        public void AddPlate(string sName)
        {
#if DEBUG
            if (plateVals.ContainsKey(sName))
                return;
#endif
            FileInfo fileInfo = new FileInfo(sName);
            log.InfoFormat("plate name: {0}, plate folder:{1}",fileInfo.Name,sName);
            plateVals.Add(fileInfo.Name, new PlateData(sName));
            CurrentPlateName = fileInfo.Name;
        }

        public PlateData CurrentPlateData
        {
            get
            {
                return plateVals[CurrentPlateName];
            }
        }


        public List<string> PlateNames 
        { 
            get 
            {
                return plateVals.Keys.ToList();
            }
        }
    }

    public class WellVals
    {
        public double backGround;
        public double sampleVal;
        public WellVals(double b = 0, double s = 0)
        {
            backGround = b;
            sampleVal = s;
        }
    }

    public enum AcquiredStage
    {
        Nothing = 0,
        BackGround = 1,
        SampleVal = 2
    }


    

    public class PlateData
    {
        private AcquiredStage stage;
        public string FilePath { get; set; }
        public AcquiredStage Stage
        { 
            get
            {
                return stage;
            }
            set
            {
                stage = value;
            }
        }

        public void SetValues(List<double> result)
        {
            if(stage == AcquiredStage.Nothing)
            {
                SetBackGroudData(result);
            }
            else
            {
                SetSampleValues(result);
            }
        }

        private void SetBackGroudData(List<double> bkVals)
        {
            for(int i = 0; i< bkVals.Count; i++)
            {
                vals[i].backGround = bkVals[i];
            }
            stage = AcquiredStage.BackGround;
        }

        private void SetSampleValues(List<double> smpVals)
        {
            for (int i = 0; i < smpVals.Count; i++)
            {
                vals[i].sampleVal = smpVals[i];
            }
            stage = AcquiredStage.SampleVal;
        }

        public List<WellVals> Values
        {
            get
            {
                return vals;
            }
        }

        List<WellVals> vals = new List<WellVals>();
        public WellVals this[int wellID]
        {
            get
            {
                return vals[wellID - 1];
            }
            set
            {
                vals[wellID - 1] = value;
            }
        }

        public bool BackGround
        {
            get
            {
                return Stage > AcquiredStage.Nothing;
            }
        }

        public bool SampleVal
        {
            get
            {
                return Stage == AcquiredStage.SampleVal;
            }
        }

        public PlateData(string sFilePath)
        {
            FilePath = sFilePath;
            Stage = AcquiredStage.Nothing;
            for(int i = 0; i< 96; i++)
            {
                vals.Add(new WellVals());
            }
        }

        internal double Max()
        {
            return Math.Max(vals.Max(x=>x.backGround), vals.Max(x => x.sampleVal));
        }

       
    }



}
