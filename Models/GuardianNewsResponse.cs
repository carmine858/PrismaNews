using System.Text.Json.Serialization;

namespace Prisma.Models
{
    public class GuardianNewsResponse
    {
        [JsonPropertyName("response")]
        public GuardianResponse? Response { get; set; }
    }

    public class GuardianResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("userTier")]
        public string? UserTier { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("startIndex")]
        public int StartIndex { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("pages")]
        public int Pages { get; set; }

        [JsonPropertyName("results")]
        public GuardianResult[]? Results { get; set; }
    }

    public class GuardianResult
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("sectionId")]
        public string? SectionId { get; set; }

        [JsonPropertyName("sectionName")]
        public string SectionName { get; set; } = string.Empty;

        [JsonPropertyName("webPublicationDate")]
        public DateTime WebPublicationDate { get; set; }

        [JsonPropertyName("webTitle")]
        public string WebTitle { get; set; } = string.Empty;

        [JsonPropertyName("webUrl")]
        public string WebUrl { get; set; } = string.Empty;

        [JsonPropertyName("apiUrl")]
        public string ApiUrl { get; set; } = string.Empty;

        [JsonPropertyName("fields")]
        public GuardianFields? Fields { get; set; }

        [JsonPropertyName("tags")]
        public GuardianTag[]? Tags { get; set; }
    }

    public class GuardianFields
    {
        [JsonPropertyName("headline")]
        public string? Headline { get; set; }

        [JsonPropertyName("byline")]
        public string? Byline { get; set; }

        [JsonPropertyName("trailText")]
        public string? TrailText { get; set; }

        [JsonPropertyName("firstPublicationDate")]
        public string? FirstPublicationDate { get; set; }

        [JsonPropertyName("lastModified")]
        public string? LastModified { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("wordcount")]
        public string? Wordcount { get; set; }
    }

    public class GuardianTag
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("webTitle")]
        public string WebTitle { get; set; } = string.Empty;
    }
}