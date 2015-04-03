using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Relational.Octapus.Persistence;
using Relational.Octapus.Workspace;
using System.IO;
using Relational.Octapus.Common;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Net;

namespace Relational.Octapus.BuildEngine
{
    public class BuildTask
    {
        private BuildTaskParameters buildTaskParameters;
        private OctapusLog logger;
        private List<PBLibrary> libraryList;
        private List<PBLibrary> clientLibraryList;
        private WorkspaceParams workspaceParams;
        private ApplicationParams applicationParams;
        private VersionParams versionParams;
        private IWorkspaceProvider workspace;
        string message = "";

        public BuildTask(BuildTaskParameters buildTaskParameters, IDataManager dataManager, IWorkspaceProvider workspace, IHookOctapusLogger hookLogger = null)
        {
            this.buildTaskParameters = buildTaskParameters;
            this.dataManager = dataManager;
            versionParams = dataManager.GetVersionParams(buildTaskParameters.ApplicationId);
            applicationParams = dataManager.GetApplicationParams();
            workspaceParams = dataManager.GetWorkspaceParams(buildTaskParameters.ApplicationId);
            this.workspace = workspace;
            this.libraryList = workspace.LibraryList(buildTaskParameters.ApplicationId,workspaceParams.LibraryTargetPath);
            this.clientLibraryList = workspace.ClientLibraryList(buildTaskParameters.ApplicationId, workspaceParams.LibraryTargetPath);
            logger = new OctapusLog("BuildEngine", hookLogger);
            logger.LogInfo(String.Concat("BuildEngine Task Initialized for application:", this.buildTaskParameters.ApplicationId));
        }

        public BuildTaskParameters BuildTaskParameters
        {
            get { return buildTaskParameters; }
            set { buildTaskParameters = value; }
        }

        public IDataManager dataManager { get; set; }

       private void ImplantLibrary(string path)
        {
            try
            {
                for (int i = 0; i < this.libraryList.Count(); i++)
                {
                    Commander.Exec(applicationParams.TargetPath, "bversion.exe", "put " + this.LibraryList[i].LibraryResolvedPath, ref message);
                    logger.LogInfo("Implant Library " + this.LibraryList[i].LibraryResolvedPath + " " + message);
                }
            }
            catch (Exception ex)
            {

                logger.LogInfo("ImplantLibrary: " + ex.Message + "LineNumber: " + Misc.GetLineNumber(ex));
            }
            
        }

       public bool VerifyVersion(string path)
       {
           try
           {
               logger.LogInfo("VerifyVersion: ");

               foreach (string file in Directory.GetFiles(path, "*.pbd", SearchOption.TopDirectoryOnly))
               {
                    Commander.Exec(applicationParams.TargetPath, "bversion.exe", "get " + Path.Combine(path, file), ref message);
                    logger.LogInfo("Version Verification of Library " + Path.GetFileName(file) + " " + message);
               }
               return true;

           }
           catch (Exception ex)
           {

               logger.LogInfo("VerifyVersion: " + ex.Message + "LineNumber: " + Misc.GetLineNumber(ex));
               return false;
           }

       }

