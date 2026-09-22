using LogicBuilder.App.AI.Utils.Interfaces;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Moq;
using Moq.Protected;
using System.Text.Json;

namespace LogicBuilder.App.AI.Utils.Tests
{
    public class AgentHandlerTest
    {
        private readonly Mock<IInlineCitationFormatter> citationFormatterMock = new();
        private readonly AgentHandler handler;

        public AgentHandlerTest()
        {
            handler = new AgentHandler(citationFormatterMock.Object);
        }

        [Fact]
        public async Task InitializeSession_Throws_WhenCreatedSessionIsNotChatClientAgentSession()
        {
            // ChatClientAgentSession is sealed with an internal constructor, so it cannot be produced
            // outside of the Microsoft.Agents.AI assembly. This exercises the invalid-cast flow path.
            var agentMock = new Mock<AIAgent>();
            var fakeSession = new Mock<AgentSession>().Object;

            agentMock
                .Protected()
                .Setup<ValueTask<AgentSession>>("CreateSessionCoreAsync", ItExpr.IsAny<CancellationToken>())
                .Returns(new ValueTask<AgentSession>(fakeSession));

            await Assert.ThrowsAsync<InvalidCastException>(() => handler.InitializeSession(agentMock.Object));
        }

        [Fact]
        public async Task SendMessageToAgent_ReturnsFormattedTextAndSerializedState()
        {
            var agentMock = new Mock<AIAgent>();
            var fakeSession = new Mock<AgentSession>().Object;
            var agentResponse = new AgentResponse(new ChatMessage(ChatRole.Assistant, "raw response text"));
            var serializedState = JsonDocument.Parse("""{"conversationId":"abc"}""").RootElement;

            agentMock
                .Protected()
                .Setup<ValueTask<AgentSession>>("DeserializeSessionCoreAsync", ItExpr.IsAny<JsonElement>(), ItExpr.IsAny<JsonSerializerOptions>(), ItExpr.IsAny<CancellationToken>())
                .Returns(new ValueTask<AgentSession>(fakeSession));

            agentMock
                .Protected()
                .Setup<Task<AgentResponse>>("RunCoreAsync", ItExpr.IsAny<IEnumerable<ChatMessage>>(), ItExpr.IsAny<AgentSession>(), ItExpr.IsAny<AgentRunOptions>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(agentResponse);

            agentMock
                .Protected()
                .Setup<ValueTask<JsonElement>>("SerializeSessionCoreAsync", ItExpr.IsAny<AgentSession>(), ItExpr.IsAny<JsonSerializerOptions>(), ItExpr.IsAny<CancellationToken>())
                .Returns(new ValueTask<JsonElement>(serializedState));

            citationFormatterMock
                .Setup(f => f.FormatWithInlineLinks("raw response text", It.IsAny<ICollection<CitationAnnotation>>()))
                .Returns("formatted response text");

            var result = await handler.SendMessageToAgent(agentMock.Object, "{}", "hi there", CancellationToken.None);

            Assert.Equal("formatted response text", result.ContentChunk);
            Assert.Equal(serializedState.GetRawText(), result.UpdatedThreadId);
            citationFormatterMock.Verify(f => f.FormatWithInlineLinks("raw response text", It.IsAny<ICollection<CitationAnnotation>>()), Times.Once);
        }

        [Fact]
        public async Task SendMessageToAgent_PassesExtractedCitationAnnotations_ToFormatter()
        {
            var agentMock = new Mock<AIAgent>();
            var fakeSession = new Mock<AgentSession>().Object;

            var citation = new CitationAnnotation
            {
                Title = "doc.pdf",
                Url = new Uri("https://example.com/doc.pdf"),
                AnnotatedRegions = [new TextSpanAnnotatedRegion { StartIndex = 0, EndIndex = 1 }]
            };
            var textContent = new TextContent("raw response text")
            {
                Annotations = [citation]
            };
            var agentResponse = new AgentResponse(new ChatMessage(ChatRole.Assistant, [textContent]));
            var serializedState = JsonDocument.Parse("{}").RootElement;

            agentMock
                .Protected()
                .Setup<ValueTask<AgentSession>>("DeserializeSessionCoreAsync", ItExpr.IsAny<JsonElement>(), ItExpr.IsAny<JsonSerializerOptions>(), ItExpr.IsAny<CancellationToken>())
                .Returns(new ValueTask<AgentSession>(fakeSession));

            agentMock
                .Protected()
                .Setup<Task<AgentResponse>>("RunCoreAsync", ItExpr.IsAny<IEnumerable<ChatMessage>>(), ItExpr.IsAny<AgentSession>(), ItExpr.IsAny<AgentRunOptions>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(agentResponse);

            agentMock
                .Protected()
                .Setup<ValueTask<JsonElement>>("SerializeSessionCoreAsync", ItExpr.IsAny<AgentSession>(), ItExpr.IsAny<JsonSerializerOptions>(), ItExpr.IsAny<CancellationToken>())
                .Returns(new ValueTask<JsonElement>(serializedState));

            ICollection<CitationAnnotation>? capturedCitations = null;
            citationFormatterMock
                .Setup(f => f.FormatWithInlineLinks(It.IsAny<string>(), It.IsAny<ICollection<CitationAnnotation>>()))
                .Callback<string, ICollection<CitationAnnotation>>((_, citations) => capturedCitations = citations)
                .Returns("formatted");

            await handler.SendMessageToAgent(agentMock.Object, "{}", "hi there", CancellationToken.None);

            Assert.NotNull(capturedCitations);
            Assert.Single(capturedCitations!);
            Assert.Same(citation, capturedCitations!.First());
        }

        [Fact]
        public async Task SendMessageToAgentWithStreamingResponse_YieldsChunksThenFinalState_SkippingEmptyUpdates()
        {
            var agentMock = new Mock<AIAgent>();
            var fakeSession = new Mock<AgentSession>().Object;
            var serializedState = JsonDocument.Parse("""{"conversationId":"abc"}""").RootElement;

            agentMock
                .Protected()
                .Setup<ValueTask<AgentSession>>("DeserializeSessionCoreAsync", ItExpr.IsAny<JsonElement>(), ItExpr.IsAny<JsonSerializerOptions>(), ItExpr.IsAny<CancellationToken>())
                .Returns(new ValueTask<AgentSession>(fakeSession));

            agentMock
                .Protected()
                .Setup<IAsyncEnumerable<AgentResponseUpdate>>("RunCoreStreamingAsync", ItExpr.IsAny<IEnumerable<ChatMessage>>(), ItExpr.IsAny<AgentSession>(), ItExpr.IsAny<AgentRunOptions>(), ItExpr.IsAny<CancellationToken>())
                .Returns(GetUpdatesAsync());

            agentMock
                .Protected()
                .Setup<ValueTask<JsonElement>>("SerializeSessionCoreAsync", ItExpr.IsAny<AgentSession>(), ItExpr.IsAny<JsonSerializerOptions>(), ItExpr.IsAny<CancellationToken>())
                .Returns(new ValueTask<JsonElement>(serializedState));

            citationFormatterMock
                .Setup(f => f.FormatWithInlineLinks(It.IsAny<string>(), It.IsAny<ICollection<CitationAnnotation>>()))
                .Returns<string, ICollection<CitationAnnotation>>((text, _) => $"formatted:{text}");

            var results = new List<Structures.AgentStreamResult>();
            await foreach (var chunk in handler.SendMessageToAgentWithStreamingResponse(agentMock.Object, "{}", "hi there", CancellationToken.None))
            {
                results.Add(chunk);
            }

            Assert.Equal(3, results.Count);

            Assert.Equal("formatted:first chunk", results[0].ContentChunk);
            Assert.Null(results[0].UpdatedThreadId);

            Assert.Equal("formatted:second chunk", results[1].ContentChunk);
            Assert.Null(results[1].UpdatedThreadId);

            Assert.Null(results[2].ContentChunk);
            Assert.Equal(serializedState.GetRawText(), results[2].UpdatedThreadId);

            citationFormatterMock.Verify(f => f.FormatWithInlineLinks(It.IsAny<string>(), It.IsAny<ICollection<CitationAnnotation>>()), Times.Exactly(2));

            static async IAsyncEnumerable<AgentResponseUpdate> GetUpdatesAsync()
            {
                yield return new AgentResponseUpdate(ChatRole.Assistant, "first chunk");
                yield return new AgentResponseUpdate(ChatRole.Assistant, string.Empty);
                yield return new AgentResponseUpdate(ChatRole.Assistant, (string?)null);
                yield return new AgentResponseUpdate(ChatRole.Assistant, "second chunk");
                await Task.CompletedTask;
            }
        }
    }
}
