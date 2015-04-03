using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relational.Octapus.Persistence
{
    public class WSParameter
    {

        public WSParameter(string name, string value)
        {
            this.parameterName = name;
            this.parameterValue = value;
        }
        private string parameterName;
        private string parameterValue;
        public string ParameterName { get { return this.parameterName; } set { this.parameterName = value; } }
        public string ParameterValue { get { return this.parameterValue; } set { this.parameterValue = value; } }
    }

}
