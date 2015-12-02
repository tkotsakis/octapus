using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Relational.Octapus.Persistence;
using Workspace.SourceSafe;
using System.IO;
using Relational.Octapus.Common;
using Microsoft.VisualStudio.SourceSafe.Interop;
using System.Text.RegularExpressions;
using System.Collections;
using Relational.Octapus.BuildEngine;

namespace Relational.Octapus.Workspace.SourceSafe
{
    public class SourceSafeProvider : IWorkspaceProvider
    {
        private IDataManager dataManager;
        private ApplicationParams applicationParams;
        private WorkspaceParams workspaceParams;
        private VersionParams versionParams;
        private SourceSafeControl sourceSafeControl;
        private OctapusLog logger;
        private StringDiff stringDiff;
        string pbgFromSourceControl, correctPbg;
        private List<PBLibrary> clientLibraryList;
        private List<PBLibrary> libraryList;
        private List<PBLibrary> appServerLibraryList;
        int libIndex = 99;
        string comment = "{Octapus}";

        public SourceSafeProvider()
        {
        }

        public SourceSafeProvider(IDataManager dataManager, string applicationId, IHookOctapusLogger hookLogger = null)
        {
            this.dataManager = dataManager;
            this.stringDiff = new StringDiff();
            applicationParams = dataManager.GetApplicationParams();
            workspaceParams = dataManager.GetWorkspaceParams(applicationId);
            versionParams = dataManager.GetVersionParams(applicationId);
            this.logger = new OctapusLog("OctapusLogger", hookLogger);
            sourceSafeControl = new SourceSafeControl(applicationParams.InitilizationFilePath, hookLogger);
            this.libraryList = new List<PBLibrary>();
            this.appServerLibraryList = new List<PBLibrary>();
            this.clientLibraryList = new List<PBLibrary>();
            this.libraryList = GetLibrarylist(applicationId, workspaceParams.VssPBTPath, workspaceParams.LibraryTargetPath);
            this.libraryList = GetLibrarylist(applicationId, workspaceParams.AppServerTargetPath1, workspaceParams.AppServerTargetFile1);
            sourceSafeControl.SetWorkingFolder(workspaceParams.VssProjectPath, workspaceParams.LocalVssWorkingPath);
        }

        public IDataManager DataManager { get; set; }


        public DownloadWorkspaceResult DownloadWorkspace(string ApplicationId)
        {
            return new DownloadWorkspaceResult();
        }

        public bool CheckWorkspacePbgs(string applicationId)
        {
            try
            {
                if (File.Exists(applicationParams.VssContent))
                {
                    sourceSafeControl.Connect(applicationParams.Username, applicationParams.Password);
                    sourceSafeControl.SetWorkingFolder(workspaceParams.VssProductPath, applicationParams.TargetPath);
                    sourceSafeControl.GetSpecificFiles(workspaceParams.VssProductPath, applicationParams.TempPath, applicationParams.PbgExtension);
                    sourceSafeControl.GetSpecificFiles(workspaceParams.VssCommonPath, applicationParams.TempPath, applicationParams.PbgExtension);

                    foreach (var pbgFile in Directory.GetFiles(applicationParams.TargetPath, "*.pbg", SearchOption.AllDirectories))
                    {
                        if (!pbgFile.Split('.').First().EndsWith(applicationParams.NewPbgFileSuffix))
                        {
                            pbgFromSourceControl = this.GetPbgInfo(pbgFile + Path.GetExtension(applicationParams.TargetPath));
                            string libraryName = Regex.Split(pbgFile, @"\\").Last().Split('.').First();
                            libIndex = clientLibraryList.FindIndex(x => x.LibraryName.Equals(libraryName));

                            string pbgNewFile = pbgFile.Split('.').First() + applicationParams.NewPbgFileSuffix + "." + pbgFile.Split('.').Last();
                            correctPbg = this.PbgBuilder(applicationParams.TargetPath, pbgNewFile, libraryName);
                            stringDiff.GetDiff(pbgFromSourceControl, correctPbg);
                            this.CorrectPBGFile(pbgFile, libraryName);
                        }
                    }
                    sourceSafeControl.Disconnect();
                }
            }
            catch (Exception ex)
            {
                sourceSafeControl.Disconnect();
                logger.LogInfo("CheckWorkspacePbgs:" + ex.Message);
            }
           
            return stringDiff.hasDifferences ? false : true;
        }

