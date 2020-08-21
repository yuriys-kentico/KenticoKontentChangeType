using System.Collections.Generic;
using System.Linq;

using Core.KenticoKontent.Models.Management.Items;

namespace Core.KenticoKontent.Models.Management.Elements
{
    public class RichTextElement : AbstractElement
    {
        public string? Value { get; set; }

        public IEnumerable<Component>? Components { get; set; }

        public override AbstractElement Clone()
        {
            return new RichTextElement
            {
                Element = Element,
                Value = Value,
                Components = Components
            };
        }
    }
}