using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Relational.Octapus.Common
{
    public class Packager
    {

        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest,true);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        public static void CopySourceCode(string sourcePath, string destinationPath)
        {

            foreach (string directory in Directory.GetDirectories(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                var folder = Regex.Split(directory,@"/").LastOrDefault();
                if ((!folder.StartsWith("DB_")))
                Directory.CreateDirectory(destinationPath + @"/" + folder);
            }

            System.GC.Collect();
            
            foreach (string file in Directory.GetFiles(sourcePath, "*.pbl", SearchOption.AllDirectories))
            {
                var folder = Regex.Split(Path.GetDirectoryName(file), @"\\").LastOrDefault();
                var fileName = Regex.Split(file, @"\\").LastOrDefault(); 
                File.SetAttributes(file, FileAttributes.Normal);
                File.Copy(file, Path.Combine(destinationPath + @"/" + folder, fileName), true);
            }

            System.GC.Collect();

            foreach (string file in Directory.GetFiles(sourcePath, "*.pbd", SearchOption.AllDirectories))
            {
                var folder = Regex.Split(Path.GetDirectoryName(file), @"\\").LastOrDefault();
                if (folder.StartsWith("pbd") || folder.StartsWith("dw2") || folder.StartsWith("pbsoap") || folder.StartsWith("Rule"))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    var fileName = Regex.Split(file, @"\\").LastOrDefault(); 
                    File.Copy(file, Path.Combine(destinationPath + @"/" + folder, fileName), true);
                }
            }

        }

        public static void CopyBuildLibraries (string sourcePath, string destinationPath)
        {
            foreach (string file in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                FileInfo fileInfo = new FileInfo(file);
                string dirName = Path.GetDirectoryName(Path.Combine(sourcePath, file));
                if (!dirName.Contains("PBsource"))
                {
                    if ((file.ToUpper().EndsWith(".PBL") || file.ToUpper().EndsWith(".PBD") || file.ToUpper().StartsWith("PCA")))
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        fileInfo.CopyTo(Path.Combine(destinationPath, Regex.Split(file, @"\\").LastOrDefault()), true);
                    }
                }
            }
        }

        public static void CopyLibraries(string sourcePath, string destinationPath)
        {
            try
            {
                foreach (string file in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    string dirName = Path.GetDirectoryName(Path.Combine(sourcePath, file));
                    if (!dirName.Contains("PBsource"))
                    {
                        if ((file.ToUpper().EndsWith(".PBL") || file.ToUpper().StartsWith("PCA")))
                        {
                            File.SetAttributes(file, FileAttributes.Normal);
                            fileInfo.CopyTo(Path.Combine(destinationPath, Regex.Split(file, @"\\").LastOrDefault()), true);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine("Packaging - CopyLibraries: " + ex.Message);
            }
            

        }

        public static void CopyFile(string sourcePath, string destinationPath)
        {
            try
            {
                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);
                File.Copy(sourcePath, destinationPath, true);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Packaging - CopyFile: " + ex.Message);
            }
           
        }

        public static void CopyPromotion (string sourcePath, string destinationPath, string buildNumber)
        {
            try
            {
                foreach (string file in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if ((file.ToUpper().EndsWith(".PBD")) || (file.ToUpper().EndsWith("IAPPLY.EXE")) || (file.ToUpper().StartsWith("PCA")))
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        fileInfo.CopyTo(destinationPath + @"\" + Regex.Split(file, @"\\").LastOrDefault(), true);
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Packaging - CopyPromotion: " + ex.Message);
            }
            

        }

        public static void CopyFiles(string sourcePath, string destinationPath, string extension)
        {
            try
            {
                foreach (string file in Directory.GetFiles(sourcePath, "*" + extension, SearchOption.AllDirectories))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if ((file.ToUpper().EndsWith(extension)))
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        fileInfo.CopyTo(Path.Combine(destinationPath,Regex.Split(file, @"\\").LastOrDefault()), true);
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("CopyFiles: " + ex.Message);
            }


        }

        public static void CopySpecificPbd(string sourcePath, string destinationPath, string pbdFiles)
        {
            var pbdList = pbdFiles.Split(',').ToList();
            try
            {
                foreach (var item in pbdList)
                {
                    var library = item.Trim();
                    if (!item.ToUpper().EndsWith(".PBD")) library = String.Concat(library, ".pbd");
                    FileInfo fileInfo = new FileInfo(library);
                    File.Copy(Path.Combine(sourcePath, library), Path.Combine(destinationPath, library), true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("BuildSpecificPbd: " + ex.Message);
            }
        }

        public static void DeleteFolder(string path)
        {
            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }
            Directory.Delete(path, true);
        }

        public static void PreparePackage(string buildNumber, string buildsFolder, string tempPath, string localVssProductPath,
                                   string localVssCommonPath, string clientNecFiles, string buildOutFolder,
                                   string productName, string projectResourceFile, string buildFolderName, string buildClientFolderName,
                                   string buildSourceCodeFolder, string scriptFolder, string buildPartialClientFolder,
                                   string localVssPBTPath, bool canCreateWorkableClient)
        {

            try
            {
                if (!Directory.Exists(scriptFolder)) Directory.CreateDirectory(scriptFolder);
                if (File.Exists(Path.Combine(scriptFolder, "InsertData.sql"))) File.Delete(Path.Combine(scriptFolder, "InsertData.sql"));
                File.Move(Path.Combine(tempPath, "InsertData.sql"), Path.Combine(scriptFolder, "InsertData.sql"));
                GC.Collect();

                Packager.DeleteFolder(tempPath);
                GC.Collect();

                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                GC.Collect();
                Packager.CopySourceCode(localVssProductPath, tempPath);
                GC.Collect();
                if (!localVssProductPath.Equals(localVssCommonPath))
                {
                    Packager.CopySourceCode(localVssCommonPath, tempPath);
                    GC.Collect();
                }

                if (!Directory.Exists(buildOutFolder)) Directory.CreateDirectory(buildOutFolder);
                Packager.CopyBuildLibraries(tempPath, Path.Combine(tempPath, buildOutFolder));
                GC.Collect();

                if (canCreateWorkableClient) Packager.CopyFolder(clientNecFiles, Path.Combine(tempPath, buildOutFolder));
                GC.Collect();

                if (!Directory.Exists(buildSourceCodeFolder)) Directory.CreateDirectory(buildSourceCodeFolder);
                if (!Directory.Exists(buildClientFolderName)) Directory.CreateDirectory(buildClientFolderName);

                File.SetAttributes(Path.Combine(localVssPBTPath, projectResourceFile), FileAttributes.Normal);
                File.Copy(Path.Combine(localVssPBTPath, projectResourceFile), Path.Combine(tempPath, buildOutFolder, projectResourceFile), true);
            }
            catch (Exception ex)
            {
                 Console.WriteLine("Prepare Packaging: " + ex.Message);
            }
            
        }

        public static void Package(string tempPath, string buildSourceCodeFolder, string buildClientFolder, string buildOutFolder, string buildNumber)
        {
            try
            {
                if (!Directory.Exists(buildSourceCodeFolder)) Directory.CreateDirectory(buildSourceCodeFolder);
                Packager.CopyLibraries(tempPath, buildSourceCodeFolder);
                System.GC.Collect();

                if (!Directory.Exists(buildClientFolder)) Directory.CreateDirectory(buildClientFolder);
                Packager.CopyPromotion(buildOutFolder, buildClientFolder, buildNumber);
                System.GC.Collect();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Packaging: " + ex.Message);
            }

        }

    }
}
