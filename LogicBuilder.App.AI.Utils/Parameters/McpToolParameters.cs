using LogicBuilder.App.AI.Utils.Parameters.Interfaces;

namespace LogicBuilder.App.AI.Utils.Parameters
{
    public class McpToolParameters(string serverLabel, string serverUrl, bool alwaysAutoApprove) : IAgentToolParameters
    {
        public string ServerLabel { get; } = serverLabel;
        public string ServerUrl { get; } = serverUrl;
        public bool AlwaysAutoApprove { get; } = alwaysAutoApprove;
    }
}
