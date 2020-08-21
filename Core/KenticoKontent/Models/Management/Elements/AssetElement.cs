namespace Core.KenticoKontent.Models.Management.Elements
{
    public class AssetElement : AbstractReferenceListElement
    {
        public AssetElement(AbstractReferenceListElement element)
        {
            Element = element.Element;
            Value = element.Value;
        }

        public override AbstractElement Clone()
        {
            return new AssetElement(this);
        }
    }
}