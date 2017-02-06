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
        public UserSettings UserSettings { get; set; }
        public GlobalVars()
        {
            string settingFile = Utility.GetExeFolder() + "settingInfo.xml";
            AgeDependingConstant = 1;
            if(File.Exists(settingFile))
            {
                string contend = File.ReadAllText(settingFile);
                UserSettings = Utility.Deserialize<UserSettings>(contend);
            }
            else
            {
                UserSettings = new UserSettings();
                
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

        public double AgeDependingConstant { get; set; }

        public bool Registed { get; set; }
    }

    [Serializable]
    public class UserSettings
    {
        public int Focus { get; set; }
        public double PixelLength { get; set; }

        public int Age { get; set; }
        public string Name { get; set; }
        public UserSettings()
        {
            Focus = 8;
            PixelLength = 5.5;
            Age = 23;
        }

        public UserSettings(int f, double pxLen, int age)
        {
            Focus = f;
            PixelLength = pxLen;
            Age = age;
        }
    }
}
