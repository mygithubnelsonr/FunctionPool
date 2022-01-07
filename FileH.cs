using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace NRSoft.FunctionPool
{
    /// <summary>
    /// Klasse mit statischen Utility-Methoden für das Dateisystem
    /// </summary>
    public class FileSystemUtils
    {
        #region Fields
        private string _filter;
        private string _path;
        private int _filesCopied = 0;
        private int _foldersCopied = 0;
        private int _filesProcessed;
        //private ArrayList _al = new ArrayList();

        /// <summary>
        /// Definiert, ob beim Kopieren mit <see cref="CopyFolderFaultTolerant"/>
        /// alle Dateien überschrieben werden sollen.
        /// </summary>
        private bool _overwriteAllFiles;

        /// <summary>
        /// Gibt an, ob beim Kopieren mit <see cref="CopyFolderFaultTolerant"/>
        /// bereits gefragt wurde, ob alle Dateien überschrieben werden sollen.
        /// </summary>
        //private bool _alreadyAskedForOverwriteAllFiles;

        private static List<string> _listExeptions;

        private static List<FileInfo> fileInfos;

        #endregion

        #region Properties
        public string Filter
        {
            get { return _filter; }
            set { _filter = value.ToLower(); }
        }

        public string FullPath
        {
            get { return _path; }
            set { _path = value.ToLower(); }
        }

        public int FilesCopied
        {
            get { return _filesCopied; }
        }

        public int FoldersCopied
        {
            get { return _foldersCopied; }
        }

        public bool OverwriteAllFiles { get => _overwriteAllFiles; set => _overwriteAllFiles = value; }

        #endregion

        #region CTOR
        public FileSystemUtils()
        {

        }
        public FileSystemUtils(List<string> list)
        {
            _listExeptions = list;
        }

        #endregion

        #region Methods

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        private static void ProcessDirectory(string targetDirectory, bool recursiv)
        {
            // Process the list of files found in the directory.
            try
            {
                string[] fileEntries = Directory.GetFiles(targetDirectory);
                foreach (string fileName in fileEntries)
                    ProcessFile(fileName);

                if (recursiv == true)
                {
                    // Recurse into subdirectories of this directory.
                    string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                    foreach (string subdirectory in subdirectoryEntries)
                        ProcessDirectory(subdirectory, recursiv);
                }
            }
            catch
            {
                Console.WriteLine("Directory Fehler!");
            }
        }

        // Insert logic for processing found files here.
        private static void ProcessFile(string path)
        {
            try
            {
                //Console.WriteLine("Processed file '{0}'.", path);
                fileInfos.Add(new FileInfo(path));
            }
            catch
            {
                Console.WriteLine($"File Fehler! {path}");
            }

        }

        public static List<FileInfo> GetFileinfos(string path, bool recursiv = false)
        {
            fileInfos = new List<FileInfo>();

            if (File.Exists(path))
            {
                // This path is a file
                ProcessFile(path);
            }
            else if (Directory.Exists(path))
            {
                // This path is a directory
                ProcessDirectory(path, recursiv);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", path);
            }

            return fileInfos;
        }

        public List<FileInfo> GetLastFiles(string path, int amount)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fi = di.GetFiles();

            var sl = fi.OrderByDescending(f => f.Name);
            List<FileInfo> fs = sl.ToList();

            List<FileInfo> files = new List<FileInfo>();
            int n = 0;

            foreach (FileInfo f in fs)
            {
                files.Add(f);
                n++;
                if (n == amount) break;
            }
            return files;
        }

        public static FileInfo GetFileinfo(string fullpath, bool recursiv = false)
        {
            if (File.Exists(fullpath))
            {
                return new FileInfo(fullpath);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", fullpath);
                return null;
            }
        }

        //public List<FileInfo> GetFileinfos(string path, bool recursiv = true)
        //{
        //    if (File.Exists(path))
        //    {
        //        // This path is a file
        //        ProcessFile(path);
        //    }
        //    else if (Directory.Exists(path))
        //    {
        //        // This path is a directory
        //        ProcessDirectory(path, recursiv);
        //    }
        //    else
        //    {
        //        Console.WriteLine("{0} is not a valid file or directory.", path);
        //    }

        //    return fileInfos;
        //}

        //public ArrayList GetFiles(string path, string filter)
        //{
        //    _path = path;
        //    _filter = filter;

        //    ArrayList al = GetFiles();

        //    return al;
        //}

        public ArrayList GetFiles()
        {
            ArrayList arrayList = new ArrayList();
            DirectoryInfo di = new DirectoryInfo(_path);

            if (di.Exists)
            {
                // Get a reference to each file in that directory.
                FileInfo[] fiArr = di.GetFiles();

                string strTemp = "";

                // Display the names of the files.
                foreach (FileInfo fri in fiArr)
                {
                    if (_filter != "")
                    {
                        strTemp = fri.Name;
                        if (strTemp.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            arrayList.Add(fri.FullName);
                        }
                    }
                    else
                    {
                        arrayList.Add(fri.FullName);
                    }
                }
            }

            if (arrayList != null && arrayList.Count == 0)
                arrayList = null;

            return arrayList;

        }

        public bool TouchFile(string fileName, string newfiletime)
        {
            string oldDate;
            bool success = false;

            FileInfo fi = new FileInfo(fileName);

            if (fi.Exists)
            {
                oldDate = fi.CreationTime.ToString();
                DateTime dnewDate = DateTime.Parse(newfiletime);
                fi.CreationTime = dnewDate;
                fi.LastAccessTime = dnewDate;
                fi.LastWriteTime = dnewDate;
                success = true;
            }
            else
            {
                success = false;
            }

            return success;
        }

        public bool IsFilesEqual(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            if (file1.Length == file2.Length)
            {
                for (int i = 0; i < file1.Length; i++)
                {
                    if (file1[i] != file2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sucht alle Dateien, die dem angegebenen Muster entsprechen
        /// </summary>
        /// <param name="startDirectory">Pfad zu dem Verzeichnis, dem gesucht werden soll</param>
        /// <param name="filePattern">Das Such-Muster (z. B. *.txt)</param>
        /// <param name="recursive">Gibt an, ob auch in den Unterordnern gesucht werden soll</param>
        /// <returns>Gibt ein ReadOnlyCollection vom Typ FileInfo zurück, die die 
        /// gefundenen Dateien repräsentiert</returns>
        //public static ReadOnlyCollection<FileInfo> FindFiles(string startDirectory, string filePattern, bool recursive)
        //{
        //    // Basis-Auflistung erzeugen
        //    List<FileInfo> fileList = new List<FileInfo>();

        //    // Die rekursive private Methode aufrufen
        //    fileList = Fileinfos(new DirectoryInfo(startDirectory), filePattern, recursive);

        //    // Ergebnis-Collection zurückgeben
        //    return new ReadOnlyCollection<FileInfo>(fileList);
        //}

        /// <summary>
        /// Sucht alle Dateien, die dem angegebenen Muster entsprechen
        /// </summary>
        /// <param name="directory">Das Verzeichnis, in dem gesucht werden soll</param>
        /// <param name="filePattern">Das Such-Muster (z. B. *.txt)</param>
        /// <param name="recursive">Gibt an, ob auch in den Unterordnern gesucht werden soll</param>
        /// <param name="fileList">Referenz auf eine List-Auflistung, die die 
        /// Pfade zu den gefundenen Dateien aufnimmt</param>
        /// 

        //private static void xFindFiles(DirectoryInfo directory, string filePattern, bool recursive, List<FileInfo> fileList)
        //{
        //    if(filePattern != null && filePattern.Length > 0)
        //    {
        //        // Das Dateimuster splitten
        //        string[] filePatterns = filePattern.Split(';');

        //        // Alle Dateimuster durchgehen und in dem übergebenen Verzeichnis suchen
        //        foreach(string partPattern in filePatterns)
        //        {
        //            try
        //            {
        //                foreach(FileInfo fileInfo in directory.GetFiles(partPattern))
        //                {
        //                    fileList.Add(fileInfo);
        //                }
        //            }
        //            catch(UnauthorizedAccessException)
        //            {
        //                // Der Zugriff wurde verweigert: Ignorieren
        //            }
        //        }
        //    }
        //    else
        //    {
        //        try
        //        {
        //            // Kein Suchmuster angegeben: Alle Dateien durchgehen 
        //            foreach(FileInfo fileInfo in directory.GetFiles())
        //            {
        //                fileList.Add(fileInfo);
        //            }
        //        }
        //        catch(UnauthorizedAccessException)
        //        {
        //            // Der Zugriff wurde verweigert: Ignorieren
        //        }
        //    }

        //    if(recursive)
        //    {
        //        // Wenn rekursiv gesucht werden soll:
        //        // Die Methode für alle Unterordner aufrufen
        //        try
        //        {
        //            foreach(DirectoryInfo subDirectory in directory.GetDirectories())
        //            {
        //                FindFiles(new DirectoryInfo(subDirectory), filePattern, recursive);
        //            }
        //        }
        //        catch(UnauthorizedAccessException)
        //        {
        //            // Der Zugriff wurde verweigert: Ignorieren
        //        }
        //    }
        //}

        public static List<FileInfo> Fileinfos(DirectoryInfo directory, string filePattern, bool recursive)
        {
            List<FileInfo> fileList = new List<FileInfo>();

            if (filePattern != null && filePattern.Length > 0)
            {
                // Das Dateimuster splitten
                string[] filePatterns = filePattern.Split(';');

                // Alle Dateimuster durchgehen und in dem übergebenen Verzeichnis suchen
                foreach (string partPattern in filePatterns)
                {
                    try
                    {
                        foreach (FileInfo fileInfo in directory.GetFiles(partPattern))
                        {
                            fileList.Add(fileInfo);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Der Zugriff wurde verweigert: Ignorieren
                    }
                }
            }
            else
            {
                try
                {
                    // Kein Suchmuster angegeben: Alle Dateien durchgehen 
                    foreach (FileInfo fileInfo in directory.GetFiles())
                    {
                        fileList.Add(fileInfo);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Der Zugriff wurde verweigert: Ignorieren
                }
            }

            if (recursive)
            {
                // Wenn rekursiv gesucht werden soll:
                // Die Methode für alle Unterordner aufrufen
                try
                {
                    foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                    {
                        Fileinfos(subDirectory, filePattern, recursive);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Der Zugriff wurde verweigert: Ignorieren
                }
            }

            return fileList;
        }

        public static List<FileInfo> Fileinfos(List<string> files, string filePattern)
        {
            List<FileInfo> fileList = new List<FileInfo>();

            if (filePattern != null && filePattern.Length > 0)
            {
                // Das Dateimuster splitten
                string[] filePatterns = filePattern.Split(';');

                // Alle Dateimuster durchgehen und in dem übergebenen Verzeichnis suchen
                foreach (string partPattern in filePatterns)
                {
                    try
                    {
                        foreach (string file in files)
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            fileList.Add(fileInfo);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Der Zugriff wurde verweigert: Ignorieren
                    }
                }
            }
            else
            {
                //try
                //{
                //    // Kein Suchmuster angegeben: Alle Dateien durchgehen 
                //    foreach (FileInfo fileInfo in directory.GetFiles())
                //    {
                //        fileList.Add(fileInfo);
                //    }
                //}
                //catch (UnauthorizedAccessException)
                //{
                //    // Der Zugriff wurde verweigert: Ignorieren
                //}
            }

            //if (recursive)
            //{
            //    // Wenn rekursiv gesucht werden soll:
            //    // Die Methode für alle Unterordner aufrufen
            //    try
            //    {
            //        foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            //        {
            //            Fileinfos(subDirectory, filePattern, recursive);
            //        }
            //    }
            //    catch (UnauthorizedAccessException)
            //    {
            //        // Der Zugriff wurde verweigert: Ignorieren
            //    }
            //}

            return fileList;
        }

        #endregion

        #region CopyFault
        /// <summary>
        /// Kopiert (rekursiv) alle Unterordner und Dateien eines Ordners in einen anderen Ordner
        /// </summary>
        /// <param name="sourceFolderPath">Der Pfad zum Quellordner</param>
        /// <param name="destFolderPath">Der Pfad des Zielordners</param>
        /// <returns>Gibt eine <see cref="CopyFaults"/>-Auflistung zurück 
        /// mit Informationen über fehlgeschlagene Kopier-Vorgänge</returns>
        public ReadOnlyCollection<CopyFault> CopyFolderFaultTolerant(string sourceFolderPath, string destFolderPath)
        {
            // Liste von CopyFault-Objekten erzeugen
            List<CopyFault> copyFaults = new List<CopyFault>();

            // Datei-Überschreib-Flags voreinstellen 
            OverwriteAllFiles = false;
            //_alreadyAskedForOverwriteAllFiles = false;

            // Die rekursive Methode zum Kopieren der Unterordner 
            // und Dateien aufrufen
            CopySubFoldersAndFiles(new DirectoryInfo(sourceFolderPath), sourceFolderPath, destFolderPath, copyFaults);

            // Ergebnis zurückgeben
            return new ReadOnlyCollection<CopyFault>(copyFaults);
        }

        /// <summary>
        /// Kopiert (rekursiv) alle Unterordner und Dateien eines Ordners in einen anderen Ordner
        /// </summary>
        /// <param name="folder">Referenz auf ein DirectoryInfo-Objekt, das den Ordner repräsentiert</param>
        /// <param name="mainSourceFolderPath">Pfad zum Haupt-Quellordner</param>
        /// <param name="mainDestFolderPath">Pfad zum Haupt-Zielordner</param>
        /// <param name="copyFaults">Referenz auf eine CopyFaults-Auflistung,
        /// in die diese Methode alle aufgetretenen Kopierfehler schreibt</param>
        private void CopySubFoldersAndFiles(DirectoryInfo folder, string mainSourceFolderPath, string mainDestFolderPath, List<CopyFault> copyFaults)
        {
            // Zielordner anlegen
            try
            {
                // Zielordnername ermitteln
                string destFolderPath = folder.FullName.Replace(mainSourceFolderPath,
                    mainDestFolderPath);

                // Ordner anlegen
                Directory.CreateDirectory(destFolderPath);
            }
            catch (IOException ex)
            {
                // Fehler in der CopyFaults-Auflistung dokumentieren
                copyFaults.Add(new CopyFault(false, mainSourceFolderPath,
                    mainDestFolderPath, ex.Message));
            }

            // Alle Unterordner des übergebenen Ordners durchgehen
            DirectoryInfo[] subFolders = folder.GetDirectories();
            for (int i = 0; i < subFolders.Length; i++)
            {
                // Pfad für den Ziel-Unterordner ermitteln, indem der Pfad zum 
                // Quellordner durch den Pfad zum Zielordner ersetzt wird
                string destSubFolderName = subFolders[i].FullName.Replace(mainSourceFolderPath, mainDestFolderPath);

                string subfolder = subFolders[i].ToString();
                // is folder in exeption list then skip
                if (_listExeptions != null)
                {
                    if (!_listExeptions.Contains(subfolder, StringComparer.OrdinalIgnoreCase))
                    {
                        // Funktion rekursiv aufrufen um zunächst die weiteren Unterordner 
                        // zu erzeugen
                        CopySubFoldersAndFiles(subFolders[i], mainSourceFolderPath, mainDestFolderPath, copyFaults);
                    }
                    else
                    {
                        Console.WriteLine("Exeption: skip {0}", subfolder);
                    }
                }
                else
                {
                    CopySubFoldersAndFiles(subFolders[i], mainSourceFolderPath, mainDestFolderPath, copyFaults);
                }
            }

            // Die im Ordner enthaltenen Dateien ermitteln
            FileInfo[] files = folder.GetFiles();

            // Alle Dateien durchgehen
            for (int i = 0; i < files.Length; i++)
            {
                // Ziel-Dateiname ermitteln, indem der Pfad zum Quellordner
                // durch den Pfad zum Zielordner ersetzt wird
                string destFileName = files[i].FullName.Replace(mainSourceFolderPath, mainDestFolderPath);

                // Flag setzen, das festlegt, ob die Datei kopiert werden soll
                bool performCopyOperation;
                performCopyOperation = true;

                // Datei kopieren, wenn die Operation ausgeführt werden soll
                if (performCopyOperation)
                {
                    try
                    {
                        if (files[i].Exists)
                        {
                            FileInfo fi = new FileInfo(destFileName);
                            if (files[i].LastWriteTime != fi.LastWriteTime)
                            {
                                File.Copy(files[i].FullName, destFileName, true);
                                _filesCopied += 1;
                            }
                        }
                        else
                        {
                            File.Copy(files[i].FullName, destFileName, true);
                            _filesCopied += 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Fehler in der CopyFaults-Auflistung dokumentieren
                        copyFaults.Add(new CopyFault(true, files[i].FullName,
                            destFileName, ex.Message));
                    }
                }
                else
                {
                    // Datei sollte nicht überschrieben werden: Fehler in der 
                    // CopyFaults-Auflistung dokumentieren
                    copyFaults.Add(new CopyFault(true, files[i].FullName, destFileName,
                        "Fehlende Anwender-Erlaubnis zum Überschreiben"));
                }
            }
        }

        public void CopyFile(string source, string destination)
        {
            try
            {
                FileInfo result = null;
                DirectoryInfo di = new DirectoryInfo(destination);
                if (!di.Exists) di.Create();

                FileInfo fiSource = new FileInfo(source);
                _filesProcessed += 1;

                if (fiSource.Exists)
                {
                    FileInfo fiDest = new FileInfo(destination);
                    if (fiDest.LastWriteTime != fiSource.LastWriteTime)
                    {
                        result = fiSource.CopyTo(Path.Combine(fiDest.FullName, fiSource.Name), true);
                        Console.WriteLine("result={0}", result.Length);
                        _filesCopied += 1;
                    }
                }
                else
                {
                    Console.WriteLine("file not found! {0}", fiSource.Name);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void CopyFolder(string sourceFolderName, string destFolderName)
        {
            try
            {
                ReadOnlyCollection<CopyFault> copyFaults = CopyFolderFaultTolerant(sourceFolderName, destFolderName);

                if (copyFaults.Count == 0)
                {
                    Console.WriteLine("{0} files copied", _filesCopied);
                    Console.WriteLine("Fertig");
                }
                else
                {
                    // Beim Kopieren sind Fehler aufgetreten: Alle Fehler
                    // durchgehen und ausgeben
                    foreach (CopyFault copyFault in copyFaults)
                    {
                        Console.WriteLine();
                        if (copyFault.IsFile)
                        {
                            // Es handelt sich um eine Datei
                            Console.WriteLine("Fehler beim Kopieren der " +
                               "Datei '{0}' nach '{1}': {2}",
                               copyFault.Source, copyFault.Destination,
                               copyFault.Error);
                        }
                        else
                        {
                            // Es handelt sich um einen Ordner
                            Console.WriteLine("Fehler beim Kopieren des Ordners '{0}' nach '{1}': {2}",
                               copyFault.Source, copyFault.Destination,
                               copyFault.Error);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Fehler beim Kopieren des Ordners: {0}",
                   ex.Message);
            }
        }

        #endregion
    }
}
