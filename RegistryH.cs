using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NRSoft.FunctionPool
{
    /// <summary>
    /// Aufzählung für die Registry-Wurzel-Schlüssel 
    /// </summary>

    public enum RegistryRootKeys
    {
        HKEY_CLASSES_ROOT,
        HKEY_CURRENT_CONFIG,
        HKEY_CURRENT_USER,
        HKEY_DYN_DATA,
        HKEY_LOCAL_MACHINE,
        HKEY_PERFORMANCE_DATA,
        HKEY_USERS
    }

    public class RegistryH
    {
        #region Fields
        private string _companyName = "NRSoft";
        private string _productName = "NoName";
        #endregion

        public RegistryH(string companyName, string productName)
        {
            _companyName = companyName;
            _productName = productName;
        }

        public RegistryH()
        {

        }

        #region Properties
        public string ProductName
        {
            set { _productName = value; }
            get { return _productName; }
        }

        public string CompanyName
        {
            set { _companyName = value; }
            get { return _companyName; }
        }

        #endregion

        #region Methods
        public bool SaveSetting(string subkey, string keyname, string keyDefault = "")
        {
            RegistryKey keySet;
            RegistryKey rkCurrentUser = Registry.CurrentUser;

            string sRegKey = String.Format(@"Software\{0}\{1}\{2}", CompanyName, ProductName, subkey);
            keySet = rkCurrentUser.CreateSubKey(sRegKey);
            keySet.SetValue(keyname, keyDefault, RegistryValueKind.String);

            if (keySet != null) return false; else return true;
        }

        public string GetSetting(string subkey, string keyname, string keyDefault)
        {
            string regval = "";
            RegistryKey keyGet;
            RegistryKey rkCurrentUser = Registry.CurrentUser;

            string sRegKey = String.Format(@"Software\{0}\{1}\{2}", CompanyName, ProductName, subkey);
            keyGet = rkCurrentUser.CreateSubKey(sRegKey);

            try
            {
                RegistryValueKind rvk = keyGet.GetValueKind(keyname);

                if (rvk == RegistryValueKind.String)
                {
                    regval = keyGet.GetValue(keyname).ToString();
                }

                if (rvk == RegistryValueKind.DWord)
                {
                    regval = keyGet.GetValue(keyname).ToString();
                }

                return regval;
            }
            catch
            {
                return keyDefault;
            }
        }

        public ArrayList GetAllSettings(RegistryRootKeys rootKey, string keyPath)
        {
            ArrayList al = new ArrayList();
            RegistryKey regKey = GetRegistryRootKey(rootKey).OpenSubKey(keyPath);

            if (regKey != null)
            {
                foreach (string valuename in regKey.GetValueNames())
                {
                    al.Add(new object[] { valuename, regKey.GetValue(valuename) });
                }
            }
            return al;
        }

        public List<string> GetAllSettings(string subkey, bool dummy)
        {
            RegistryKey keyGet;
            RegistryKey rkCurrentUser = Registry.CurrentUser;
            List<string> querys = new List<string>();

            string sReg = String.Format(@"Software\{0}\{1}\{2}", CompanyName, ProductName, subkey);
            keyGet = rkCurrentUser.CreateSubKey(sReg);
            string[] allValueNames = keyGet.GetValueNames();

            foreach (string strRegValueName in allValueNames)
            {
                querys.Add(strRegValueName);
            }

            querys.Sort();
            return querys;
        }

        public string[,] GetAllSettings(string subkey)
        {
            RegistryKey keyGet;
            RegistryKey rkCurrentUser = Registry.CurrentUser;

            string sReg = String.Format(@"Software\{0}\{1}\{2}", CompanyName, ProductName, subkey);
            keyGet = rkCurrentUser.CreateSubKey(sReg);

            string[] allValueNames = keyGet.GetValueNames();
            int x = allValueNames.Length;
            string[,] allNamesAndValues = new string[x, 2];

            string regValue;
            int n = 0;
            foreach (string strRegValueName in allValueNames)
            {
                regValue = this.GetSetting(subkey, strRegValueName, "");
                allNamesAndValues[n, 0] = strRegValueName;
                allNamesAndValues[n, 1] = regValue;
                n++;
            }

            return allNamesAndValues;
        }

        public void DeleteSubKeyTree(RegistryRootKeys rootKey, string keyPath)
        {
            try
            {
                RegistryKey regKey = GetRegistryRootKey(rootKey);
                regKey.DeleteSubKeyTree(keyPath);
            }
            catch
            { }

        }

        /// <summary>
        /// Gibt den Registry-Wurzel-Schlüssel zurück, 
        /// der zu dem übergebenen RegistryRootKeys-Wert gehört
        /// </summary>
        public RegistryKey GetRegistryRootKey(RegistryRootKeys rootKey)
        {
            RegistryKey regKey = null;
            switch (rootKey)
            {
                case RegistryRootKeys.HKEY_CLASSES_ROOT:
                    regKey = Registry.ClassesRoot;
                    break;

                case RegistryRootKeys.HKEY_CURRENT_CONFIG:
                    regKey = Registry.CurrentConfig;
                    break;

                case RegistryRootKeys.HKEY_CURRENT_USER:
                    regKey = Registry.CurrentUser;
                    break;

                case RegistryRootKeys.HKEY_DYN_DATA:
                    regKey = Registry.PerformanceData;
                    break;

                case RegistryRootKeys.HKEY_LOCAL_MACHINE:
                    regKey = Registry.LocalMachine;
                    break;

                case RegistryRootKeys.HKEY_PERFORMANCE_DATA:
                    regKey = Registry.PerformanceData;
                    break;

                case RegistryRootKeys.HKEY_USERS:
                    regKey = Registry.Users;
                    break;
            }

            return regKey;
        }

        /// <summary>
        /// Liest einen Registry-Eintrag
        /// </summary>
        /// <param name="rootKey">Die Wurzel, ab der gesucht werden soll</param>
        /// <param name="keyPath">Der Pfad zu dem Registry-Schlüssel (ohne Wurzel)</param>
        /// <param name="valueName">Der Name des auszulesenden Wertes</param>
        /// <param name="defaultValue">Defaultwert, der zurückgegeben wird, falls der Wert nicht gefunden wird</param>
        /// <returns>Gibt den gelesenen Wert zurück falls der Registry-Eintrag gefunden wird,
        /// ansonsten wird der Defaultwert zurückgegeben</returns>
        public object ReadValue(RegistryRootKeys rootKey, string keyPath, string valueName, object defaultValue)
        {
            // Schlüssel ermitteln
            RegistryKey regKey = GetRegistryRootKey(rootKey).OpenSubKey(keyPath);

            // Wert auslesen, wenn der Unterschlüssel gefunden wurde
            object regValue = defaultValue;
            if (regKey != null)
            {
                regValue = regKey.GetValue(valueName);
            }

            return regValue;
        }

        /// <summary>
        /// Schreibt einen Registry-Eintrag
        /// </summary>
        /// <param name="rootKey">Die Wurzel, ab der gesucht werden soll</param>
        /// <param name="keyPath">Der Pfad zu dem Registry-Schlüssel (ohne Wurzel)</param>
        /// <param name="valueName">Der Name des zu schreibenden Wertes</param>
        /// <param name="value">Der zu schreibenden Wert</param>
        /// <param name="createIfNotExist">Gibt an, ob der Registry-Eintrag erzeugt werden soll, 
        /// falls dieser noch nicht existiert</param>
        public void SetValue(RegistryRootKeys rootKey, string keyPath,
           string valueName, object value, bool createIfNotExist, RegistryValueKind valueKind = RegistryValueKind.String)
        {
            // Wurzel-Schlüssel ermitteln
            RegistryKey regKey = GetRegistryRootKey(rootKey);

            // Den Pfad in seine Einzelteile zerlegen
            string[] pathToken = keyPath.Split('\\');

            // Pfad durchgehen und den Schlüssel referenzieren
            RegistryKey subKey = null;
            for (int i = 0; i < pathToken.Length; i++)
            {
                if (regKey != null)
                {
                    // Unterschlüssel zum Lesen und Schreiben öffnen
                    subKey = regKey.OpenSubKey(pathToken[i], true);

                    if (subKey == null && createIfNotExist)
                    {
                        // Unterschlüssel nicht gefunden: Schlüssel erzeugen 
                        // falls dies gewünscht ist
                        subKey = regKey.CreateSubKey(pathToken[i]);
                    }
                    regKey = subKey;
                }
            }

            // Wert schreiben, wenn der Unterschlüssel gefunden wurde
            if (regKey != null)
            {
                regKey.SetValue(valueName, value, valueKind);
            }
            else
            {
                // Ausnahme werfen
                throw new Exception("Schlüssel " + rootKey.ToString() + "\\" +
                   keyPath + " nicht gefunden");
            }
        }

        /// <summary>
        /// Löscht einen Registry-Eintrag
        /// </summary>
        /// <param name="rootKey">Die Wurzel, ab der gesucht werden soll</param>
        /// <param name="keyPath">Der Pfad zu dem Registry-Schlüssel (ohne Wurzel)</param>
        /// <param name="valueName">Der Name des zu löschenden Wertes</param>
        public void DeleteValue(RegistryRootKeys rootKey, string keyPath, string valueName)
        {
            // Unterschlüssel zum Schreiben öffnen
            RegistryKey regKey = GetRegistryRootKey(rootKey).OpenSubKey(
               keyPath, true);

            // Wert über den übergeordneten Schlüssel löschen 
            // wenn dieser gefunden wurde
            if (regKey != null)
            {
                regKey.DeleteValue(valueName, false);
            }
            else
            {
                // Schlüssel nicht gefunden: Ausnahme werfen
                throw new Exception("Schlüssel " + rootKey.ToString() + "\\" +
                   keyPath + " nicht gefunden");
            }
        }

        /// <summary>
        /// Löscht einen Schlüssel aus der Registry
        /// </summary>
        /// <param name="rootKey">Die Wurzel, ab der gesucht werden soll</param>
        /// <param name="keyPath">Der Pfad zu dem Registry-Schlüssel (ohne Wurzel)</param>
        public void DeleteKey(RegistryRootKeys rootKey, string keyPath)
        {
            // Den Pfad zum übergeordneten Schlüssel und den Namen des zu löschenden
            // Schlüssels ermitteln
            int i = keyPath.LastIndexOf("\\");
            string parentKeyPath = keyPath.Substring(0, i);
            string keyName = keyPath.Substring(i + 1, keyPath.Length - i - 1);

            // Den dem zu löschenden Schlüssel übergeordneten Schlüssel zum
            // Schreiben öffnen
            RegistryKey regKey = GetRegistryRootKey(rootKey).OpenSubKey(
               parentKeyPath, true);

            if (regKey != null)
            {
                // Schlüssel über den übergeordneten Schlüssel löschen
                regKey.DeleteSubKey(keyName);
            }
            else
            {
                // Schlüssel nicht gefunden: Ausnahme werfen
                throw new Exception("Schlüssel " + rootKey.ToString() + "\\" +
                   keyPath + " nicht gefunden");
            }
        }

        #endregion

    }
}
