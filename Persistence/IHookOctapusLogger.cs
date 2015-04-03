using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relational.Octapus.Persistence
{
    public interface IHookOctapusLogger
    {
        void LogMessage(string loggerName, string type, string message);
    }
}
