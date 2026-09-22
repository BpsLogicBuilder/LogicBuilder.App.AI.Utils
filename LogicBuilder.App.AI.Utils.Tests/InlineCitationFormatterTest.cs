using Microsoft.Extensions.AI;

namespace LogicBuilder.App.AI.Utils.Tests
{
    public class InlineCitationFormatterTest
    {
        private readonly InlineCitationFormatter formatter = new();

        private readonly string InitialText = @"Contoso offers tents in these main types (with current models in each category):

- **Backpacking tents (2-person)**: *Alpine Explorer 2P* and *TrailMaster Lite 2P*【5:0†source】【5:1†source】  
- **Family tents (4-person)**: *SummitDome 4P* (dome-style family tent) and *MegaSpace 4P Cabin* (cabin-style, more stand-up room)【5:1†source】  
- **Group tents (6-person)**: *BaseCamp Pro 6P* (large-capacity, multi-room style)【5:2†source】  
- **Specialty tents**: *FourSeason Alpine 2P* (4-season/mountaineering) and *UltraLite Solo 1P* (minimalist/ultralight solo shelter)【5:2†source】【5:3†source】

If you tell me how many people you’re camping with and the season/conditions (summer, shoulder-season, winter), I can point you to the best-fit options from the lineup.";

        private readonly List<CitationAnnotation> CitationAnnotations = [
                new CitationAnnotation
                {
                    Title = "https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf",
                    Url = new Uri("https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf"),
                    AnnotatedRegions=
                    [
                        new TextSpanAnnotatedRegion { StartIndex = 164, EndIndex = 176 }
                    ]
                },
                new CitationAnnotation
                {
                    Title = "https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf",
                    Url = new Uri("https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf"),
                    AnnotatedRegions=
                    [
                        new TextSpanAnnotatedRegion { StartIndex = 176, EndIndex = 188 }
                    ]
                },
                new CitationAnnotation
                {
                    Title = "https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf",
                    Url = new Uri("https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf"),
                    AnnotatedRegions=
                    [
                        new TextSpanAnnotatedRegion { StartIndex = 321, EndIndex = 333 }
                    ]
                },
                new CitationAnnotation
                {
                    Title = "https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf",
                    Url = new Uri("https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf"),
                    AnnotatedRegions=
                    [
                        new TextSpanAnnotatedRegion { StartIndex = 418, EndIndex = 430 }
                    ]
                },
                new CitationAnnotation
                {
                    Title = "https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf",
                    Url = new Uri("https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf"),
                    AnnotatedRegions=
                    [
                        new TextSpanAnnotatedRegion { StartIndex = 565, EndIndex = 577 }
                    ]
                },
                new CitationAnnotation
                {
                    Title = "https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf",
                    Url = new Uri("https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf"),
                    AnnotatedRegions=
                    [
                        new TextSpanAnnotatedRegion { StartIndex = 577, EndIndex = 589 }
                    ]
                },
            ];

        [Fact]
        public void CitationFormatterYieldsExpectedResults()
        {
            string result = formatter.FormatWithInlineLinks(InitialText, CitationAnnotations);

            const string link = " [contoso-tents-catalog.pdf](https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf)";

            // Regions (ascending): (164,176) (176,188) (321,333) (418,430) (565,577) (577,589)
            // Adjacent regions (164-176/176-188 and 565-577/577-589) share a boundary, so their
            // replacement links are emitted back-to-back with no original text between them.
            string expected =
                InitialText[..164] +
                link +
                link +
                InitialText[188..321] +
                link +
                InitialText[333..418] +
                link +
                InitialText[430..565] +
                link +
                link +
                InitialText[589..];

            Assert.Equal(expected, result);
        }


        [Fact]
        public void ReturnsOriginalText_WhenTextIsNull()
        {
            string? result = formatter.FormatWithInlineLinks(null!, [CreateCitation("Source", "https://example.com/doc.pdf", 0, 5)]);
            Assert.Null(result);
        }

