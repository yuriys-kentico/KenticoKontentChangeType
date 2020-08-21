namespace Core.KenticoKontent.Models.Management.References
{
    public class ExternalIdReference : Reference
    {
        public ExternalIdReference(string external_id) : base(external_id)
        {
        }

        protected override string ResolveValue() => $"external-id/{Value}";
    }
}