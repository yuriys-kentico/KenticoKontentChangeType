namespace Core.KenticoKontent.Models.Management.Elements
{
    public class NumberElement : AbstractElement
    {
        public float? Value { get; set; }

        public override AbstractElement Clone()
        {
            return new NumberElement
            {
                Element = Element,
                Value = Value
            };
        }
    }
}