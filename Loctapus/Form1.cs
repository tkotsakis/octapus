using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Relational.Octapus.Common;
using System.Diagnostics;
using Relational.Octapus.Persistence;
using Relational.Octapus.Workspace.SourceSafe;
using Relational.Octapus.Workspace;
using Relational.Octapus.BuildEngine;
using NLog.Targets;
using NLog;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Loctapus
{
    public partial class Form1 : Form
    {
        string version, applicationId;
        private List<PBLibrary> libraryList;
        private IWorkspaceProvider workspace;
        private OctapusLog logger;
        DataManagerFactory dataManagerFactory;
        IDataManager dataManager;
        List<string> availableWorkspaces;
        string message = "", pbdFiles="",path="";
        ApplicationParams applicationParams;
        WorkspaceParams workspaceParams;
        VersionParams versionParams;
        BuildTask buildTask;

        public Form1()
        {
            InitializeComponent();
            dataManagerFactory = new DataManagerFactory();
            dataManager = dataManagerFactory.New();
            logger = new OctapusLog("WindowLogger");
            availableWorkspaces = dataManager.GetAvailableWorkspaces();
            listBox1.DataSource = availableWorkspaces;
            
            FormControlTarget target = new FormControlTarget();
            target.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";
            target.ControlName = "textBox4";
            target.FormName = "Form1";
            target.Append = true;

            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, LogLevel.Trace);
            Logger logger1 = LogManager.GetLogger("WindowLogger");
            logger1.Info("info log message, " + Environment.NewLine);
            logger.LogInfo(AppDomain.CurrentDomain.BaseDirectory);

            ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
            ToolTip1.SetToolTip(this.button3, "RootPath");

            ToolTip ToolTip2 = new System.Windows.Forms.ToolTip();
            ToolTip2.SetToolTip(this.button4, "ExtractionPath");

            ToolTip ToolTip3 = new System.Windows.Forms.ToolTip();
            ToolTip2.SetToolTip(this.button2, "Put Version Information to Libraries");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            version = Guid.NewGuid().ToString();
            textBox1.Text = version;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Build();
        }

        private void Build()
        {
            try
            {
                string scriptPath = "";
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                version = Guid.NewGuid().ToString();
                textBox1.Text = version;

                applicationId = listBox1.GetItemText(listBox1.SelectedItem);

                if (String.IsNullOrWhiteSpace(applicationId))
                {
                    logger.LogInfo("No Workspace Defined!");
                    return;
                }

                if (String.IsNullOrWhiteSpace(textBox3.Text))
                {
                    scriptPath = applicationParams.TempPath + "InsertData.sql";
                    textBox3.Text = scriptPath;
                }
                else
                {
                    scriptPath = textBox3.Text + @"\InsertData.sql";
                }

                for (int i = 0; i < this.libraryList.Count(); i++)
                {
                    if (!listBox2.SelectedItems.Contains(libraryList[i].LibraryName)) continue;
                    if (this.LibraryList[i].LocalFullPath.EndsWith(".pbd")) continue;
                    var fullPath = this.LibraryList[i].LocalFullPath.Replace("/", @"\");

                    Commander.Exec(applicationParams.TargetPath, "bversion.exe", "PUT_IDE " + fullPath + " " + version, ref message);

                    pbdFiles += libraryList[i].LibraryNameWithExtension + ",";
                    logger.LogInfo("Implant Library " + this.LibraryList[i].LocalFullPath + " " + message);
                }
                if (pbdFiles.EndsWith(",")) pbdFiles = pbdFiles.Substring(0, pbdFiles.Length - 1);

                buildTask.InsertBuildData(BuildMode.SpecificPbds, scriptPath, version, LibraryList, versionParams,applicationId,ref pbdFiles);

                stopwatch.Stop();
                logger.LogInfo("Time elapsed: " + stopwatch.Elapsed);

                MessageBox.Show("Implant Version Finished");
            }
            catch (Exception ex)
            {
                
                 logger.LogInfo(ex.Message + Misc.GetLineNumber(ex));
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            applicationId = listBox1.GetItemText(listBox1.SelectedItem);
            this.LoadWorkspace(applicationId);
            libraryList = workspace.GetLocalLibrarylist(applicationId, workspaceParams.LocalPBTPath, workspaceParams.LibraryTargetPath);
            libraryList = (
            from o in libraryList
            orderby o.LibraryName, o.LibraryFullPath descending
            group o by o.LibraryName into g
            select g.First()).ToList();

            libraryList.RemoveAll(x=> x.LibraryNameWithExtension.EndsWith(".pbd"));
            listBox2.DataSource = libraryList.Select(x => x.LibraryName).ToList();
            textBox4.Text = String.Empty;

        }

        public void LoadWorkspace(string applicationId)
        {
            path = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(path))
            {
                Environment.CurrentDirectory = path;
                applicationParams = dataManager.GetApplicationParams(path);
                workspaceParams = dataManager.GetWorkspaceParamsPerPath(applicationId, path);
                versionParams = dataManager.GetVersionParams(applicationId, path);
            }
            else
            {
                applicationParams = dataManager.GetApplicationParams();
                workspaceParams = dataManager.GetWorkspaceParams(applicationId);
                versionParams = dataManager.GetVersionParams(applicationId);
            }
            workspace = new SourceSafeProvider(dataManager, applicationId, "LOCAL");
            buildTask = new BuildTask();
            if (!Directory.Exists(applicationParams.TempPath)) Directory.CreateDirectory(applicationParams.TempPath);
            textBox3.Text = applicationParams.TempPath;
        }

        public ApplicationParams ApplicationParams { get { return applicationParams; } set { } }
        public WorkspaceParams WorkspaceParams { get { return workspaceParams; } set { } }
        public VersionParams VersionParams { get { return versionParams; } set { } }
        public List<PBLibrary> LibraryList { get { return this.libraryList; } set { } }

        private void checkBoxAll_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAll.Checked)
            {
                for (int i = 0; i < listBox2.Items.Count; i++)
                {
                    listBox2.SetSelected(i, true);
                }
            }
            else
            {
                for (int i = 0; i < listBox2.Items.Count; i++)
                {
                    listBox2.SetSelected(i, false);
                }
            }
        }

    }
}
