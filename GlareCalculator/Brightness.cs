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
        public byte[] grayValsInArray;
        public string pngFile;
        public int[] GrayLevelCounts = new int[256];
        public int Width { get; set; }
        public int Height { get; set; }

        public double Max { get; set; }
        public double Min { get; set; }

        public void Read(string sFile)
        {
            Max = 0;
            Min = 1000000;
            pngFile = sFile.Replace(".txt",".png");
            
            orgVals.Clear();
            int width = 0;
            int height = 0;
            bool xyMode = true;
            using (StreamReader sr = new StreamReader(sFile))
            {
                int lineNum = 1;
                while (true)
                {
                    string str = sr.ReadLine();

                    if(lineNum == 3)//get x
                    {
                        width = GetNum(str);
                        xyMode = str.Contains("nx");
                       
                    }

                    if(lineNum == 4)//get y
                    {
                        height = GetNum(str);
                        if (!xyMode)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                List<double> oneRowVals = new List<double>(width);
                                for (int x = 0; x < width; x++)
                                {
                                    oneRowVals.Add(0);
                                }
                                orgVals.Add(oneRowVals);
                            }
                        }
                    }
                    if (lineNum <= 8)
                    {
                        lineNum++;
                        continue;
                    }
            
                    if (String.IsNullOrEmpty(str))
                        break;

                    if(xyMode)
                        orgVals.Add(ParseLine(str));
                    else
                    {
                        ParseOneVal(orgVals, str);
                    }
                }
                Debug.WriteLine("Finished!");
            }
            grayVals =  Convert2Gray(orgVals);
        }

        private void ParseOneVal(List<List<double>> orgVals, string content)
        {
            string[] strs = content.Split('\t');
            int y = int.Parse(strs[0]);
            int x = int.Parse(strs[1]);
            if (y < 5)
            {
                orgVals[y][x] = 0;
                return;
            }
            orgVals[y][x] = double.Parse(strs[2]);
        }

        private int GetNum(string str)
        {
            string chStr = "";
            foreach(var ch in str)
            {
                if (Char.IsDigit(ch))
                    chStr += ch;
            }
            return int.Parse(chStr);
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
            Height = orgVals.Count;
            Width = orgVals[0].Count;
            List<double> maxList = orgVals.Select(l => l.Max()).ToList();
            List<double> minList = orgVals.Select(l => l.Min()).ToList();
            double max = maxList.Max();
            double min = minList.Min();
            double grayUnit = (max - min) / 255;
            grayValsInArray = new byte[Height * Width];
            int pixelCnt = 0;
            for (int y = 0; y < Height; y++)
            {
                List<byte> thisLineGrayVals = new List<byte>();
                for (int x = 0; x < Width; x++)
                {
                    byte val = (byte)((orgVals[y][x] - min) / grayUnit);
                    GrayLevelCounts[val]++;
                    thisLineGrayVals.Add(val);
                    grayValsInArray[pixelCnt++] = val;
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
                {
                    double v = Parse(s);
                    if (v > Max)
                        Max = v;
                    if (v < Min)
                        Min = v;
                    vals.Add(v);
                }
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

        internal List<byte> Threshold(double thresholdVal)
        {
            List<byte> newVals = new List<byte>();
            byte val = (byte)thresholdVal;
            for(int i = 0; i< grayValsInArray.Length; i++)
            {
                newVals.Add(grayValsInArray[i] > val ? (byte)255 : (byte)0);
            }
            return newVals;
        }
    }
}
