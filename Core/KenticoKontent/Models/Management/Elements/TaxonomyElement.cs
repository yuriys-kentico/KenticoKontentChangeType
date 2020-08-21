namespace Core.KenticoKontent.Models.Management.Elements
{
    public class TaxonomyElement : AbstractReferenceListElement
    {
        public TaxonomyElement(AbstractReferenceListElement element)
        {
            Element = element.Element;
            Value = element.Value;
        }

        public override AbstractElement Clone()
        {
            return new TaxonomyElement(this);
        }
    }
}