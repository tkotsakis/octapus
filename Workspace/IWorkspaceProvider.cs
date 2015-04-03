using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Relational.Octapus.Persistence;
using System.Collections;

namespace Relational.Octapus.Workspace
{
    public interface IWorkspaceProvider
    {
        IDataManager DataManager { get; set; }
        DownloadWorkspaceResult DownloadWorkspace(string applicationId);
        bool CheckWorkspacePbgs(string applicationId);
        void GetPBTFile(string path, string pbtFile);
        List<PBLibrary> LibraryList (string applicationId, string targetFile);
        List<PBLibrary> AppServerLibraryList (string applicationId, string targetFile);
        List<PBLibrary> ClientLibraryList (string applicationId, string targetFile);
        bool GetLatestProject(string projectPath, string targetPath);
        void CorrectPBGFile(string pbgFile, string libraryName);
        void AddItemToProject(string vssProject, string localProject, string vssObject, string comment);
        void RemoveItemFromProject(string vssObject);
    }
}
