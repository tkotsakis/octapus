using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relational.Octapus.Persistence
{
    public interface IDataManager
    {
        int GetNextBuildNo(string applicationId);
        void SetNextBuildNo(string applicationId);
        ApplicationParams GetApplicationParams();
        WorkspaceParams GetWorkspaceParams(string applicationId);
        VersionParams GetVersionParams (string applicationId);
        WorkspaceParams GetRetrofitParamsFrom (string applicationIdFrom);
        WorkspaceParams GetRetrofitParamsTo (string applicationIdTo);
        List<string> GetAvailableWorkspaces();
        string GetPropertyList(string path, object obj);
        string GetClientHostName();
        void AddParameter(string parameterName, string parameterValue);
        string GetParameterValue(string paramaterName);
        string GetExecutableVersion(string applicationId);
        void SetExecutableVersion(string applicationId,string versionInput);
        void CheckOutObjectsExist(string vssProject);
    }
}
