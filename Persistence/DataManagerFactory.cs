using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Configuration;
using System.Collections.Specialized;

namespace Relational.Octapus.Persistence
{
    public class DataManagerFactory
    {
        public IDataManager New()
        {
            var dataManagerSettings = ConfigurationManager.GetSection("dataManagerSettings") as NameValueCollection;

            Assembly dataManagerAssemply = Assembly.Load(dataManagerSettings["AssemblyName"]);
            Type dataManagerType = dataManagerAssemply.GetType(dataManagerSettings["DataManagerType"]);
            var dataManagerInstance = (IDataManager)Activator.CreateInstance(dataManagerType);
            return dataManagerInstance;
        }
    }
}