       public bool GetLatest()
        {
            var result = true;
            try
            {
                var vssContentFile = Path.Combine(applicationParams.TargetPath, Path.GetFileName(applicationParams.VssContent));
                if (File.Exists(vssContentFile)) File.Delete(vssContentFile);

                var localWorkingPathNoBlanks = Regex.Replace(workspaceParams.LocalVssWorkingPath, @"\s+", "");

                Commander.Execute(applicationParams.TargetPath, Path.Combine(applicationParams.TargetPath,"vss_GetDb.bat"),
                localWorkingPathNoBlanks.Remove(localWorkingPathNoBlanks.Length - 1) + 
                " " + workspaceParams.VssProjectPath.Remove(workspaceParams.VssProjectPath.Length-1) + " " +applicationParams.Username + " " + applicationParams.Password, ref message);

                logger.LogInfo("VSS DB Downloaded");

                //workspace.CheckWorkspacePbgs(buildTaskParameters.ApplicationId);

                CreateGetLatestScript();

                logger.LogInfo("GetLatestScript Created");

                logger.LogInfo("Getting Latest Code");

                if (!Directory.Exists(applicationParams.LogPath)) Directory.CreateDirectory(applicationParams.LogPath);

                /*if ((!workspace.GetLatestProject(workspaceParams.VssProductPath, workspaceParams.LocalVssProductPath)) && (!workspace.GetLatestProject(workspaceParams.VssCommonPath, workspaceParams.LocalVssCommonPath)))
                {
                    logger.LogInfo("GetLatest Code Failed");
                    result = false;
                    return result;
                }
                else
                {*/
                    if (Commander.Exec(applicationParams.TargetPath, Path.Combine(applicationParams.TargetPath, "getLatest.bat"), " ", ref message))
                    {
                        logger.LogInfo("GetLatest Code Complete");
                    }
                    else
                    {
                        logger.LogInfo("GetLatest Code Failed");
                        result = false;
                        return result;
                    }
                //}
            }
        
            catch (Exception ex)
            {

                logger.LogInfo("GetLatest:" + ex.Message + "LineNumber: " + Misc.GetLineNumber(ex));
            }

            return result;
        }


       private bool PrepareBuild(BuildMode buildMode, string applicationId, string pbdFiles = null)
        {
            var buildFolder = buildTaskParameters.BuildPrefix + versionParams.BuildSeqNo;
            var buildClientFolder = Path.Combine(buildTaskParameters.BuildsFolder, Path.Combine(buildFolder, buildTaskParameters.BuildClientFolderName));
            var buildPartialClientFolder = Path.Combine(buildTaskParameters.BuildsFolder, Path.Combine(buildFolder, buildTaskParameters.BuildPartialClientFolderName));
            var scriptFolder = Path.Combine(buildTaskParameters.BuildsFolder, buildFolder, buildTaskParameters.BuildScriptFolderName);
            var buildSourceCodeFolder = Path.Combine(buildTaskParameters.BuildsFolder, Path.Combine(buildFolder, buildTaskParameters.BuildSourceCodeFolderName));
            var product = buildTaskParameters.Productname.Replace("-", "");
            var canCreateWorkableClient = buildTaskParameters.PrepareWorkableClient;
            
            try
            {

                logger.LogInfo(applicationId + " Creating Data Script");

                InsertBuildData(buildMode, versionParams.BuildNumber, ref pbdFiles);

                logger.LogInfo(applicationId + " Copying Source Code " );

                Packager.PreparePackage(versionParams.BuildSeqNo, buildTaskParameters.BuildsFolder, applicationParams.TempPath, workspaceParams.LocalVssProductPath,
                                     workspaceParams.LocalVssCommonPath, workspaceParams.ClientNecFilesPath,
                                     buildTaskParameters.BuildOutFolder, product, buildTaskParameters.ProjectResourceFile, buildFolder, buildClientFolder,
                                     buildSourceCodeFolder, scriptFolder, buildPartialClientFolder, workspaceParams.LocalPBTPath, canCreateWorkableClient);
                
                logger.LogInfo(applicationId + " Creating Version Info ");
                if (!CreateVersionInfo(versionParams.BuildNumber, workspaceParams)) return false;
                
                logger.LogInfo(applicationId + " Implant ");
                ImplantLibrary(buildTaskParameters.BuildOutFolder);

                Packager.CopyLibraries(applicationParams.TempPath, buildTaskParameters.BuildOutFolder);
                GC.Collect();

                logger.LogInfo("Creating OrcaScript");
                BuildOrcaScript();

                return true;
            }
            catch (Exception ex)
            {

                logger.LogInfo("PrepareBuild" + ex.Message + "LineNumber: " + Misc.GetLineNumber(ex));
                return false;
            }
            

        }

