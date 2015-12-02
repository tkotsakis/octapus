using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Relational.Octapus.Persistence;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Net;
using Cassia;

namespace Relational.Octapus.Persistence.ExternalFiles
{
    public class FileDataManager : IDataManager
    {
        public int GetNextBuildNo(string applicationId)
        {
            VersionParams versionParams = new VersionParams();
            var buildSeqNo = 0;

            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/VersionInfo.xml");

            buildSeqNo = Convert.ToInt32(String.IsNullOrWhiteSpace(doc.SelectSingleNode("versionInfo/" + applicationId + "/BuildSeqNo").InnerText) ? 1 : Convert.ToInt32(doc.SelectSingleNode("versionInfo/" + applicationId + "/BuildSeqNo").InnerText));
            return buildSeqNo;
        }

        public void SetNextBuildNo(string applicationId)
        {
            VersionParams versionParams = new VersionParams();
            var buildSeqNo = 0;

            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/VersionInfo.xml");

            buildSeqNo = Convert.ToInt32(doc.SelectSingleNode("versionInfo/" + applicationId + "/BuildSeqNo").InnerText);
            buildSeqNo++;
            doc.SelectSingleNode("versionInfo/" + applicationId + "/BuildSeqNo").InnerText = buildSeqNo.ToString();
            doc.Save(AppDomain.CurrentDomain.BaseDirectory + "/VersionInfo.xml");
            doc.Save(AppDomain.CurrentDomain.BaseDirectory + "/VersionInfo.ini");
        }

        public string GetExecutableVersion(string applicationId)
        {
            var executableVersion = "";

            WorkspaceParams workspaceParams = this.GetWorkspaceParams(applicationId);
            OctapusLog logger = new OctapusLog("BuildEngine", null);

            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/WorkspaceProfile.config");

            if (workspaceParams.ConfigurationMode.Equals(ConfigurationMode.DB))
            {
                string cmd = @"DECLARE @value VARCHAR(50) select @value = value from pbbuild_config where name = 'productversionnum' and application_id='" + applicationId + "'" +
                               " select @value";
                DBConnection dbConnection = new DBConnection(DBMS.Sybase);
                dbConnection.Execute(workspaceParams, cmd, logger, dbConnection);
                executableVersion = dbConnection.GetSqlResult();
            }
            else
            {
                executableVersion = String.IsNullOrWhiteSpace(doc.SelectSingleNode("workspace/PBORCA/" + applicationId + "/productversionnum").InnerText) ? "2.00.00" : doc.SelectSingleNode("workspace/PBORCA/" + applicationId + "/productversionnum").InnerText;
            }

            executableVersion = String.IsNullOrWhiteSpace(executableVersion) ? doc.SelectSingleNode("workspace/PBORCA/" + applicationId + "/productversionnum").InnerText : executableVersion;
            return executableVersion;
        }

        public void SetExecutableVersion(string applicationId,string versionInput)
        {
            WorkspaceParams workspaceParams = this.GetWorkspaceParams(applicationId);

            OctapusLog logger = new OctapusLog("BuildEngine",null);

            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/WorkspaceProfile.config");
            doc.SelectSingleNode("workspace/PBORCA/" + applicationId + "/productversionnum").InnerText = versionInput;
            doc.Save(AppDomain.CurrentDomain.BaseDirectory + "/WorkspaceProfile.config");
            if (workspaceParams.ConfigurationMode.Equals(ConfigurationMode.DB))
            {
                string cmd = @"update pbbuild_config set value='" + versionInput + "' where name ='productversionnum' and application_id='" + applicationId + "'";
                DBConnection dbConnection = new DBConnection(DBMS.Sybase);
                dbConnection.Execute(workspaceParams, cmd, logger, dbConnection);
            }
        }

        public ApplicationParams GetApplicationParams()
        {
            ApplicationParams appParams = new ApplicationParams();

            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory +"/ApplicationParams.xml");

