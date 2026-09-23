using LogicBuilder.App.AI.Utils.Factories;
using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Responses;
using LogicBuilder.RulesDirector;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace LogicBuilder.App.AI.Utils
{
    public class FlowManager : IFlowManager
    {
        public FlowManager(
            IFlowDataCache flowDataCache,
            IFlowFactory flowFactory,
            ILogger<FlowManager> logger,
            Progress progress,
            IRulesCache rulesCache,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            FlowDataCache = flowDataCache;
            Progress = progress;
            RulesCache = rulesCache;
            ServiceProvider = serviceProvider;
            Director = flowFactory.GetDirector(this);
            FlowActivity = flowFactory.GetFlowActivity(this);
        }

        private readonly ILogger<FlowManager> _logger;

        public DirectorBase Director { get; }

        public IFlowActivity FlowActivity { get; }

        public IFlowDataCache FlowDataCache { get; }

        public Progress Progress { get; }

        public IRulesCache RulesCache { get; }

        public IServiceProvider ServiceProvider { get; }

        public void FlowComplete()
        {
            if (FlowDataCache.Response == null)
            {
                _logger.LogError("Response cannot be null.");
                throw new InvalidOperationException("Response cannot be null.");
            }
        }

        public void SetCurrentBusinessBackupData()
        {
        }

        public void Start(string module)
        {
            try
            {
                this.Director.StartInitialFlow(module);
            }
            catch (Exception ex)
            {
                FlowDataCache.Response = new ErrorResponse
                {
                    Success = false,
                    ErrorMessages = [ex.Message]
                };
                _logger.LogWarning(0, "Progress Start {Progress}", JsonSerializer.Serialize(this.Progress));
                _logger.LogError(ex, "{ExceptionType} : {ExceptionMessage}", ex.GetType().Name, ex.Message);
            }
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}
