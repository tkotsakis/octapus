using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using Relational.Octapus.Workspace;
using Relational.Octapus.Persistence;
using System.IO;
using System.Data;

namespace Relational.Octapus.BuildEngine
{

    public class BuildTaskParameters
    {
        private string configurationId;
        private string applicationObject;
        private string executableLibrary;
        private string buildMode;
        private string companyname;
        private string productname;
        private string copyright;
        private string description;
        private string fileversion;
        private string fileversionnum;
        private string productversion;
        private string productversionnum;
        private string eXEfilename;
        private string eXEico;
        private string librariesTargetFile;
        private string vssProjectPath;
        private string vssPBTPath;
        private string vssProductPath;
        private string projectResourceFile;
        private string buildSourceFolder;
        private string buildOutFolder;
        private string buildResourceFolder;
        private string buildOutScriptFolder;
        private string buildTempFolder;
        private string buildsFolder;
        private string buildPrefix;
        private string buildScriptFolderName;
        private string buildClientFolderName;
        private string buildPartialClientFolderName;
        private string buildSourceCodeFolderName;
        private string buildlogFolder;
        private bool prepareWorkableClient;
        private WSParametersList wsParameterList;

        private OctapusLog logger;

        public BuildTaskParameters (string applicationId)
	    {
            this.ApplicationId = applicationId;
            this.wsParameterList = new WSParametersList();
	    }

        public bool LoadConfiguration(string configFile , string applicationId, ConfigurationMode configurationMode, string connectionString)
        {
            bool retValue = true;
            try
            {
                ConfigOperator config = new ConfigOperator(configFile, applicationId);

                if (configurationMode.Equals(ConfigurationMode.DB))
                {
                    string msg = "";
                    string cmd = @"select name,value  from pbbuild_config where application_id = '" + applicationId + "'";
                    DBConnection cn = new DBConnection(DBMS.Sybase);
                    cn.ConnectionString = connectionString;
                    cn.Open();
                    DataTable dt = cn.GetDataTable(cmd, ref msg);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow r in dt.Rows)
                        {
                            wsParameterList.AddParameter(r.ItemArray.GetValue(0).ToString(), r.ItemArray.GetValue(1).ToString());
                        }
                    }
                    cn.Close();
                }

