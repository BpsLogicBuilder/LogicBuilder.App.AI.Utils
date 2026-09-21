using Azure.AI.Projects.Agents;
using LogicBuilder.App.AI.Utils.Builders.Interfaces;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using System;

namespace LogicBuilder.App.AI.Utils.Builders
{
    #pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public class McpToolBuilder(string serverLabel, string serverUrl, bool alwaysAutoApprove) : IAgentToolBuilder
    {
        public string ServerLabel { get; } = serverLabel;
        public string ServerUrl { get; } = serverUrl;
        public bool AlwaysAutoApprove { get; } = alwaysAutoApprove;

        public AITool Build()
        {
            var mcpTool = ResponseTool.CreateMcpTool
            (
                serverLabel: ServerLabel,
                serverUri: new Uri(ServerUrl),
                toolCallApprovalPolicy: AlwaysAutoApprove ? new McpToolCallApprovalPolicy(GlobalMcpToolCallApprovalPolicy.NeverRequireApproval) : new McpToolCallApprovalPolicy(GlobalMcpToolCallApprovalPolicy.AlwaysRequireApproval)
            );
            mcpTool.ProjectConnectionId = ServerLabel;

            return mcpTool.AsAITool();
        }
    }
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}
