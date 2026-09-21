# LogicBuilder.App.AI.Utils

**LogicBuilder.App.AI.Utils** is a .NET library for building [Microsoft Agent Foundry](https://learn.microsoft.com/azure/ai-foundry/) AI agents and their associated tool collections from a simple, serializable object hierarchy (parameters), rather than hand-coding the Azure AI Projects / `Microsoft.Agents.AI` SDK calls directly.

It is intended for applications that need to define AI agents (name, model, instructions, and tools such as MCP servers or web search) declaratively — e.g. from configuration, a database, or a UI — and then materialize those definitions into runnable `AIAgent` instances.

## Key Concepts

The library is organized around two parallel object hierarchies connected by an [AutoMapper](https://automapper.org/) profile:

- **Parameters** (`LogicBuilder.App.AI.Utils.Parameters`) — plain data objects describing *what* an agent should look like:
  - `IAgentParameters` / `AgentParameters` — agent name, model, instructions, and a collection of tool parameters.
  - `IAgentToolParameters` — base interface for tool definitions, implemented by:
    - `McpToolParameters` — describes an MCP (Model Context Protocol) tool/server.
    - `WebSearchToolParameters` — describes a web search tool.

- **Builders** (`LogicBuilder.App.AI.Utils.Builders`) — objects that know *how* to construct the corresponding Azure AI Foundry / Agents SDK artifacts:
  - `IAgentBuilder` / `AgentBuilder` — wraps an `AIProjectClient` plus agent metadata and tool builders, and exposes `Build()` to produce a live `AIAgent`.
  - `IAgentToolBuilder` implementations (e.g. `McpToolBuilder`, `WebSearchToolBuilder`) — each know how to build the corresponding tool definition consumed by the agent.

- **Mapping** (`LogicBuilder.App.AI.Utils.Mapping`) — `ParametersToBuilderMappingProfile` is an AutoMapper `Profile` that maps the parameter hierarchy onto the builder hierarchy (including polymorphic mapping of tool parameter types to their matching tool builder types). An `AIProjectClient` is supplied via the AutoMapper `ResolutionContext.Items` (see `MappingConstants.AI_PROJECT_CLIENT_CONTEXT`) so it can be injected into the constructed `AgentBuilder`.

## Typical Usage

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

## Solution Layout

| Project | Purpose |
|---|---|
| `LogicBuilder.App.AI.Utils` | Core library (parameters, builders, AutoMapper profile) — targets `.NET Standard 2.0`. |
| `LogicBuilder.App.AI.Utils.Tests` | xUnit test suite validating parameter → builder mapping and agent construction — targets `.NET 10`. |

## Dependencies

- [AutoMapper](https://www.nuget.org/packages/AutoMapper) — maps parameter objects to builder objects.
- [Microsoft.Agents.AI.Foundry](https://www.nuget.org/packages/Microsoft.Agents.AI.Foundry) — provides the `AIProjectClient`/`AIAgent` types used to construct and run agents in Microsoft Agent Foundry
