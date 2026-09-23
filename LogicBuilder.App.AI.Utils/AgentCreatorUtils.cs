using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Parameters;
using LogicBuilder.Attributes;
using Microsoft.Agents.AI;

namespace LogicBuilder.App.AI.Utils
{
    public static class AgentCreatorUtils
    {
        [AlsoKnownAs("CreateAgent")]
        public static AIAgent CreateAgent(IAgentCreator agentCreator, AgentParameters agentParameters)
            => agentCreator.CreateAgent(agentParameters);
    }
}
