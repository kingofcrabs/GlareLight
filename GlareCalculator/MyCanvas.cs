using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace GlareCalculator
{
    public class MyCanvas : Canvas
    {

        BitmapImage _img = null;
        List<Polygon> polygons = new List<Polygon>();
        Polygon tempPolygon = new Polygon();
        Point invalidPt = new Point(-1,-1);
        int selectedIndex = -1;
        bool shouldBlow = false;
        public delegate void PolygonChanged(List<Polygon> polygons);
        public event PolygonChanged onPolygonChanged;
        private Timer timer = new Timer(500);

        public MyCanvas()
        {
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }
        public void SetBkGroundImage()
        {
            string sFile = ConfigurationManager.AppSettings["srcImage"];
            _img = new BitmapImage();
            _img.BeginInit();
            _img.CacheOption = BitmapCacheOption.OnLoad;
            _img.UriSource = new Uri(sFile, UriKind.Absolute);
            _img.EndInit();
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = _img;
            this.Background = imageBrush;
        }

        public void SetBkGroundImage(BitmapImage bmpImage)
        {
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = bmpImage;
            this.Background = imageBrush;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            shouldBlow = !shouldBlow;
            this.Dispatcher.Invoke(new System.Action(() =>
            {
                InvalidateVisual();
            }));
        }
       
        internal void LeftMouseUp(Point pt) //add pt to polygon
        {
            tempPolygon.pts.Add(pt);
            tempPolygon.currentPt = pt;
            InvalidateVisual();
        }

        internal static bool IsInDesignMode()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location.Contains("VisualStudio");
        }
        internal void Add()
        {
            if (tempPolygon.pts.Count < 3)
                throw new Exception("多边形顶点必须大于等于3！");
            if (!tempPolygon.Finished)
                throw new Exception("未添加完成！");

            polygons.Add(tempPolygon);
            tempPolygon = new Polygon();
            polygons.ForEach(x => x.Selected = false);
            
            InvalidateVisual();
            NotifyPolygonChanged();
        }

        internal void DeleteLastPoint()
        {
            tempPolygon.RemoveLast();
            InvalidateVisual();
        }

   
        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (IsInDesignMode())
                return;
            //if(tempPolygon.currentPt != invalidPt)
            //{
            //    int radius = shouldBlow ? 3 : 1;
            //    drawingContext.DrawEllipse(null, new Pen(Brushes.Red, 2), tempPolygon.currentPt, radius, radius);
            //}
            
            //DrawPolygon(tempPolygon,Brushes.Blue,drawingContext);
            tempPolygon.Render(drawingContext, shouldBlow);
            var grayBrush = Brushes.Gray;
            for (int i = 0; i < polygons.Count; i++)
            {
                polygons[i].Render(drawingContext, shouldBlow);
            }
        }

        private void DrawPolygon(Polygon polygon, SolidColorBrush brush, DrawingContext drawingContext)
        {
            if (polygon == null)
                return;
            polygon.Render(drawingContext, shouldBlow);
        }

        internal void SelectionChanged(int selectIndex)
        {
            selectedIndex = selectIndex;
            InvalidateVisual();
        }

        internal void CompletePolygon()
        {
            tempPolygon.Enclose();
            InvalidateVisual();
        }

        internal void Delete(int index)
        {
            polygons.RemoveAt(index);
            NotifyPolygonChanged();
            InvalidateVisual();
        }

        void NotifyPolygonChanged()
        {
            if (onPolygonChanged != null)
                onPolygonChanged(polygons);
        }
    }

    interface IShape
    {
        void Render(DrawingContext drawingContext, bool shouldBlow);
    }
    public class Polygon : IShape
    {
        public List<Point> pts;
        public Point currentPt;
        private bool finished = false;
        Point invalidPt = new Point(-1, -1);

        public Polygon()
        {
            pts = new List<Point>();
            currentPt = invalidPt;
            Selected = false;
        }

        public void RemoveLast()
        {
            if (pts.Count == 0)
                return;
            pts.RemoveAt(pts.Count - 1);
            finished = false;
        }

        public bool  Selected { get; set; }
        public bool Finished
        {
            get
            {
                return finished;
            }
        }
        public void Enclose()
        {
            if (pts.Count < 3)
                throw new Exception("多边形最少要3个点!");
            finished = true;
            currentPt = pts[0];
        }

        public void Render(DrawingContext drawingContext, bool shouldBlow)
        {
            if(currentPt != invalidPt)
            {
                int radius = shouldBlow ? 3 : 1;
                drawingContext.DrawEllipse(null, new Pen(Brushes.Red, 2), currentPt, radius, radius);
            }
            

            Brush brush = Selected ? Brushes.Red : Brushes.Blue;
            for (int i = 0; i < pts.Count; i++)
            {
                Point ptStart;
                Point ptEnd;
                if (!Finished) //ignore the last
                {
                    if (i == pts.Count - 1)
                        break;
                }

                if (i != pts.Count - 1)
                {
                    ptStart = pts[i];
                    ptEnd = pts[i + 1];
                }
                else
                {
                    ptStart = pts[i];
                    ptEnd = pts[0];
                }
                drawingContext.DrawLine(new Pen(brush, 1), ptStart, ptEnd);
            }
            if (currentPt != invalidPt && pts.Count != 0)
                drawingContext.DrawLine(new Pen(brush, 1), pts.Last(), currentPt);
        }
    }
}