        public void Build(BuildMode buildMode, string pbdFiles = null)
        {
            var buildFolder = buildTaskParameters.BuildPrefix + versionParams.BuildSeqNo;
            var buildClientFolder = Path.Combine(buildTaskParameters.BuildsFolder, Path.Combine(buildFolder, buildTaskParameters.BuildClientFolderName));
            var buildPartialClientFolder = Path.Combine(buildTaskParameters.BuildsFolder, Path.Combine(buildFolder, buildTaskParameters.BuildPartialClientFolderName));
            var buildSourceCodeFolder = Path.Combine(buildTaskParameters.BuildsFolder, Path.Combine(buildFolder, buildTaskParameters.BuildSourceCodeFolderName));
            var logsFolder = Path.Combine(buildTaskParameters.BuildsFolder, Path.Combine(buildFolder, buildTaskParameters.BuildLogFolderName));
            
                try
                {
                    if (!Commander.DiskHasFreeSpace((applicationParams.TempPath.Substring(0, 2)), applicationParams.DiskSpaceNeededInMb))
                    {
                        logger.LogInfo("Not available Free Disk Space!");
                        return;
                    }

                    if (!GetLatest()) return;
                    
                    logger.LogInfo("Preparing Build Environment " );
                    if (!PrepareBuild(buildMode, buildTaskParameters.ApplicationId, pbdFiles)) return;

                    logger.LogInfo("Build Application Started ");

                    if (Commander.Exec(applicationParams.TargetPath, Path.Combine(applicationParams.TargetPath, "buildPB.bat") + " ", versionParams.BuildNumber, ref message))
                    {
                        logger.LogInfo("Build Application Completed");
                    }
                    else
                    {
                        logger.LogInfo("Build Application Failed");
                    }

                    logger.LogInfo("Packaging");
                    Packager.Package(applicationParams.TempPath, buildSourceCodeFolder, buildClientFolder,
                                         buildTaskParameters.BuildOutFolder, versionParams.BuildSeqNo);

                    if (buildMode == BuildMode.SpecificPbds)
                    {
                        if (!Directory.Exists(buildPartialClientFolder)) Directory.CreateDirectory(buildPartialClientFolder);
                        Packager.CopySpecificPbd(buildTaskParameters.BuildOutFolder, buildPartialClientFolder, pbdFiles);
                    }

                    dataManager.SetNextBuildNo(buildTaskParameters.ApplicationId);
                    
                    if (!Directory.Exists(logsFolder)) Directory.CreateDirectory(logsFolder);
                    Packager.CopyFolder(applicationParams.LogPath, logsFolder);
                    
                    logger.LogInfo("GetParameters");
                    dataManager.GetPropertyList(logsFolder, buildTaskParameters);
                    dataManager.GetPropertyList(logsFolder, workspaceParams);
                    dataManager.GetPropertyList(logsFolder, applicationParams);
                    dataManager.GetPropertyList(logsFolder, versionParams);
                    
                    logger.LogInfo("Verifying Version Information");
                    if (!VerifyVersion(buildClientFolder)) return;

                }
                catch (Exception ex)
                {
                    logger.LogInfo("Build Process: " + ex.Message + "LineNumber: " + Misc.GetLineNumber(ex));
                }
            
        }


