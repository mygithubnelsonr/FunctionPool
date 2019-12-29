using System;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace NRSoft.FunctionPool
{
    /// <summary>
    /// Class with some usefull forms functions.
    /// </summary>
    public static class FormsUtilities
    {
        public static void FillCombo(ComboBox comb, string strNode)
        {
            comb.Items.Clear();
            comb.Items.Add("");

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string inidir = appdata + "\\LogAn";
            string ininame = "\\LogAn.xml";

            DirectoryInfo di = new DirectoryInfo(inidir);

            if (!di.Exists)
                di.Create();

            FileInfo fi = new FileInfo(inidir + ininame);

            if (!fi.Exists)
            {
                StreamWriter sw = new StreamWriter(fi.ToString());
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"ISO-8859-1\" ?>\n");
                sw.WriteLine("<Settings>");
                sw.WriteLine("\t<Folders>");
                sw.WriteLine("\t\t<Folder></Folder>");
                sw.WriteLine("\t</Folders>");
                sw.WriteLine("\t<Filters>");
                sw.WriteLine("\t\t<Filter></Filter>");
                sw.WriteLine("\t</Filters>");
                sw.WriteLine("</Settings>");
                sw.Close();
            }

            XmlDocument oXMLDoc = new XmlDocument();
            oXMLDoc.Load(fi.ToString());
            XmlNode oRoot = oXMLDoc.FirstChild;

            if (oRoot.HasChildNodes)
            {
                for (int i = 0; i < oRoot.ChildNodes.Count; i++)
                {
                    if (oRoot.ChildNodes[i].Name == strNode)
                    {
                        XmlNode oFolders = oRoot.ChildNodes[i];
                        foreach (XmlNode oFolder in oFolders)
                        {
                            string s = oFolder.InnerText;
                            if (s.Trim() != "")
                                comb.Items.Add(s);
                        }
                        break;
                    }
                }
            }
        }

        public static string ToProperCase(string sText)
        {
            //Get the culture property of the thread.
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            //Create TextInfo object.
            TextInfo textInfo = cultureInfo.TextInfo;

            string propercase = textInfo.ToLower(sText);
            propercase = textInfo.ToTitleCase(propercase);

            return propercase;
        }

        public static bool IsNumeric(object ValueToCheck)
        {
            double Dummy = new double();
            string InputValue = Convert.ToString(ValueToCheck);

            bool Numeric = double.TryParse(InputValue, System.Globalization.NumberStyles.Any, null, out Dummy);

            return Numeric;
        }

        public static bool IsMenuChecked(ToolStripMenuItem men)
        {
            /// <summary>
            /// Determines whether [is menu checked] [the specified men].
            /// </summary>
            /// <param name="men">The men.</param>
            /// <returns>
            /// 	<c>true</c> if [is menu checked] [the specified men]; otherwise, <c>false</c>.
            /// </returns>

            bool bCheched = false;
            foreach (ToolStripMenuItem n in men.DropDownItems)
            {
                bCheched = bCheched || n.Checked;
            }

            if (bCheched == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void UncheckMenu(ToolStripMenuItem men)
        {
            foreach (ToolStripMenuItem n in men.DropDownItems)
            {
                n.Checked = false;
            }
        }

        public static int GetCheckedMenuItem(ToolStripMenuItem men)
        {
            string retval = string.Empty;
            int cnt = -1;

            foreach (ToolStripMenuItem n in men.DropDownItems)
            {
                ++cnt;
                if (n.Checked == true)
                {
                    retval = Convert.ToString(n);
                    break;
                }
            }
            if (retval == string.Empty)
                cnt = -1;

            return cnt;
        }

        public static string IsSingleFile(DragEventArgs args)
        {
            // If the data object in args is a single file, this method will return the filename.
            // Otherwise, it returns null.
            // Check for files in the hovering data object.
            if (args.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] fileNames = args.Data.GetData(DataFormats.FileDrop, true) as string[];
                // Check fo a single file or folder.
                if (fileNames.Length == 1)
                {
                    // Check for a file (a directory will return false).
                    if (File.Exists(fileNames[0]))
                    {
                        // At this point we know there is a single file.
                        return fileNames[0];
                    }
                }
            }
            return null;
        }
    }
}
