namespace Core.KenticoKontent.Models.Management.Elements
{
    public class UrlSlugElement : AbstractElement
    {
        public string? Mode { get; set; }

        public string? Value { get; set; }

        public override AbstractElement Clone()
        {
            return new UrlSlugElement
            {
                Element = Element,
                Mode = Mode,
                Value = Value
            };
        }
    }
}