                this.ApplicationId = applicationId;
                this.applicationObject = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("applicationObject") : config.GetParameterValue("PBORCA", "applicationObject");
                this.executableLibrary = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("executableLibrary") : config.GetParameterValue("PBORCA", "executableLibrary");
                this.buildMode = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("buildMode") : config.GetParameterValue("PBORCA", "buildMode");
                this.companyname = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("companyname") : config.GetParameterValue("PBORCA", "companyname");
                this.productname = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("productname") : config.GetParameterValue("PBORCA", "productname");
                this.copyright = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("copyright") : config.GetParameterValue("PBORCA", "copyright");
                this.description = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("description") : config.GetParameterValue("PBORCA", "description");
                this.fileversion = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("fileversion") : config.GetParameterValue("PBORCA", "fileversion");
                this.fileversionnum = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("fileversionnum") : config.GetParameterValue("PBORCA", "fileversionnum");
                this.productversion = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("productversion") : config.GetParameterValue("PBORCA", "productversion");
                this.productversionnum = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("productversionnum") : config.GetParameterValue("PBORCA", "productversionnum");
                this.eXEfilename = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("eXEfilename") : config.GetParameterValue("PBORCA", "eXEfilename");
                this.eXEico = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("eXEico") : config.GetParameterValue("PBORCA", "eXEico");
                this.librariesTargetFile = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("librariesTargetFile") : config.GetParameterValue("PBORCA", "librariesTargetFile");
                this.projectResourceFile = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("projectResourceFile") : config.GetParameterValue("PBORCA", "projectResourceFile");
                this.buildSourceFolder = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("BuildSourceFolder") : config.GetParameterValue("PBORCA", "BuildSourceFolder");
                this.buildOutFolder = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("BuildOutFolder") : config.GetParameterValue("PBORCA", "BuildOutFolder");
                this.buildOutScriptFolder = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("BuildOutScriptFolder") : config.GetParameterValue("PBORCA", "BuildOutScriptFolder");
                this.buildResourceFolder = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("BuildResourceFolder") : config.GetParameterValue("PBORCA", "BuildResourceFolder");
                this.buildTempFolder = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("BuildTempFolder") : config.GetParameterValue("PBORCA", "BuildTempFolder");
                this.buildsFolder = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("BuildsFolder") : config.GetParameterValue("PBORCA", "BuildsFolder");
                this.buildPrefix = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("BuildPrefix") : config.GetParameterValue("PBORCA", "BuildPrefix");
                this.buildScriptFolderName = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("BuildScriptFolderName") : config.GetParameterValue("PBORCA", "BuildScriptFolderName");
                this.buildClientFolderName = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("BuildClientFolderName") : config.GetParameterValue("PBORCA", "BuildClientFolderName");
                this.buildPartialClientFolderName = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("BuildPartialClientFolderName") : config.GetParameterValue("PBORCA", "BuildPartialClientFolderName");
                this.buildSourceCodeFolderName = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("BuildSourceCodeFolderName") : config.GetParameterValue("PBORCA", "BuildSourceCodeFolderName");
                this.buildlogFolder = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("LogsFile") : config.GetParameterValue("PBORCA", "LogsFile");
                this.vssPBTPath = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("vssPBTPath") : config.GetParameterValue("VSS", "vssPBTPath");
                this.vssProjectPath = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("vssProjectPath") : config.GetParameterValue("VSS", "vssProjectPath");
                this.vssProductPath = configurationMode.Equals(ConfigurationMode.DB) ? wsParameterList.GetParameterValue("vssProductPath") : config.GetParameterValue("VSS", "vssProductPath");
            }
            catch (Exception)
            {
                retValue = false;
            }
            return retValue;

        }

        
        public string ApplicationId { get; set; }


        public string ConfigurationId { get { return this.configurationId; } set { } }
        public string ApplicationObject { get { return this.applicationObject; } set { } }
        public string ExecutableLibrary { get { return this.executableLibrary; } set { } }
        public string BuildMode { get { return this.buildMode; } set { } }
        public string Companyname { get { return this.companyname; } set { } }
        public string Productname { get { return this.productname; } set { } }
        public string Copyright { get { return this.copyright; } set { } }
        public string Description { get { return this.description; } set { } }
        public string Fileversion { get { return this.fileversion; } set { } }
        public string Fileversionnum { get { return this.fileversionnum; } set{} }
        public string Productversion { get { return this.productversion; } set{} }
        public string Productversionnum { get { return this.productversionnum; } set{} }
        public string EXEfilename { get { return this.eXEfilename; } set{} }
        public string EXEico { get { return this.eXEico; } set{} }
        public string LibrariesTargetFile { get { return this.librariesTargetFile; } set { } }
        public string VssProjectPath { get { return this.vssProjectPath; } set { } }
        public string VssPBTPath { get { return this.vssPBTPath; } set { } }
        public string VssProductPath { get { return this.vssProductPath; } set { } }
        public string ProjectResourceFile { get { return this.projectResourceFile; } set { } }
        public string BuildSourceFolder { get { return this.buildSourceFolder; } set { } }
        public string BuildOutFolder { get { return this.buildOutFolder; } set { } }
        public string BuildOutScriptFolder { get { return this.buildOutScriptFolder; } set { } }
        public string BuildResourceFolder { get { return this.buildResourceFolder; } set { } }
        public string BuildTempFolder { get { return this.buildTempFolder; } set { } }
        public string BuildsFolder { get { return this.buildsFolder; } set { } }
        public string BuildPrefix { get { return this.buildPrefix; } set { } }
        public string BuildScriptFolderName { get { return this.buildScriptFolderName; } set { } }
        public string BuildClientFolderName { get { return this.buildClientFolderName; } set { } }
        public string BuildPartialClientFolderName { get { return this.buildPartialClientFolderName; } set { } }
        public string BuildSourceCodeFolderName { get { return this.buildSourceCodeFolderName; } set { } }
        public string BuildLogFolderName { get { return this.buildlogFolder; } set { } }
        public WSParametersList WSParametersList { get { return this.wsParameterList; } set { } }
        public bool PrepareWorkableClient { get { return this.prepareWorkableClient; } set { prepareWorkableClient = value; } }
    }
}
