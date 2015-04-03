using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relational.Octapus.Common
{
    public class OctapusException : Exception
    {
        bool exitApplication = false;

        public OctapusException(string assemblyFullname, Exception innerException, bool exitApplication);

        public string AssemblyFullName { get; set; }
        public bool ExitApplication { get; set; }
    }
}
