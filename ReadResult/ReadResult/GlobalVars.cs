using System;
using System.Collections.Generic;
using System.ComponentModel;
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
         }

        public string WorkingFolder { get; set; }

        public System.Windows.Automation.AutomationElement StartButton { get; set; }

        public ManagedWinapi.Windows.SystemWindow IControlWindow { get; set; }
    }

   


    public class PlatesInfo
    {
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
            plateVals.Add(sName, new PlateData());
            CurrentPlateName = sName;
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

        public PlateData()
        {
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
