namespace LogicBuilder.App.AI.Utils.Structures
{
    public class AgentStreamResult(string? contentChunk, string? updatedThreadId)
    {
        public string? ContentChunk { get; } = contentChunk;
        public string? UpdatedThreadId { get; } = updatedThreadId;

        // Factory methods to keep creation clean
        public static AgentStreamResult FromChunk(string chunk) => new(chunk, null);
        public static AgentStreamResult FromState(string state) => new(null , state);
    }
}
