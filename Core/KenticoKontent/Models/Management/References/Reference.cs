using System;

using Newtonsoft.Json;

namespace Core.KenticoKontent.Models.Management.References
{
    [JsonConverter(typeof(ReferenceConverter))]
    public abstract class Reference
    {
        public string Value { get; }

        protected Reference(string value)
        {
            Value = value;
        }

        public static implicit operator string(Reference reference) => reference.ResolveValue();

        protected abstract string ResolveValue();

        public override string ToString() => this;

        public override int GetHashCode() => HashCode.Combine(Value);

        public override bool Equals(object? obj)
        {
            if (obj is Reference other)
            {
                return Value == other.Value;
            }

            return base.Equals(obj);
        }
    }
}