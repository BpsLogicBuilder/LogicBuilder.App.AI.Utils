using AutoMapper;
using Azure.AI.Projects;
using Azure.Identity;
using LogicBuilder.App.AI.Utils.Builders;
using LogicBuilder.App.AI.Utils.Mapping;
using LogicBuilder.App.AI.Utils.Parameters;
using LogicBuilder.App.AI.Utils.Parameters.Interfaces;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace LogicBuilder.App.AI.Utils.Tests
{
    public class MappingTest
    {
        static MappingTest()
        {
            InitializeMapperConfiguration();
        }

        public MappingTest()
        {
            Initialize();
        }

        private IServiceProvider serviceProvider;

        [Fact]
        public void TestAgentMapping()
        {
            //arrange
            DefaultAzureCredentialOptions credentialOptions = new()
            {
                ExcludeEnvironmentCredential = true,
                ExcludeManagedIdentityCredential = true
            };
            var credential = new DefaultAzureCredential(credentialOptions);

            AIProjectClient projectClient = new(endpoint: new Uri("http://www.google.com"), tokenProvider: credential);

            IAgentParameters agentParameters = new AgentParameters
            (
                "AgentName",
                "gpt-5.2",
                "You are a helpful AI assistant for Contoso, specializing in outdoor camping and hiking products. \r\nYou must ALWAYS search the knowledge base to answer questions about our products or product \r\ncatalog. Provide detailed, accurate information and always cite your sources.\r\nIf you don't find relevant information in the knowledge base, say so clearly.",
                [
                    new McpToolParameters("GreetingTool", "http://www.microsoft.com", true),
                    new McpToolParameters("kb-knowledgebase576-3r10c", "http://www.google.com", true),
                    new WebSearchToolParameters()
                ]
            );
            IMapper mapper = serviceProvider.GetRequiredService<IMapper>();

            //act
            AgentBuilder agentBuilder = mapper.Map<AgentBuilder>
            (
                agentParameters,
                opts => opts.Items[MappingConstants.AI_PROJECT_CLIENT_CONTEXT] = projectClient
            );
            AIAgent agent = agentBuilder.Build();

            //assert
            Assert.Equal("AgentName", agentBuilder.AgentName);
            Assert.Equal("gpt-5.2", agentBuilder.Model);
            Assert.Equal(3, agentBuilder.Tools.Count);
            Assert.NotNull(agent);
        }

        [MemberNotNull(nameof(MapperConfiguration))]
        private static void InitializeMapperConfiguration()
        {
            MapperConfiguration ??= new(
                cfg =>
                {
                    cfg.AddProfile<ParametersToBuilderMappingProfile>();
                },
                NullLoggerFactory.Instance
            );
        }

        static MapperConfiguration MapperConfiguration;

        [MemberNotNull(nameof(serviceProvider))]
        private void Initialize()
        {
            serviceProvider = new ServiceCollection()
                .AddSingleton<AutoMapper.IConfigurationProvider>
                (
                    MapperConfiguration
                )
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService))
                .BuildServiceProvider();
        }
    }
}
