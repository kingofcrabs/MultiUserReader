using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ReadResult
{
    public class UIBase : UIElement
    {
        protected double width;
        protected double height;
        protected double border;

        private bool bAcceptInput = false;
        public Size BoundingSize
        {
            set
            {
                this.width = value.Width;
                this.height = value.Height;
                InvalidateVisual();
            }
        }
        public UIBase(double w, double h)
        {
            width = w;
            height = h;
        }

        public bool AcceptInput
        {
            get
            {
                return bAcceptInput;
            }
            set
            {
                bAcceptInput = value;
            }
        }

        protected void CalcuUsable(out double usableWidth, out double usableHeight)
        {
            usableWidth = width;
            usableHeight = height;
            border = Math.Min(usableWidth, usableHeight) * 0.1;

            double screenRatio = (width - border) / (height - border);
            double realRatio = 1.5;
            if (realRatio > screenRatio)//x方向占满
            {
                usableHeight = (height - border) / (realRatio / screenRatio);
                usableWidth = width - border;
            }
            else //y方向占满
            {
                usableWidth = (width - border) / (screenRatio / realRatio);
                usableHeight = height - border;
            }
        }

        protected void DrawXAxis(int x, double unit, DrawingContext drawingContext)
        {
            var txt = new FormattedText(string.Format("{0}", x + 1),
               System.Globalization.CultureInfo.CurrentCulture,
               FlowDirection.LeftToRight,
               new Typeface("Courier new"),
               unit * 0.6,
               Brushes.Black);

            drawingContext.DrawText(txt, new Point(border + (0.25 + x) * unit, unit * 0.2));
        }

        protected void DrawYAxis(double i, double yUnit, DrawingContext drawingContext)
        {
            var txt = new FormattedText(string.Format("{0}", (char)('A' + i)),
               System.Globalization.CultureInfo.CurrentCulture,
               FlowDirection.LeftToRight,
               new Typeface("Courier new"),
               yUnit / 2,
               Brushes.Black);

            drawingContext.DrawText(txt, new Point(border / 2, border + (0.25 + i) * yUnit));
        }
    }
}
