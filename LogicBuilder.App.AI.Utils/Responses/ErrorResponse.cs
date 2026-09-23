using System.Collections.Generic;

namespace LogicBuilder.App.AI.Utils.Responses
{
    public class ErrorResponse : IResponse
    {
        public bool Success { get; set; }
        public ICollection<string> ErrorMessages { get; set; } = [];
    }
}
