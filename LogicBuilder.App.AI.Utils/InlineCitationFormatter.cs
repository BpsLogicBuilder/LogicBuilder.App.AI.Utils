using LogicBuilder.App.AI.Utils.Interfaces;
using LogicBuilder.App.AI.Utils.Structures;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogicBuilder.App.AI.Utils
{
    public class InlineCitationFormatter : IInlineCitationFormatter
    {
        public string FormatWithInlineLinks(string originalText, ICollection<CitationAnnotation> citations)
        {
            if (string.IsNullOrEmpty(originalText) || citations.Count == 0)
                return originalText;

            var citationRegions = citations
                .SelectMany
                (
                    c => c.AnnotatedRegions?
                            .OfType<TextSpanAnnotatedRegion>()
                            .Select(r => new CitationRegion(c, r)) ?? []
                )
                .Where(x => x.AnnotatedRegion != null)
                .OrderByDescending(x => x.AnnotatedRegion.StartIndex).ToList();

            if (citationRegions.Count == 0)
                return originalText;

            return citationRegions.Aggregate
            (
                new StringBuilder(originalText), 
                (textBuilder, citationRegion) =>
                {
                    string linkUrl = citationRegion.CitationAnnotation.Url?.AbsoluteUri ?? "#";
                    string linkLabel = GetFriendlyLabel(citationRegion.CitationAnnotation.Title ?? "Source");
                    string markdownLink = $" [{linkLabel}]({linkUrl})";

                    if (citationRegion.AnnotatedRegion.StartIndex.HasValue
                        && citationRegion.AnnotatedRegion.EndIndex.HasValue
                        && citationRegion.AnnotatedRegion.StartIndex >= 0
                        && citationRegion.AnnotatedRegion.EndIndex <= textBuilder.Length
                        && citationRegion.AnnotatedRegion.StartIndex <= citationRegion.AnnotatedRegion.EndIndex)
                    {
                        int length = citationRegion.AnnotatedRegion.EndIndex.Value - citationRegion.AnnotatedRegion.StartIndex.Value;
                        textBuilder.Remove(citationRegion.AnnotatedRegion.StartIndex.Value, length);
                        textBuilder.Insert(citationRegion.AnnotatedRegion.StartIndex.Value, markdownLink);
                    }

                    return textBuilder;
                }
            ).ToString();
        }

        private static string GetFriendlyLabel(string titleOrUrl)
        {
            if (string.IsNullOrWhiteSpace(titleOrUrl)) return "Source";

            try
            {
                if (Uri.TryCreate(titleOrUrl, UriKind.Absolute, out Uri? uri))
                {
                    string fileName = Path.GetFileName(uri.LocalPath);
                    if (!string.IsNullOrEmpty(fileName))
                        return fileName;
                }
            }
            catch
            {
                // Graceful fallback to raw value if string parsing fails
            }

            return titleOrUrl;
        }
    }
}
