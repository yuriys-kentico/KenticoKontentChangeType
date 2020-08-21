namespace Core.KenticoKontent.Models.Management.Elements
{
    public class TextElement : AbstractElement
    {
        public string? Value { get; set; }

        public override AbstractElement Clone()
        {
            return new TextElement
            {
                Element = Element,
                Value = Value
            };
        }
    }
}