using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.RulesDirector;

namespace LogicBuilder.App.AI.Utils.Factories
{
    public interface IFlowFactory
    {
        DirectorBase GetDirector(IFlowManager flowManager);
        IFlowActivity GetFlowActivity(IFlowManager flowManager);
    }
}
