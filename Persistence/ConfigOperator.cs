using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Relational.Octapus.Persistence
{
    public class ConfigOperator
    {
        private string configFile,value;
        private string applicationId;

        private XmlDocument configDocument;
        private XmlElement rootConfig;

        public ConfigOperator(string configFile, string applicationId)
        {
            this.configFile = configFile;
            this.applicationId = applicationId;
            this.configDocument = new XmlDocument();
            this.configDocument.Load(this.configFile);
            this.rootConfig = this.configDocument.DocumentElement;
        }

        public string GetParameterValue(string configSection, string tag)
        {
            if (this.rootConfig == null)
                return "";
            else
                value = String.IsNullOrWhiteSpace(String.Concat(configSection, "/", this.applicationId, "/", tag)) ? "" : this.rootConfig.SelectSingleNode(String.Concat(configSection, "/", this.applicationId, "/", tag)).InnerText.ToString();
                return value;
        }

        public string GetApplicationParameter(string configSection, string tag)
        {
            if (this.rootConfig == null)
                return "";
            else
                return this.rootConfig.SelectSingleNode(String.Concat(configSection, "/", tag)).InnerText;
             
        }

    }
}
