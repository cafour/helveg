// <auto-generated />
//
// To parse this JSON data, add NuGet 'System.Text.Json' then do:
//
//    using Helveg.Visualization;
//
//    var dataModel = DataModel.FromJson(jsonString);
#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace Helveg.Visualization
{
    using System;
    using System.Collections.Generic;

    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Globalization;

    /// <summary>
    /// Data produced and consumed by Helveg, a software visualization toolkit.
    /// </summary>
    public partial class DataModel
    {
        /// <summary>
        /// Metadata about the analyzer that produced this data set.
        /// </summary>
        [JsonPropertyName("analyzer")]
        public virtual AnalyzerMetadata Analyzer { get; set; }

        /// <summary>
        /// The creation time of this document.
        /// </summary>
        [JsonPropertyName("createdOn")]
        public virtual DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// The analyzed data in the form of a multigraph.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("data")]
        public virtual Multigraph Data { get; set; }

        /// <summary>
        /// The name of the data set.
        /// </summary>
        [JsonPropertyName("name")]
        public virtual string Name { get; set; }
    }

    /// <summary>
    /// Metadata about the analyzer that produced this data set.
    /// </summary>
    public partial class AnalyzerMetadata
    {
        /// <summary>
        /// Name of the analyzer.
        /// </summary>
        [JsonPropertyName("name")]
        public virtual string Name { get; set; }

        /// <summary>
        /// Version of the analyzer.
        /// </summary>
        [JsonPropertyName("version")]
        public virtual string Version { get; set; }
    }

    /// <summary>
    /// The analyzed data in the form of a multigraph.
    /// </summary>
    public partial class Multigraph
    {
        /// <summary>
        /// The nodes of the multigraph.
        /// </summary>
        [JsonPropertyName("nodes")]
        public virtual Dictionary<string, MultigraphNode> Nodes { get; set; }

        /// <summary>
        /// The relations of the multigraph.
        /// </summary>
        [JsonPropertyName("relations")]
        public virtual Dictionary<string, MultigraphRelation> Relations { get; set; }
    }

    /// <summary>
    /// A node of the multigraph.
    /// </summary>
    public partial class MultigraphNode
    {
        /// <summary>
        /// Comments attached to the node.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("comments")]
        public virtual List<MultigraphComment> Comments { get; set; }

        /// <summary>
        /// Diagnostics attached to the node.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("diagnostics")]
        public virtual List<MultigraphDiagnostic> Diagnostics { get; set; }

        /// <summary>
        /// The `diff` status of the node.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("diff")]
        public virtual MultigraphNodeDiffStatus? Diff { get; set; }

        /// <summary>
        /// The kind of the node.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("kind")]
        public virtual string Kind { get; set; }

        /// <summary>
        /// Name of the entity this node represents.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("name")]
        public virtual string Name { get; set; }
    }

    /// <summary>
    /// A comment regarding a node.
    /// </summary>
    public partial class MultigraphComment
    {
        [JsonPropertyName("content")]
        public virtual string Content { get; set; }

        [JsonPropertyName("format")]
        public virtual MultigraphCommentFormat Format { get; set; }
    }

    /// <summary>
    /// A diagnostic message (warning, error, etc.) regarding a node.
    /// </summary>
    public partial class MultigraphDiagnostic
    {
        [JsonPropertyName("id")]
        public virtual string Id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("message")]
        public virtual string Message { get; set; }

        [JsonPropertyName("severity")]
        public virtual MultigraphDiagnosticSeverity Severity { get; set; }
    }

    /// <summary>
    /// A relation of the multigraph.
    /// </summary>
    public partial class MultigraphRelation
    {
        /// <summary>
        /// The edges of the relation.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("edges")]
        public virtual Dictionary<string, MultigraphEdge> Edges { get; set; }

        /// <summary>
        /// Whether or not the relation is transitive. That is a -> b -> c => a -> c.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("isTransitive")]
        public virtual bool? IsTransitive { get; set; }

        /// <summary>
        /// The name of the relation.
        /// </summary>
        [JsonPropertyName("name")]
        public virtual string Name { get; set; }
    }

    /// <summary>
    /// An edge of the relation.
    /// </summary>
    public partial class MultigraphEdge
    {
        /// <summary>
        /// The id of the destination node.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("dst")]
        public virtual string Dst { get; set; }

        /// <summary>
        /// The id of the source node.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("src")]
        public virtual string Src { get; set; }
    }

    public enum MultigraphCommentFormat { Markdown, Plain };

    public enum MultigraphDiagnosticSeverity { Error, Hidden, Info, Warning };

    /// <summary>
    /// The `diff` status of the node.
    /// </summary>
    public enum MultigraphNodeDiffStatus { Added, Deleted, Modified, Unmodified };

    public partial class DataModel
    {
        public static DataModel FromJson(string json) => JsonSerializer.Deserialize<DataModel>(json, Helveg.Visualization.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this DataModel self) => JsonSerializer.Serialize(self, Helveg.Visualization.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General)
        {
            Converters =
            {
                MultigraphCommentFormatConverter.Singleton,
                MultigraphDiagnosticSeverityConverter.Singleton,
                MultigraphNodeDiffStatusConverter.Singleton,
                new DateOnlyConverter(),
                new TimeOnlyConverter(),
                IsoDateTimeOffsetConverter.Singleton
            },
        };
    }

    internal class MultigraphCommentFormatConverter : JsonConverter<MultigraphCommentFormat>
    {
        public override bool CanConvert(Type t) => t == typeof(MultigraphCommentFormat);

        public override MultigraphCommentFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            switch (value)
            {
                case "markdown":
                    return MultigraphCommentFormat.Markdown;
                case "plain":
                    return MultigraphCommentFormat.Plain;
            }
            throw new Exception("Cannot unmarshal type MultigraphCommentFormat");
        }

        public override void Write(Utf8JsonWriter writer, MultigraphCommentFormat value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case MultigraphCommentFormat.Markdown:
                    JsonSerializer.Serialize(writer, "markdown", options);
                    return;
                case MultigraphCommentFormat.Plain:
                    JsonSerializer.Serialize(writer, "plain", options);
                    return;
            }
            throw new Exception("Cannot marshal type MultigraphCommentFormat");
        }

        public static readonly MultigraphCommentFormatConverter Singleton = new MultigraphCommentFormatConverter();
    }

    internal class MultigraphDiagnosticSeverityConverter : JsonConverter<MultigraphDiagnosticSeverity>
    {
        public override bool CanConvert(Type t) => t == typeof(MultigraphDiagnosticSeverity);

        public override MultigraphDiagnosticSeverity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            switch (value)
            {
                case "error":
                    return MultigraphDiagnosticSeverity.Error;
                case "hidden":
                    return MultigraphDiagnosticSeverity.Hidden;
                case "info":
                    return MultigraphDiagnosticSeverity.Info;
                case "warning":
                    return MultigraphDiagnosticSeverity.Warning;
            }
            throw new Exception("Cannot unmarshal type MultigraphDiagnosticSeverity");
        }

        public override void Write(Utf8JsonWriter writer, MultigraphDiagnosticSeverity value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case MultigraphDiagnosticSeverity.Error:
                    JsonSerializer.Serialize(writer, "error", options);
                    return;
                case MultigraphDiagnosticSeverity.Hidden:
                    JsonSerializer.Serialize(writer, "hidden", options);
                    return;
                case MultigraphDiagnosticSeverity.Info:
                    JsonSerializer.Serialize(writer, "info", options);
                    return;
                case MultigraphDiagnosticSeverity.Warning:
                    JsonSerializer.Serialize(writer, "warning", options);
                    return;
            }
            throw new Exception("Cannot marshal type MultigraphDiagnosticSeverity");
        }

        public static readonly MultigraphDiagnosticSeverityConverter Singleton = new MultigraphDiagnosticSeverityConverter();
    }

    internal class MultigraphNodeDiffStatusConverter : JsonConverter<MultigraphNodeDiffStatus>
    {
        public override bool CanConvert(Type t) => t == typeof(MultigraphNodeDiffStatus);

        public override MultigraphNodeDiffStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            switch (value)
            {
                case "added":
                    return MultigraphNodeDiffStatus.Added;
                case "deleted":
                    return MultigraphNodeDiffStatus.Deleted;
                case "modified":
                    return MultigraphNodeDiffStatus.Modified;
                case "unmodified":
                    return MultigraphNodeDiffStatus.Unmodified;
            }
            throw new Exception("Cannot unmarshal type MultigraphNodeDiffStatus");
        }

        public override void Write(Utf8JsonWriter writer, MultigraphNodeDiffStatus value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case MultigraphNodeDiffStatus.Added:
                    JsonSerializer.Serialize(writer, "added", options);
                    return;
                case MultigraphNodeDiffStatus.Deleted:
                    JsonSerializer.Serialize(writer, "deleted", options);
                    return;
                case MultigraphNodeDiffStatus.Modified:
                    JsonSerializer.Serialize(writer, "modified", options);
                    return;
                case MultigraphNodeDiffStatus.Unmodified:
                    JsonSerializer.Serialize(writer, "unmodified", options);
                    return;
            }
            throw new Exception("Cannot marshal type MultigraphNodeDiffStatus");
        }

        public static readonly MultigraphNodeDiffStatusConverter Singleton = new MultigraphNodeDiffStatusConverter();
    }
    
    public class DateOnlyConverter : JsonConverter<DateOnly>
    {
        private readonly string serializationFormat;
        public DateOnlyConverter() : this(null) { }

        public DateOnlyConverter(string? serializationFormat)
        {
            this.serializationFormat = serializationFormat ?? "yyyy-MM-dd";
        }

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return DateOnly.Parse(value!);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(serializationFormat));
    }

    public class TimeOnlyConverter : JsonConverter<TimeOnly>
    {
        private readonly string serializationFormat;

        public TimeOnlyConverter() : this(null) { }

        public TimeOnlyConverter(string? serializationFormat)
        {
            this.serializationFormat = serializationFormat ?? "HH:mm:ss.fff";
        }

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return TimeOnly.Parse(value!);
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(serializationFormat));
    }

    internal class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override bool CanConvert(Type t) => t == typeof(DateTimeOffset);

        private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

        private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
        private string? _dateTimeFormat;
        private CultureInfo? _culture;

        public DateTimeStyles DateTimeStyles
        {
            get => _dateTimeStyles;
            set => _dateTimeStyles = value;
        }

        public string? DateTimeFormat
        {
            get => _dateTimeFormat ?? string.Empty;
            set => _dateTimeFormat = (string.IsNullOrEmpty(value)) ? null : value;
        }

        public CultureInfo Culture
        {
            get => _culture ?? CultureInfo.CurrentCulture;
            set => _culture = value;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            string text;


            if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
                || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
            {
                value = value.ToUniversalTime();
            }

            text = value.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);

            writer.WriteStringValue(text);
        }

        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? dateText = reader.GetString();

            if (string.IsNullOrEmpty(dateText) == false)
            {
                if (!string.IsNullOrEmpty(_dateTimeFormat))
                {
                    return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
                }
                else
                {
                    return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
                }
            }
            else
            {
                return default(DateTimeOffset);
            }
        }


        public static readonly IsoDateTimeOffsetConverter Singleton = new IsoDateTimeOffsetConverter();
    }
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603