        private string GetPbgInfo(string pbgFile)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(pbgFile))
            {
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();

        }

        public void CorrectPBGFile (string pbgFile, string libraryName)
        {
            if (stringDiff.hasDifferences)
            {
                sourceSafeControl.SetWorkingFolder(clientLibraryList[libIndex].VssLibPath, clientLibraryList[libIndex].LocalVssWorkingPath);
                sourceSafeControl.CheckOut(clientLibraryList[libIndex].VssLibPath + libraryName + applicationParams.PbgExtension, comment);
                sourceSafeControl.UpdateFileToSourceControl((@clientLibraryList[libIndex].LocalVssWorkingPath + libraryName + applicationParams.PbgExtension), @correctPbg);
                sourceSafeControl.CheckIn(clientLibraryList[libIndex].VssLibPath + libraryName + applicationParams.PbgExtension, comment);
                logger.LogInfo(pbgFile + " updated!");
            }
        }

        private string PbgBuilder(string targetPath, string pbgFile, string libraryName)
        {
            List<string> libraryPathList = new List<string>();
            try
            {
                List<string> objectList = this.GetObjectsFromPbg(applicationParams.VssContent, libraryName);
                libIndex = clientLibraryList.FindIndex(x => x.LibraryName.Equals(libraryName));
                string jetPath = clientLibraryList[libIndex].ExtLibPlusLibName.Replace(@"\", @"\\"); //@"iapply\\admflow\\";
                string libraryPath = clientLibraryList[libIndex].ExtLibPlusLibNameWithExtension.Replace(@"\", @"\\");//@"iapply\\admflow\\admflow.pbl";

                foreach (var item in objectList)
                {
                    libraryPathList.Add(" " + '"' + jetPath);
                }
                for (int i = 0; i < objectList.Count; i++)
                {
                    libraryPathList[i] += objectList[i] + '"' + " " + '"' + libraryPath + '"' + ';';
                }
                var header = applicationParams.PbgFormat + "\r\n" + applicationParams.PbgBeginLib + "\r\n" + " " + '"' + libraryPath + '"' + " " + '"' + '"' + ';' + "\r\n" + applicationParams.PbgEnd + "\r\n" + applicationParams.PbgBeginObj + "\r\n";
                File.WriteAllText(@pbgFile, header);
                File.AppendAllLines(@pbgFile, libraryPathList);
                File.AppendAllText(@pbgFile, applicationParams.PbgEnd);
            }
            catch (Exception ex)
            {

                logger.LogError("PbgBuilder: " + ex.Message);
            }
            
            return this.GetPbgInfo(@pbgFile);
        }

        public List<string> GetObjectsFromPbg(string textPath, string libraryName)
        {
            List<string> objectList = new List<string>();
            try
            {
                libIndex = clientLibraryList.FindIndex(x => x.LibraryName.Equals(libraryName));
                string getPath = clientLibraryList[libIndex].VssLibPath.Remove(clientLibraryList[libIndex].VssLibPath.Length - 1); //@"$/PROD/PB105/iapply/ALPHA/SE/iApply/admflow";
                string objects = "";
                var vssContent = File.ReadAllText(textPath);
                if (vssContent.Contains(getPath + ':'))
                {
                    objects = Regex.Split(vssContent, ("\r\n\r\n")).FirstOrDefault(x => x.StartsWith(getPath));
                    objectList = objects.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    objectList.RemoveAll(line => objectList.Any(s => line.StartsWith("$")));
                    objectList.RemoveAll(line => objectList.Any(s => line.EndsWith(".pbg")));
                }
            }
            catch (Exception ex)
            {

                logger.LogError("GetObjectsFromPbg: " + ex.Message);
            }
            
            return objectList;
        }

        public void GetPBTFile (string path, string pbtFile)
        {
            try
            {
                sourceSafeControl.Connect(applicationParams.Username, applicationParams.Password);
                sourceSafeControl.SetWorkingFolder(workspaceParams.VssProjectPath, applicationParams.TargetPath);
                sourceSafeControl.GetLatestFile(path, pbtFile, applicationParams.TempPath);
                sourceSafeControl.Disconnect();
                logger.LogInfo(pbtFile + " downloaded! ");
            }
            catch (Exception ex)
            {

                logger.LogError("GetPBTFile: " + ex.Message);
            }
            
        }

        public bool CheckObjectsStatus(string vssProject)
        {
            sourceSafeControl.Connect(applicationParams.Username, applicationParams.Password);
            sourceSafeControl.SetWorkingFolder(workspaceParams.VssProjectPath, applicationParams.TargetPath);
            var checkedOutObjectsExist = sourceSafeControl.CheckOutObjectsExist(vssProject);
            if (checkedOutObjectsExist)
            {
                logger.LogInfo("Checkout Objects Exist!");
            }
            return checkedOutObjectsExist;
        }

        public List<PBLibrary> GetLibrarylist(string applicationId, string targetPath, string targetFile)
        {
            List<string> LibList, LibItem;
            string libNameWithExt, libName, vssLIbraryPath, libFullPath, common = "common", iapply = "iapply", libCommon, localFullPath;
            string pbtLiblist = "";
            char quotes = '"';
            bool isApplicationLibrary = false;
            string extLibPlusLibName, extLibPlusLibNameWithExt, libraryResolvedPath;

            try
            {
                GetPBTFile(targetPath, targetFile);
                var targetContent = File.ReadAllText(applicationParams.TempPath + "/" + targetFile);
                pbtLiblist = Regex.Split(targetContent, @"LibList|liblist").Last().Split(new string[] { @";\r\ntype" }, StringSplitOptions.RemoveEmptyEntries).First();
                string[] pathLevelPattern = { "..\\" };
                string PBTfilename = String.Concat(targetPath, targetFile);

                LibList = pbtLiblist.Split(';').ToList();
                for (int i = 0; i < LibList.Count; i++)
                {

                    LibItem = LibList[i].Split(pathLevelPattern, StringSplitOptions.RemoveEmptyEntries).ToList();
                    string basePath = applicationParams.TargetPath + "\\";
                    int pathLevels = Regex.Matches(LibList[i], "[.][.]\\\\").Count;
                    if (Regex.Matches(LibList[i], "[.][.]\\\\").Count > 0)
                    {
                        for (int level = 0; level < pathLevels; level++)
                        {
                            basePath = basePath.Substring(0, basePath.LastIndexOf('\\'));
                        }
                    }

                    if (targetFile == workspaceParams.LibraryTargetPath) isApplicationLibrary = true;
                    var targetPathList = targetPath.Split('/').ToList().Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                    var libWithExtension = LibList[i].Replace("\\\\", "-").Replace('"', '/').Replace("/", "").Replace('"', ' ').Split('-').LastOrDefault();
                    libNameWithExt = String.IsNullOrWhiteSpace(LibList[i].Split('\\').Last().ToString().Split('"').Last()) ? Regex.Split(LibList[i], ("\\ ")).Last().ToString().Split(quotes).ToList().Distinct().LastOrDefault() : LibList[i].Split('\\').Last().ToString().Split('"').Last();
                    libNameWithExt = String.IsNullOrWhiteSpace(libNameWithExt) ? libWithExtension : libNameWithExt;
                    libName = isApplicationLibrary ? String.IsNullOrWhiteSpace(libNameWithExt.Split('.').First().ToString()) ? libNameWithExt.Split('.').First() : libNameWithExt.Split('.').First().ToString() : targetPathList.Last();
                    libCommon = isApplicationLibrary ? (LibList[i].Contains(common) ? common + @"\" + libName + @"\" : iapply + @"\" + libName + @"\") : targetPathList.Take(targetPathList.Count() - 1).LastOrDefault() + @"\";
                    libFullPath = applicationParams.TempPath + @"\" + libCommon + @"\" + @libNameWithExt;
                    extLibPlusLibName = LibList[i].Contains(common) ? common + @"\" + libName + @"\" : iapply + @"\" + libName + @"\";
                    extLibPlusLibNameWithExt = extLibPlusLibName + libNameWithExt;
                    libraryResolvedPath = applicationParams.TempPath + libName + @"\" + libNameWithExt;
                    vssLIbraryPath = (workspaceParams.VssProjectPath + extLibPlusLibName).Replace("\\", "/");
                    localFullPath = workspaceParams.LocalVssWorkingPath + extLibPlusLibNameWithExt.Replace("\\","/");
                     libIndex = i;
                    
                     if (workspaceParams.EasLibNeeded == "1")
                     {
                        if (libNameWithExt.ToUpper().EndsWith("PBL") || (libNameWithExt.ToUpper().EndsWith("PBD")))
                            this.libraryList.Add(new PBLibrary(libName, libNameWithExt, libraryResolvedPath, workspaceParams.VssProjectPath,
                                                               workspaceParams.VssProductPath, vssLIbraryPath, libFullPath, extLibPlusLibName,
                                                               extLibPlusLibNameWithExt, workspaceParams.LocalVssWorkingPath, libIndex, 
                                                               isApplicationLibrary,localFullPath));
                     }

                     if (workspaceParams.EasLibNeeded != "1")
                     {
                         if (isApplicationLibrary)
                         {
                             if (libNameWithExt.ToUpper().EndsWith("PBL") || (libNameWithExt.ToUpper().EndsWith("PBD")))
                                 this.libraryList.Add(new PBLibrary(libName, libNameWithExt, libraryResolvedPath, workspaceParams.VssProjectPath,
                                                                    workspaceParams.VssProductPath, vssLIbraryPath, libFullPath, extLibPlusLibName,
                                                                    extLibPlusLibNameWithExt, workspaceParams.LocalVssWorkingPath, libIndex,
                                                                    isApplicationLibrary, localFullPath));
                         }
                     }

                        if ((libNameWithExt.ToUpper().EndsWith("PBL") || (libNameWithExt.ToUpper().EndsWith("PBD"))) && (!isApplicationLibrary))
                            this.appServerLibraryList.Add(new PBLibrary(libName, libNameWithExt, libraryResolvedPath, workspaceParams.VssProjectPath,
                                                               workspaceParams.VssProductPath, vssLIbraryPath, libFullPath, extLibPlusLibName,
                                                               extLibPlusLibNameWithExt, workspaceParams.LocalVssWorkingPath, libIndex,
                                                               isApplicationLibrary, localFullPath));

                    if ((libNameWithExt.ToUpper().EndsWith("PBL")) && (isApplicationLibrary))
                        this.clientLibraryList.Add(new PBLibrary(libName, libNameWithExt, libraryResolvedPath, workspaceParams.VssProjectPath,
                                                           workspaceParams.VssProductPath, vssLIbraryPath, libFullPath, extLibPlusLibName,
                                                           extLibPlusLibNameWithExt, workspaceParams.LocalVssWorkingPath, libIndex, 
                                                           isApplicationLibrary,localFullPath));


                }
                libraryList = libraryList.OrderByDescending(x => x.LibraryNameWithExtension.Split('.').LastOrDefault()).ToList();
            }
            catch (Exception ex)
            {

                logger.LogInfo("GetLibrarylist: " + ex.StackTrace + "-" + ex.Message);
            }
            
            return libraryList; ;
        }

        private void GetSourceSafeDB(string project)
        {
            try
            {
                var message = "";
                Commander.Exec(applicationParams.TargetPath, "vss_GetDb.bat", "", ref message);
                logger.LogInfo("GetSourceSafeDB: " + message);
            }
            catch (Exception ex)
            {

                logger.LogInfo("GetSourceSafeDB: " + ex.Message);
            }

        }

        public List<PBLibrary> LibraryList(string applicationId, string targetFile)
        {
            return libraryList;
        }

        public List<PBLibrary> AppServerLibraryList(string applicationId, string targetFile)
        {
            return appServerLibraryList;
        }

        public List<PBLibrary> ClientLibraryList(string applicationId, string targetFile)
        {
            return clientLibraryList;
        }

        public WorkspaceParams WorkspaceParams(string applicationId)
        {
            return this.workspaceParams;
        }

        public bool GetLatestProject(string projectPath, string targetPath)
        {
            var check = true;
            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
            //sourceSafeControl.Connect(applicationParams.Username, applicationParams.Password);
            sourceSafeControl.SetWorkingFolder(projectPath, applicationParams.TargetPath);
            check = sourceSafeControl.GetLatest(projectPath, targetPath);
            if (sourceSafeControl.IsConnected(applicationParams.Username, applicationParams.Password))sourceSafeControl.Disconnect();
            GC.Collect();
            return check;
        }

        public void AddItemToProject(string vssProject, string localProject, string vssObject, string comment)
        {
            //sourceSafeControl.Connect(applicationParams.Username, applicationParams.Password);
            sourceSafeControl.SetWorkingFolder(workspaceParams.VssProductPath, localProject);
            sourceSafeControl.AddItemToProject(vssProject, localProject,vssObject, comment);
        }

        public void RemoveItemFromProject(string vssObject)
        {
            //sourceSafeControl.Connect(applicationParams.Username, applicationParams.Password);
            sourceSafeControl.SetWorkingFolder(workspaceParams.VssProductPath, applicationParams.TargetPath);
            sourceSafeControl.RemoveItemFromProject(vssObject);
        }

        public List<PBLibrary> GetLocalLibrarylist(string applicationId, string targetPath, string targetFile)
        {
            List<string> LibList, LibItem;
            string libNameWithExt, libName, vssLIbraryPath, libFullPath, common = "common", iapply = "iapply", libCommon, localFullPath;
            string pbtLiblist = "";
            char quotes = '"';
            bool isApplicationLibrary = false;
            string extLibPlusLibName, extLibPlusLibNameWithExt, libraryResolvedPath;

            try
            {
                var targetContent = File.ReadAllText(targetPath + "/" + targetFile);
                pbtLiblist = Regex.Split(targetContent, @"LibList|liblist").Last().Split(new string[] { @";\r\ntype" }, StringSplitOptions.RemoveEmptyEntries).First();
                string[] pathLevelPattern = { "..\\" };
                string PBTfilename = String.Concat(targetPath, targetFile);

                LibList = pbtLiblist.Split(';').ToList();
                for (int i = 0; i < LibList.Count; i++)
                {

                    LibItem = LibList[i].Split(pathLevelPattern, StringSplitOptions.RemoveEmptyEntries).ToList();
                    string basePath = applicationParams.TargetPath + "\\";
                    int pathLevels = Regex.Matches(LibList[i], "[.][.]\\\\").Count;
                    if (Regex.Matches(LibList[i], "[.][.]\\\\").Count > 0)
                    {
                        for (int level = 0; level < pathLevels; level++)
                        {
                            basePath = basePath.Substring(0, basePath.LastIndexOf('\\'));
                        }
                    }

                    if (targetFile == workspaceParams.LibraryTargetPath) isApplicationLibrary = true;
                    var targetPathList = targetPath.Split('/').ToList().Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                    var libWithExtension = LibList[i].Replace("\\\\", "-").Replace('"', '/').Replace("/", "").Replace('"', ' ').Split('-').LastOrDefault();
                    libNameWithExt = String.IsNullOrWhiteSpace(LibList[i].Split('\\').Last().ToString().Split('"').Last()) ? Regex.Split(LibList[i], ("\\ ")).Last().ToString().Split(quotes).ToList().Distinct().LastOrDefault() : LibList[i].Split('\\').Last().ToString().Split('"').Last();
                    libNameWithExt = String.IsNullOrWhiteSpace(libNameWithExt) ? libWithExtension : libNameWithExt;
                    libName = isApplicationLibrary ? String.IsNullOrWhiteSpace(libNameWithExt.Split('.').First().ToString()) ? libNameWithExt.Split('.').First() : libNameWithExt.Split('.').First().ToString() : targetPathList.Last();
                    libCommon = isApplicationLibrary ? (LibList[i].Contains(common) ? common + @"\" + libName + @"\" : iapply + @"\" + libName + @"\") : targetPathList.Take(targetPathList.Count() - 1).LastOrDefault() + @"\";
                    libFullPath = applicationParams.TempPath + @"\" + libCommon + @"\" + @libNameWithExt;
                    extLibPlusLibName = LibList[i].Contains(common) ? common + @"\" + libName + @"\" : iapply + @"\" + libName + @"\";
                    extLibPlusLibNameWithExt = extLibPlusLibName + libNameWithExt;
                    libraryResolvedPath = applicationParams.TempPath + libName + @"\" + libNameWithExt;
                    vssLIbraryPath = (workspaceParams.VssProjectPath + extLibPlusLibName).Replace("\\", "/");
                    localFullPath = workspaceParams.LocalVssWorkingPath + extLibPlusLibNameWithExt.Replace("\\", "/");
                    libIndex = i;

                    if (workspaceParams.EasLibNeeded == "1")
                    {
                        if (libNameWithExt.ToUpper().EndsWith("PBL") || (libNameWithExt.ToUpper().EndsWith("PBD")))
                            this.libraryList.Add(new PBLibrary(libName, libNameWithExt, libraryResolvedPath, workspaceParams.VssProjectPath,
                                                               workspaceParams.VssProductPath, vssLIbraryPath, libFullPath, extLibPlusLibName,
                                                               extLibPlusLibNameWithExt, workspaceParams.LocalVssWorkingPath, libIndex,
                                                               isApplicationLibrary, localFullPath));
                    }

                    if (workspaceParams.EasLibNeeded != "1")
                    {
                        if (isApplicationLibrary)
                        {
                            if (libNameWithExt.ToUpper().EndsWith("PBL") || (libNameWithExt.ToUpper().EndsWith("PBD")))
                                this.libraryList.Add(new PBLibrary(libName, libNameWithExt, libraryResolvedPath, workspaceParams.VssProjectPath,
                                                                   workspaceParams.VssProductPath, vssLIbraryPath, libFullPath, extLibPlusLibName,
                                                                   extLibPlusLibNameWithExt, workspaceParams.LocalVssWorkingPath, libIndex,
                                                                   isApplicationLibrary, localFullPath));
                        }
                    }

                    if ((libNameWithExt.ToUpper().EndsWith("PBL") || (libNameWithExt.ToUpper().EndsWith("PBD"))) && (!isApplicationLibrary))
                        this.appServerLibraryList.Add(new PBLibrary(libName, libNameWithExt, libraryResolvedPath, workspaceParams.VssProjectPath,
                                                           workspaceParams.VssProductPath, vssLIbraryPath, libFullPath, extLibPlusLibName,
                                                           extLibPlusLibNameWithExt, workspaceParams.LocalVssWorkingPath, libIndex,
                                                           isApplicationLibrary, localFullPath));

                    if ((libNameWithExt.ToUpper().EndsWith("PBL")) && (isApplicationLibrary))
                        this.clientLibraryList.Add(new PBLibrary(libName, libNameWithExt, libraryResolvedPath, workspaceParams.VssProjectPath,
                                                           workspaceParams.VssProductPath, vssLIbraryPath, libFullPath, extLibPlusLibName,
                                                           extLibPlusLibNameWithExt, workspaceParams.LocalVssWorkingPath, libIndex,
                                                           isApplicationLibrary, localFullPath));


                }
                libraryList = libraryList.OrderByDescending(x => x.LibraryNameWithExtension.Split('.').LastOrDefault()).ToList();
            }
            catch (Exception ex)
            {

                logger.LogInfo("GetLocalLibrarylist: " + ex.StackTrace + "-" + ex.Message);
            }

            return libraryList; ;
        }
        
        public void ListCheckOutObjects()
        {
            sourceSafeControl.CheckOutObjectsExist(workspaceParams.VssProjectPath);
        }
    }
}
