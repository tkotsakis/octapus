using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relational.Octapus.Persistence
{
    public class ApplicationParams
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string InitilizationFilePath { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PbgFormat { get; set; }
        public string PbgBeginLib { get; set; }
        public string PbgBeginObj { get; set; }
        public string PbgEnd { get; set; }
        public string PbgExtension { get; set; }
        public string NewLineSeparator { get; set; }
        public string NewPbgFileSuffix { get; set; }
        public string Quote { get; set; }
        public string Space { get; set; }
        public string QuestionMark { get; set; }
        public string DiskSpaceNeededInMb { get; set; }
        public string TargetPath { get; set; }
        public string TempPath { get; set; }
        public string LogPath { get; set; }
        public string RootPath { get; set; }
        public string VssContent { get; set; }

    }
}
