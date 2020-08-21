namespace Core.KenticoKontent.Models.Management.References
{
    public class CodenameReference : Reference
    {
        public CodenameReference(string codename) : base(codename)
        {
        }

        protected override string ResolveValue() => $"codename/{Value}";
    }
}