            appParams.InitilizationFilePath = doc.SelectSingleNode("applicationParameters/InitializationFile").InnerText;
            appParams.Username              = doc.SelectSingleNode("applicationParameters/Username").InnerText;
            appParams.Password              = doc.SelectSingleNode("applicationParameters/Password").InnerText;
            appParams.PbgFormat             = doc.SelectSingleNode("applicationParameters/PbgFormat").InnerText;
            appParams.PbgBeginLib           = doc.SelectSingleNode("applicationParameters/PbgBeginLib").InnerText;
            appParams.PbgBeginObj           = doc.SelectSingleNode("applicationParameters/PbgBeginObj").InnerText;
            appParams.PbgEnd                = doc.SelectSingleNode("applicationParameters/PbgEnd").InnerText;
            appParams.PbgExtension          = doc.SelectSingleNode("applicationParameters/PbgExtension").InnerText;
            appParams.NewLineSeparator      = doc.SelectSingleNode("applicationParameters/NewLineSeparator").InnerText;
            appParams.NewPbgFileSuffix      = doc.SelectSingleNode("applicationParameters/NewPbgFileSuffix").InnerText;
            appParams.Quote                 = doc.SelectSingleNode("applicationParameters/Quote").InnerText;
            appParams.Space                 = doc.SelectSingleNode("applicationParameters/Space").InnerText;
            appParams.QuestionMark          = doc.SelectSingleNode("applicationParameters/QuestionMark").InnerText;
            appParams.DiskSpaceNeededInMb   = doc.SelectSingleNode("applicationParameters/DiskSpaceNeededInMb").InnerText;
            appParams.TargetPath            = doc.SelectSingleNode("applicationParameters/TargetPath").InnerText;
            appParams.TempPath              = doc.SelectSingleNode("applicationParameters/TempPath").InnerText;
            appParams.LogPath               = doc.SelectSingleNode("applicationParameters/LogPath").InnerText;
            appParams.RootPath              = doc.SelectSingleNode("applicationParameters/RootPath").InnerText;
            appParams.VssContent            = doc.SelectSingleNode("applicationParameters/VssContent").InnerText;
            
            return appParams;
        }

        public WorkspaceParams GetWorkspaceParams(string applicationId)
        {
            WorkspaceParams workspaceParams = new WorkspaceParams();
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/WorkspaceProfile.config");
            ConfigOperator configOperator  = new ConfigOperator("WorkspaceProfile.config", applicationId);

            workspaceParams.VssPBTPath = configOperator.GetParameterValue("VSS", "vssPBTPath");
            workspaceParams.VssProjectPath = configOperator.GetParameterValue("VSS", "vssProjectPath");
            workspaceParams.LibraryTargetPath = configOperator.GetParameterValue("PBORCA", "librariesTargetFile");
            workspaceParams.VssProductPath = configOperator.GetParameterValue("VSS", "vssProductPath");
            workspaceParams.VssCommonPath = configOperator.GetParameterValue("VSS", "vssCommonPath");
            workspaceParams.AppServerTargetFile1 = configOperator.GetParameterValue("PBORCA", "AppServerTargetFile1");
            workspaceParams.AppServerTargetFile2 = configOperator.GetParameterValue("PBORCA", "AppServerTargetFile2");
            workspaceParams.AppServerTargetPath1 = configOperator.GetParameterValue("VSS", "vssAppServerPBTPath1");
            workspaceParams.AppServerTargetPath2 = configOperator.GetParameterValue("VSS", "vssAppServerPBTPath2");
            workspaceParams.LocalVssWorkingPath = configOperator.GetParameterValue("VSS", "localVssWorkingPath");
            workspaceParams.LocalVssProductPath = configOperator.GetParameterValue("VSS", "localVssProductPath");
            workspaceParams.LocalVssCommonPath = configOperator.GetParameterValue("VSS", "localVssCommonPath");
            workspaceParams.LocalPBTPath        = configOperator.GetParameterValue("VSS", "localPBTPath");
            workspaceParams.ClientNecFilesPath = configOperator.GetParameterValue("VSS", "clientNecFilesPath");
            workspaceParams.Pbw                = configOperator.GetParameterValue("PBORCA", "Pbw");
            workspaceParams.ApplicationId = applicationId;
            workspaceParams.EasLibNeeded = configOperator.GetParameterValue("VSS", "EasLibNeeded");
            workspaceParams.DBConnectionString = doc.SelectSingleNode("workspace/DBConnection/" + applicationId).InnerText;
            var configurationMode = doc.SelectSingleNode("workspace/ConfigurationMode/" + applicationId).InnerText;
            ConfigurationMode configMode = (ConfigurationMode)Enum.Parse(typeof(ConfigurationMode), configurationMode);
            workspaceParams.ConfigurationMode = configMode;      

            return workspaceParams;
        }

