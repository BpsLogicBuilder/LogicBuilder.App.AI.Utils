using AutoMapper;
using Azure.AI.Projects;
using Azure.Identity;
using LogicBuilder.App.AI.Utils.Builders;
using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Mapping;
using LogicBuilder.App.AI.Utils.Parameters;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using System;

namespace LogicBuilder.App.AI.Utils
{
    public class AgentCreator(IConfiguration configuration, IMapper mapper) : IAgentCreator
    {
        public AIAgent CreateAgent(AgentParameters agentParameters)
        {
            var endpoint = configuration["AZURE_AI_FOUNDRY_PROJECT_ENDPOINT"];
            if (string.IsNullOrEmpty(endpoint))
                throw new InvalidOperationException("Missing Foundry Endpoint config.");

            DefaultAzureCredentialOptions credentialOptions = new()
            {
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = true
            };

            return mapper.Map<AgentBuilder>
            (
                agentParameters,
                opts => opts.Items[MappingConstants.AI_PROJECT_CLIENT_CONTEXT] = new AIProjectClient
                (
                    endpoint: new Uri(endpoint), 
                    tokenProvider: new DefaultAzureCredential(credentialOptions)
                )
            ).Build();
        }
    }
}
