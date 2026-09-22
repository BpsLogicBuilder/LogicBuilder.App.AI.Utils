using System.Diagnostics.CodeAnalysis;

namespace LogicBuilder.App.AI.Utils.Requests
{
    [ExcludeFromCodeCoverage]
    public class GetAgentRequest(string agentIdentifier) : IRequest
    {
        public string AgentIdentifier { get; } = agentIdentifier;
    }
}
