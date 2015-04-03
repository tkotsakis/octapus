using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Relational.Octapus.Persistence;
using Relational.Octapus.Common;
using Relational.Octapus.Workspace;
using Workspace.SourceSafe;

namespace Retrofit
{
    public class RetrofitTask
    {
        private RetrofitParams retrofitParams;
        private IWorkspaceProvider workspaceFrom;
        private IWorkspaceProvider workspaceTo;
        private OctapusLog logger;
        private WorkspaceParams workspaceParamsFrom;
        private WorkspaceParams workspaceParamsTo;
        private SourceSafeControl sourceSafeControl;
        private ApplicationParams applicationParams;
        string comment = "OCTAPUS RETROFIT CODE";

        public RetrofitTask(IDataManager dataManager, WorkspaceParams workspaceParamsFrom,WorkspaceParams workspaceParamsTo,
                            IWorkspaceProvider workspaceFrom,IWorkspaceProvider workspaceTo, IHookOctapusLogger hookLogger = null)
        {
            applicationParams = dataManager.GetApplicationParams();
            this.workspaceParamsFrom = workspaceParamsFrom;
            this.workspaceParamsTo = workspaceParamsTo;
            this.workspaceFrom = workspaceFrom;
            this.workspaceTo = workspaceTo;
            this.dataManager = dataManager;
            workspaceParamsFrom = dataManager.GetWorkspaceParams(workspaceParamsFrom.ApplicationId);
            workspaceParamsTo = dataManager.GetWorkspaceParams(workspaceParamsTo.ApplicationId);
            logger = new OctapusLog("RetrofitEngine", hookLogger);
            logger.LogInfo(String.Concat("RetrofitEngine Task Initialized for applications:", workspaceParamsFrom.ApplicationIdFrom + "-" + workspaceParamsTo.ApplicationIdTo));
            sourceSafeControl = new SourceSafeControl(applicationParams.InitilizationFilePath, hookLogger);
        }

        public IDataManager dataManager { get; set; }

        public void Retrofit()
        {
            try
            {
                //if ((!sourceSafeControl.CheckOutObjectsExist(workspaceParamsFrom.VssProductPath)) ||  (!sourceSafeControl.CheckOutObjectsExist(workspaceParamsFrom.VssCommonPath))) return false;
                //if ((!sourceSafeControl.CheckOutObjectsExist(workspaceParamsTo.VssProductPath)) || (!sourceSafeControl.CheckOutObjectsExist(workspaceParamsTo.VssCommonPath))) return false;
                sourceSafeControl.Disconnect();
                sourceSafeControl.Connect(applicationParams.Username, applicationParams.Password);
                 
                workspaceFrom.GetLatestProject(workspaceParamsFrom.VssProductPath, workspaceParamsFrom.LocalVssProductPath);
                workspaceFrom.GetLatestProject(workspaceParamsFrom.VssCommonPath, workspaceParamsFrom.LocalVssCommonPath);

                workspaceTo.GetLatestProject(workspaceParamsTo.VssCommonPath, workspaceParamsTo.LocalVssProductPath);
                workspaceTo.GetLatestProject(workspaceParamsTo.VssCommonPath, workspaceParamsTo.LocalVssCommonPath);

                sourceSafeControl.CheckOutProject(workspaceParamsTo.VssProductPath, comment);
                sourceSafeControl.CheckOutProject(workspaceParamsTo.VssCommonPath, comment);

                //if (!Directory.Exists(retrofitParams.RetrofitNewObjectsPath)) Directory.CreateDirectory(retrofitParams.RetrofitNewObjectsPath);

                Packager.CopyFiles(workspaceParamsFrom.LocalVssProductPath, workspaceParamsTo.LocalVssProductPath, "pbl");
                Packager.CopyFiles(workspaceParamsFrom.LocalVssCommonPath, workspaceParamsTo.LocalVssCommonPath, "pbl");

                sourceSafeControl.CheckInProject(workspaceParamsTo.VssProductPath, comment);
                sourceSafeControl.CheckInProject(workspaceParamsTo.VssCommonPath, comment);

                sourceSafeControl.Disconnect();

                logger.LogInfo("GetParameters");
                dataManager.GetPropertyList(applicationParams.TempPath, retrofitParams);
            }
            catch (Exception ex)
            {
                sourceSafeControl.Disconnect();
                logger.LogInfo("Retrofit :" + ex.Message);
            }

        }
    }
}
