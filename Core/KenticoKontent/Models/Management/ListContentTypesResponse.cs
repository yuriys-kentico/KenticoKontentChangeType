using System.Collections.Generic;

using Core.KenticoKontent.Models.Management.Types;

namespace Core.KenticoKontent.Models.Management
{
    public class ListContentTypesResponse : AbstractListingResponse
    {
        public IEnumerable<ContentType>? Types { get; set; }
    }
}