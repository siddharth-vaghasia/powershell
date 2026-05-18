using System;
using System.Text.Json.Serialization;

namespace PnP.PowerShell.Commands.Model.PowerPlatform.CopilotAgent
{
    public class CopilotAgent
    {
        [JsonPropertyName("botid")]
        public string BotId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("owner")]
        public string Owner { get; set; }

        [JsonPropertyName("cdsBotId")]
        public string CdsBotId { get; set; }

        [JsonPropertyName("configuration")]
        public string Configuration { get; set; }

        [JsonPropertyName("authenticationTrigger")]
        public int? AuthenticationTrigger { get; set; }

        [JsonPropertyName("stateCode")]
        public int? StateCode { get; set; }

        [JsonPropertyName("createdOn")]
        public DateTime? CreatedOn { get; set; }

        [JsonPropertyName("schemaName")]
        public string SchemaName { get; set; }

        [JsonPropertyName("ownerId")]
        public string OwnerId { get; set; }

        [JsonPropertyName("botModifiedOn")]
        public DateTime? BotModifiedOn { get; set; }

        [JsonPropertyName("botModifiedBy")]
        public string BotModifiedBy { get; set; }

        [JsonPropertyName("solutionId")]
        public string SolutionId { get; set; }

        [JsonPropertyName("isManaged")]
        public bool? IsManaged { get; set; }

        [JsonPropertyName("versionNumber")]
        public long? VersionNumber { get; set; }

        [JsonPropertyName("timezoneRuleVersionNumber")]
        public int? TimezoneRuleVersionNumber { get; set; }

        [JsonPropertyName("statusCode")]
        public int? StatusCode { get; set; }

        [JsonPropertyName("applicationManifestInformation")]
        public string ApplicationManifestInformation { get; set; }

        [JsonPropertyName("authenticationMode")]
        public int AuthenticationMode { get; set; }

        [JsonPropertyName("componentIdUnique")]
        public string ComponentIdUnique { get; set; }

        [JsonPropertyName("componentState")]
        public int? ComponentState { get; set; }

        [JsonPropertyName("overwriteTime")]
        public DateTime? OverwriteTime { get; set; }

        [JsonPropertyName("publishedOn")]
        public DateTime? PublishedOn { get; set; }

        [JsonPropertyName("synchronizationStatus")]
        public string SynchronizationStatus { get; set; }

        [JsonPropertyName("accessControlPolicy")]
        public int? AccessControlPolicy { get; set; }

        [JsonPropertyName("iconBase64")]
        public string IconBase64 { get; set; }
    }
}