using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Structures;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LogicBuilder.App.AI.Utils
{
    public class AgentHandler(IInlineCitationFormatter inlineCitationFormatter) : IAgentHandler
    {
        public async Task<string> InitializeSession(AIAgent agent)
        {
            ChatClientAgentSession session = (ChatClientAgentSession)await agent.CreateSessionAsync();
            return (await agent.SerializeSessionAsync(session)).GetRawText();
        }

        public async Task<AgentStreamResult> SendMessageToAgent(AIAgent agent, string threadId, string userMessage, CancellationToken cancellationToken = default)
        {
            using var doc = JsonDocument.Parse(threadId);
            AgentSession session = await agent.DeserializeSessionAsync(doc.RootElement, cancellationToken: cancellationToken);

            var message = new ChatMessage(ChatRole.User, userMessage);

            AgentResponse agentResponse = await agent.RunAsync(message, session);
            string updatedForCitations = inlineCitationFormatter.FormatWithInlineLinks
            (
                agentResponse.Text,
                [.. agentResponse.Messages.SelectMany(m => m.Contents).SelectMany(content => content.Annotations ?? []).OfType<CitationAnnotation>()]
            );
            JsonElement updatedJson = await agent.SerializeSessionAsync(session, cancellationToken: cancellationToken);

            return new AgentStreamResult(updatedForCitations, updatedJson.GetRawText());
        }

        public async IAsyncEnumerable<AgentStreamResult> SendMessageToAgentWithStreamingResponse(AIAgent agent, string threadId, string userMessage, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var doc = JsonDocument.Parse(threadId);
            AgentSession session = await agent.DeserializeSessionAsync(doc.RootElement, cancellationToken: cancellationToken);

            var message = new ChatMessage(ChatRole.User, userMessage);

            await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(message, session, cancellationToken: cancellationToken))
            {
                if (string.IsNullOrEmpty(update.Text))
                    continue;

                string updatedForCitations = inlineCitationFormatter.FormatWithInlineLinks
                (
                    update.Text,
                    [.. update.Contents.SelectMany(content => content.Annotations ?? []).OfType<CitationAnnotation>()]
                );
                yield return AgentStreamResult.FromChunk(updatedForCitations);
            }

            JsonElement updatedJson = await agent.SerializeSessionAsync(session, cancellationToken: cancellationToken);

            yield return AgentStreamResult.FromState(updatedJson.GetRawText());
        }
    }
}
