using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Relational.Octapus.BuildEngine;
using Relational.Octapus.Persistence;
using Relational.Octapus.Workspace.SourceSafe;
using Relational.Octapus.Workspace;
using WinClient;
using Relational.Octapus.WinClient.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System.IO;
using Relational.Octapus.Common;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;


namespace Relational.Octapus.WinClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IHookOctapusLogger
    {
        private IDataManager dataManager;
        private IWorkspaceProvider workspaceProvider;
        private WorkspaceParams workspaceParams;
        private ObservableCollection<ListBoxMultiselectItem<string>> remainingLibraries;
        private ObservableCollection<ListBoxMultiselectItem<string>> librariesToBuild;
        private string workspaceToBuild;
        private string buildNumber,executableBuildNumber;
        private bool autoBuildNumber;
        private bool stopIfCheckedOut,prepareClient;
        private bool hasLoggerData;
        private ICallbackProvider callbackProvider;

        public MainWindowViewModel(ICallbackProvider callbackProvider = null)
        {
            this.buildNumber = null;
            this.executableBuildNumber = null;
            this.autoBuildNumber = true;
            this.stopIfCheckedOut = true;
            this.prepareClient = false;

            this.remainingLibraries = new ObservableCollection<ListBoxMultiselectItem<string>>();
            this.librariesToBuild = new ObservableCollection<ListBoxMultiselectItem<string>>();

            var dataManagerFactory = new DataManagerFactory();
            dataManager = dataManagerFactory.New();

            CommandBuild = new RelayCommand(o => Build(), a => !string.IsNullOrWhiteSpace(buildNumber) && librariesToBuild.Any() && !string.IsNullOrWhiteSpace(WorkspaceToBuild) && workspaceProvider != null && !isBusy);

            CommandSelectLibraries = new RelayCommand(o => SelectLibraries(a => a.IsSelected));
            CommandRemoveLibraries = new RelayCommand(o => RemoveLibraries(a => a.IsSelected));
            CommandSelectAllLibraries = new RelayCommand(o => SelectLibraries(a => true));
            CommandRemoveAllLibraries = new RelayCommand(o => RemoveLibraries(a => true));

            CommandExitApplication = new RelayCommand(o => Application.Current.Shutdown(), a => !isBusy);

            CommandReset = new RelayCommand(o => ResetForm(), a=> !isBusy);

            this.Workspaces = dataManager.GetAvailableWorkspaces();
            HasLoggerData = false;
            LoggerData = new ObservableCollection<LogData>();
            this.callbackProvider = callbackProvider;

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }



        #region Commands
        public RelayCommand CommandBuild { get; private set; }
        public RelayCommand CommandSelectLibraries { get; private set; }
        public RelayCommand CommandRemoveLibraries { get; private set; }
        public RelayCommand CommandSelectAllLibraries { get; private set; }
        public RelayCommand CommandRemoveAllLibraries { get; private set; }
        public RelayCommand CommandExitApplication { get; private set; }
        public RelayCommand CommandReset { get; private set; }

        #endregion

        #region public Properties
        public List<string> Workspaces { get; set; }
        public string WorkspaceToBuild {
            get
            {
                return workspaceToBuild;
            }
            set
            {
                workspaceToBuild = value;
                ShowWorkspaceDetails();
                OnPropertyChanged("WorkspaceToBuild");
                
            }
        }
        public ObservableCollection<ListBoxMultiselectItem<string>> RemainingLibraries
        {
            get
            {
                return remainingLibraries;
            }
            set
            {
                remainingLibraries = value;
                OnPropertyChanged("RemainingLibraries");
                
            }
        }
        public ObservableCollection<ListBoxMultiselectItem<string>> LibrariesToBuild
        {
            get
            {
                return librariesToBuild;
            }
            set
            {
                librariesToBuild = value;
                OnPropertyChanged("LibrariesToBuild");

            }
        }
        public string BuildNumber { 
            get
            {
                return buildNumber;
            }
            set
            {
                buildNumber = value;
                OnPropertyChanged("BuildNumber");
            }
        }
        public string ExecutableBuildNumber
        {
            get
            {
                return executableBuildNumber;
            }
            set
            {
                string oldValue = executableBuildNumber;
                executableBuildNumber = value;
                NotifyChanged("ExecutableBuildNumber", oldValue, value);
            }
        }
        public bool AutoBuildNumber
        {
            get
            {
                return autoBuildNumber;
            }
            set
            {
                autoBuildNumber = value;
                ShowBuildNumber();
                OnPropertyChanged("AutoBuildNumber");
            }

        }
        public bool StopIfCheckedOut
        {
            get
            {
                return stopIfCheckedOut;
            }
            set
            {
                stopIfCheckedOut = value;
                OnPropertyChanged("StopIfCheckedOut");
            }

        }

        public bool PrepareWorkableClient
        {
            get
            {
                return prepareClient;
            }
            set
            {
                prepareClient = value;
                OnPropertyChanged("PrepareWorkableClient");
            }

        }
        public bool HasLoggerData { 
            get 
            { 
                return hasLoggerData; 
            }
            set 
            {
                hasLoggerData = value;
                OnPropertyChanged("HasLoggerData");
            }
        }
        #endregion


        private void Build()
        {
            LoggerData.Remove(a => true);

            worker.RunWorkerAsync();
            
        }

        private void ShowBuildNumber()
        {
            if (AutoBuildNumber)
            {
                string tempBuildNo = null;

                try
                {
                    tempBuildNo = dataManager.GetNextBuildNo(workspaceToBuild).ToString();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    this.BuildNumber = tempBuildNo;
                }
                
            }
        }

        private void SetExecutableVersion()
        {
            dataManager.SetExecutableVersion(workspaceToBuild, ExecutableBuildNumber);
        }

        private void GetExecutableVersion()
        {
            string executableVersion = null;

            try
            {
                executableVersion = dataManager.GetExecutableVersion(workspaceToBuild);
            }
            catch
            {
                throw;
            }
            finally
            {
                this.ExecutableBuildNumber = executableVersion;
            }

        }

        private void PrepareClient()
        {
            try
            {
                this.PrepareWorkableClient = prepareClient;
            }
            catch
            {
                throw;
            }

        }

        private void ShowWorkspaceDetails()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            this.RemainingLibraries.Remove(a => true);
            this.LibrariesToBuild.Remove(a => true);
            this.workspaceProvider = null;
            this.workspaceParams = null;

            IWorkspaceProvider tempWorkspaceProvider = null;

            if (!string.IsNullOrWhiteSpace(workspaceToBuild) && !isBusy)
            {
                try
                {
                    
                    tempWorkspaceProvider = new SourceSafeProvider(dataManager, workspaceToBuild);
                    workspaceParams = dataManager.GetWorkspaceParams(WorkspaceToBuild);
                    var libraries = tempWorkspaceProvider.ClientLibraryList(workspaceToBuild, workspaceParams.LibraryTargetPath);
                    libraries.ForEach(a => this.RemainingLibraries.Add(new ListBoxMultiselectItem<string>(a.LibraryName)));
                    ShowBuildNumber();
                    GetExecutableVersion();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }

                
                this.workspaceProvider = tempWorkspaceProvider;
            }
        }

        private void ResetForm()
        {
            BuildNumber = string.Empty ;
            LibrariesToBuild.Clear();
            RemainingLibraries.Clear();
            LoggerData.Clear();
            Workspaces.Clear();
            StopIfCheckedOut = false;
            PrepareWorkableClient = false;
            ExecutableBuildNumber = string.Empty;
        }


        #region Library Relocation

        private void SelectLibraries(Func<ListBoxMultiselectItem<string>, bool> whereClause)
        {
            var selection = RemainingLibraries.Where(whereClause).ToList();
            foreach (var item in selection)
            {
                item.IsSelected = false;
                LibrariesToBuild.Add(item);
                RemainingLibraries.Remove(item);
            }
            LibrariesToBuild = new ObservableCollection<ListBoxMultiselectItem<string>>(LibrariesToBuild.OrderBy(a => a.Value));
            
        }

        private void RemoveLibraries(Func<ListBoxMultiselectItem<string>, bool> whereClause)
        {
            var selection = LibrariesToBuild.Where(whereClause).ToList();
            foreach (var item in selection)
            {
                item.IsSelected = false;
                RemainingLibraries.Add(item);
                LibrariesToBuild.Remove(item);
            }

            
            RemainingLibraries = new ObservableCollection<ListBoxMultiselectItem<string>>(RemainingLibraries.OrderBy(a => a.Value));
        }
        
        #endregion

        #region Logger

        public class LogData
        {
            public string Logger { get; set; }
            public string Type { get; set; }
            public string Message { get; set; }
        }

        
        public ObservableCollection<LogData> LoggerData { get; set; }
        
        public void LogMessage(string loggerName, string type, string message)
        {


            var logData = new LogData()
            {
                Logger = loggerName,
                Type = type,
                Message = message
            };

            if (callbackProvider != null)
                callbackProvider.CallBack(logData);

            
        }

        #endregion

        #region Background Worker

        private readonly BackgroundWorker worker = new BackgroundWorker();


        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;


            SetExecutableVersion();
            PrepareClient();
            var buildMode = BuildMode.Full;

            StringBuilder pbdFilesBuilder = new StringBuilder();

            if (RemainingLibraries.Count() != 0)
            {
                buildMode = BuildMode.SpecificPbds;
                LibrariesToBuild.ToList().ForEach(a => pbdFilesBuilder.AppendFormat(",{0}", a.Value));
                if (pbdFilesBuilder.Length != 0)
                    pbdFilesBuilder.Remove(0, 1);

            }

            var pbdFiles = pbdFilesBuilder.ToString();

            OctapusLog logger = new OctapusLog("OctapusLogger", this);
            
            try
            {
                
                var workSpace = new SourceSafeProvider(dataManager, WorkspaceToBuild, this);
                

                if (stopIfCheckedOut && (workSpace.CheckObjectsStatus(workspaceParams.VssProductPath) || workSpace.CheckObjectsStatus(workspaceParams.VssCommonPath)))
                    if (MessageBox.Show("There are checked out objects on Source Safe! Continue?", "Octapus", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    {
                        isBusy = false;
                        return;
                    }

                var dataManagerFactory = new DataManagerFactory();
                var applicationParams = dataManager.GetApplicationParams();
                var versionParameters = dataManager.GetVersionParams(WorkspaceToBuild);

                var buildParameters = new BuildTaskParameters(WorkspaceToBuild);
                var buildTask = new BuildTask(buildParameters, dataManager, workSpace, this);
                buildParameters.LoadConfiguration("WorkspaceProfile.config", WorkspaceToBuild, workspaceParams.ConfigurationMode, workspaceParams.DBConnectionString);

                buildParameters.PrepareWorkableClient = PrepareWorkableClient;
                var builder = new BuildMachine(dataManager, workSpace, this);
                var buildResult = builder.Build(buildTask);
                
                logger.LogInfo(WorkspaceToBuild + " BUILD STARTED ");

                buildTask.Build(buildMode, pbdFiles);

                if (prepareClient)
                {
                    var buildFolder = buildParameters.BuildPrefix + versionParameters.BuildSeqNo;
                    var buildClientFolder = Path.Combine(buildParameters.BuildsFolder, Path.Combine(buildFolder, buildParameters.BuildClientFolderName));
                    if (!Directory.Exists(buildClientFolder)) Directory.CreateDirectory(buildClientFolder);
                    Packager.CopyPromotion(buildParameters.BuildOutFolder, buildClientFolder, versionParameters.BuildSeqNo);
                }
                System.GC.Collect();
                                
                logger.LogInfo(WorkspaceToBuild + " BUILD FINISHED");

                logger.LogInfo(WorkspaceToBuild + " OCTAPUS PROCESS COMPLETED");
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                logger.LogInfo(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Build has finished","Octapus");
            IsBusy = false;
        }
        #endregion
    }
}
