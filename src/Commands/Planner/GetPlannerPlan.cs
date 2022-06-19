using PnP.PowerShell.Commands.Attributes;
using PnP.PowerShell.Commands.Base;
using PnP.PowerShell.Commands.Base.PipeBinds;
using PnP.PowerShell.Commands.Utilities;
using System.Management.Automation;

namespace PnP.PowerShell.Commands.Planner
{
    [Cmdlet(VerbsCommon.Get, "PnPPlannerPlan")]
    [RequiredMinimalApiPermissions("Group.Read.All")]
    public class GetPlannerPlan : PnPGraphCmdlet
    {
        private const string ParameterName_BYGROUP = "By Group";
        private const string ParameterName_BYPLANID = "By Plan Id";

        [Parameter(Mandatory = true, HelpMessage = "Specify the group id of group owning the plan.", ParameterSetName = ParameterName_BYGROUP)]
        public PlannerGroupPipeBind Group;

        [Parameter(Mandatory = false, HelpMessage = "Specify the name of the plan.", ParameterSetName = ParameterName_BYGROUP)]
        public PlannerPlanPipeBind Identity;

        [Parameter(Mandatory = true, HelpMessage = "Specify the ID of the plan.", ParameterSetName = ParameterName_BYPLANID)]
        public string Id;

        [Parameter(Mandatory = false)]
        public SwitchParameter ResolveIdentities;

        protected override void ExecuteCmdlet()
        {
            if (ParameterSetName == ParameterName_BYGROUP)
            {
                var groupId = Group.GetGroupId(Connection, AccessToken);
                if (groupId != null)
                {
                    if (ParameterSpecified(nameof(Identity)))
                    {
                        WriteObject(Identity.GetPlanAsync(Connection, AccessToken, groupId, ResolveIdentities).GetAwaiter().GetResult());
                    }
                    else
                    {
                        WriteObject(PlannerUtility.GetPlansAsync(Connection, AccessToken, groupId, ResolveIdentities).GetAwaiter().GetResult(), true);
                    }
                }
                else
                {
                    throw new PSArgumentException("Group not found");
                }
            }
            else
            {
                WriteObject(PlannerUtility.GetPlanAsync(Connection, AccessToken, Id, ResolveIdentities).GetAwaiter().GetResult());
            }
        }
    }
}