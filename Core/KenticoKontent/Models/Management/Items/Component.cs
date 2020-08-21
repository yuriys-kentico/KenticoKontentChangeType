using System;
using System.Collections.Generic;

using Core.KenticoKontent.Models.Management.Elements;
using Core.KenticoKontent.Models.Management.References;

namespace Core.KenticoKontent.Models.Management.Items
{
    public class Component
    {
        public Guid Id { get; set; }

        public Reference? Type { get; set; }

        public IList<AbstractElement> Elements { get; set; } = new List<AbstractElement>();
    }
}