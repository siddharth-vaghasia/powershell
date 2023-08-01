﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using Microsoft.SharePoint.Client;
using PnP.Framework.Utilities;
using PnP.PowerShell.Commands.Base.PipeBinds;
using Folder = Microsoft.SharePoint.Client.Folder;

namespace PnP.PowerShell.Commands.Files
{
    [Cmdlet(VerbsCommon.Get, "PnPFolderFolder", DefaultParameterSetName = ParameterSet_FOLDERSBYPIPE)]
    [OutputType(typeof(IEnumerable<Folder>))]
    public class GetFolderFolder : PnPWebRetrievalsCmdlet<Folder>
    {
        private const string ParameterSet_FOLDERSBYPIPE = "Folder via pipebind";
        private const string ParameterSet_FOLDERBYURL = "Folder via url";

        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 0, ParameterSetName = ParameterSet_FOLDERBYURL)]
        public string FolderSiteRelativeUrl;

        [Parameter(Mandatory = false, ValueFromPipeline = true, Position = 0, ParameterSetName = ParameterSet_FOLDERSBYPIPE)]
        public FolderPipeBind Identity;

        [Parameter(Mandatory = false)]
        public string ItemName = string.Empty;

        [Parameter(Mandatory = false, Position = 4)]
        public SwitchParameter Recursive;

        [Parameter(Mandatory = false)]
        public SwitchParameter ExcludeSystemFolders;        

        protected override void ExecuteCmdlet()
        {
            if(ExcludeSystemFolders.ToBool())
            {
                DefaultRetrievalExpressions = new Expression<Func<Folder, object>>[] { f => f.ListItemAllFields };
            }

            var contents = GetContents(FolderSiteRelativeUrl);

            if (!string.IsNullOrEmpty(ItemName))
            {
                contents = contents.Where(f => f.Name.Equals(ItemName, StringComparison.InvariantCultureIgnoreCase));
            }

            WriteObject(contents, true);
        }

        private IEnumerable<Folder> GetContents(string FolderSiteRelativeUrl)
        {
            Folder targetFolder = null;
            if (string.IsNullOrEmpty(FolderSiteRelativeUrl) && ParameterSetName == ParameterSet_FOLDERSBYPIPE && Identity != null)
            {
                targetFolder = Identity.GetFolder(CurrentWeb);
                CurrentWeb.EnsureProperty(w => w.ServerRelativeUrl);
            }
            else
            {
                string serverRelativeUrl = null;
                if (!string.IsNullOrEmpty(FolderSiteRelativeUrl))
                {
                    var webUrl = CurrentWeb.EnsureProperty(w => w.ServerRelativeUrl);
                    serverRelativeUrl = UrlUtility.Combine(webUrl, FolderSiteRelativeUrl);
                }

                targetFolder = (string.IsNullOrEmpty(FolderSiteRelativeUrl)) ? CurrentWeb.RootFolder : CurrentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(serverRelativeUrl));
            }

            IEnumerable<Folder> folders = ClientContext.LoadQuery(targetFolder.Folders.IncludeWithDefaultProperties(RetrievalExpressions)).Where(f => !ExcludeSystemFolders.ToBool() || !f.ListItemAllFields.ServerObjectIsNull.GetValueOrDefault(false)).OrderBy(f => f.Name);
            ClientContext.ExecuteQueryRetry();        

            IEnumerable<Folder> folderContent = folders;

            if (Recursive && folders.Count() > 0)
            {
                foreach (var folder in folders)
                {
                    var relativeUrl = folder.ServerRelativeUrl.Replace(CurrentWeb.ServerRelativeUrl, "");

                    WriteVerbose($"Processing folder {relativeUrl}");
                    
                    var subFolderContents = GetContents(relativeUrl);
                    folderContent = folderContent.Concat<Folder>(subFolderContents);
                }
            }

            return folderContent;
        }
    }
}
