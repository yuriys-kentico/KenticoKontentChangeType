using Core.KenticoKontent.Models.Management.References;

namespace Core.KenticoKontent.Models.Management.Types
{
    public class ElementType
    {
        public string? Id { get; set; }

        public string? Type { get; set; }

        public string? Name { get; set; }

        public string? Codename { get; set; }

        public Reference? Snippet { get; set; }
    }
}