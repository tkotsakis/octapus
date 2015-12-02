using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.SourceSafe.Interop;
using System.IO;
using System.Security.AccessControl;
using Relational.Octapus.Persistence;


namespace Workspace.SourceSafe
{
    public class SourceSafeControl
    {
        private VSSDatabase vssDB;
        private OctapusLog logger;
        string dbaseIniFile;
        bool checkOutItemsExist = false,isConnected = false;

        public SourceSafeControl(string iniFile, IHookOctapusLogger hookLogger = null)
        {
            this.dbaseIniFile = iniFile;
            this.vssDB = new VSSDatabase();
            this.logger = new OctapusLog("OctapusLogger", hookLogger);
        }

        public void Connect(string userName, string passwd) 
        {
            vssDB.Open(this.dbaseIniFile, userName, passwd);
        }

        public void Disconnect()
        {
            vssDB.Close();
        }

        public bool IsConnected(string userName, string passwd)
        {
           return false;
        }

        public bool IsItemCheckedOut(string vssObject)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssObject, false);
            return vssItem.IsCheckedOut == 0 ? true : false;
        }

        public int VersionNumber(string vssObject)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssObject, false);
            return vssItem.VersionNumber;
        }

        public void SetCurrentProject(string projectPath)
        {
            vssDB.CurrentProject = projectPath;
        }

        public string GetLastCheckInDate(string vssObject)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssObject, false);
            return vssItem.VSSVersion.Date.ToString();
        }

        public string CheckIn(string vssObject,string comment)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssObject, false);
            try
            {

                if (vssItem.IsCheckedOut != 0)
                    vssItem.Checkin(comment,vssItem.LocalSpec,0);
                else
                    return "Item " + vssObject + " is not checked out";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }

        public string CheckOut(string vssObject,string comment)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssObject, false);
            try
            {
                if (vssItem.IsCheckedOut == 0)
                    vssItem.Checkout(comment, vssItem.LocalSpec, 0);
                else
                    return "Item " + vssObject + " is already checked out";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }

        public string UndoCheckOut(string vssObject)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssObject, false);
            try
            {
                if (vssItem.IsCheckedOut != 0)
                    vssItem.UndoCheckout();
                else
                    return "Item " + vssObject + " is not checked out";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }

        public string GetLatestFile(string vssProject, string vssFile, string targetPath)
        {
            string vssObject = String.Concat(vssProject, vssFile);
            IVSSItem vssItem = vssDB.get_VSSItem(vssObject, false);

            try
            {
                if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
                vssItem.Get(String.Concat(targetPath, @"\", vssFile));
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }

        public string GetLatestProject(string vssProject, string targetPath)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssProject, false);
            int a = vssItem.Type;
            try
            {
                if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
                
                if (a == 0)
                {
                    foreach (IVSSItem i in vssItem.Items)
                    {
                        i.Get(String.Concat(targetPath, @"\" , i.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }

        public string CheckOutProject(string vssProject,string comment)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssProject, false);
            int a = vssItem.Type;
            try
            {
                if (a == 0)
                {
                    foreach (IVSSItem i in vssItem.Items)
                    {
                            this.CheckOut(i.Name,comment);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }

        public string CheckInProject(string vssProject,string comment)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssProject, false);
            int a = vssItem.Type;
            try
            {
                if (a == 0)
                {
                    foreach (IVSSItem i in vssItem.Items)
                    {
                        this.CheckIn(i.Name,comment);
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }

        public bool GetLatest (string vssProject, string targetPath)
        {
            var result = true;
            try
            {
                IVSSItem vssItem = vssDB.get_VSSItem(vssProject, false);
                IVSSItems childItems = vssItem.get_Items(false);

                foreach (VSSItem childItem in childItems)
                {
                    if (!childItem.Name.StartsWith("DB_") && !childItem.Name.StartsWith("cms_") && !childItem.Name.StartsWith("Rule"))
                    {
                        childItem.Get(String.Concat(targetPath, @"\", childItem.Name));
                    }
                }
            }
            catch (Exception)
            {
                result = false;
                return result;
            }
            return result;

        }

        public void GetCompleteCode(string vssProject, string targetPath)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssProject, false);
            IVSSItems childItems = vssItem.get_Items(false);

            foreach (VSSItem childItem in childItems)
            {
                if (!childItem.Name.StartsWith("DB_") && !childItem.Name.StartsWith("cms_") && !childItem.Name.StartsWith("Rule"))
                {
                    childItem.Get(String.Concat(targetPath, @"\", childItem.Name));
                }
            }
        }

        public void GetSpecificFiles(string vssProject, string targetPath, string extension)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssProject, false);

            foreach (IVSSItem i in vssItem.Items)
            {
                if (!i.Name.StartsWith("DB_") && !i.Name.StartsWith("cms_") && !i.Name.StartsWith("Rule") && !i.Name.EndsWith("eascbs"))
                {
                    this.GetLatestFile(vssProject, i.Name + @"\" + i.Name + extension, targetPath);
                    logger.LogInfo("Getting " + i.Name + extension);
                }
            }

        }

        public void UpdateFileToSourceControl(string vssFile, string text)
        {
            File.SetAttributes(vssFile, FileAttributes.Normal);
            using (StreamWriter writer = new StreamWriter(@vssFile, false))
            {
                writer.WriteLine(text);
            }
        }

        public string SetWorkingFolder(string vssProject, string localFolder)
        {
            try
            {
                IVSSItem vssItem = vssDB.get_VSSItem(vssProject, false);
                vssItem.LocalSpec = localFolder;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }

        public bool CheckOutObjectsExist(string vssProject)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssProject, false);

            foreach (IVSSItem i in vssItem.get_Items(false))
            {
                ListCheckedOutItems(vssItem);
                checkOutItemsExist = true;
                break;
            }
            return checkOutItemsExist;
        }

        public void ListCheckedOutItems(IVSSItem item)
        {
            foreach (IVSSItem i in item.get_Items(false))
            {
                if (i.Type == 1 && i.IsCheckedOut != 0)
                {
                    foreach (IVSSCheckout vssCheckout in i.Checkouts)
                    {
                        logger.LogInfo("File : " +  i.Spec);
                        logger.LogInfo("Checked out to : " + vssCheckout.Username);
                        logger.LogInfo("Date           : " +  vssCheckout.Date);
                        logger.LogInfo("VersionNumber  : " +  vssCheckout.VersionNumber);
                    }
                }
                else if (i.Type == 0 && i.Items.Count != 0)
                    ListCheckedOutItems(i);
            }
        }

        public bool CheckOutObjectsList(string vssProject, List<VSSItemInfo> chekcedOutList)
        {
            IVSSItem vssItem = vssDB.get_VSSItem(vssProject, false);
            //List<VSSItemInfo> chekcedOutList = new List<VSSItemInfo>();
            GetListCheckedOutItems(vssItem, ref chekcedOutList);
            return (chekcedOutList.Count == 0 ? false : true);
        }

        private void GetListCheckedOutItems(IVSSItem item, ref List<VSSItemInfo> chekcedOutList)
        {
            foreach (IVSSItem i in item.get_Items(false))
            {
                if (i.Type == 1 && i.IsCheckedOut != 0)
                {
                    foreach (IVSSCheckout vssCheckout in i.Checkouts)
                    {
                        chekcedOutList.Add(
                            new VSSItemInfo (i.Spec.ToString(),true, vssCheckout.Username,vssCheckout.VersionNumber,vssCheckout.Date.ToString(),vssCheckout.Machine)
                            );
                    }
                }
                else if (i.Type == 0 && i.Items.Count != 0)
                    GetListCheckedOutItems(i, ref chekcedOutList);
            }
        }

        public void AddItemToProject(string vssProject, string localProject, string vssObjectFile,string comment)
        {
            try
            {
                IVSSItem vssItem = vssDB.get_VSSItem(vssProject, false);
                vssItem.LocalSpec = localProject;

                string vssObject = String.Concat(localProject, @"\", vssObjectFile);
                vssItem.Add(vssObject, comment);
            }
            catch (Exception)
            {
                
                throw;
            } 

        }

        public void RemoveItemFromProject(string vssObject)
        {
            try
            {
                IVSSItem vssItem = vssDB.get_VSSItem(vssObject, false);
                vssItem.Deleted = true;
            }
            catch (Exception)
            {

                throw;
            }

        }

    }
}
