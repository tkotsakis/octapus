using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relational.Octapus.Persistence
{
    public class VersionParams
    {
        public string BuildNumber { get; set; }
        public string BuildSeqNo { get; set; }
        public string BuildDate { get; set; }
        public string Username { get; set; }
        public string Computer { get; set; }
        public string Domain { get; set; }
        public string ClientHostName { get; set; }
        public string ExtraInfo { get; set; }
    }
}
