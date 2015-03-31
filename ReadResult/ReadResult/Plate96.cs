using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;


namespace ReadResult
{
    enum XYFULL
    {
        XFull = 0,
        YFull
    };
    

    public class MicroPlate : UIBase
    {
        readonly int cols = 12;
        readonly int rows = 8;

        public MicroPlate(double w, double h)
            : base(w, h)
        {
            //this.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(LabwareUI_MouseLeftButtonDown);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            double usableWidth, usableHeight;
            CalcuUsable(out usableWidth, out usableHeight);
            if (usableHeight == 0 || usableWidth == 0)
                return;
            string curPlateName = GlobalVars.Instance.PlatesInfo.CurrentPlateName;
            if (curPlateName == null || curPlateName == "")
                return;

            DrawWells(drawingContext, usableWidth, usableHeight);
        }

        private void DrawWells(DrawingContext drawingContext, double usableWidth, double usableHeight)
        {
            double yUnit = usableHeight / rows;
            double xUnit = usableWidth  / cols;

            PlateData plateData = GlobalVars.Instance.PlatesInfo.CurrentPlateData;
            double maxVol = plateData.Max();
            for (int y = 0; y < rows; y++)
            {
                DrawYAxis(y, yUnit, drawingContext);
                for (int x = 0; x < cols; x++)
                {
                    if (y == 0)
                        DrawXAxis(x, xUnit, drawingContext);
                 
                    double xStart = xUnit * x;
                    double yStart = yUnit * y;

                    DrawRects(new Point(border + xStart, border + yStart), xUnit, yUnit, x, y, plateData, maxVol, drawingContext);
                }
            }
        }

        private int GetID(int x, int y)
        {
            return x * 8 + y + 1;
        }

        private void DrawRects(Point ptStart, double width, double height,
            int x, int y,
            PlateData plateDate, double maxVol,
            DrawingContext drawingContext)
        {
            int id = GetID(x, y);
            Size cellSize = new Size(width, height);
            //rect
            Color thinPenColor = Colors.Gray;
            Pen thinPen = new Pen(new SolidColorBrush(thinPenColor), 1);
            SolidColorBrush renderBrush = new SolidColorBrush(Colors.Transparent);
            drawingContext.DrawRectangle(renderBrush, thinPen, new Rect(ptStart, cellSize));
            
            //vol
            double vol2Show = 0;
      
            if (plateDate.Stage == AcquiredStage.BackGround)
            {
                SolidColorBrush volBrush = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
                double val = plateDate[id].backGround;
                vol2Show = val;
                DrawVol(val, maxVol, cellSize, ptStart, volBrush, thinPen, drawingContext);
            }
            else
            {
                SolidColorBrush volBrush = new SolidColorBrush(Colors.LightBlue);
                double val = plateDate[id].sampleVal - plateDate[id].backGround;
                vol2Show = val;
                DrawVol(val, maxVol, cellSize, ptStart, volBrush, thinPen, drawingContext);
            }

            //id & volume
            int xStart = (int)(ptStart.X + width * 0.1);
            int yStart = (int)(ptStart.Y + height * 0.2);

            var txtSrcPos = new FormattedText(
             string.Format("{0}{1}", (char)(y + 'A'), x + 1),
             System.Globalization.CultureInfo.CurrentCulture,
             FlowDirection.LeftToRight,
             new Typeface("Courier new"),
             height / 4,
             Brushes.Blue);
          
            int yStartVol = (int)(ptStart.Y + height * 0.6);
            var txtVol = new FormattedText(
            string.Format("{0}", vol2Show),
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Courier new"),
            height / 4,
            Brushes.Blue);

            //drawingContext.DrawText(txt, new Point(xStart, yStart));
            drawingContext.DrawText(txtSrcPos, new Point(xStart, yStart));
            drawingContext.DrawText(txtVol, new Point(xStart, yStartVol));
        }

        private void DrawVol(double vol, double max,Size cellSize, Point ptStart, SolidColorBrush volBrush, Pen thinPen, DrawingContext drawingContext)
        {
            if (vol < 0)
                return;
            double percent = vol / max;
            Point ptOffSet = new Point(ptStart.X, ptStart.Y);
            ptOffSet.Y += (1 - percent) * cellSize.Height; ;
            drawingContext.DrawRectangle(volBrush, thinPen, new Rect(ptOffSet, new Size(cellSize.Width, cellSize.Height * percent)));
        }
    }
}
