using AutoMapper;
using Azure.AI.Projects;
using Azure.Core;
using LogicBuilder.App.AI.Utils.Builders;
using LogicBuilder.App.AI.Utils.Builders.Interfaces;
using LogicBuilder.App.AI.Utils.Mapping;
using LogicBuilder.App.AI.Utils.Parameters;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using Xunit;

namespace LogicBuilder.App.AI.Utils.Tests
{
    public class AgentCreatorTest
    {
        private readonly Mock<IConfiguration> configurationMock = new();
        private readonly Mock<IMapper> mapperMock = new();

        [Fact]
        public void CreateAgent_Throws_WhenEndpointConfigIsMissing()
        {
            
            configurationMock.Setup(c => c["AZURE_AI_FOUNDRY_PROJECT_ENDPOINT"]).Returns((string?)null);

            var creator = new AgentCreator(configurationMock.Object, mapperMock.Object);
            var agentParameters = new AgentParameters("agent", "model", "instructions", []);

            var ex = Assert.Throws<InvalidOperationException>(() => creator.CreateAgent(agentParameters));

            Assert.Equal("Missing Foundry Endpoint config.", ex.Message);
            mapperMock.Verify(m => m.Map<AgentBuilder>
            (
                It.IsAny<object>(), 
                It.IsAny<Action<IMappingOperationOptions<object, AgentBuilder>>>()), 
                Times.Never
            );
        }

        [Fact]
        public void CreateAgent_Throws_WhenEndpointConfigIsEmpty()
        {
            configurationMock.Setup(c => c["AZURE_AI_FOUNDRY_PROJECT_ENDPOINT"]).Returns("");

            var creator = new AgentCreator(configurationMock.Object, mapperMock.Object);
            var agentParameters = new AgentParameters("agent", "model", "instructions", []);

            Assert.Throws<InvalidOperationException>(() => creator.CreateAgent(agentParameters));
        }

        [Fact]
        public void CreateAgent_PassesAgentParametersAndProjectClient_ToMapper()
        {
            const string endpoint = "https://fake-foundry-endpoint.services.ai.azure.com/api/projects/fake-project";
            configurationMock.Setup(c => c["AZURE_AI_FOUNDRY_PROJECT_ENDPOINT"]).Returns(endpoint);

            var agentParameters = new AgentParameters("agent-name", "gpt-test-model", "test-instructions", []);

            object? capturedSource = null;
            AIProjectClient? capturedClient = null;

            mapperMock
                .Setup(m => m.Map<AgentBuilder>(It.IsAny<object>(), It.IsAny<Action<IMappingOperationOptions<object, AgentBuilder>>>()))
                .Returns((object source, Action<IMappingOperationOptions<object, AgentBuilder>> opts) =>
                {
                    capturedSource = source;

                    var options = new FakeMappingOperationOptions();
                    opts(options);

                    capturedClient = options.Items[MappingConstants.AI_PROJECT_CLIENT_CONTEXT] as AIProjectClient;

                    return new AgentBuilder(capturedClient!, "agent-name", "gpt-test-model", "test-instructions", []);
                });

            var creator = new AgentCreator(configurationMock.Object, mapperMock.Object);

            var result = creator.CreateAgent(agentParameters);
            Assert.NotNull(result);

            Assert.Same(agentParameters, capturedSource);
            Assert.NotNull(capturedClient);
            mapperMock.Verify(m => m.Map<AgentBuilder>(agentParameters, It.IsAny<Action<IMappingOperationOptions<object, AgentBuilder>>>()), Times.Once);
        }

        [Fact]
        public void CreateAgent_ReturnsResultOf_AgentBuilderBuild()
        {
            const string endpoint = "https://fake-foundry-endpoint.services.ai.azure.com/api/projects/fake-project";
            configurationMock.Setup(c => c["AZURE_AI_FOUNDRY_PROJECT_ENDPOINT"]).Returns(endpoint);

            var agentParameters = new AgentParameters("agent-name", "gpt-test-model", "test-instructions", []);

            mapperMock
                .Setup(m => m.Map<AgentBuilder>(It.IsAny<object>(), It.IsAny<Action<IMappingOperationOptions<object, AgentBuilder>>>()))
                .Returns((object source, Action<IMappingOperationOptions<object, AgentBuilder>> opts) =>
                {
                    var options = new FakeMappingOperationOptions();
                    opts(options);

                    var client = (AIProjectClient)options.Items[MappingConstants.AI_PROJECT_CLIENT_CONTEXT];

                    return new AgentBuilder(client, "agent-name", "gpt-test-model", "test-instructions", []);
                });

            var creator = new AgentCreator(configurationMock.Object, mapperMock.Object);

            var result = creator.CreateAgent(agentParameters);

            Assert.NotNull(result);
        }

        // Minimal fake implementation used to capture the Items dictionary populated by
        // AgentCreator's mapping options callback (AutoMapper's concrete options type is
        // not directly instantiable without going through the real mapping pipeline).
        private sealed class FakeMappingOperationOptions : IMappingOperationOptions<object, AgentBuilder>
        {
            public Dictionary<string, object> Items { get; } = new Dictionary<string, object>() { [MappingConstants.AI_PROJECT_CLIENT_CONTEXT] = new AIProjectClient(endpoint: new Uri("http://www.google.com"), tokenProvider: new ApiKeyTokenCredential("")) };

            public Func<Type, object> ServiceCtor => throw new NotImplementedException();

            public object State { get => null!; set { /*used for testing*/} }

            Dictionary<string, object> IMappingOperationOptions.Items => new() { [MappingConstants.AI_PROJECT_CLIENT_CONTEXT] = new AIProjectClient(endpoint: new Uri("http://www.google.com"), tokenProvider: new ApiKeyTokenCredential("")) };

            public void BeforeMap(Action<object, AgentBuilder> beforeFunction)
            {
            }

            public void AfterMap(Action<object, AgentBuilder> afterFunction)
            {
            }

            public void ConstructServicesUsing(Func<Type, object> constructor)
            {
                throw new NotImplementedException();
            }

            public void BeforeMap(Action<object, object> beforeFunction)
            {
                throw new NotImplementedException();
            }

            public void AfterMap(Action<object, object> afterFunction)
            {
                throw new NotImplementedException();
            }
        }

        public class ApiKeyTokenCredential(string apiKey) : TokenCredential
        {
            private readonly string _apiKey = apiKey;

            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => new(_apiKey, DateTimeOffset.MaxValue);

            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => new(new AccessToken(_apiKey, DateTimeOffset.MaxValue));
        }
    }
}
