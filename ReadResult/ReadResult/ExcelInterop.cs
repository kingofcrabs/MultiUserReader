using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReadResult
{
    class ExcelInterop
    {
        public static void Write()
        {
            PlateData plateData = GlobalVars.Instance.PlatesInfo.CurrentPlateData;
            string sFileName = GlobalVars.Instance.PlatesInfo.CurrentPlateData.FilePath;
            AcquiredStage curStage = plateData.Stage;
            Application excel = new Application();
            Workbook workBook = excel.Workbooks.Open(sFileName);
            Worksheet xlsWs = null;
            Range ExcelCellText;
            xlsWs = (Worksheet)workBook.Worksheets.get_Item(2);
            string sCell = curStage == AcquiredStage.BackGround ? "C12" : "C2";
            ExcelCellText = xlsWs.get_Range(sCell, Missing.Value);
            ExcelCellText = ExcelCellText.get_Resize(8, 12);
            double[,] myArray = new double[8, 12];
            int curIndex = 0;
            for (int r = 0; r < myArray.GetLength(0); r++)
            {
                for (int c = 0; c < myArray.GetLength(1); c++)
                {
                    double val = curStage == AcquiredStage.BackGround ? plateData.Values[curIndex].backGround : plateData.Values[curIndex].sampleVal;
                    myArray[r, c] = val;
                    curIndex++;
                }
            }
            ExcelCellText.set_Value(Missing.Value, myArray);
            workBook.Save();
            try
            {
                if (curStage == AcquiredStage.SampleVal)
                {
                    Process.Start("explorer.exe", sFileName);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("未能打印！");
            }
            workBook.Close();
            excel.Quit();
        }
    }
}
