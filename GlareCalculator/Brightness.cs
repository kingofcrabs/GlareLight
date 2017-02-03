using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace GlareCalculator
{
    class Brightness
    {
        public List<List<byte>> grayVals = new List<List<byte>>();
        public List<List<double>> orgVals = new List<List<double>>();
        public string pngFile;
        public int[] GrayLevelCounts = new int[256];
        public void Read(string sFile)
        {
            pngFile = sFile.Replace(".txt",".png");
            
            orgVals.Clear();
            using (StreamReader sr = new StreamReader(sFile))
            {
                int lineNum = 1;
                while (true)
                {
                    string str = sr.ReadLine();
                    if (lineNum <= 8)
                    {
                        lineNum++;
                        continue;
                    }
            
                    if (String.IsNullOrEmpty(str))
                        break;
                    orgVals.Add(ParseLine(str));
                }
                Debug.WriteLine("Finished!");
            }
            grayVals =  Convert2Gray(orgVals);
        }

        

        private List<List<byte>> Convert2Gray(List<List<double>> orgVals)
        {
            List<List<byte>> vals = new List<List<byte>>();
            for(int i = 0; i< 255; i++)
            {
                GrayLevelCounts[i] = 0;
            }
            byte[] map = new byte[256];
            long[] lCounts = new long[256];
            //each gray level count
            int height = orgVals.Count;
            int width = orgVals[0].Count;
            List<double> maxList = orgVals.Select(l => l.Max()).ToList();
            List<double> minList = orgVals.Select(l => l.Min()).ToList();
            double max = maxList.Max();
            double min = minList.Min();
            double grayUnit = (max - min) / 255;
            for (int y = 0; y < height; y++)
            {
                List<byte> thisLineGrayVals = new List<byte>();
                for (int x = 0; x < width; x++)
                {
                    byte val = (byte)((orgVals[y][x] - min) / grayUnit);
                    GrayLevelCounts[val]++;
                    thisLineGrayVals.Add(val);
                }
                vals.Add(thisLineGrayVals);
            }
            return vals;
        }

        //void List<List<byte>> Convert2Gray(List<List<double>> dbVals)
        //{
        //    List<List<byte>> vals = new List<List<byte>>();
            
        //    byte[] map = new byte[256];
        //    long[] lCounts = new long[256];
        //    //each gray level count
        //    int height = dbVals.Count;
        //    int width = dbVals[0].Count;
        //    List<double> maxList = dbVals.Select(l => l.Max()).ToList();
        //    List<double> minList = dbVals.Select(l => l.Min()).ToList();
        //    double max = maxList.Max();
        //    double min = minList.Min();
        //    double grayUnit = (max - min) / 255;
        //    for (int y = 0; y < height; y++)
        //    {
        //        List<byte> thisLineGrayVals = new List<byte>();
        //        for (int x = 0; x < width; x++)
        //        {
        //            byte val = (byte)((dbVals[y][x] - min) / grayUnit);
        //            thisLineGrayVals.Add(val);
        //        }
        //        vals.Add(thisLineGrayVals);
        //    }
        //}

        
           
       

        private List<double> ParseLine(string str)
        {
            string[] strs = str.Split('\t');
            List<double> vals = new List<double>();
            foreach(var s in strs)
            {
                if(s != "")
                    vals.Add(Parse(s));
            }
            return vals;
        }

        private double Parse(string s)
        {
            return double.Parse(s);
        }

        internal List<ViewModels.GrayInfo> GetHistogram()
        {
            if (GrayLevelCounts.Count() == 0)
                return null;

            List<ViewModels.GrayInfo> grayInfos = new List<ViewModels.GrayInfo>();
            for(int i = 0; i< 255; i++)
            {
                grayInfos.Add(new ViewModels.GrayInfo(i,GrayLevelCounts[i]));
            }
            return grayInfos;
        }
    }
}
