using System.Collections.Generic;

using Core.KenticoKontent.Models.Management.Elements;
using Core.KenticoKontent.Models.Management.References;

using Newtonsoft.Json;

namespace Core.KenticoKontent.Models.Management.Items
{
    public class LanguageVariant
    {
        [JsonProperty("item")]
        public Reference? ItemReference { get; set; }

        [JsonProperty("workflow_step")]
        public Reference? WorkflowStep { get; set; }

        public IList<AbstractElement>? Elements { get; set; }
    }
}