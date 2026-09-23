using AutoMapper;
using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Mapping;
using LogicBuilder.App.AI.Utils.Requests;
using LogicBuilder.App.AI.Utils.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace LogicBuilder.App.AI.Utils.Tests
{
    public class FlowTest
    {
        static FlowTest()
        {
            InitializeMapperConfiguration();
        }

        public FlowTest()
        {
            Initialize();
        }
        #region Fields
        private static MapperConfiguration MapperConfiguration;
        private IServiceProvider serviceProvider;
        private const string initialFlow = "initial";
        #endregion Fields

        [Fact]
        public void FlowWithUnrecognizedAgent_ReturnsAResponseWithNullAgent()
        {
            //arrange
            IFlowManager flowManager = serviceProvider!.GetRequiredService<IFlowManager>();
            flowManager.FlowDataCache.Request = new GetAgentRequest("unidentified", initialFlow);

            //act
            flowManager.Start(initialFlow);
            GetAgentResponse response = (GetAgentResponse)flowManager.FlowDataCache.Response!;

            //assert
            Assert.Null(response.AIAgent);
        }

        [Theory]
        [InlineData("greeting-only")]
        [InlineData("websearch-only")]
        [InlineData("contoso-only")]
        [InlineData("greeting-contoso-websearch")]
        public void FlowWithRecognizedAgent_ReturnsAResponseWithAgent(string agentName)
        {
            //arrange
            IFlowManager flowManager = serviceProvider!.GetRequiredService<IFlowManager>();
            flowManager.FlowDataCache.Request = new GetAgentRequest(agentName, initialFlow);

            //act
            flowManager.Start(initialFlow);
            GetAgentResponse response = (GetAgentResponse)flowManager.FlowDataCache.Response!;

            //assert
            Assert.NotNull(response.AIAgent);
        }//IGetAgentFlowHelper

        [Theory]
        [InlineData("greeting-only")]
        [InlineData("websearch-only")]
        [InlineData("contoso-only")]
        [InlineData("greeting-contoso-websearch")]
        public void FlowWithRecognizedAgent_ReturnsAResponseWithAgent_UsingGetAgentFlowHelper(string agentName)
        {
            //arrange
            IGetAgentFlowHelper helper = serviceProvider!.GetRequiredService<IGetAgentFlowHelper>();

            //act
            GetAgentResponse response = helper.RunFlow(new GetAgentRequest(agentName, initialFlow));

            //assert
            Assert.NotNull(response.AIAgent);
        }

        [Fact]
        public void CallTerminateFlow_ReturnsErrorResponse()
        {
            //arrange
            IFlowManager flowManager = serviceProvider!.GetRequiredService<IFlowManager>();

            //act
            flowManager.Start("callterminateflow");
            ErrorResponse response = (ErrorResponse)flowManager.FlowDataCache.Response!;

            //assert
            Assert.False(response.Success);
        }

        [Fact]
        public void ReturnWithNullResponse_ReturnsErrorResponse()
        {
            //arrange
            IFlowManager flowManager = serviceProvider!.GetRequiredService<IFlowManager>();

            //act
            flowManager.Start("returnwithnullresponse");
            ErrorResponse response = (ErrorResponse)flowManager.FlowDataCache.Response!;

            //assert
            Assert.False(response.Success);
        }

        [Fact]
        public void ReturnWithNullResponse_ReturnsErrorResponse_UsingGetAgentFlowHelper()
        {
            //arrange
            IGetAgentFlowHelper helper = serviceProvider!.GetRequiredService<IGetAgentFlowHelper>();

            //act //assert
            Assert.Throws<InvalidOperationException>(() => helper.RunFlow(new GetAgentRequest("agentName", "returnwithnullresponse")));
        }

        #region Helpers
        [MemberNotNull(nameof(MapperConfiguration))]
        private static void InitializeMapperConfiguration()
        {
            MapperConfiguration ??= new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ParametersToBuilderMappingProfile>();
            }, new NullLoggerFactory());
            MapperConfiguration.AssertConfigurationIsValid();
        }

        [MemberNotNull(nameof(serviceProvider))]
        private void Initialize()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            serviceProvider = new ServiceCollection()
                .AddSingleton<AutoMapper.IConfigurationProvider>
                (
                    MapperConfiguration
                )
                .AddSingleton(configuration)
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService))
                .AddLogging()
                .AddChatHubFlowServices()
                .BuildServiceProvider();
        }
        #endregion Helpers
    }
}
