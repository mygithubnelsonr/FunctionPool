using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace NRSoft.FunctionPool
{
    /// <summary>
    /// Class to handle INI file values and sections.
    /// </summary>
	public static class IniFileH
	{
		private static string m_inifile;
		
        public static string IniFile
		{
			private set { m_inifile = value; }
			get { return m_inifile; }
		}

		public static bool FileExist()
		{
			if (string.IsNullOrEmpty(m_inifile)) return false;
			if (File.Exists(m_inifile))
				return true;
			else
				return false;
		}

		public static bool IniWriteValue(string Section,string Key,string Value)
		{
			return API.WritePrivateProfileString(Section, Key, Value, IniFile);
		}
	   
		public static string IniReadValue(string Section, string Key)
		{
		  StringBuilder temp = new StringBuilder(255);
		  int i = API.GetPrivateProfileString(Section, Key, "", temp, 255, IniFile);
		  return temp.ToString();
		}

        public static string IniReadSection(string Section)
        {
            StringBuilder temp = new StringBuilder(255);

			int i = API.GetPrivateProfileSection(Section, temp, 255, IniFile);

            return temp.ToString();
        }

        /// <summary>
        /// Returns Value from INI file.
        /// </summary>
        /// <param name="iniFileName">Full path to the INI file.</param>
        /// <param name="section">The section containing the key.</param>
        /// <param name="key">The value's Key.</param>
        /// <returns>The returned string, or null if none existed.</returns>
        public static string GetValue(
          string iniFileName,
          string section,
          string key)
        {

            //Check Args:
            if (string.IsNullOrEmpty(iniFileName))
            {
                throw new System.ArgumentNullException("iniFileName");
            }
            if (string.IsNullOrEmpty(section))
            {
                throw new System.ArgumentNullException("section");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new System.ArgumentNullException("key");
            }

            StringBuilder buffer = new StringBuilder(256);
            string sDefault = "";

            //Get a single string value from the INI file:
            if (API.GetPrivateProfileString(
              section, key, sDefault,
              buffer, buffer.Capacity, iniFileName) != 0)
            {
                //We had a result, so return it:
                return buffer.ToString();
            }
            else
            {
                //No result -- return null:
                return null;
            }
        }


        /// <summary>
        /// Writes a string to an INI file.
        /// </summary>
        /// <param name="iniFileName">Full path to the INI file.</param>
        /// <param name="section">The section containing the key.</param>
        /// <param name="key">The value's key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if the write was successful.</returns>
        public static bool SetValue(
          string iniFileName,
          string section,
          string key,
          string value)
        {
            //Check Args:
            if (string.IsNullOrEmpty(iniFileName))
            {
                throw new System.ArgumentNullException("iniFileName");
            }
            if (string.IsNullOrEmpty(section))
            {
                throw new System.ArgumentNullException("section");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new System.ArgumentNullException("key");
            }

            if (string.IsNullOrEmpty(value))
            {
                //Null's cause delete.
                //so convert to string, and use DeleteValue
                //method for explicit deletes:
                value = string.Empty;
            }

            return API.WritePrivateProfileString(section, key,
              value, iniFileName);
        }


        /// <summary>
        /// Deletes the value.
        /// </summary>
        /// <param name="iniFileName">Name of the ini file.</param>
        /// <param name="section">The section.</param>
        /// <param name="key">The key.</param>
        /// <returns>True if the key was successfully deleted.</returns>
        public static bool DeleteValue(
          string iniFileName,
          string section,
          string key
          )
        {
            if (string.IsNullOrEmpty(iniFileName))
            {
                throw new System.ArgumentNullException("iniFileName");
            }
            if (string.IsNullOrEmpty(section))
            {
                throw new System.ArgumentNullException("section");
            }

            //key, but no value = delete key:
            return API.WritePrivateProfileString(section, key, null, iniFileName);
        }


        /// <summary>
        /// Get Int value from an INI File.
        /// </summary>
        /// <param name="iniFileName">Full path to the INI file.</param>
        /// <param name="section">The section containing the key.</param>
        /// <param name="key">The value's key.</param>
        /// <returns>Returns the integer, or -1 if not found.</returns>
        /*
        public static int GetValue(
          string iniFileName,
          string section,
          string key) {
          //Check Args:
          if (string.IsNullOrEmpty(iniFileName)) {
            throw new System.ArgumentNullException("iniFileName");
          }
          if (string.IsNullOrEmpty(section)) {
            throw new System.ArgumentNullException("section");
          }
          if (string.IsNullOrEmpty(key)) {
            throw new System.ArgumentNullException("key");
          }

          int iDefault = -1;
          return API.GetPrivateProfileInt(section, key,
            iDefault, iniFileName);
        }
        */


        /// <summary>
        /// Gets List of values from an INI section.
        /// </summary>
        /// <param name="iniFileName">Full path to the INI file.</param>
        /// <param name="section">The section containing the key.</param>
        /// <returns>An array of strings, or null.</returns>
        public static bool GetSection(
          string iniFileName,
          string section,
          out string[] results)
        {
            //Check Args:
            if (string.IsNullOrEmpty(iniFileName))
            {
                throw new System.ArgumentNullException("iniFileName");
            }
            if (string.IsNullOrEmpty(section))
            {
                throw new System.ArgumentNullException("section");
            }
            List<string> items = new List<string>();
            byte[] buffer = new byte[32768];
            int bufLen = 0;
            bufLen = API.GetPrivateProfileSection(section, buffer,
              buffer.GetUpperBound(0), iniFileName);
            if (bufLen == 0)
            {
                results = null;
                return false;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bufLen; i++)
                {
                    if (buffer[i] != 0)
                    {
                        sb.Append((char)buffer[i]);
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
							try
							{
								string[] parts = sb.ToString().Split('=');
								items.Add(parts[1].Trim());
							}
							catch
							{
								//string[] parts = sb.ToString();
								items.Add(sb.ToString());
							}
                            //reset sb:
                            sb = new StringBuilder();
                        }
                    }
                }
            }
            results = items.ToArray();
            return true;
        }


        /// <summary>
        /// Gets List of values from an INI section.
        /// </summary>
        /// <param name="iniFileName">Full path to the INI file.</param>
        /// <param name="section">The section containing the key.</param>
        /// <returns>An array of strings, or null.</returns>
        public static bool GetSection(
          string iniFileName,
          string section,
          out Dictionary<string, string> results)
        {
            //Check Args:
            if (string.IsNullOrEmpty(iniFileName))
            {
                throw new System.ArgumentNullException("iniFileName");
            }
            if (string.IsNullOrEmpty(section))
            {
                throw new System.ArgumentNullException("section");
            }
            List<string> items = new List<string>();
            byte[] buffer = new byte[32768];
            int bufLen = 0;
            bufLen = API.GetPrivateProfileSection(section, buffer,
              buffer.GetUpperBound(0), iniFileName);
            if (bufLen == 0)
            {
                results = null;
                return false;
            }
            else
            {
                results = new Dictionary<string, string>();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bufLen; i++)
                {
                    if (buffer[i] != 0)
                    {
                        sb.Append((char)buffer[i]);
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            string[] parts = sb.ToString().Split('=');
                            results[parts[0].Trim()] = parts[1].Trim();
                            //reset sb:
                            sb = new StringBuilder();
                        }
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Writes a List to an INI Section
        /// </summary>
        /// <param name="iniFileName">Full path to the INI file.</param>
        /// <param name="section">The section containing the key.</param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool SetSection(
			string iniFileName,
			string section,
			string[] items)
        {
            //Check Args:
            if (string.IsNullOrEmpty(iniFileName))
            {
                throw new System.ArgumentNullException("iniFileName");
            }
            if (string.IsNullOrEmpty(section))
            {
                throw new System.ArgumentNullException("section");
            }
            //byte[] b = new byte[32768];
            //int j = 0;
            int keyCounter = 0;

            StringBuilder sb = new StringBuilder();
            if (items == null)
            {
                //Delete the section:
                return API.WritePrivateProfileSection(section, null, iniFileName);
            }
            foreach (string item in items)
            {
                keyCounter++;
                sb.Append(keyCounter.ToString());
                sb.Append("=");
                sb.Append(item);
                sb.Append((char)0);//Line ending

                /*
              string key = counter.ToString() + "=";
              ASCIIEncoding.ASCII.GetBytes(key, 0, key.Length, b, j);
              j += key.Length;
              ASCIIEncoding.ASCII.GetBytes(item, 0, item.Length, b, j);
              j += item.Length;
              b[j] = 0;
              j += 1;
              */
            }
            //b[j] = 0;
            sb.Append((char)0);//second vbNullString
            return API.WritePrivateProfileSection(section, sb, iniFileName);
        }

        public static bool SetSection(
          string iniFileName,
          string section,
          Dictionary<string, string> dictionary)
        {
            //Check Args:
            if (string.IsNullOrEmpty(iniFileName))
            {
                throw new System.ArgumentNullException("iniFileName");
            }
            if (string.IsNullOrEmpty(section))
            {
                throw new System.ArgumentNullException("section");
            }
            //byte[] b = new byte[32768];
            //int j = 0;
            // int keyCounter = 0;

            StringBuilder sb = new StringBuilder();
            if (dictionary.Count != 0)
            {
                //Delete the section:
                return API.WritePrivateProfileSection(section, null, iniFileName);
            }
            foreach (KeyValuePair<string, string> item in dictionary)
            {
                sb.Append(item.Key);
                sb.Append("=");
                sb.Append(item.Value);
                sb.Append((char)0);//Line ending
            }
            sb.Append((char)0);//second vbNullString
            return API.WritePrivateProfileSection(section, sb, iniFileName);
        }

        /// <summary>
        /// Explicitly deletes the section.
        /// </summary>
        /// <param name="iniFileName">Full path to the INI file.</param>
        /// <param name="section">The section to delete.</param>
        /// <returns></returns>
        public static bool DeleteSection(string iniFileName, string section)
        {
            //Check Args:
            if (string.IsNullOrEmpty(iniFileName))
            {
                throw new System.ArgumentNullException("iniFileName");
            }
            if (string.IsNullOrEmpty(section))
            {
                throw new System.ArgumentNullException("section");
            }
            //byte[] b = null;
            //return API.WritePrivateProfileSection(section, b, iniFileName);

            //No key = delete section:
            return API.WritePrivateProfileString(section, null, null, iniFileName);
        }

        /// <summary>
        /// Get name of all sections in INI File.
        /// </summary>
        /// <param name="iniFileName">Full path to the INI file.</param>
        /// <returns>an array of strings or null if empty.</returns>
        public static string[] GetSectionNames(
          string iniFileName)
        {
            //Check Args:
            if (string.IsNullOrEmpty(iniFileName))
            {
                throw new System.ArgumentNullException("iniFileName");
            }
            List<string> sections = new List<string>();
            byte[] buffer = new byte[32768];
            int bufLen = 0;
            bufLen = API.GetPrivateProfileSectionNames(
              buffer,
              buffer.GetUpperBound(0),
              iniFileName);
            if (bufLen == 0)
            {
                return null;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bufLen; i++)
                {
                    if (buffer[i] != 0)
                    {
                        sb.Append((char)buffer[i]);
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            sections.Add(sb.ToString());
                            sb = new StringBuilder();
                        }
                    }
                }
            }
            return sections.ToArray();
        }

        /// <summary>
        /// Protected API class:
        /// </summary>
        protected class API
        {
            /// <summary>
            /// Wrapper for API Call
            /// </summary>
            [DllImport("KERNEL32.DLL", EntryPoint = "WritePrivateProfileString")]
            public static extern bool WritePrivateProfileString(
                string Section,
                string KeyName,
                string String,
                string FileName
                );

            /// <summary>
            /// Wrapper for API Call
            /// </summary>
            [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileString")]
            public static extern int GetPrivateProfileString(
                string Section,
                string KeyName,
                string Default,
                StringBuilder ReturnedString,
                int Size,
                string FileName
                );

            /// <summary>
            /// Wrapper for API Call
            /// </summary>
            [DllImport("KERNEL32.DLL", EntryPoint = "WritePrivateProfileSection")]
            public static extern bool WritePrivateProfileSection(
                string SectionName,
                StringBuilder Data,
                string FileName
                );

            /// <summary>
            /// Wrapper for API Call
            /// </summary>
            [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileSection")]
            public static extern int GetPrivateProfileSection(
                string SectionName,
                StringBuilder ReturnedString,
                int Size,
                string FileName
                );

            [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileSection")]
            public static extern int GetPrivateProfileSection(
                string lpAppName,
                byte[] lpReturnedString, 
                int nSize,
                string lpFileName
                );

            /// <summary>
            /// Wrapper for API Call
            /// </summary>
            [DllImport("KERNEL32.DLL", EntryPoint = "GetPrivateProfileSectionNames")]
            public static extern int GetPrivateProfileSectionNames(
                byte[] lpReturnedString,
                int nSize,
                string lpFileName
                );

        } //Class API:End
	} //Class IniFileH:End
}//Namespace:End
