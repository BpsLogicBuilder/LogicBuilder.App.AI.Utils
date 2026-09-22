using Microsoft.Extensions.AI;
using System.Collections.Generic;

namespace LogicBuilder.App.AI.Utils.Interfaces
{
    public interface IInlineCitationFormatter
    {
        string FormatWithInlineLinks(string originalText, ICollection<CitationAnnotation> citations);
    }
}
