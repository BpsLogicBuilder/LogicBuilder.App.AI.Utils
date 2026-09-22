# LogicBuilder.App.AI.Utils

**LogicBuilder.App.AI.Utils** is a .NET library for declaratively building, running, and consuming [Microsoft Agent Foundry](https://learn.microsoft.com/azure/ai-foundry/) AI agents, rather than hand-coding the Azure AI Projects / `Microsoft.Agents.AI` SDK calls directly.

It is intended for applications that need to:

- Define AI agents (name, model, instructions, and tools such as MCP servers or web search) declaratively — e.g. from configuration, a database, or a UI — and materialize those definitions into runnable `AIAgent` instances.
- Create those agents from application configuration (e.g. an `AZURE_AI_FOUNDRY_PROJECT_ENDPOINT` setting) without directly wiring up Azure credentials or the `AIProjectClient`.
- Drive conversations with an `AIAgent` (session/thread initialization, single-shot responses, and streaming responses) using a simple, serializable session/thread identifier that callers can persist between requests.
- Present citation/source references returned by an agent as inline Markdown links within the response text, rather than raw annotation metadata.

## Key Concepts

The library is organized around a few cooperating areas:

### 1. Declarative agent definition (Parameters → Builders)

Two parallel object hierarchies connected by an [AutoMapper](https://automapper.org/) profile:

- **Parameters** (`LogicBuilder.App.AI.Utils.Parameters`) — plain data objects describing *what* an agent should look like:
  - `IAgentParameters` / `AgentParameters` — agent name, model, instructions, and a collection of tool parameters.
  - `IAgentToolParameters` — base interface for tool definitions, implemented by:
    - `McpToolParameters` — describes an MCP (Model Context Protocol) tool/server.
    - `WebSearchToolParameters` — describes a web search tool.

- **Builders** (`LogicBuilder.App.AI.Utils.Builders`) — objects that know *how* to construct the corresponding Azure AI Foundry / Agents SDK artifacts:
  - `IAgentBuilder` / `AgentBuilder` — wraps an `AIProjectClient` plus agent metadata and tool builders, and exposes `Build()` to produce a live `AIAgent`.
  - `IAgentToolBuilder` implementations (e.g. `McpToolBuilder`, `WebSearchToolBuilder`) — each know how to build the corresponding tool definition consumed by the agent.

- **Mapping** (`LogicBuilder.App.AI.Utils.Mapping`) — `ParametersToBuilderMappingProfile` is an AutoMapper `Profile` that maps the parameter hierarchy onto the builder hierarchy (including polymorphic mapping of tool parameter types to their matching tool builder types). An `AIProjectClient` is supplied via the AutoMapper `ResolutionContext.Items` (see `MappingConstants.AI_PROJECT_CLIENT_CONTEXT`) so it can be injected into the constructed `AgentBuilder`.

### 2. Configuration-driven agent creation

- `IAgentCreator` / `AgentCreator` — reads the Foundry project endpoint from `IConfiguration` (`AZURE_AI_FOUNDRY_PROJECT_ENDPOINT`), constructs an `AIProjectClient` using `DefaultAzureCredential`, and uses AutoMapper to turn an `AgentParameters` instance directly into a ready-to-use `AIAgent` via `AgentBuilder.Build()`. This removes the need for calling code to manage Azure credentials or `AIProjectClient` construction itself.

### 3. Conversation/session handling

- `IAgentHandler` / `AgentHandler` — a thin, serialization-friendly wrapper around `AIAgent` conversation APIs:
  - `InitializeSession(AIAgent)` — creates a new `ChatClientAgentSession` for the agent and returns it serialized as a JSON string, suitable for persisting as a "thread ID" between requests.
  - `SendMessageToAgent(...)` — deserializes a previously-saved thread ID, sends a user message to the agent, and returns an `AgentStreamResult` containing the (citation-formatted) response text along with the updated, re-serialized thread ID.
  - `SendMessageToAgentWithStreamingResponse(...)` — same as above, but streams response chunks as they arrive (via `IAsyncEnumerable<AgentStreamResult>`), yielding a final chunk with the updated serialized thread state once streaming completes.
  - `AgentStreamResult` (`LogicBuilder.App.AI.Utils.Structures`) — a simple result type distinguishing streamed content chunks (`FromChunk`) from the final updated thread state (`FromState`).

### 4. Inline citation formatting

- `IInlineCitationFormatter` / `InlineCitationFormatter` — takes agent response text plus its `CitationAnnotation` metadata (character-span based source references) and rewrites the text so each citation span is replaced with an inline Markdown link (e.g. `[contoso-tents-catalog.pdf](https://.../contoso-tents-catalog.pdf)`), using the citation's title/URL to derive a friendly link label. This lets UIs render citations as normal Markdown without needing to understand the underlying annotation model.

## Typical Usage

### Defining and building an agent directly via AutoMapper

1. Construct an `AIProjectClient` (via Azure Identity credentials) for your Foundry project.
2. Build up an `IAgentParameters` object graph describing the agent (name, model, instructions, and tools).
3. Use AutoMapper (configured with `ParametersToBuilderMappingProfile`) to map the parameters into an `IAgentBuilder`/`AgentBuilder`, passing the `AIProjectClient` through the mapping context.
4. Call `Build()` on the resulting `AgentBuilder` to obtain a ready-to-use `AIAgent`.

```c#
IAgentParameters agentParameters = new AgentParameters
( 
    "AgentName", 
    "gpt-5.2", 
    "You are a helpful AI assistant...", 
    [ 
        new McpToolParameters("GreetingTool", "https://example.com/mcp", true), 
        new WebSearchToolParameters() 
    ] 
); 

AgentBuilder agentBuilder = mapper.Map<AgentBuilder> 
( 
    agentParameters, 
    opts => opts.Items[MappingConstants.AI_PROJECT_CLIENT_CONTEXT] = projectClient 
); 

AIAgent agent = agentBuilder.Build();
```

### Creating an agent from configuration with `AgentCreator`
// AZURE_AI_FOUNDRY_PROJECT_ENDPOINT must be present in IConfiguration. 
```c#
IAgentCreator agentCreator = new AgentCreator(configuration, mapper);
AgentParameters agentParameters = new 
( 
    "AgentName", 
    "gpt-5.2", 
    "You are a helpful AI assistant...", 
    [ new WebSearchToolParameters() ] 
);
AIAgent agent = agentCreator.CreateAgent(agentParameters);
```

### Running a conversation with `AgentHandler`
```c#
IAgentHandler agentHandler = new AgentHandler(new InlineCitationFormatter());
// Start a new conversation and persist the returned thread ID (e.g. in a cookie, DB row, etc.). 
string threadId = await agentHandler.InitializeSession(agent);

// Send a message using the persisted thread ID; receive citation-formatted text plus the updated thread ID. 
AgentStreamResult result = await agentHandler.SendMessageToAgent(agent, threadId, "What tents do you offer?"); 
string responseText = result.ContentChunk; threadId = result.UpdatedThreadId;
// Or stream the response as it is generated. 
await foreach (AgentStreamResult chunk in agentHandler.SendMessageToAgentWithStreamingResponse(agent, threadId, "Tell me more.")) 
{ 
    if (chunk.ContentChunk is not null) 
    { // Append chunk.ContentChunk to the UI as it streams in. 
    } 
    else if (chunk.UpdatedThreadId is not null) 
    { threadId = chunk.UpdatedThreadId; 
    } 
}
```


## Solution Layout

| Project | Purpose |
|---|---|
| `LogicBuilder.App.AI.Utils` | Core library (parameters, builders, AutoMapper profile, agent creation, conversation handling, and inline citation formatting) — targets `.NET Standard 2.0`. |
| `LogicBuilder.App.AI.Utils.Tests` | xUnit/Moq test suite validating parameter → builder mapping, configuration-driven agent creation, conversation/session handling, and inline citation formatting — targets `.NET 10`. |

## Dependencies

- [AutoMapper](https://www.nuget.org/packages/AutoMapper) — maps parameter objects to builder objects.
- [Microsoft.Agents.AI.Foundry](https://www.nuget.org/packages/Microsoft.Agents.AI.Foundry) — provides the `AIProjectClient`/`AIAgent` types used to construct and run agents in Microsoft Agent Foundry.
- [Azure.Identity](https://www.nuget.org/packages/Azure.Identity) — supplies `DefaultAzureCredential` used by `AgentCreator` to authenticate the `AIProjectClient`.
- [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Abstractions) — used by `AgentCreator` to read the Foundry project endpoint from application configuration.