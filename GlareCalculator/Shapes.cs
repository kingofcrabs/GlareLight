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
    public class ShapeBase
    {
        public Point invalidPt = new Point(-1, -1);
        public virtual void Render(DrawingContext drawingContext, bool shouldBlow)
        {
            throw new NotImplementedException();
        }

        public virtual void OnMouseMove(Point newPt)
        {
            throw new NotImplementedException();
        }
        public bool Selected { get; set; }
        public bool Finished { get; set; }

        public virtual void OnLeftMouseUp(Point pt)
        {
            throw new NotImplementedException();
        }

        public virtual void OnLeftMouseDown(Point pt)
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
            //if (ptCircle != invalidPt && Selected)
            //{
            //    penWidth = shouldBlow ? 3 : 1;
            //    int r = penWidth;
            //    drawingContext.DrawEllipse(null, new Pen(Brushes.Red, penWidth), ptCircle, r, r);
            //}
            if (Radius == 0)
                return;
            Brush brush = Brushes.Blue;
            penWidth = Selected ? 2 : 1;
            drawingContext.DrawEllipse(null, new Pen(brush, penWidth), ptCircle, Radius, Radius);
        }

    }

    public class Polygon : ShapeBase
    {
        public List<Point> pts;
        Point currentPt;

        public Polygon()
        {
            pts = new List<Point>();
            currentPt = invalidPt;
            Selected = true;
            Finished = false;
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

        public override void Render(DrawingContext drawingContext, bool shouldBlow)
        {
            if (pts.Count > 0 && Selected)
            {
                int radius = shouldBlow ? 3 : 1;
                drawingContext.DrawEllipse(null, new Pen(Brushes.Red, 2), pts.Last(), radius, radius);
            }


            Brush brush = Brushes.Blue;
            int width = Selected ? 2 : 1;
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
            if (currentPt != invalidPt && pts.Count != 0)
                drawingContext.DrawLine(new Pen(brush, width), pts.Last(), currentPt);
        }
    }


    public enum Operation
    {
        none,
        polygon,
        circle,
        //move,
        select
    };
}
