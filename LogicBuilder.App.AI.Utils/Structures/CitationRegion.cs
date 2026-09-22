using Microsoft.Extensions.AI;

namespace LogicBuilder.App.AI.Utils.Structures
{
    public class CitationRegion(CitationAnnotation citationAnnotation, TextSpanAnnotatedRegion annotatedRegion)
    {
        public CitationAnnotation CitationAnnotation { get; } = citationAnnotation;
        public TextSpanAnnotatedRegion AnnotatedRegion { get; } = annotatedRegion;
    }
}
