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
        public List<List<Color>> colorVals = new List<List<Color>>();
        public void Read(string sFile)
        {
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
            Strech();
            PseudoColor pseudoColor = new PseudoColor();
            colorVals = pseudoColor.Convert(grayVals);
        }

        void Strech()
        {
            grayVals.Clear();
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
            for (int y = 0; y < height ; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte val = (byte)((orgVals[y][x] - min) / grayUnit);
                    lCounts[val]++; 
                }
            }
         
            // 保存运算中的临时值 
            long lTemp;
            double tmpVal = 255.0f / height / width;
            for (int i = 0; i < 256; i++) 
            { 
                lTemp = 0; 
                for (int j = 0; j <= i; j++) 
                    lTemp += lCounts[j];

                map[i] = (byte)(lTemp * tmpVal); 
            }
            for (int y = 0; y < height; y++)
            {
                List<byte> thisLineGrayVals = new List<byte>();
                for (int x = 0; x < width; x++)
                {
                    byte val = (byte)((orgVals[y][x] - min) / grayUnit);
                    val = map[val];
                    thisLineGrayVals.Add(val);
                }
                grayVals.Add(thisLineGrayVals);
            }
        }
           
       

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

    }
}
