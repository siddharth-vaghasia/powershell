using PnP.PowerShell.Commands.Base;
using PnP.PowerShell.Commands.Base.PipeBinds;
using PnP.PowerShell.Commands.Utilities;
using PnP.PowerShell.Commands.Utilities.REST;
using System;
using System.Linq;
using System.Management.Automation;
using System.Web;

namespace PnP.PowerShell.Commands.PowerPlatform.Copilot
{
    [Cmdlet(VerbsCommon.Get, "PnPCopilotAgentList")]
    public class GetPnPCopilotAgentList : PnPAzureManagementApiCmdlet
    {
        [Parameter(Mandatory = false)]
        public PowerPlatformEnvironmentPipeBind Environment;

        [Parameter(Mandatory = false)]
        public CopilotAgentPipeBind Identity;

        protected override void ExecuteCmdlet()
        {
            var environmentName = ParameterSpecified(nameof(Environment)) ? Environment.GetName() : PowerPlatformUtility.GetDefaultEnvironment(ArmRequestHelper, Connection.AzureEnvironment)?.Name;
            string dynamicsScopeUrl = null;
            string baseUrl = PowerPlatformUtility.GetPowerAutomateEndpoint(Connection.AzureEnvironment);
            var environments = ArmRequestHelper.GetResultCollection<Model.PowerPlatform.Environment.Environment>($"{baseUrl}/providers/Microsoft.ProcessSimple/environments?api-version=2016-11-01");
            if (ParameterSpecified(nameof(Environment)))
            {
                LogDebug($"Using environment as provided '{environmentName}'");
                dynamicsScopeUrl = environments.FirstOrDefault(e => e.Properties.DisplayName.ToLower() == environmentName || e.Name.ToLower() == environmentName)?.Properties.LinkedEnvironmentMetadata.InstanceApiUrl;
            }
            else
            {
                dynamicsScopeUrl = environments.FirstOrDefault(e => e.Properties.IsDefault.HasValue && e.Properties.IsDefault == true)?.Properties.LinkedEnvironmentMetadata.InstanceApiUrl;
                if (string.IsNullOrEmpty(environmentName))
                {
                    throw new Exception($"No default environment found, please pass in a specific environment name using the {nameof(Environment)} parameter");
                }

                LogDebug($"Using default environment as retrieved '{environmentName}'");
            }

            var fetchXml = @"
                <fetch mapping='logical' version='1.0'>
                    <entity name='bot'>
                        <attribute name='accesscontrolpolicy' alias='accessControlPolicy' />,
                        <attribute name='applicationmanifestinformation' alias='applicationManifestInformation' />,
                        <attribute name='authenticationmode' alias='authenticationMode' />,
                        <attribute name='authenticationtrigger' alias='authenticationTrigger' />,
                        <attribute name='authorizedsecuritygroupids' alias='authorizedSecurityGroupIds' />,
                        <attribute name='componentidunique' alias='componentIdUnique' />,
                        <attribute name='componentstate' alias='componentState' />,
                        <attribute name='configuration' alias='configuration' />,
                        <attribute name='createdon' alias='createdOn' />,
                        <attribute name='importsequencenumber' alias='importSequenceNumber' />,
                        <attribute name='ismanaged' alias='isManaged' />,
                        <attribute name='language' alias='language' />,
                        <attribute name='modifiedon' alias='botModifiedOn' />,
                        <attribute name='overriddencreatedon' alias='overriddenCreatedOn' />,
                        <attribute name='overwritetime' alias='overwriteTime' />,
                        <attribute name='iconbase64' alias='iconBase64' />,
                        <attribute name='publishedon' alias='publishedOn' />,
                        <attribute name='schemaname' alias='schemaName' />,
                        <attribute name='solutionid' alias='solutionId' />,
                        <attribute name='statecode' alias='stateCode' />,
                        <attribute name='statuscode' alias='statusCode' />,
                        <attribute name='timezoneruleversionnumber' alias='timezoneRuleVersionNumber' />,
                        <attribute name='utcconversiontimezonecode' alias='utcConversionTimezoneCode' />,
                        <attribute name='versionnumber' alias='versionNumber' />,
                        <attribute name='name' alias='name' />,
                        <attribute name='botid' alias='cdsBotId' />,
                        <attribute name='ownerid' alias='ownerId' />,
                        <attribute name='synchronizationstatus' alias='synchronizationStatus' />
                        <link-entity name='systemuser' to='ownerid' from='systemuserid' link-type='inner' >
                            <attribute name='fullname' alias='owner' />
                        </link-entity>
                        <link-entity name='systemuser' to='modifiedby' from='systemuserid' link-type='inner' >
                            <attribute name='fullname' alias='botModifiedBy' />
                        </link-entity>
                    </entity>
                 </fetch>";

            var encodedFetchXml = HttpUtility.UrlEncode(fetchXml);

            var requestUrl = $"{dynamicsScopeUrl}/api/data/v9.1/bots?fetchXml={encodedFetchXml}";

            WriteVerbose("Retrieving list of Copilot agents...");

            try
            {
                var dataverseToken = TokenHandler.GetAccessToken(dynamicsScopeUrl, Connection);
                var result = RestHelper.Get<RestResultCollection<Model.PowerPlatform.CopilotAgent.CopilotAgent>>(Connection.HttpClient, requestUrl, dataverseToken);

                if (ParameterSpecified(nameof(Identity)))
                {
                    var copilotName = Identity.GetName();
                    LogDebug($"Retrieving specific Copilot agent with provided identity '{copilotName}' within environment '{environmentName}'");

                    var copilot = result.Items.FirstOrDefault(c =>
                        c.Name.Equals(copilotName, StringComparison.OrdinalIgnoreCase) ||
                        c.BotId.Equals(copilotName, StringComparison.OrdinalIgnoreCase));

                    WriteObject(copilot, false);
                }
                else
                {
                    LogDebug($"Retrieving all Copilot agents within environment '{environmentName}'");

                    WriteObject(result.Items, true);
                }
            }
            catch (Exception ex)
            {
                throw new PSInvalidOperationException($"Failed to retrieve Copilot agents: {ex.Message}");
            }
        }
    }
}