        private bool CreateVersionInfo(string buildNumber,WorkspaceParams workspaceParams)
        {
            try
            {
                buildNumber = String.IsNullOrWhiteSpace(buildNumber) ? versionParams.BuildNumber : buildNumber;
                var buildSeqNo = dataManager.GetNextBuildNo(buildTaskParameters.ApplicationId).ToString();
                var buildDate = String.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                var userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                var computer = System.Environment.MachineName;
                var hostName = versionParams.ClientHostName;
                var domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;

                StringBuilder stringBuilder = new StringBuilder();
                string path = AppDomain.CurrentDomain.BaseDirectory + "/VersionInfo.ini";
                string xmlPath = AppDomain.CurrentDomain.BaseDirectory + "/VersionInfo.xml";
                var header = "[VersionInfo]";
                XmlDocument document = new XmlDocument();

                document.Load(AppDomain.CurrentDomain.BaseDirectory + "/VersionInfo.xml");
                document.DocumentElement.SelectSingleNode(buildTaskParameters.ApplicationId + "/BuildNumber").InnerText = buildNumber;
                document.DocumentElement.SelectSingleNode(buildTaskParameters.ApplicationId + "/BuildSeqNo").InnerText = buildSeqNo;
                document.DocumentElement.SelectSingleNode(buildTaskParameters.ApplicationId + "/BuildDate").InnerText = buildDate;
                document.DocumentElement.SelectSingleNode(buildTaskParameters.ApplicationId + "/Username").InnerText = userName;
                document.DocumentElement.SelectSingleNode(buildTaskParameters.ApplicationId + "/Computer").InnerText = computer;
                document.DocumentElement.SelectSingleNode(buildTaskParameters.ApplicationId + "/ClientHostName").InnerText = hostName;
                document.DocumentElement.SelectSingleNode(buildTaskParameters.ApplicationId + "/Domain").InnerText = domain;
                document.Save(xmlPath);
                document.DocumentElement.InnerXml = document.SelectSingleNode("//" + buildTaskParameters.ApplicationId).InnerXml;
                document.Save(path);

                if (workspaceParams.ConfigurationMode.Equals(ConfigurationMode.DB))
                {
                    string cmd = @"update pbbuild_config set value='" + buildNumber + "' where name ='BuildNumber' and application_id='" + buildTaskParameters.ApplicationId + "'";
                    cmd += @"update pbbuild_config set value='" + buildSeqNo + "' where name ='BuildSeqNo' and application_id='" + buildTaskParameters.ApplicationId + "'";
                    cmd += @"update pbbuild_config set value='" + buildDate + "' where name ='BuildDate' and application_id='" + buildTaskParameters.ApplicationId + "'";
                    cmd += @"update pbbuild_config set value='" + userName + "' where name ='Username' and application_id='" + buildTaskParameters.ApplicationId + "'";
                    cmd += @"update pbbuild_config set value='" + computer + "' where name ='Computer' and application_id='" + buildTaskParameters.ApplicationId + "'";
                    cmd += @"update pbbuild_config set value='" + hostName + "' where name ='ClientHostName' and application_id='" + buildTaskParameters.ApplicationId + "'";
                    cmd += @"update pbbuild_config set value='" + domain + "' where name ='Domain' and application_id='" + buildTaskParameters.ApplicationId + "'";

                    DBConnection dbConnection = new DBConnection(DBMS.Sybase);
                    dbConnection.Execute(workspaceParams, cmd, logger, dbConnection);
                    
                }
 
                var lines = File.ReadAllLines(path);
                File.WriteAllLines(path, lines);
                File.WriteAllLines(path, lines.Skip(1).Take(lines.Length - 2));
                File.WriteAllText(path, Regex.Replace(File.ReadAllText(path), "<versionInfo>", header));

                using (StreamReader streamReader = new StreamReader(path))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        var fixedLine = Regex.Split(line, ("</")).First().Split('<').Last().Replace('>', '=').Trim();
                        stringBuilder.AppendLine(fixedLine + @"");
                    }
                }
                File.WriteAllText(path, stringBuilder.ToString());
                return true;
            }
            catch (Exception ex)
            {

                logger.LogInfo("CreateVersionInfo: " + ex.Message + "LineNumber: " + Misc.GetLineNumber(ex));
                return false;
            }
            
        }

        private void BuildOrcaScript()
        {
            try
            {
                string session = "start session", sourceFolder = "set SourceFolder=", setApplicationLibraryList = "set ApplicationLibraryList =",
            applicationLibListFinal = "set ApplicationLibraryList  +=", setLibList = "set liblist ApplicationLibraryList",
            setApplication = "set application", buildApplication = "build application full", buildLibrary = "build library",
            buildExecutable = "build executable", changeLine = "\r\n", orcaScriptName = "buildApplication.orc",
            path = applicationParams.TargetPath + @"\" + orcaScriptName, changeLines = changeLine + changeLine,
            setApplicationLibListSourceFolder = "set ApplicationLibraryList  += SourceFolder",
            space = " ", pbdExtension = " PBD", pblFlags = "", exeInfoProperty = "set exeinfo property ", exeInfoCompany = "companyname",
            exeInfoProductName = "productname", exeInfoCopyright = "copyright", exeInfoDescription = "description",
            exeInfoFileVersion = "fileversion", exeInfoProductVersion = "productversion", exeInfoFileVersionNum = "fileversionnum",
            exeInfoProductVersionNum = "productversionnum";
                char quotes = '"', questionMark = ';';

                File.WriteAllText(path, session + changeLines + sourceFolder + quotes + buildTaskParameters.BuildOutFolder + quotes +
                                  changeLines + setApplicationLibraryList + quotes + quotes + changeLines);

                for (int i = 0; i < this.libraryList.Count(); i++)
                {
                    File.AppendAllText(path, setApplicationLibListSourceFolder + changeLine);
                    File.AppendAllText(path, applicationLibListFinal + space + quotes + this.LibraryList[i].LibraryNameWithExtension +
                                       questionMark + quotes + space + changeLine);
                    if ((this.LibraryList[i].IsApplicationLibrary) && (this.LibraryList[i].LibraryNameWithExtension.EndsWith(".pbl")))
                    {
                        pblFlags += "y";
                    }
                    else
                    {
                        pblFlags += "n";
                    }
                }

                File.AppendAllText(path, changeLine + setLibList + changeLines + setApplication + space + quotes +
                                   buildTaskParameters.BuildOutFolder + buildTaskParameters.ExecutableLibrary + quotes + space +
                                   quotes + buildTaskParameters.ApplicationObject + quotes + changeLines + buildApplication +
                                   changeLines);

                for (int i = 0; i < this.clientLibraryList.Count(); i++)
                {
                    if (this.ClientLibraryList[i].LibraryNameWithExtension.ToUpper().EndsWith(".PBL"))
                    {
                        File.AppendAllText(path, buildLibrary + space + quotes + buildTaskParameters.BuildOutFolder +
                            this.ClientLibraryList[i].LibraryNameWithExtension + quotes + space + quotes + quotes +
                                           space + pbdExtension + changeLine);
                    }
                }

                File.AppendAllText(path, changeLine + exeInfoProperty + exeInfoCompany + space + quotes + buildTaskParameters.Companyname + quotes + changeLine);
                File.AppendAllText(path, exeInfoProperty + exeInfoProductName + space + quotes + buildTaskParameters.Productname + quotes + changeLine);
                File.AppendAllText(path, exeInfoProperty + exeInfoCopyright + space + quotes + buildTaskParameters.Copyright + quotes + changeLine);
                File.AppendAllText(path, exeInfoProperty + exeInfoDescription + space + quotes + buildTaskParameters.Description + quotes + changeLine);
                File.AppendAllText(path, exeInfoProperty + exeInfoFileVersion + space + quotes + buildTaskParameters.Fileversion + quotes + changeLine);
                File.AppendAllText(path, exeInfoProperty + exeInfoFileVersionNum + space + quotes + buildTaskParameters.Fileversionnum + quotes + changeLine);
                File.AppendAllText(path, exeInfoProperty + exeInfoProductVersion + space + quotes + buildTaskParameters.Productversion + quotes + changeLine);
                File.AppendAllText(path, exeInfoProperty + exeInfoProductVersionNum + space + quotes + buildTaskParameters.Productversionnum + quotes + changeLine);

                File.AppendAllText(path, changeLine + buildExecutable + space + quotes + buildTaskParameters.BuildOutFolder +
                                   buildTaskParameters.EXEfilename + quotes + space + quotes + buildTaskParameters.BuildOutFolder +
                                   buildTaskParameters.EXEico + quotes + space + quotes + buildTaskParameters.BuildOutFolder +
                                   buildTaskParameters.ProjectResourceFile + quotes + space + quotes + pblFlags + quotes);
            }
            catch (Exception ex)
            {

                logger.LogInfo("BuildOrcaScript: " + ex.Message + "LineNumber: " + Misc.GetLineNumber(ex));
            }
            
            
        }

        private void GetLatestScript()
        {
            try
            {
                string path = applicationParams.TargetPath + "/getLatest.orc";
                string startSession = "start session", getConnect = "scc get connect properties ", changeLine = "\r\n",
                setConnectPropertyUname = "scc set connect property userid ", setConnectPropertyPass = "scc set connect property password ",
                sccConnect = "scc connect", sccSetTarget = "scc set target ", sccRefreshTarget = "scc refresh target ", mode = "FULL",
                sccClose = "scc close", endSession = "end session", getLatestVersion = "scc get latest version ", comments = ";;";
                char quotes = '"';

                File.WriteAllText(path, "");
                File.WriteAllText(path, startSession + changeLine + getConnect + quotes + workspaceParams.LocalVssWorkingPath.Replace("/", @"\") +
                                  workspaceParams.Pbw + quotes + changeLine + setConnectPropertyUname + quotes + applicationParams.Username +
                                  quotes + changeLine + setConnectPropertyPass + quotes + quotes + changeLine + sccConnect + changeLine + changeLine);

                for (int i = 0; i < this.libraryList.Count(); i++)
                {
                    if ((this.LibraryList[i].IsApplicationLibrary) && this.LibraryList[i].LibraryNameWithExtension.ToUpper().EndsWith(".PBL"))
                    {
                        File.AppendAllText(path, getLatestVersion + quotes + this.LibraryList[i].LocalFullPath + quotes + changeLine);
                    }
                }

                File.AppendAllText(path, changeLine + comments + sccSetTarget +
                                 quotes + workspaceParams.LocalPBTPath.Replace("/", @"\") + workspaceParams.LibraryTargetPath + quotes + changeLine +
                                 changeLine + comments + sccRefreshTarget + quotes + mode + quotes + changeLine + changeLine + sccClose + changeLine +
                                 changeLine + endSession);
            }
            catch (Exception ex)
            {

                logger.LogInfo("GetLatestScript: " + ex.Message + "LineNumber: " + Misc.GetLineNumber(ex));
            }
            


        }

        private void CreateGetLatestScript()
        {
            try
            {
                string path = applicationParams.TargetPath + "/getLatest.orc";
                string startSession = "start session", getConnect = "scc get connect properties ", changeLine = "\r\n",
                setConnectPropertyUname = "scc set connect property userid ", setConnectPropertyPass = "scc set connect property password ",
                sccConnect = "scc connect", sccSetTarget = "scc set target ", sccRefreshTarget = "scc refresh target ", mode = "FULL",
                sccClose = "scc close", endSession = "end session";
                char quotes = '"';

                File.WriteAllText(path, "");
                File.WriteAllText(path, startSession + changeLine + getConnect + quotes + workspaceParams.LocalVssWorkingPath.Replace("/", @"\") +
                                  workspaceParams.Pbw + quotes + changeLine + setConnectPropertyUname + quotes + applicationParams.Username +
                                  quotes + changeLine + setConnectPropertyPass + quotes + quotes + changeLine + sccConnect + changeLine + sccSetTarget +
                                  quotes + workspaceParams.LocalPBTPath.Replace("/", @"\") + workspaceParams.LibraryTargetPath + quotes + changeLine +
                                  changeLine + sccRefreshTarget + quotes + mode + quotes + changeLine + changeLine + sccClose + changeLine +
                                  changeLine + endSession);
            }
            catch (Exception ex)
            {

                logger.LogInfo("CreateGetLatestScript: " + ex.Message + "LineNumber: " + Misc.GetLineNumber(ex));
            }
            



        }

        private void InsertBuildData(BuildMode buildMode, string buildNumber, ref string pbdFiles)
        {
            try
            {
                string path = applicationParams.TempPath + "/InsertData.sql";
                string delete = "DELETE FROM pbbuild WHERE lib_name = '", ifExists = "IF EXISTS (SELECT 1 FROM sysobjects WHERE name = 'pbbuild' AND type = 'U')";
                string statement = "INSERT INTO dbo.pbbuild (application_id, lib_name, build_number,build_seqno, build_date, active)", values = "VALUES (", changeLine = "\r\n";
                string singleQuote = "'", comma = ",", par = ")", content = "", begin = "BEGIN", end = "END", libraryName;
                int active = 1;
                buildNumber = String.IsNullOrWhiteSpace(buildNumber) ? versionParams.BuildNumber : buildNumber;
                var buildSeqNo = versionParams.BuildSeqNo;
                var buildDate = DateTime.Parse(versionParams.BuildDate).ToString("yyyyMMdd HH:mm:ss");
                var pbdList = pbdFiles.Split(',').ToList();
                File.WriteAllText(path, "");
                File.WriteAllText(path, ifExists + changeLine + begin + changeLine);
                if (buildMode == BuildMode.Full)
                {
                    for (int i = 0; i < this.libraryList.Count(); i++)
                    {
                        content = (delete + this.LibraryList[i].LibraryName + singleQuote + changeLine +
                                   statement + changeLine + values + singleQuote + buildTaskParameters.ApplicationId + singleQuote + comma + 
                                   singleQuote + this.LibraryList[i].LibraryName + singleQuote + comma + singleQuote + buildNumber + singleQuote + 
                                   comma + singleQuote + buildSeqNo + singleQuote + comma + singleQuote + buildDate + singleQuote + comma + 
                                   active + par + changeLine).ToString();

                        if ((this.LibraryList[i].IsApplicationLibrary) && this.LibraryList[i].LibraryNameWithExtension.ToUpper().EndsWith(".PBL"))
                            File.AppendAllText(path, content);
                    }
                }
                else
                {
                    for (int i = 0; i < pbdList.Count(); i++)
                    {
                        if (!pbdList[i].ToUpper().EndsWith(".PBD")) libraryName = pbdList[i].Split('.').FirstOrDefault();
                        content = (delete + pbdList[i] + singleQuote + changeLine +
                                   statement + changeLine + values + singleQuote + buildTaskParameters.ApplicationId + singleQuote + comma + 
                                   singleQuote + pbdList[i] + singleQuote + comma + singleQuote + buildNumber + singleQuote + comma + 
                                   singleQuote + buildSeqNo + singleQuote + comma + singleQuote + 
                                   buildDate + singleQuote + comma + active + par + changeLine).ToString();

                        if ((this.LibraryList[i].IsApplicationLibrary) && this.LibraryList[i].LibraryNameWithExtension.ToUpper().EndsWith(".PBL"))
                            File.AppendAllText(path, content);
                    }
                }
                File.AppendAllText(path, end);
            }
            catch (Exception ex)
            {

                logger.LogInfo("InsertBuildData: " + ex.Message + "LineNumber: " + Misc.GetLineNumber(ex));
            }

        }

        public List<PBLibrary> LibraryList { get { return this.libraryList; } set { } }
        public List<PBLibrary> ClientLibraryList { get { return this.clientLibraryList; } set { } }

        
    }

}