        [Fact]
        public void ReturnsOriginalText_WhenTextIsEmpty()
        {
            string result = formatter.FormatWithInlineLinks(string.Empty, [CreateCitation("Source", "https://example.com/doc.pdf", 0, 5)]);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ReturnsOriginalText_WhenCitationsCollectionIsEmpty()
        {
            string text = "Hello world";
            string result = formatter.FormatWithInlineLinks(text, []);
            Assert.Equal(text, result);
        }

        [Fact]
        public void ReturnsOriginalText_WhenCitationsHaveNoAnnotatedRegions()
        {
            string text = "Hello world";
            var citations = new List<CitationAnnotation>
            {
                new() { Title = "Source", Url = new Uri("https://example.com/doc.pdf"), AnnotatedRegions = null }
            };

            string result = formatter.FormatWithInlineLinks(text, citations);
            Assert.Equal(text, result);
        }

        [Fact]
        public void ReturnsOriginalText_WhenAnnotatedRegionsAreEmptyCollection()
        {
            string text = "Hello world";
            var citations = new List<CitationAnnotation>
            {
                new() { Title = "Source", Url = new Uri("https://example.com/doc.pdf"), AnnotatedRegions = [] }
            };

            string result = formatter.FormatWithInlineLinks(text, citations);
            Assert.Equal(text, result);
        }

        [Fact]
        public void InsertsMarkdownLink_ForSingleValidRegion()
        {
            string text = "Hello world";
            var citation = CreateCitation("doc.pdf", "https://example.com/files/doc.pdf", 5, 6); // removes the single space

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal("Hello [doc.pdf](https://example.com/files/doc.pdf)world", result);
        }

        [Fact]
        public void InsertsMultipleMarkdownLinks_InCorrectOrder_ForMultipleRegions()
        {
            string text = "AABBCC";
            var citation1 = CreateCitation("first", "https://example.com/first.pdf", 0, 2);
            var citation2 = CreateCitation("second", "https://example.com/second.pdf", 2, 4);
            var citation3 = CreateCitation("third", "https://example.com/third.pdf", 4, 6);

            // Citations passed out of index order to validate descending sort handles overlap-free removals correctly.
            string result = formatter.FormatWithInlineLinks(text, [citation2, citation3, citation1]);

            Assert.Equal(
                " [first](https://example.com/first.pdf) [second](https://example.com/second.pdf) [third](https://example.com/third.pdf)",
                result);
        }

        [Fact]
        public void SkipsRegion_WhenStartIndexIsNull()
        {
            string text = "Hello world";
            var citation = CreateCitation("doc.pdf", "https://example.com/doc.pdf", null, 6);

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal(text, result);
        }

        [Fact]
        public void SkipsRegion_WhenEndIndexIsNull()
        {
            string text = "Hello world";
            var citation = CreateCitation("doc.pdf", "https://example.com/doc.pdf", 5, null);

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal(text, result);
        }

        [Fact]
        public void SkipsRegion_WhenStartIndexIsNegative()
        {
            string text = "Hello world";
            var citation = CreateCitation("doc.pdf", "https://example.com/doc.pdf", -1, 5);

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal(text, result);
        }

        [Fact]
        public void SkipsRegion_WhenEndIndexExceedsTextLength()
        {
            string text = "Hello world";
            var citation = CreateCitation("doc.pdf", "https://example.com/doc.pdf", 5, text.Length + 1);

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal(text, result);
        }

        [Fact]
        public void SkipsRegion_WhenStartIndexGreaterThanEndIndex()
        {
            string text = "Hello world";
            var citation = CreateCitation("doc.pdf", "https://example.com/doc.pdf", 6, 5);

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal(text, result);
        }

        [Fact]
        public void UsesHashFallback_WhenUrlIsNull()
        {
            string text = "Hello world";
            var citation = new CitationAnnotation
            {
                Title = "Source",
                Url = null,
                AnnotatedRegions = [new TextSpanAnnotatedRegion { StartIndex = 5, EndIndex = 6 }]
            };

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal("Hello [Source](#)world", result);
        }

        [Fact]
        public void UsesFileNameFromUrl_WhenTitleIsAbsoluteUri()
        {
            string text = "Hello world";
            var citation = CreateCitation(
                "https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf",
                "https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf",
                5, 6);

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal(
                "Hello [contoso-tents-catalog.pdf](https://bpsfoundryidstorage.blob.core.windows.net/contosoproducts/contoso-tents-catalog.pdf)world",
                result);
        }

        [Fact]
        public void UsesTitleAsLabel_WhenTitleIsPlainText()
        {
            string text = "Hello world";
            var citation = CreateCitation("Plain Title", "https://example.com/doc.pdf", 5, 6);

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal("Hello [Plain Title](https://example.com/doc.pdf)world", result);
        }

        [Fact]
        public void UsesSourceFallback_WhenTitleIsNull()
        {
            string text = "Hello world";
            var citation = new CitationAnnotation
            {
                Title = null,
                Url = new Uri("https://example.com/doc.pdf"),
                AnnotatedRegions = [new TextSpanAnnotatedRegion { StartIndex = 5, EndIndex = 6 }]
            };

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal("Hello [Source](https://example.com/doc.pdf)world", result);
        }

        [Fact]
        public void UsesSourceFallback_WhenTitleIsWhitespace()
        {
            string text = "Hello world";
            var citation = CreateCitation("   ", "https://example.com/doc.pdf", 5, 6);

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal("Hello [Source](https://example.com/doc.pdf)world", result);
        }

        [Fact]
        public void HandlesMultipleAnnotatedRegions_OnSameCitation()
        {
            string text = "AABBCC";
            var citation = new CitationAnnotation
            {
                Title = "doc.pdf",
                Url = new Uri("https://example.com/doc.pdf"),
                AnnotatedRegions =
                [
                    new TextSpanAnnotatedRegion { StartIndex = 0, EndIndex = 2 },
                    new TextSpanAnnotatedRegion { StartIndex = 4, EndIndex = 6 }
                ]
            };

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal(
                " [doc.pdf](https://example.com/doc.pdf)BB [doc.pdf](https://example.com/doc.pdf)",
                result);
        }

        [Fact]
        public void IgnoresNonTextSpanAnnotatedRegions()
        {
            string text = "Hello world";
            var citation = new CitationAnnotation
            {
                Title = "doc.pdf",
                Url = new Uri("https://example.com/doc.pdf"),
                AnnotatedRegions = [new AnnotatedRegion()] // base type, not TextSpanAnnotatedRegion
            };

            string result = formatter.FormatWithInlineLinks(text, [citation]);

            Assert.Equal(text, result);
        }

        private static CitationAnnotation CreateCitation(string title, string url, int? startIndex, int? endIndex)
        {
            return new CitationAnnotation
            {
                Title = title,
                Url = new Uri(url),
                AnnotatedRegions = [new TextSpanAnnotatedRegion { StartIndex = startIndex, EndIndex = endIndex }]
            };
        }
    }
}
