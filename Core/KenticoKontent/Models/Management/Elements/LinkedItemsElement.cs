namespace Core.KenticoKontent.Models.Management.Elements
{
    public class LinkedItemsElement : AbstractReferenceListElement
    {
        public LinkedItemsElement(AbstractReferenceListElement element)
        {
            Element = element.Element;
            Value = element.Value;
        }

        public override AbstractElement Clone()
        {
            return new LinkedItemsElement(this);
        }
    }
}