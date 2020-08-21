using System.Collections.Generic;

namespace Core.KenticoKontent.Models.Management.Types
{
    public class ContentType
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public IList<ElementType>? Elements { get; set; }
    }
}