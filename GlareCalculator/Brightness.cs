using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace GlareCalculator
{
    class Brightness
    {
        public List<List<double>> vals = new List<List<double>>();
        public void Read(string sFile)
        {
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
                    vals.Add(ParseLine(str));
                }
                Debug.WriteLine("Finished!");
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
