using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Relational.Octapus.Persistence;
using Relational.Octapus.Workspace.SourceSafe;
using Relational.Octapus.BuildEngine;
using Relational.Octapus.Common;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Retrofit;

namespace ConsoleClient
{
        public class Program
        {
            static string applicationId;
            static string applicationIdFrom;
            static string applicationIdTo;

            static void Main(string[] args)
            {
                //var dataManagerFactory = new DataManagerFactory();
                //var dataManager = dataManagerFactory.New();
                //var applicationParams = dataManager.GetApplicationParams();
                //var versionParameters = dataManager.GetVersionParams(applicationId);

                //var workSpace = new SourceSafeProvider(dataManager, applicationId);
                //var workspaceParams = dataManager.GetWorkspaceParams(applicationId);

                //workSpace.AddItemToProject(@"$/DEV/PB105/iapply/CODE_BACKUP/ALPHA/SE/iapply/admflow",@"C:\DEV\PB105\iapply\CODE_BACKUP\ALPHA/SE\iapply\admflow", "w_123.srw", "Retrofit");
                //workSpace.RemoveItemFromProject(@"$/DEV/PB105/iapply/CODE_BACKUP/ALPHA/SE/iapply/admflow/w_123.srw");
                RetrofitConsole();
            }

            public static void BuildConsole()
            {
                string pbdFiles = "";

                Console.WriteLine("*************************** OCTAPUS ************************");
                Console.WriteLine("");
                Console.WriteLine("*********** AUTOMATED BUILD AND DEPLOYMENT TOOL ***********");
                Console.WriteLine("");
                Console.Write("Application Id: ");
                applicationId = Console.ReadLine();
                Console.WriteLine("");
                Console.WriteLine("1 - FULL ");
                Console.WriteLine("0 - SPECIFIC PBDs ");
                Console.WriteLine("");
                Console.Write("Enter Build Mode: ");
                string buildModeInput = Console.ReadLine();
                var buildMode = BuildMode.Full;
                if (buildModeInput == "0")
                {
                    buildMode = BuildMode.SpecificPbds;
                    Console.WriteLine("");
                    Console.Write("Enter libraries to build (comma separated) : ");
                    pbdFiles = Console.ReadLine().ToString();
                }
                if (buildModeInput != "0" && buildModeInput != "1") Console.Write("Enter a valid Mode:");

                OctapusLog logger = new OctapusLog("OctapusLogger");

                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var dataManagerFactory = new DataManagerFactory();
                    var dataManager = dataManagerFactory.New();
                    var applicationParams = dataManager.GetApplicationParams();
                    var versionParameters = dataManager.GetVersionParams(applicationId);

                    var workSpace = new SourceSafeProvider(dataManager, applicationId);
                    var workspaceParams = dataManager.GetWorkspaceParams(applicationId);
                    var buildParameters = new BuildTaskParameters(applicationId);
                    var buildTask = new BuildTask(buildParameters, dataManager, workSpace);
                    buildParameters.LoadConfiguration("WorkspaceProfile.config", applicationId, workspaceParams.ConfigurationMode, workspaceParams.DBConnectionString);

                    var builder = new BuildMachine(dataManager, workSpace);
                    var buildResult = builder.Build(buildTask);

                    logger.LogInfo(applicationId + " BUILD STARTED ");
                    buildTask.Build(buildMode, pbdFiles);

                    logger.LogInfo(applicationId + " BUILD FINISHED");

                    stopwatch.Stop();
                    logger.LogInfo("Time elapsed: " + stopwatch.Elapsed);

                    logger.LogInfo(applicationId + " OCTAPUS PROCESS COMPLETED");
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                    logger.LogInfo(ex.Message);
                }
            }

            public static void RetrofitConsole()
            {
                Console.Write("Application Id From: ");
                applicationIdFrom = Console.ReadLine();
                Console.WriteLine("");

                Console.Write("Application Id To: ");
                applicationIdTo = Console.ReadLine();
                Console.WriteLine("");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var dataManagerFactory = new DataManagerFactory();
                var dataManager = dataManagerFactory.New();
                var applicationParams = dataManager.GetApplicationParams();

                var retrofitParamsFrom = dataManager.GetRetrofitParamsFrom(applicationIdFrom);
                var retrofitParamsTo = dataManager.GetRetrofitParamsTo(applicationIdTo);
                OctapusLog logger = new OctapusLog("ConsoleLogger");

                var workSpaceFrom = new SourceSafeProvider(dataManager, applicationIdFrom);
                var workSpaceTo = new SourceSafeProvider(dataManager, applicationIdTo);

                var workspaceParamsFrom = dataManager.GetWorkspaceParams(applicationIdFrom);
                var workspaceParamsTo = dataManager.GetWorkspaceParams(applicationIdTo);

                RetrofitTask retrofitTask = new RetrofitTask(dataManager, workspaceParamsFrom, workspaceParamsTo, workSpaceFrom, workSpaceTo,null);
                retrofitTask.Retrofit();
            }

        }
}
