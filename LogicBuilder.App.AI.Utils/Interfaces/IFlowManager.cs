using LogicBuilder.RulesDirector;
using System;

namespace LogicBuilder.App.AI.Utils.Interfaces
{
    public interface IFlowManager
    {
        DirectorBase Director { get; }
        IFlowActivity FlowActivity { get; }
        IFlowDataCache FlowDataCache { get; }
        Progress Progress { get; }
        IRulesCache RulesCache { get; }
        IServiceProvider ServiceProvider { get; }

        void Start(string module);
        void SetCurrentBusinessBackupData();
        void FlowComplete();
        void Terminate();
    }
}
