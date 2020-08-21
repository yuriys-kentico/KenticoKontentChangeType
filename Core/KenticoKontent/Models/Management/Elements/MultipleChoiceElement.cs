namespace Core.KenticoKontent.Models.Management.Elements
{
    public class MultipleChoiceElement : AbstractReferenceListElement
    {
        public MultipleChoiceElement(AbstractReferenceListElement element)
        {
            Element = element.Element;
            Value = element.Value;
        }

        public override AbstractElement Clone()
        {
            return new MultipleChoiceElement(this);
        }
    }
}