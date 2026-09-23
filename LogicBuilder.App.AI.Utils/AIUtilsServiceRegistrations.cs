using LogicBuilder.App.AI.Utils;
using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.RulesDirector;

#pragma warning disable IDE0130 //Microsoft recommended namespace for service registrations
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130
{
    public static class AIUtilsServiceRegistrations
    {
        public static IServiceCollection AddAIUtilsServices(this IServiceCollection services)
        {
            return services
                .AddAppUtilsServices()
                .AddTransient<IAgentCreator, AgentCreator>()
                .AddTransient<IAgentHandler, AgentHandler>()
                .AddTransient<IFlowManager, FlowManager>()
                .AddTransient<IGetAgentFlowHelper, GetAgentFlowHelper>()
                .AddTransient<IInlineCitationFormatter, InlineCitationFormatter>()
                .AddScoped<IFlowDataCache, FlowDataCache>()
                .AddScoped<Progress>();
        }
    }
}
