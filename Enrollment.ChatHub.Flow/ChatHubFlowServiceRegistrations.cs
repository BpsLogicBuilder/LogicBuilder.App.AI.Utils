using Enrollment.ChatHub.Flow;
using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.Utils.Rules;
using LogicBuilder.RulesDirector;
using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable IDE0130 //Microsoft recommended namespace for service registrations
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class ChatHubFlowServiceRegistrations
    {
        public static IServiceCollection AddChatHubFlowServices(this IServiceCollection services)
        {
            return services
                .AddAIUtilsServices()
                .AddFlowFactories()
                .AddRulesCacheService
                (
                    new RulesLoaderRequest
                    (
                        "Enrollment.ChatHub.Flow.Rulesets",
                        typeof(FlowActivity),
                        [
                            typeof(LogicBuilder.App.Utils.Interfaces.ITypeHelper).Assembly,
                            typeof(IAgentCreator).Assembly,
                            typeof(DirectorBase).Assembly,
                            typeof(string).Assembly
                        ]
                    )
                );
        }
    }
}
