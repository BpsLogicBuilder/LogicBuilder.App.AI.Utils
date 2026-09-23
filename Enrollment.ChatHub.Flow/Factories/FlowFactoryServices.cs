using Enrollment.ChatHub.Flow;
using LogicBuilder.App.AI.Utils;
using LogicBuilder.App.AI.Utils.Factories;
using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.RulesDirector;
using System;

#pragma warning disable IDE0130 //Microsoft recommended namespace for service registrations
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130
{
    internal static class FlowFactoryServices
    {
        internal static IServiceCollection AddFlowFactories(this IServiceCollection services)
        {
            return services
                .AddTransient<Func<IFlowManager, DirectorBase>>
                (
                    provider =>
                    flowManager => new Director(flowManager)
                )
                .AddTransient<Func<IFlowManager, IFlowActivity>>
                (
                    provider =>
                    flowManager => new FlowActivity(flowManager)
                )
                .AddTransient<IFlowFactory, FlowFactory>();
        }
    }
}
