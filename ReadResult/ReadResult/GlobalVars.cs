using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadResult
{
    public class GlobalVars
    {
        private static GlobalVars vars = null;
        public PlatesInfo PlateInfo { get; set; }
        public RenderSelection RenderSelection { get; set; }
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
            PlateInfo = new PlatesInfo(new List<string>(){"Rex","Shark"});
        }

        public string WorkingFolder { get; set; }
    }

    public class RenderSelection
    {
        public bool backGround;
        public bool sampleVal;
        public bool calculated;
        public RenderSelection(bool b, bool s, bool c)
        {
            backGround = b;
            sampleVal = s;
            calculated = c;
        }
    }


    public class PlatesInfo
    {
        Dictionary<string, PlateData> plateVals = new Dictionary<string, PlateData>();
        
        public string CurrentPlateName { get; set; }

        public PlatesInfo( List<string> userNames)
        {
            foreach (string userName in userNames)
            {
                plateVals.Add(userName, new PlateData());
            }
            CurrentPlateName = userNames.First();
        }

        public PlateData CurrentPlateData
        {
            get
            {
                return plateVals[CurrentPlateName];
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
        public AcquiredStage Stage { get; set; }
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
