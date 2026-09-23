using LogicBuilder.App.AI.Utils.Requests;
using LogicBuilder.App.AI.Utils.Responses;

namespace LogicBuilder.App.AI.Utils.Interfaces
{
    public interface IGetAgentFlowHelper
    {
        GetAgentResponse RunFlow(GetAgentRequest selectorFlowRequest);
    }
}
