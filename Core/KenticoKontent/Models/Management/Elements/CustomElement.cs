using Newtonsoft.Json;

namespace Core.KenticoKontent.Models.Management.Elements
{
    public class CustomElement : AbstractElement
    {
        public string? Value { get; set; }

        [JsonProperty("searchable_value")]
        public string? Searchable_Value { get; set; }

        public override AbstractElement Clone()
        {
            return new CustomElement
            {
                Element = Element,
                Value = Value,
                Searchable_Value = Searchable_Value
            };
        }
    }
}