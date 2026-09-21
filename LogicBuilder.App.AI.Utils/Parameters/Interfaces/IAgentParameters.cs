using System.Collections.Generic;

namespace LogicBuilder.App.AI.Utils.Parameters.Interfaces
{
    public interface IAgentParameters
    {
        string AgentName { get; }
        string Model { get; }
        string Instructions { get; }
        ICollection<IAgentToolParameters> Tools { get; }
    }
}
