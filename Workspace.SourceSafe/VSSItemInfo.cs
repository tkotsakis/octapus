using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workspace.SourceSafe
{
    public class VSSItemInfo
    {

        private string name;
        private bool isCheckedOut ;
        private string checkedOutUser ;
        private int versionNumber ;
        private string checkedOutDate ;
        private string machineName ;
        private string comment;


        public VSSItemInfo(string name, 
                           bool isCheckedOut = false , 
                           string checkedOutUser = null, 
                           int versionNumber = -1, 
                           string checkedOutDate = null,
                           string machineName = null,
                           string comment = null
            )
        {
        this.name  = name;
        this.isCheckedOut=isCheckedOut ;
        this.checkedOutUser = checkedOutUser ;
        this.versionNumber = versionNumber ;
        this.checkedOutDate = checkedOutDate ;
        this.machineName = machineName;

        }


        public string Name { get { return this.name; } }
        public bool IsCheckedOut { get { return this.isCheckedOut; }  }
        public string CheckedOutUser { get { return this.checkedOutUser; }  }
        public int VersionNumber { get { return this.versionNumber; }  }
        public string CheckedOutDate { get { return this.checkedOutDate; }  }
        public string MachineName { get { return this.machineName; } }
        public string Comment { get { return this.comment; }  }
    }
}
