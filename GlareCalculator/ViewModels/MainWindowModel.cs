using System;
using System.ComponentModel;
using OxyPlot;
using System.Collections.Generic;
using OxyPlot.Wpf;
using OxyPlot.Axes;

namespace GlareCalculator.ViewModels
{
     public class HistogramModel:  INotifyPropertyChanged
    {
        private PlotModel plotModel;
        public PlotModel PlotModel
        {
            get { return plotModel; }
            set { 
                plotModel = value; 
                OnPropertyChanged("PlotModel");
            }
        }

        public HistogramModel()
        {
            SetUpModel();
            Histogram = new List<GrayInfo>();
            for(int i = 0; i<= 255; i++)
            {
                Histogram.Add(new GrayInfo(i, 0));
            }
        }


        private void SetUpModel()
        {
            PlotModel = new OxyPlot.PlotModel();
            PlotModel.LegendTitle = "Histogram";
            PlotModel.LegendOrientation = LegendOrientation.Horizontal;
            PlotModel.LegendPlacement = LegendPlacement.Outside;
            PlotModel.LegendPosition = LegendPosition.TopRight;
            PlotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            PlotModel.LegendBorder = OxyColors.Black;

            PlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Left, Minimum = 0, Title = "Count" });
            PlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = 255, Title = "Lightness" });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<GrayInfo> histogram;

        public List<GrayInfo> Histogram
        { 
            get 
            { 
                return histogram;
            }
            set
            {
                histogram = value;
                OnPropertyChanged("Histogram");
            }

        }
    }

     public class GrayInfo
     {
         public int Count { get; set; }
         public int GrayLevel { get; set; }


         public GrayInfo(int grayLevel, int cnt)
         {
             GrayLevel = grayLevel;
             Count = cnt;
         }
         public override string ToString()
         {
             return String.Format("Count:{0} Gray level", this.Count, this.GrayLevel);
         }
     }
}
