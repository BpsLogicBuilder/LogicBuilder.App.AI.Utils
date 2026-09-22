using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Parameters;
using Microsoft.Agents.AI;

namespace LogicBuilder.App.AI.Utils
{
    public static class AgentCreatorUtils
    {
        public static AIAgent CreateAgent(IAgentCreator agentCreator, AgentParameters agentParameters)
            => agentCreator.CreateAgent(agentParameters);
    }
}
