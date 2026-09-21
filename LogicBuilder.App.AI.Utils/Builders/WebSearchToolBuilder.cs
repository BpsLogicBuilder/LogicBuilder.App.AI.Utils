using LogicBuilder.App.AI.Utils.Builders.Interfaces;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

namespace LogicBuilder.App.AI.Utils.Builders
{
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public class WebSearchToolBuilder : IAgentToolBuilder
    {
        public AITool Build()
        {
            return ResponseTool.CreateWebSearchTool().AsAITool();
        }
    }
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}
