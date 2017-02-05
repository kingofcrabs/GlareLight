using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GlareCalculator
{
    public class GlobalVars
    {
        private static GlobalVars _instance = null;
        public CameraInfo CameraInfo { get; set; }
        public GlobalVars()
        {
            string cameraInfoFile = Utility.GetExeFolder() + "cameraInfo.xml";
            ThresholdChangeFinished = true;
            if(File.Exists(cameraInfoFile))
            {
                string contend = File.ReadAllText(cameraInfoFile);
                CameraInfo = Utility.Deserialize<CameraInfo>(contend);
            }
            else
            {
                CameraInfo = new CameraInfo();
            }
        }

        public static GlobalVars Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GlobalVars();
                return _instance;
            }
        }

        public bool ThresholdChangeFinished { get; set; }

        public bool Registed { get; set; }
    }

    [Serializable]
    public class CameraInfo
    {
        public int Focus { get; set; }
        public double PixelLength { get; set; }

        public string Name { get; set; }
        public CameraInfo()
        {
            Focus = 8;
            PixelLength = 5.5;
        }
    }
}
