namespace PnP.PowerShell.Commands.Base.PipeBinds
{
    public sealed class CopilotAgentPipeBind
    {
        private readonly string _name;
        private readonly Model.PowerPlatform.CopilotAgent.CopilotAgent _copilotAgent;

        public CopilotAgentPipeBind(string input)
        {
            _name = input;
        }

        public CopilotAgentPipeBind(Model.PowerPlatform.CopilotAgent.CopilotAgent copilotAgent)
        {
            _copilotAgent = copilotAgent;
        }

        public string GetName()
        {
            if (_copilotAgent != null)
            {
                return _copilotAgent.Name;
            }

            return _name;
        }
    }
}