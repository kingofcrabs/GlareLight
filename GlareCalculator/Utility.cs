using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace GlareCalculator
{
    public class Utility
    {
        #region
        static public string GetExeFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }

        static public string GetDescription(int sampleID)
        {
            int sampleIndex = sampleID - 1;
            int colIndex = sampleIndex / 8;
            int rowIndex = sampleIndex - colIndex * 8;
            return string.Format("{0}{1:D2}", (char)('A' + rowIndex), colIndex + 1);
        }


        static public void WriteExecuteResult(bool bok, string sPath)
        {

            sPath = GetSaveFolder() + sPath;

            using (StreamWriter sw = new StreamWriter(sPath))
            {
                sw.WriteLine(bok.ToString());
            }
        }

        static public string ReadFolder(string sPath)
        {
            string result = "";
            sPath = GetSaveFolder() + sPath;
            using (StreamReader sr = new StreamReader(sPath))
            {
                result = sr.ReadLine();
            }
            return result;

        }


        static public string GetExeParentFolder()
        {
            string s = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            int index = s.LastIndexOf("\\");
            return s.Substring(0, index) + "\\";
        }

        static public string GetConfigFolder()
        {
            string sConfigFolder = GetExeParentFolder() + "Config\\";
            CreateIfNotExist(sConfigFolder);
            return sConfigFolder;
        }

        private static void CreateIfNotExist(string sFolder)
        {
            if (!Directory.Exists(sFolder))
                Directory.CreateDirectory(sFolder);
        }

        public static string GetOutputFolder()
        {
            string sExeParent = GetExeParentFolder();
            string sOutputFolder = sExeParent + "Output\\";
            CreateIfNotExist(sOutputFolder);
            return sOutputFolder;
        }

        static public string GetSaveFolder()
        {
            return GetOutputFolder();
        }


        #endregion
        public static void SaveSettings<T>(T settings, string sFile)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            Stream stream = new FileStream(sFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            xs.Serialize(stream, settings);
            stream.Close();

        }

        static public void Write2File(string fileName, List<string> strs)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                foreach (string s in strs)
                    sw.WriteLine(s);
            }
        }

        static public void Write2File(string fileName, string s)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine(s);
            }
        }
      
        public static string Serialize<T>(T value)
        {
            if (value == null)
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            XmlWriterSettings settings = new XmlWriterSettings();

            //StringWriter textWriter = new StringWriter()

            using (StringWriterWithEncoding textWriter = new StringWriterWithEncoding(Encoding.UTF7))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, value);
                }
                return textWriter.ToString();
            }
        }

        public static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlReaderSettings settings = new XmlReaderSettings();

            using (StringReader textReader = new StringReader(xml))
            {
                using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }

        public static bool IsValidBarcode(string s)
        {
            foreach (char ch in s)
            {
                if (char.IsDigit(ch))
                    return true;
            }
            return false;
        }
    }

    public sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }
}
