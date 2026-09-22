using LogicBuilder.App.AI.Utils.Parameters;
using Microsoft.Agents.AI;

namespace LogicBuilder.App.AI.Utils.Interfaces
{
    public interface IAgentCreator
    {
        AIAgent CreateAgent(AgentParameters agentParameters);
    }
}
