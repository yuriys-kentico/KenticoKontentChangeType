using System;

namespace Core.KenticoKontent.Models.Management.Elements
{
    public class DateTimeElement : AbstractElement
    {
        public DateTime? Value { get; set; }

        public override AbstractElement Clone()
        {
            return new DateTimeElement
            {
                Element = Element,
                Value = Value
            };
        }
    }
}