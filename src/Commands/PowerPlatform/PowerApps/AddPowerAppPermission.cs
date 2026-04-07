using Microsoft.SharePoint.Client;
using PnP.PowerShell.Commands.Base;
using PnP.PowerShell.Commands.Base.PipeBinds;
using PnP.PowerShell.Commands.Utilities;
using PnP.PowerShell.Commands.Utilities.REST;
using System;
using System.Management.Automation;

namespace PnP.PowerShell.Commands.PowerPlatform.PowerApps
{
    [Cmdlet(VerbsCommon.Add, "PnPPowerAppPermission")]
    public class AddPowerAppPermission : PnPAzureManagementApiCmdlet
    {
        [Parameter(Mandatory = false)]
        public PowerPlatformEnvironmentPipeBind Environment;

        [Parameter(Mandatory = true)]
        public PowerAppPipeBind Identity;

        [Parameter(Mandatory = false)]
        public string User;

        [Parameter(Mandatory = false)]
        public string Group;

        [Parameter(Mandatory = false)]
        public SwitchParameter Tenant;

        [Parameter(Mandatory = true)]
        [ValidateSet("CanView", "CanEdit")]
        public string RoleName;

        [Parameter(Mandatory = false)]
        public SwitchParameter AsAdmin;

        [Parameter(Mandatory = false)]
        public SwitchParameter Force;

        [Parameter(Mandatory = false)]
        public SwitchParameter Notify;

        protected override void ExecuteCmdlet()
        {
            var environmentName = ParameterSpecified(nameof(Environment)) ? Environment.GetName() : PowerPlatformUtility.GetDefaultEnvironment(ArmRequestHelper, Connection.AzureEnvironment)?.Name;

            if (string.IsNullOrEmpty(environmentName))
                throw new PSArgumentException("Environment not found.", nameof(Environment));

            var appName = Identity.GetName();
            if (string.IsNullOrEmpty(appName))
            {
                throw new PSArgumentException("PowerApp not found.", nameof(Identity));
            }

            if (string.IsNullOrEmpty(User) && string.IsNullOrEmpty(Group) && !Tenant.IsPresent)
                throw new PSArgumentException("Either User, Group, or Tenant must be specified.");

            if ((Tenant.IsPresent && (!string.IsNullOrEmpty(User) || !string.IsNullOrEmpty(Group))) || (!string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Group)))
            {
                throw new PSArgumentException("Specify only one of User, Group, or Tenant.");
            }

            string graphAccessToken = TokenHandler.GetAccessToken($"https://{Connection.GraphEndPoint}/.default", Connection);
            WriteVerbose("Microsoft Graph access token acquired");

            var graphRequestHelper = new ApiRequestHelper(GetType(), Connection, $"https://{Connection.GraphEndPoint}/.default");

            string tenantGuid = TenantExtensions.GetTenantIdByUrl(Connection.Url, Connection.AzureEnvironment);

            string entityId = null;

            if (!string.IsNullOrEmpty(User))
            {
                WriteVerbose("Processing User parameter");
                Model.AzureAD.User graphUser;

                if (Guid.TryParse(User, out Guid userGuid))
                {
                    WriteVerbose($"Looking up user through Microsoft Graph by user id {userGuid}");
                    graphUser = Utilities.AzureAdUtility.GetUser(graphAccessToken, userGuid, azureEnvironment: Connection.AzureEnvironment);
                }

                else
                {
                    WriteVerbose($"Looking up user through Microsoft Graph by user principal name {User}");
                    graphUser = Utilities.AzureAdUtility.GetUser(graphAccessToken, User, azureEnvironment: Connection.AzureEnvironment);
                }

                if (graphUser == null)
                {
                    throw new PSArgumentException("User not found.", nameof(User));
                }

                entityId = graphUser.Id.ToString();
            }

            else if (!string.IsNullOrEmpty(Group))
            {
                WriteVerbose("Processing Group parameter");
                var graphGroup = Guid.TryParse(Group, out Guid groupGuid)
                    ? Utilities.AzureADGroupsUtility.GetGroup(graphRequestHelper, groupGuid)
                    : Utilities.AzureADGroupsUtility.GetGroup(graphRequestHelper, Group);

                if (graphGroup == null)
                    throw new PSArgumentException("Group not found.", nameof(Group));

                entityId = graphGroup.Id.ToString();
            }

            string notifyOption = Notify.IsPresent ? "Notify" : "DoNotNotify";
            string principalType = Tenant.IsPresent ? "Tenant" : !string.IsNullOrEmpty(Group) ? "Group" : "User";

            var payload = new
            {
                put = new[]
                {
                    new
                    {
                        properties = new
                        {
                            NotifyShareTargetOption = notifyOption,
                            roleName = RoleName,
                            principal = new
                            {
                                id = entityId,
                                type = principalType,
                                tenantId = tenantGuid
                            }
                        }
                    }
                }
            };

            if (Force || ShouldContinue($"Add PowerApp permission '{RoleName}' for '{entityId}' on app '{appName}'?", Properties.Resources.Confirm))
            {
                try
                {
                    string baseUrl = PowerPlatformUtility.GetPowerAppsEndpoint(Connection.AzureEnvironment);
                    WriteVerbose($"Adding '{RoleName}' permission for entity {entityId} to PowerApp {appName} in environment {environmentName}");
                    PowerAppsRequestHelper.Post($"{baseUrl}/providers/Microsoft.PowerApps{(AsAdmin ? "/scopes/admin/environments/" + environmentName : "")}/apps/{appName}/modifyPermissions?api-version=2022-11-01", payload);
                }
                catch (Exception ex)
                {
                    throw new PSInvalidOperationException($"Failed to add PowerApp permission: {ex.Message}");
                }
            }
        }
    }
}