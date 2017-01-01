﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GlareCalculator
{
    internal class PseudoColor
    {
#region mapping
        private static Color[] mapping = 
    {
        Color.FromArgb(155, 188, 255), //    40000
        Color.FromArgb(155, 188, 255), //    39500
        Color.FromArgb(155, 188, 255), //    39000
        Color.FromArgb(155, 188, 255), //    38500
        Color.FromArgb(156, 188, 255), //    38000
        Color.FromArgb(156, 188, 255), //    37500
        Color.FromArgb(156, 189, 255), //    37000
        Color.FromArgb(156, 189, 255), //    36500
        Color.FromArgb(156, 189, 255), //    36000
        Color.FromArgb(157, 189, 255), //    35500
        Color.FromArgb(157, 189, 255), //    35000
        Color.FromArgb(157, 189, 255), //    34500
        Color.FromArgb(157, 189, 255), //    34000
        Color.FromArgb(157, 189, 255), //    33500
        Color.FromArgb(158, 190, 255), //    33000
        Color.FromArgb(158, 190, 255), //    32500
        Color.FromArgb(158, 190, 255), //    32000
        Color.FromArgb(158, 190, 255), //    31500
        Color.FromArgb(159, 190, 255), //    31000
        Color.FromArgb(159, 190, 255), //    30500
        Color.FromArgb(159, 191, 255), //    30000
        Color.FromArgb(159, 191, 255), //    29500
        Color.FromArgb(160, 191, 255), //    29000
        Color.FromArgb(160, 191, 255), //    28500
        Color.FromArgb(160, 191, 255), //    28000
        Color.FromArgb(161, 192, 255), //    27500
        Color.FromArgb(161, 192, 255), //    27000
        Color.FromArgb(161, 192, 255), //    26500
        Color.FromArgb(162, 192, 255), //    26000
        Color.FromArgb(162, 193, 255), //    25500
        Color.FromArgb(163, 193, 255), //    25000
        Color.FromArgb(163, 193, 255), //    24500
        Color.FromArgb(163, 194, 255), //    24000
        Color.FromArgb(164, 194, 255), //    23500
        Color.FromArgb(164, 194, 255), //    23000
        Color.FromArgb(165, 195, 255), //    22500
        Color.FromArgb(166, 195, 255), //    22000
        Color.FromArgb(166, 195, 255), //    21500
        Color.FromArgb(167, 196, 255), //    21000
        Color.FromArgb(168, 196, 255), //    20500
        Color.FromArgb(168, 197, 255), //    20000
        Color.FromArgb(169, 197, 255), //    19500
        Color.FromArgb(170, 198, 255), //    19000
        Color.FromArgb(171, 198, 255), //    18500
        Color.FromArgb(172, 199, 255), //    18000
        Color.FromArgb(173, 200, 255), //    17500
        Color.FromArgb(174, 200, 255), //    17000
        Color.FromArgb(175, 201, 255), //    16500
        Color.FromArgb(176, 202, 255), //    16000
        Color.FromArgb(177, 203, 255), //    15500
        Color.FromArgb(179, 204, 255), //    15000
        Color.FromArgb(180, 205, 255), //    14500
        Color.FromArgb(182, 206, 255), //    14000
        Color.FromArgb(184, 207, 255), //    13500
        Color.FromArgb(186, 208, 255), //    13000
        Color.FromArgb(188, 210, 255), //    12500
        Color.FromArgb(191, 211, 255), //    12000
        Color.FromArgb(193, 213, 255), //    11500
        Color.FromArgb(196, 215, 255), //    11000
        Color.FromArgb(200, 217, 255), //    10500  
        Color.FromArgb(204, 219, 255), //    10000
        Color.FromArgb(208, 222, 255), //    9500
        Color.FromArgb(214, 225, 255), //    9000
        Color.FromArgb(220, 229, 255), //    8500
        Color.FromArgb(227, 233, 255), //    8000
        Color.FromArgb(235, 238, 255), //    7500
        Color.FromArgb(245, 243, 255), //    7000
        Color.FromArgb(255, 249, 253), //    6500
        Color.FromArgb(255, 243, 239), //    6000
        Color.FromArgb(255, 236, 224), //    5500
        Color.FromArgb(255, 228, 206), //    5000
        Color.FromArgb(255, 219, 186), //    4500
        Color.FromArgb(255, 209, 163), //    4000
        Color.FromArgb(255, 196, 137), //    3500
        Color.FromArgb(255, 180, 107), //    3000
        Color.FromArgb(255, 161,  72), //    2500
        Color.FromArgb(255, 137,  18), //    2000
        Color.FromArgb(255, 109,   0), //    1500 
        Color.FromArgb(255,  51,   0), //    1000
        Color.FromArgb(255,  0,   0), //     500
    };
#endregion
        public List<List<Color>> Colors
        {
            get;
            set;
        }
        public List<List<Color>> Convert(List<List<double>> grayVals)
        {
            List<double> maxList = grayVals.Select(l => l.Max()).ToList();
            List<double> minList = grayVals.Select(l => l.Min()).ToList();
            double max = maxList.Max();
            double min = minList.Min();
            double unit = (max-min)/80;
            Colors = new List<List<Color>>();
            foreach(var lineVals in grayVals)
            {
                List<Color> lineColors = new List<Color>();
                foreach(var val in lineVals)
                {
                    lineColors.Add(Convert(val-min,unit));
                }
                Colors.Add(lineColors);
            }

            return Colors;
        }

        private Color Convert(double val,double unit)
        {
            int index = (int)(val / unit);
            if (index == 80)
                index = 79;
            return mapping[index];
        }
        
    }
}
