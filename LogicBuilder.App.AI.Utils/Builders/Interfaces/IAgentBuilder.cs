using Microsoft.Agents.AI;
using System.Collections.Generic;

namespace LogicBuilder.App.AI.Utils.Builders.Interfaces
{
    public interface IAgentBuilder
    {
        string AgentName { get; }
        string Model { get; }
        string Instructions { get; }
        ICollection<IAgentToolBuilder> Tools { get; }

        AIAgent Build();
    }
}
