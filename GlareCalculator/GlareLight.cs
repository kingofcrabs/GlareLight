using GlareCalculator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace GlareCalculator
{
    class GlareLight
    {
        double pixelUnit = GlobalVars.Instance.CameraInfo.PixelLength / 1000000;//um
        double f = GlobalVars.Instance.CameraInfo.Focus / 1000; //mm
        GuthIndexes guthIndexes = new GuthIndexes();
        //UGR La
        public double Calculate(List<List<double>> vals,List<ShapeBase> shapes, ref List<GlareResult> results,ref double LA)
        {
            double max = 0;
            List<Point> insidePolygonPts = new List<Point>();
            Dictionary<ShapeBase, List<Point>> eachShape_pts = new Dictionary<ShapeBase, List<Point>>();
            shapes.ForEach(x => eachShape_pts.Add(x, new List<Point>()));
            for(int y = 0 ; y< vals.Count; y++)
            {
                for(int x = 0; x< vals[y].Count; x++)
                {
                    if (vals[y][x] > max)
                        max = vals[y][x];
                    Point pt = new Point(x, y);
                    foreach(ShapeBase shape in shapes)
                    {
                        if(shape.PtIsInside(pt))
                        {
                            eachShape_pts[shape].Add(pt);
                            break;
                        }
                    }
                }
            }

            LA = Average(vals);
            double sum = 0;
            foreach(var pair in eachShape_pts)
            {
                var pts = pair.Value;
                if (pts.Count == 0)
                    continue;
                double totalOmega = 0;
                double totalP = 0;
                double totalLa = 0;
                foreach(Point pt in pts)
                {
                    double L_si = vals[(int)pt.Y][(int)pt.X];
                    
                    double ω = 0;
                    double p = 0;
                    CalculateOmegaAndGuth(vals, (int)pt.X, (int)pt.Y, ref ω, ref p);
                    totalOmega += ω;
                    totalP += p;
                    totalLa += L_si;
                    sum += ω * L_si * L_si / (p * p);
                }
                results.Add(new GlareResult(totalLa / pts.Count, totalOmega, totalP / pts.Count));
            }
            return 8 * Math.Log(0.25 * sum / LA);
        }

        


        internal void Test()
        {
            List<List<double>> vals = new List<List<double>>();
            for(int y = 0;　y< 581;y++)
            {
                List<double> array = new List<double>();
                for(int x = 0; x < 1362; x++)
                {
                    array.Add(100);
                }
                vals.Add(array);
            }
            double ω = 0; 
            double p = 0;
            CalculateOmegaAndGuth2(vals, 680, 60,25,50, ref ω, ref p);
            var sum = ω * 100 * 100 / (p * p);
            var ugr = 8 * Math.Log(0.25 * sum / 8);
            //Debug.WriteLine(ugr);
        }

        void CalculateOmegaAndGuth2(List<List<double>> vals, int x, int y, int ww, int hh, ref double ω, ref double p)
        {
            int width = vals[0].Count;
            int height = vals.Count;
            double xDis = Math.Abs(x - width / 2) * pixelUnit;
            double yDis = Math.Abs(y - height / 2) * pixelUnit;
            double dis = Math.Sqrt(xDis * xDis + yDis * yDis);
            double r = Math.Sqrt(xDis * xDis + yDis * yDis + f * f);

            double cosθ = f / r;
            double Ap = pixelUnit * pixelUnit * ww* hh * cosθ;
            ω = Ap / (r * r);
            double H2R = yDis / r;
            double T2R = xDis / r;
            p = guthIndexes.GetVal(T2R, H2R);
        }



        void CalculateOmegaAndGuth(List<List<double>> vals, int x, int y, ref double ω, ref double p)
        {
            int width = vals[0].Count;
	        int height = vals.Count;
	        double xDis = Math.Abs(x - width / 2) * pixelUnit;
            double yDis = Math.Abs(y - height / 2) * pixelUnit;
	        double dis = Math.Sqrt(xDis*xDis + yDis * yDis);
            double r = Math.Sqrt(xDis * xDis + yDis * yDis + f * f);

	        double cosθ = f / r;
	        double Ap = pixelUnit * pixelUnit * cosθ;
	        ω = Ap / (r*r);
            double H2R = yDis / r;
            double T2R = xDis / r;
            p = guthIndexes.GetVal(T2R, H2R);
        }
      
        double Average(List<List<double>> vals)
        {
            double totalVal = 0;
            double totalCnt = 0;
            foreach (var lst in vals)
            {
                foreach (var val in lst)
                {
                    if (val >= 0)
                    {
                        totalCnt++;
                        totalVal += val;
                    }
                }
            }
            return totalVal / totalCnt;
        }

        
    }

    internal class GlareResult
    {
        public double La;
        public double ω;
        public double P;
        public GlareResult(double La, double ω,double P)
        {
            this.La = La;
            this.ω = ω;
            this.P = P;
        }
    }
}
