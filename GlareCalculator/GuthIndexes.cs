using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace GlareCalculator
{
    class GuthIndexes
    {
        List<double> H2RVals = new List<double>();
        List<double> T2RVals = new List<double>();
        List<List<double>> T2R_H2Rs = new List<List<double>>();
        
        public GuthIndexes()
        {
           string sFile = Utility.GetExeFolder() + "guth.csv";
           Read(sFile);
        }
        private void Read(string sFile)
        {
            using (StreamReader sr = new StreamReader(sFile))
            {
                string firstLine = sr.ReadLine();
                H2RVals = GetH2Rs(firstLine);
                
                while (true)
                {
                    string str = sr.ReadLine();
                     if (String.IsNullOrEmpty(str))
                        break;
                    var subStrs = str.Split(',').ToList();
                    T2RVals.Add(double.Parse(subStrs[0]));
                    List<double> vals = new List<double>();
                    for (int i = 1; i < subStrs.Count; i++ )
                    {
                        double val;
                        string tmpStr = subStrs[i];
                        val = double.Parse(tmpStr);
                        vals.Add(val);
                    }
                    T2R_H2Rs.Add(vals);
                }
                Debug.WriteLine("Finished! read guth indexes.");
            }
        }


        public double GetVal(double T2R, double H2R)
        {
            int firstT2RIndex, secondT2RIndex;
            int firstH2RIndex, secondH2RIndex;
            GetT2RNeighbors(T2R*10, out firstT2RIndex, out secondT2RIndex);
            GetH2RNeighbors(H2R*10, out firstH2RIndex, out secondH2RIndex);
            double topLeft = T2R_H2Rs[firstT2RIndex][firstH2RIndex];
            double topRight = T2R_H2Rs[firstT2RIndex][secondH2RIndex];
            double bottomLeft = T2R_H2Rs[secondT2RIndex][firstH2RIndex];
            double bottomRight = T2R_H2Rs[secondT2RIndex][secondH2RIndex];
            double topMidX = GetInterop(H2R, firstH2RIndex, topLeft, topRight);
            double bottomMidX = GetInterop(H2R, firstH2RIndex, bottomLeft, bottomRight);
            double theVal = GetInterop(T2R, firstT2RIndex, topMidX, bottomMidX);
            return theVal;
        }

        private void GetT2RNeighbors(double T2R, out int firstT2RIndex, out int secondT2RIndex)
        {
            if (T2R >= T2RVals.Count-1)
            {
                firstT2RIndex = secondT2RIndex = T2RVals.Count - 1;
                return;
            }
            try
            {
                GetNeighbors(T2R, out firstT2RIndex, out secondT2RIndex);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid T/R value: " + ex.Message);
            }
        }


        private void GetH2RNeighbors(double H2RIndex, out int firstH2RIndex, out int secondH2RIndex)
        {
            if (H2RIndex >= H2RVals.Count -1)
            {
                firstH2RIndex = secondH2RIndex = H2RVals.Count - 1;
                return;
            }
            try
            {
                GetNeighbors(H2RIndex, out firstH2RIndex, out secondH2RIndex);
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid H/R value: " + ex.Message);
            }
        }

        private double GetInterop(double val, int indexPosition, double first, double second)
        {
            double distance = val*10 - indexPosition;
            return distance * first + (1 - distance) * second;
        }

        //private void GetInteropH2Rs(double firstH2R, double secondH2R, double firstT2R, double secondT2R,
        //    out double h2rInterop1, out double h2rInterop2)
        //{
            
        //}


      
        private void GetNeighbors(double v, out int front, out int behind)
        {
            front = -1;
            behind = -1;
            front = (int)Math.Floor(v);
            behind = (int)Math.Ceiling(v);
        }

        private List<double> GetH2Rs(string firstLine)
        {
            string[] strs = firstLine.Split(',');
            strs = strs.Skip(1).ToArray();
            List<double> vals = new List<double>();
            foreach(string s in strs)
            {
                vals.Add(double.Parse(s));
            }
            return vals;
        }
    }
}
