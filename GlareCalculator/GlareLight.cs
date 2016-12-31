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
        double pixelUnit = 5.5 / 1000000;//um
        double f = 8.0 / 1000; //mm
        GuthIndexes guthIndexes = new GuthIndexes();
        //UGR La
        public double Calculate(List<List<double>> vals,List<Polygon> polygons)
        {
            double max = 0;
            List<Point> insidePolygonPts = new List<Point>();
            for(int y = 0 ; y< vals.Count; y++)
            {
                for(int x = 0; x< vals[y].Count; x++)
                {
                    if (vals[y][x] > max)
                        max = vals[y][x];
                    Point pt = new Point(x, y);
                    if (polygons.Exists(poly=>poly.IsInside(pt)))
                        insidePolygonPts.Add(new Point(x, y));
                }
            }

            double LA = Average(vals);
            double sum = 0;
            foreach(Point pt in insidePolygonPts)
            {
                double L_si = vals[(int)pt.Y][(int)pt.X];
                double ω = 0;
                double p = 0;
                CalculateOmegaAndGuth(vals, (int)pt.X, (int)pt.Y, ref ω, ref p );
                sum += ω*L_si * L_si / (p * p);
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
}
