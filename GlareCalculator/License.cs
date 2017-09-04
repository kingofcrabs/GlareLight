using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace GlareCalculator
{
    class License
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string GetPCID()
        {
            return GetHDDInfo() + GetMacAddress();
        }
        private string GetCPUInfo()    //读取CPU信息
        {
            ManagementClass mobj = new ManagementClass("Win32_Processor");
            ManagementObjectCollection moc = mobj.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                return mo.Properties["ProcessorId"].Value.ToString();
            }
            return "";
        }

        private string GetMacAddress()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration where IPEnabled=true");
            IEnumerable<ManagementObject> objects = searcher.Get().Cast<ManagementObject>();
            string mac = (from o in objects orderby o["IPConnectionMetric"] select o["MACAddress"].ToString()).FirstOrDefault();
            if (mac == null)
                throw new Exception("Cannot get pc ID");
            mac = mac.Replace(":", "");
            return mac;
        }
        private string GetHDDInfo()    //读取硬盘信息
        {
            ManagementClass mobj = new ManagementClass("Win32_PhysicalMedia");

            ManagementObjectCollection moc = mobj.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                return mo.Properties["SerialNumber"].Value.ToString().Trim();
            }
            return "";
        }

        public string GetRegistCode()
        {
            string pcID = GetPCID();
            log.InfoFormat("PC code is:{0}", pcID);
            Byte[] bytes = System.Text.Encoding.Default.GetBytes(pcID);
            BigInteger bigVal = new BigInteger(0);
            for (int i = 0; i < bytes.Length; i++)
            {
                bigVal *= 100;
                bigVal += (int)bytes[i];
            }

            for (; ; )
            {
                bool isPrime = BigIntegerExtensions.IsProbablePrime(bigVal, 10);
                if (isPrime)
                    break;
                bigVal++;

            }
            return bigVal.ToString();
        }
        public bool CheckRegistCode(string sRegCode)
        {
            sRegCode = sRegCode.Trim();
            if (sRegCode.Length < 10)
                return false;
            log.InfoFormat("regist code is:{0}", sRegCode);
            string expectedRegistCode = GetRegistCode();
            return sRegCode == expectedRegistCode;
        }
    }

    public static class BigIntegerExtensions
    {
        public static bool IsProbablePrime(this BigInteger source, int certainty)
        {
            if (source == 2 || source == 3)
                return true; if (source < 2 || source % 2 == 0)
                return false;
            BigInteger d = source - 1;
            int s = 0;
            while (d % 2 == 0)
            { d /= 2; s += 1; }
            // There is no built-in method for generating random BigInteger values.   
            // Instead, random BigIntegers are constructed from randomly generated   
            // byte arrays of the same length as the source. 
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[source.ToByteArray().LongLength];
            BigInteger a;
            for (int i = 0; i < certainty; i++)
            {
                do
                {        // This may raise an exception in Mono 2.10.8 and earlier.       
                    // http://bugzilla.xamarin.com/show_bug.cgi?id=2761        
                    rng.GetBytes(bytes);
                    a = new BigInteger(bytes);
                }
                while (a < 2 || a >= source - 2);
                BigInteger x = BigInteger.ModPow(a, d, source);
                if (x == 1 || x == source - 1)
                    continue;
                for (int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, source);
                    if (x == 1)
                        return false;
                    if (x == source - 1)
                        break;
                }
                if (x != source - 1)
                    return false;
            } return true;
        }
    }
}
