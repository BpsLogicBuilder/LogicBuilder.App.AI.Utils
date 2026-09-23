using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.RulesDirector;

namespace LogicBuilder.App.AI.Utils
{
    public class Director(IFlowManager flowManager) : AppDirectorBase
    {
        protected override IRulesCache RulesCache => flowManager.RulesCache;
        protected override IFlowActivity FlowActivity => flowManager.FlowActivity;
        protected override Progress Progress => flowManager.Progress;

        public override void SetCurrentBusinessBackupData() => flowManager.SetCurrentBusinessBackupData();
    }
}
