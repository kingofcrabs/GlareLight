using System;
using System.ComponentModel;
using OxyPlot;
using System.Collections.Generic;
using OxyPlot.Wpf;
using OxyPlot.Axes;
using System.Linq;
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
            PlotModel = new OxyPlot.PlotModel();
            Histogram = new List<GrayInfo>();
            for(int i = 0; i<= 255; i++)
            {
                Histogram.Add(new GrayInfo(i, 0));
            }
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
                if (histogram.Count == 0)
                {
                    Desc = "";
                }
                else
                {
                    Desc = GetDescription(histogram);
                    AnnotationPosition = new DataPoint(200, histogram.Max(x => x.Count / 1.5));
                }
                OnPropertyChanged("Histogram");
            }

        }

        DataPoint position;
        public DataPoint AnnotationPosition
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
                OnPropertyChanged("AnnotationPosition");
            }
        }
        private string GetDescription(List<GrayInfo> histogram)
        {
            int total = histogram.Sum(x => x.Count);
            int min = 255, max = 0;
            int threshold = total / 100;
            for (int i = 0; i < histogram.Count; i++ )
            {
                if(histogram[i].Count > threshold)
                {
                    if (i > max)
                        max = i;
                    if (i < min)
                        min = i;
                }
            }
            int cnt = max - min + 1;
            int quality = 100 - (int)((255 - cnt) / 2.55);

            return string.Format("图像质量：{0}", quality);
        }

        private string description;
        public string Desc
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                OnPropertyChanged("Desc");
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
