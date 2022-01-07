using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NRSoft.FunctionPool
{
    public class GeneralH
    {

        #region Fields
        private static string _EncryptPw;
        private static string _DecryptPw;
        private static string _yyyy;

        public string EncryptSting
        {
            private set { _EncryptPw = value; }
            get { return _EncryptPw; }
        }

        public string DecryptSting
        {
            private set { _DecryptPw = value; }
            get { return _DecryptPw; }
        }

        public static string EncryptPw { get => _EncryptPw; set => _EncryptPw = value; }

        public static string DecryptPw { get => _DecryptPw; set => _DecryptPw = value; }

        public static string Yyyy { get => _yyyy; set => _yyyy = value; }
        #endregion Fields

        #region Methodes
        public string Encrypt()
        {
            if (_EncryptPw == String.Empty)
                return String.Empty;

            int l = _EncryptPw.Length + 32;
            string aa = String.Concat((char)l, "@", _EncryptPw);
            l = aa.Length;
            aa = String.Concat(aa, "JKHJY&GY&RTFUI&YGRDRDTYY*()&*(&*GYGGYR");
            string ab = FixString(aa, "&GG&^R^$", 32);
            int xx;
            string np = "";
            char c;
            for (int i = 0; i <= l; i++)
            {
                c = ab[i];
                xx = (int)c + 125;
                np += (char)xx;
            }
            return np;
        }

        public string Decrypt()
        {
            if (_DecryptPw == String.Empty)
                return String.Empty;

            int l = _DecryptPw.Length;
            string pn = "";
            int xx = 0;
            char c = new char();
            for (int i = 0; i < l; i++)
            {
                c = _DecryptPw[i];
                xx = (int)c - 125;
                pn += (char)xx;
            }
            string pw;
            pw = pn.Substring(2);
            pw = pw.Substring(0, pw.Length - 1);
            return pw;
        }

        // string functions
        public string RightString(string s, int count)
        {
            string newString = String.Empty;
            if (s != null && count > 0)
            {
                int startIndex = s.Length - count;
                if (startIndex > 0)
                    newString = s.Substring(startIndex, count);
                else
                    newString = s;
            }
            return newString;
        }

        public string LeftString(string s, int count)
        {
            string newString = s;
            if (s != null && count > 0 && s.Length > count)
            {
                newString = s.Substring(1, count);
            }
            return newString;
        }

        public string FixString(string bstr, string pstr, int pl)
        {
            StringBuilder sb = new StringBuilder();
            sb.Insert(0, pstr, pl).ToString();
            string aa = bstr + sb.ToString();
            string ab = aa.Substring(0, pl);
            return ab;
        }

        public static string ValidatePath(string s)
        {
            string newString = String.Empty;
            if (s.Substring(s.Length - 1, 1) != "\\")
                return s + "\\";
            else
                return s;
        }

        public static string CamelSpaceOut(string str)
        {
            for (int i = 1; i < str.Length; i++)
                if (Char.IsUpper(str[i]))
                    str = str.Insert(i++, " ");
            return str;
        }

        public static string ToProperCase(string sText)
        {
            //Get the culture property of the thread.
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            //Create TextInfo object.
            TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(sText);
        }

        public string SmallCapitals(string s)
        {
            if (s == "")
                return s;

            string result = "";
            var ar = s.Split();

            foreach (string t in ar)
            {
                result += t[0].ToString().ToUpper() + t.Substring(1) + " ";
            }

            return result.Trim();
        }

        // drive functions
        public bool DriveExist(string drive)
        {

            if (drive.Trim() == String.Empty)
                return false;

            if (drive.Substring(1, 1) != ":")
                drive += ":";
            drive = ValidatePath(drive);
            var drives = DriveInfo.GetDrives();
            if (drives.Where(data => data.Name == drive).Count() == 1)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Disconnects a network drive
        /// </summary>
        /// <param name="drive">Drive (z.B. L:)</param>
        public void MapNetworkDriveDisconnect(string drive)
        {
            Process p = new Process();
            p.StartInfo.FileName = "net";
            p.StartInfo.Arguments = string.Format("use {0} /DELETE", drive);
            p.StartInfo.UseShellExecute = false;
            p.Start();
        }
        /// <summary>
        /// Connects a network drive
        /// </summary>
        /// <param name="drive">The drive letter (e.g. L:)</param>
        /// <param name="server">The UNC path to the remote drive (e.g. \\MyServer\MyPrinter)</param>
        /// <param name="user">The User</param>
        /// <param name="password">The Password Used For Login</param>
        public void MapNetworkDriveConnect(string drive, string server, string user, string password)
        {
            Process p = new Process();
            p.StartInfo.FileName = "net";
            p.StartInfo.Arguments = string.Format("use {0} {1} /user:{2} {3}", drive, server, user, password);
            p.StartInfo.UseShellExecute = false;
            p.Start();
        }
        #endregion Methodes
    }
}
