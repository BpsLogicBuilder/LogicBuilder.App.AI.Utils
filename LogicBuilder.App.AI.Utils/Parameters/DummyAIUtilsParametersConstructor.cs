using System.Diagnostics.CodeAnalysis;

namespace LogicBuilder.App.AI.Utils.Parameters
{
#pragma warning disable CS9113//used for configuration by reflection
    [ExcludeFromCodeCoverage]
    public class DummyAIUtilsParametersConstructor(
        AgentParameters agentParameters,
        McpToolParameters mcpToolParameters,
        WebSearchToolParameters webSearchToolParameters)
#pragma warning restore CS9113
    {
    }
}
