using LogicBuilder.App.AI.Utils.Structures;
using Microsoft.Agents.AI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LogicBuilder.App.AI.Utils.Interfaces
{
    public interface IAgentHandler
    {
        Task<string> InitializeSession(AIAgent agent);
        Task<AgentStreamResult> SendMessageToAgent(AIAgent agent, string threadId, string userMessage, CancellationToken cancellationToken = default);
        IAsyncEnumerable<AgentStreamResult> SendMessageToAgentWithStreamingResponse(AIAgent agent, string threadId, string userMessage, CancellationToken cancellationToken = default);
    }
}
