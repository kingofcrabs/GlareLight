using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GlareCalculator
{
    class ColorCanvas:Canvas
    {
        ColorMap map;
        int[,] cmap = new int[128, 4];
        double min, max;
        public ColorCanvas()
        {
            map = new ColorMap();
            cmap = map.Jet();
        }
        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            double width = this.ActualWidth/3;
            double height = this.ActualHeight;
            int cnt = 128;
            double dy = height / cnt;
            for (int i = 0; i < cnt; i++)
            {
                int colorIndex = i;
                SolidColorBrush brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    (byte)cmap[colorIndex, 0], (byte)cmap[colorIndex, 1],
                    (byte)cmap[colorIndex, 2], (byte)cmap[colorIndex, 3]));
                drawingContext.DrawRectangle(brush, new Pen(brush, 1),
                    new System.Windows.Rect(0, height - dy - i * dy, width, dy));
            }
            
            double adjustMax = ((int)max) / 1000 * 1000;
            double hUnit = (adjustMax / max) * height / 10.0;

            double vUnit = adjustMax / 10;

            double startX = width * 1.2;
            double yOffset = (max - adjustMax) / max * height;
            for(int i = 0; i < 10; i++)
            {
                double curHeight = i * hUnit + yOffset;
                double curV = adjustMax - vUnit * i;
                string sLV = ((int)curV).ToString();
                FormattedText ft = new FormattedText(sLV, CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,new Typeface("Arial"), 10, Brushes.Black);
                drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(width, curHeight), new Point(startX, curHeight));
                drawingContext.DrawText(ft, new Point(startX, curHeight));
            }
        }

        internal void SetMinMax(double min, double max)
        {
            this.min = min;
            this.max = max;
        }
    }

   
}
