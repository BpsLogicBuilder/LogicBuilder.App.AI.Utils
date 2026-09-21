using Microsoft.Extensions.AI;

namespace LogicBuilder.App.AI.Utils.Builders.Interfaces
{
    public interface IAgentToolBuilder
    {
        AITool Build();
    }
}
