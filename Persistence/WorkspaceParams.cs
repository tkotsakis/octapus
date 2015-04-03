using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relational.Octapus.Persistence
{
    public class WorkspaceParams
    {
        public string VssProjectPath { get; set; }
        public string VssPBTPath { get; set; }
        public string VssProductPath { get; set; }
        public string VssCommonPath { get; set; }
        public string LibraryTargetPath { get; set; }
        public string AppServerTargetPath1 { get; set; }
        public string AppServerTargetPath2 { get; set; }
        public string AppServerTargetFile1 { get; set; }
        public string AppServerTargetFile2 { get; set; }
        public string LocalVssWorkingPath { get; set; }
        public string LocalVssProductPath { get; set; }
        public string LocalVssCommonPath { get; set; }
        public string LocalPBTPath { get; set; }
        public string ClientNecFilesPath { get; set; }
        public string Pbw { get; set; }
        public string RetrofitNewObjectsPath { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationIdFrom { get; set; }
        public string ApplicationIdTo { get; set; }
        public string DBConnectionString { get; set; }
        public ConfigurationMode ConfigurationMode { get; set; }
    }
}
