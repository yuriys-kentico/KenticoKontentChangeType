using System;

using Core.KenticoKontent.Models.Management.References;

using Newtonsoft.Json;

namespace Core.KenticoKontent.Models.Management.Elements
{
    [JsonConverter(typeof(AbstractElementConverter))]
    public class AbstractElement
    {
        public Reference? Element { get; set; }

        public virtual AbstractElement Clone() => throw new NotImplementedException();
    }
}