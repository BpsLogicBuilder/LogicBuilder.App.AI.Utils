using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Requests;
using LogicBuilder.App.AI.Utils.Responses;
using System.Collections.Generic;

namespace LogicBuilder.App.AI.Utils
{
    public class FlowDataCache : IFlowDataCache
    {
        public IRequest? Request { get; set; }
        public IResponse? Response { get; set; }
        public Dictionary<string, object> Items { get; set; } = [];
    }
}
