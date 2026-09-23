using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Requests;
using LogicBuilder.App.AI.Utils.Responses;
using System;

namespace LogicBuilder.App.AI.Utils
{
    public class GetAgentFlowHelper(IFlowManager flowManager) : IGetAgentFlowHelper
    {
        public GetAgentResponse RunFlow(GetAgentRequest selectorFlowRequest)
        {
            flowManager.FlowDataCache.Request = selectorFlowRequest;

            flowManager.Start(selectorFlowRequest.FlowName);

            if (flowManager.FlowDataCache.Response is ErrorResponse errorResponse)
                throw new InvalidOperationException(string.Join(Environment.NewLine, errorResponse.ErrorMessages));

            return (GetAgentResponse)flowManager.FlowDataCache.Response!;//Response is never null - an ErrorResponse will alwasy be returned if the initial response is nul.
        }
    }
}
