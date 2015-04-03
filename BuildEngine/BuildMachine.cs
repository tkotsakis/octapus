using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Relational.Octapus.Persistence;
using Relational.Octapus.Workspace;

namespace Relational.Octapus.BuildEngine
{

    public class BuildMachine
    {
        private IDataManager dataManager;
        private IWorkspaceProvider workSpaceProvider;
        private OctapusLog logger;


        public BuildMachine(IDataManager dataManager, IWorkspaceProvider workSpaceProvider, IHookOctapusLogger hookLogger = null)
        {
            this.dataManager = dataManager;
            this.workSpaceProvider = workSpaceProvider;
            this.logger = new OctapusLog("ConsoleLogger", hookLogger);//this.GetType().ToString());
        }
        public BuildTaskResult Build(BuildTask buildTask)
        {
            /*var hasValidPbgs = workSpaceProvider.CheckWorkspacePbgs(buildTask.BuildTaskParameters.ApplicationId);


            if (hasValidPbgs)
            {
                // continue with next steps
            }
            else
            {
                logger.LogInfo("PBG Mismatch");
            }
            */
            return new BuildTaskResult();
        }


        public OctapusLog Logger
        {
            get {return this.logger;}
            set { }
        }



    }
}
