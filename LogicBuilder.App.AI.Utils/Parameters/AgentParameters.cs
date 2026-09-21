using LogicBuilder.App.AI.Utils.Parameters.Interfaces;
using System.Collections.Generic;

namespace LogicBuilder.App.AI.Utils.Parameters
{
    public class AgentParameters(string agentName, string model, string instructions, ICollection<IAgentToolParameters> tools) : IAgentParameters
    {
        public string AgentName { get; } = agentName;

        public string Model { get; } = model;

        public string Instructions { get; } = instructions;

        public ICollection<IAgentToolParameters> Tools { get; } = tools;
    }
}
