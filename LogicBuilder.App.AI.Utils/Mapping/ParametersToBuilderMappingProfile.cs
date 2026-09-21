using AutoMapper;
using Azure.AI.Projects;
using LogicBuilder.App.AI.Utils.Builders;
using LogicBuilder.App.AI.Utils.Builders.Interfaces;
using LogicBuilder.App.AI.Utils.Parameters;
using LogicBuilder.App.AI.Utils.Parameters.Interfaces;
using System.Collections.Generic;

namespace LogicBuilder.App.AI.Utils.Mapping
{
    public class ParametersToBuilderMappingProfile : Profile
    {
        public ParametersToBuilderMappingProfile()
        {
            CreateMap<McpToolParameters, McpToolBuilder>();
            CreateMap<WebSearchToolParameters, WebSearchToolBuilder>();
            CreateMap<AgentParameters, AgentBuilder>()
                .ConstructUsing
                (
                    (src, context) => new AgentBuilder
                    (
                        (AIProjectClient)context.Items[MappingConstants.AI_PROJECT_CLIENT_CONTEXT],
                        src.AgentName,
                        src.Model,
                        src.Instructions,
                        context.Mapper.Map<ICollection<IAgentToolBuilder>>(src.Tools)
                    )
                );

            CreateMap<IAgentToolParameters, IAgentToolBuilder>()
                .Include<McpToolParameters, McpToolBuilder>()
                .Include<WebSearchToolParameters, WebSearchToolBuilder>();

            CreateMap<IAgentParameters, IAgentBuilder>()
                .Include<AgentParameters, AgentBuilder>();
        }
    }
}
