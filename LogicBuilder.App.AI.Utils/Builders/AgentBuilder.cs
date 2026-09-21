using Azure.AI.Projects;
using LogicBuilder.App.AI.Utils.Builders.Interfaces;
using Microsoft.Agents.AI;
using System.Collections.Generic;
using System.Linq;

namespace LogicBuilder.App.AI.Utils.Builders
{
    public class AgentBuilder(AIProjectClient projectClient, string agentName, string model, string instructions, ICollection<IAgentToolBuilder> tools) : IAgentBuilder
    {
        public string AgentName { get; } = agentName;
        public string Model { get; } = model;
        public string Instructions { get; } = instructions;
        public ICollection<IAgentToolBuilder> Tools { get; } = tools;

        public AIAgent Build()
        {
            return projectClient.AsAIAgent
            (
                model: Model,
                name: AgentName,
                instructions: Instructions,
                tools: [.. Tools.Select(t => t.Build())]
            );
        }
    }
}
