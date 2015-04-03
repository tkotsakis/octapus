using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relational.Octapus.Persistence
{
    
    public class WSParametersList
    {
        private List<WSParameter> parametersList = new List<WSParameter>();

        public List<WSParameter> ParametersList { get { return this.parametersList; } }

        public void AddParameter(string parameterName, string parameterValue)
        {
            this.parametersList.Add(new WSParameter(parameterName, parameterValue));
        }

        public string GetParameterValue(string paramaterName)
        {
            var parameter = this.parametersList.First(i => i.ParameterName.Equals(paramaterName));
            return parameter.ParameterValue.ToString();
        }

    }

}
