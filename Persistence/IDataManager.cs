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
        ApplicationParams GetApplicationParams(string path);
        WorkspaceParams GetWorkspaceParams(string applicationId);
        WorkspaceParams GetWorkspaceParamsPerPath(string applicationId,string path);
        VersionParams GetVersionParams (string applicationId);
        VersionParams GetVersionParams(string applicationId,string path);
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
