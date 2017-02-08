using EngineDll;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GlareCalculator
{
    interface IShape
    {
        void Render(DrawingContext drawingContext, bool shouldBlow);
        void OnMouseMove(Point newPt);
        void OnLeftMouseUp(Point pt);
        void OnLeftMouseDown(Point pt);
        bool PtIsInside(Point pt);
       
    }
    public class ShapeBase:IShape
    {
        public Point invalidPt = new Point(-1, -1);
       
        public bool Selected { get; set; }
        public bool Finished { get; set; }
        public string ID { get; set; }

        public virtual void Render(DrawingContext drawingContext, bool shouldBlow)
        {
            throw new NotImplementedException();
        }

        public virtual void OnMouseMove(Point newPt)
        {
            throw new NotImplementedException();
        }

        public virtual void OnLeftMouseDown(Point newPt)
        {
            throw new NotImplementedException();
        }

        public virtual void OnLeftMouseUp(Point pt)
        {
            throw new NotImplementedException();
        }

        public virtual bool PtIsInside(Point pt)
        {
            throw new NotImplementedException();
        }

        public virtual List<Point> GetPossiblePts()
        {
            throw new NotImplementedException();
        }
    }

    public class Circle : ShapeBase
    {
        public Point ptCircle { get; set; }
        public double Radius { get; set; }
        public Circle()
        {
            ptCircle = invalidPt;
            Selected = true;
            Radius = 0;
            Finished = false;
            ID = "";
        }

        public override bool PtIsInside(Point pt)
        {
            if(!Finished)
                return false;

            return GetDistance(ptCircle, pt) <= Radius;
        }

        public override List<Point> GetPossiblePts()
        {
            int startX = (int)Math.Floor(ptCircle.X);
            int startY = (int)Math.Floor(ptCircle.Y);
            int endX =  (int)Math.Ceiling(ptCircle.X + Radius);
            int endY = (int)Math.Ceiling(ptCircle.Y + Radius);
            List<Point> pts = new List<Point>();
            for(int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                    pts.Add(new Point(x, y));
            }
            return pts;
        }

        public override void OnLeftMouseDown(Point newPt)
        {
            if (ptCircle == invalidPt)
            {
                ptCircle = newPt;
            }
        }
        public override void OnLeftMouseUp(Point pt)
        {
            if (Finished)
                return;
            Radius = GetDistance(ptCircle, pt);
            Finished = true;
            Selected = false;
        }
        public override void OnMouseMove(Point newPt)
        {
            if (Finished)
                return;
            if (ptCircle == invalidPt)
                return;
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;
            Radius = GetDistance(ptCircle, newPt);
        }

        private double GetDistance(Point ptCircle, Point newPt)
        {
            double disX = (double)(ptCircle.X - newPt.X);
            double disY = (double)(ptCircle.Y - newPt.Y);
            return Math.Sqrt(disX * disX + disY * disY);
        }

        override public void Render(DrawingContext drawingContext, bool shouldBlow)
        {
            int penWidth = 0;
            if (ptCircle != invalidPt && Selected)
            {
                penWidth = shouldBlow ? 3 : 1;
                int r = penWidth;
                drawingContext.DrawEllipse(null, new Pen(Brushes.Red, penWidth), ptCircle, r, r);
            }
            if (Radius == 0)
                return;
            Brush brush = Brushes.Blue;
            penWidth = Selected ? 2 : 1;
            drawingContext.DrawEllipse(null, new Pen(brush, penWidth), ptCircle, Radius, Radius);

            FormattedText ft = new FormattedText(ID, CultureInfo.CurrentCulture,
                          FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.Red);
            drawingContext.DrawText(ft, ptCircle);
        }
    }
    public class PlayGround : Polygon
    {
        public PlayGround():base()
        {

        }

        public override void Render(DrawingContext drawingContext, bool shouldBlow)
        {
            RenderBoundary(drawingContext, shouldBlow, Brushes.Blue);
        }
    }
    public class RoadPolygon : Polygon
    {
        Point ptBottomLeft;
        Point ptBottomRight;
        Point ptTopLeft;
        Point ptTopRight;
        int laneCnt;
        int ptCnt;
        //double realH;
        public List<List<Point>> eachLanePts = new List<List<Point>>();
        public Point BottomLeft { 
            get 
            {
                return ptBottomLeft;
            } 
        }
        public Point BottomRight
        {
            get
            {
                return ptBottomRight;
            }
        }
        public RoadPolygon(int laneCnt, int ptCnt)
            :base()
           
        {
            this.laneCnt = laneCnt;
            this.ptCnt = ptCnt;
        }

        public RoadPolygon(List<MPoint> pts,int laneCnt, int ptCnt):base(pts)
        {
            this.laneCnt = laneCnt;
            this.ptCnt = ptCnt;
        }

        public void Normalize()
        {
            var sortedPts = pts.OrderBy(pt => pt.X).ToList();
            ptBottomLeft = sortedPts.First();
            ptBottomRight = sortedPts.Last();
            ptTopLeft = sortedPts[1];
            ptTopRight = sortedPts[2];
            ptBottomLeft.Y = ptBottomRight.Y = (BottomLeft.Y + BottomRight.Y) / 2.0;
            ptTopLeft.Y = ptTopRight.Y = (ptTopLeft.Y + ptTopRight.Y) / 2.0;
            pts.Clear();
            pts.Add(ptTopLeft);
            pts.Add(ptTopRight);
            pts.Add(ptBottomRight);
            pts.Add(ptBottomLeft);
            currentPt = ptTopLeft;
            //get realH
            double topWith = ptTopRight.X - ptTopLeft.X;
            double bottomWidth = ptBottomRight.X - ptBottomLeft.X;
            double h = ptBottomLeft.Y - ptTopLeft.Y;
            int yIndex = 0;
            double realH = 0;
            double eachStepWidthDiff = (bottomWidth - topWith) / h;
           
            for (int yPixel = (int)ptTopRight.Y; yPixel < (int)ptBottomRight.Y; yPixel++)
            {
                double currentWidth = topWith + yIndex * eachStepWidthDiff;
                yIndex++;
                realH += bottomWidth / currentWidth;
            }
            if (ptCnt == 1)
                throw new Exception("参考点数不能为1个！");
            double expectedDistance = realH / (ptCnt-1);

            //calculate points
            double k1 = (ptBottomLeft.Y - ptTopLeft.Y) / (ptBottomLeft.X - ptTopLeft.X);
            //double k2 = (ptBottomRight.Y - ptTopRight.Y) / (ptTopRight.X - ptBottomRight.X);
            List<int> yPositions = new List<int>();
            double totalDistance = 0;
            yIndex = 0;
            eachLanePts.Clear();
            for (int i = 0; i < laneCnt; i++ )
            {
                eachLanePts.Add(new List<Point>());
            }

           
            for (int yPixel = (int)ptTopRight.Y; yPixel < (int)ptBottomRight.Y; yPixel++)
            {
                double currentWidth = topWith + yIndex * eachStepWidthDiff;
                yIndex++;
                totalDistance += bottomWidth / currentWidth;
               
                if (totalDistance > expectedDistance ||
                    yPixel == (int)ptTopRight.Y ||
                    yPixel == (int)ptBottomRight.Y-1)
                {
                    totalDistance = 0;
                    yPositions.Add(yPixel);
                    double xStart = ptTopLeft.X + yIndex / k1;
                    double xEnd = xStart + currentWidth;
                    List<Point> linePts = new List<Point>();
                    if (laneCnt == 1)
                        throw new Exception("不支持一条车道！");
                    double tmpDis = (xEnd - xStart) / (laneCnt - 1);
                    for (int i = 0; i < laneCnt; i++)
                    {
                        eachLanePts[i].Add(new Point(xStart+i*tmpDis,yPixel));
                    }
                }

            }
        }

        public override void Render(DrawingContext drawingContext, bool shouldBlow)
        {
            RenderBoundary(drawingContext, shouldBlow, Brushes.Pink);
            RenderNet(drawingContext);
        }

        private void RenderNet(DrawingContext drawingContext)
        {
            if (!Finished)
                return;
            
            foreach(var linePts in eachLanePts)
            {
                if (linePts.Count == 0)
                    continue;
                var firstPt = linePts.First();
                var lastPt = linePts.Last();
                drawingContext.DrawLine(new Pen(Brushes.Green, 1), firstPt, lastPt);

                foreach(var pt in linePts)
                    drawingContext.DrawEllipse(Brushes.Red,new Pen(Brushes.Red, 1),pt,1,1);
            }
        }


       
    }

    public class Polygon : ShapeBase
    {
        public List<Point> pts;
        protected Point currentPt;
        protected double mostLeft;
        protected double mostTop;
        protected double mostRight;
        protected double mostBottom;
        public Polygon()
        {
            pts = new List<Point>();
            currentPt = invalidPt;
            Selected = true;
            Finished = false;
            ID = ""; 
        }

  
        public Polygon(List<MPoint> pts)
            
        {
            this.pts = new List<Point>();
            pts.ForEach( p=>this.pts.Add(new Point(p.x,p.y)));
            currentPt = invalidPt;
            Selected = false;
            Finished = true;
            ID = ""; 
            FindBoundary();
        }

        private void FindBoundary()
        {
            mostLeft = pts.Min(pt => pt.X);
            mostRight = pts.Max(pt => pt.X);
            mostTop = pts.Min(pt => pt.Y);
            mostBottom = pts.Max(pt => pt.Y);
        }

        public override bool PtIsInside(Point pt)
        {
            if (!Finished)
                return false;
            return IsPointInPolygon(pt, pts);
        }

        public override List<Point> GetPossiblePts()
        {
            int startX = (int)mostLeft;
            int startY = (int)mostTop;
            int endX = (int)mostRight;
            int endY = (int)mostBottom;
            List<Point> pts = new List<Point>();
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                    pts.Add(new Point(x, y));
            }
            return pts;
        }

        private bool IsPointInPolygon(Point point, List<Point> polygon)
        {
            int polygonLineCnt = polygon.Count;
            int i = 0;
            bool inside = false;
            // x, y for tested point.
            double pointX = point.X;
            double pointY = point.Y;

            bool ptInBoundary = pointX <= mostRight 
                && pointX >= mostLeft 
                && pointY <= mostBottom
                && pointY >= mostTop;
            if (!ptInBoundary)
                return false;
            // start / end point for the current polygon segment.
            double startX, startY, endX, endY;
            Point endPoint = polygon[polygonLineCnt - 1];
            endX = endPoint.X;
            endY = endPoint.Y;
            while (i < polygonLineCnt)
            {
                startX = endX; startY = endY;
                endPoint = polygon[i++];
                endX = endPoint.X; endY = endPoint.Y;
                //
                inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                          && /* if so, test if it is under the segment */
                          ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
            }
            return inside;
        }

        public void RemoveLast()
        {
            if (pts.Count == 0)
                return;
            pts.RemoveAt(pts.Count - 1);
            Finished = false;
        }

        public void Enclose()
        {
            if (pts.Count < 3)
                throw new Exception("多边形最少要3个点!");
            Finished = true;
            Selected = false;
            currentPt = pts[0];
            FindBoundary();
        }

        public override void OnMouseMove(Point newPt)
        {
            if (Finished)
                return;
            currentPt = newPt;
        }

        public override void OnLeftMouseDown(Point newPt)
        {
            return; //do nothing for polygon
        }

        public override void OnLeftMouseUp(Point pt)
        {
            if (Finished)
                return;

            pts.Add(pt);
            currentPt = pt;
        }

        protected void RenderBoundary(DrawingContext drawingContext, bool shouldBlow, Brush brush)
        {
            if (pts.Count > 0 && Selected)
            {
                int radius = shouldBlow ? 3 : 1;
                drawingContext.DrawEllipse(null, new Pen(Brushes.Red, 2), pts.Last(), radius, radius);
            }

            if (pts.Count == 0)
                return;

            int width = Selected ? 3 : 1;
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
                drawingContext.DrawLine(new Pen(brush, width), ptStart, ptEnd);
            }
            FormattedText ft = new FormattedText(ID, CultureInfo.CurrentCulture, 
                FlowDirection.LeftToRight,new Typeface("Arial"), 12, Brushes.Red);
            drawingContext.DrawText(ft, pts[0]);
            if (currentPt != invalidPt && pts.Count != 0)
                drawingContext.DrawLine(new Pen(brush, width), pts.Last(), currentPt);
        }
        public override void Render(DrawingContext drawingContext, bool shouldBlow)
        {
            RenderBoundary(drawingContext, shouldBlow, Brushes.Red);
        }
    }


    public enum Operation
    {
        none,
        polygon,
        circle,
        fakeColor,
        select,
        histogram,
        road,
        search,
        playground
    };
}
