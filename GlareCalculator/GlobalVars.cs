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
        CameraInfo cameraInfo;
        public GlobalVars()
        {
            string cameraInfoFile = Utility.GetExeFolder() + "cameraInfo.xml";
            if(File.Exists(cameraInfoFile))
            {
                string contend = File.ReadAllText(cameraInfoFile);
                cameraInfo = Utility.Deserialize<CameraInfo>(contend);
            }
            else
            {
                cameraInfo = new CameraInfo();
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
