using System.Diagnostics.CodeAnalysis;

namespace LogicBuilder.App.AI.Utils.Requests
{
    [ExcludeFromCodeCoverage]
    public class GetAgentRequest(string agentIdentifier, string flowName) : IRequest
    {
        public string AgentIdentifier { get; } = agentIdentifier;
        public string FlowName { get; } = flowName;
    }
}
