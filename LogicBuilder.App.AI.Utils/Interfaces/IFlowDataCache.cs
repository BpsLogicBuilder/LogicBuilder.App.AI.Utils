using LogicBuilder.App.AI.Utils.Requests;
using LogicBuilder.App.AI.Utils.Responses;
using System.Collections.Generic;

namespace LogicBuilder.App.AI.Utils.Interfaces
{
    public interface IFlowDataCache
    {
        IRequest? Request { get; set; }
        IResponse? Response { get; set; }
        Dictionary<string, object> Items { get; set; }
    }
}
