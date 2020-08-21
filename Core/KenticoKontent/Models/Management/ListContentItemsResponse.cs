using System.Collections.Generic;

using Core.KenticoKontent.Models.Management.Items;

namespace Core.KenticoKontent.Models.Management
{
    public class ListContentItemsResponse : AbstractListingResponse
    {
        public IEnumerable<ContentItem>? Items { get; set; }
    }
}