using System.Collections.Generic;

using Core.KenticoKontent.Models.Management.Items;

namespace Core.KenticoKontent.Models.Management
{
    public class ListLanguageVariantsResponse : AbstractListingResponse
    {
        public IEnumerable<LanguageVariant>? Variants { get; set; }
    }
}