        public VersionParams GetVersionParams(string applicationId)
        {
            VersionParams versionParams = new VersionParams();
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/VersionInfo.xml");

            versionParams.BuildNumber = Guid.NewGuid().ToString();
            versionParams.BuildSeqNo  = doc.SelectSingleNode("versionInfo/" + applicationId + "/BuildSeqNo").InnerText;
            versionParams.BuildDate   = String.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            versionParams.Username    = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            versionParams.Computer    = System.Environment.MachineName;
            versionParams.ClientHostName = Dns.GetHostName();
            versionParams.Domain      = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            versionParams.ExtraInfo   = doc.SelectSingleNode("versionInfo/" + applicationId + "/ExtraInfo").InnerText;

            return versionParams;
        }

        public WorkspaceParams GetRetrofitParamsFrom (string applicationIdFrom)
        {
            WorkspaceParams workspaceParams = this.GetWorkspaceParams(applicationIdFrom);
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/WorkspaceProfile.config");

            workspaceParams.ApplicationIdFrom = applicationIdFrom;
            workspaceParams.RetrofitNewObjectsPath = doc.SelectSingleNode("workspace/VSS/" + applicationIdFrom + "/retrofitNewObjectsPath").InnerText;

            return workspaceParams;
        }

        public WorkspaceParams GetRetrofitParamsTo (string applicationIdTo)
        {
            WorkspaceParams workspaceParams = this.GetWorkspaceParams(applicationIdTo);
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/WorkspaceProfile.config");

            workspaceParams.ApplicationIdTo = applicationIdTo;
            workspaceParams.RetrofitNewObjectsPath = doc.SelectSingleNode("workspace/VSS/" + applicationIdTo + "/retrofitNewObjectsPath").InnerText;

            return workspaceParams;
        }

        public List<string> GetAvailableWorkspaces()
        {
            var workspaceList = new List<string>();
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "/WorkspaceProfile.config");
            XmlNode node = doc.DocumentElement.FirstChild;
            XmlNodeList xnList = node.ChildNodes;

            for (int i = 0; i < xnList.Count; i++)
            {
                workspaceList.Add(xnList[i].Name);
            }
            
            return workspaceList;
        }

        public string GetPropertyList(string path, object obj)
        {
            var properties = obj.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var property in properties)
            {
                sb.AppendLine(property.Name + ": " + property.GetValue(obj, null));
                File.AppendAllText(Path.Combine(path,"Parameters.xml"), sb.ToString());
            }
            return sb.ToString();
            
        }

        public string GetClientHostName()
        {
            try
            {
                var clientHostName = "";
                var manager = new TerminalServicesManager();
                using (var server = manager.GetLocalServer())
                {
                    server.Open();
                    foreach (var session in server.GetSessions())
                    {
                        if (session.ConnectionState == ConnectionState.Active)
                        {
                            clientHostName = session.ClientName;
                        }
                    }
                }
                return clientHostName;
            }
            catch (Exception)
            {
                
                throw;
            }
           
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public void AddParameter(string parameterName, string parameterValue)
        {
            throw new NotImplementedException();
        }

        public string GetParameterValue(string paramaterName)
        {
            throw new NotImplementedException();
        }


        public void CheckOutObjectsExist(string vssProject)
        {
            
        }
    }
}
