using System.Collections.Generic;

using Core.KenticoKontent.Models.Management.References;

namespace Core.KenticoKontent.Models.Management.Elements
{
    public class AbstractReferenceListElement : AbstractElement
    {
        public IList<Reference>? Value { get; set; }

        public override AbstractElement Clone()
        {
            return new AbstractReferenceListElement
            {
                Element = Element,
                Value = Value
            };
        }
    }
}