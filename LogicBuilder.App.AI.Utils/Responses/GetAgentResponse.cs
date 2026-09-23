using Microsoft.Agents.AI;
using System.Diagnostics.CodeAnalysis;

namespace LogicBuilder.App.AI.Utils.Responses
{
    [ExcludeFromCodeCoverage]
    public class GetAgentResponse : IResponse
    {
        public AIAgent? AIAgent { get; set; }
        public bool Success { get; set; }
    }
}
