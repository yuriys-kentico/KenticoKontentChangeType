using System;

using Core.KenticoKontent.Models.Management.References;

namespace Core.KenticoKontent.Services
{
    public class GetItemVariantParameters
    {
        public Reference? ItemReference { get; set; }

        public Reference? LanguageReference { get; set; }

        public void Deconstruct(
            out Reference itemReference,
            out Reference languageReference
        )
        {
            itemReference = ItemReference ?? throw new ArgumentNullException(nameof(ItemReference));
            languageReference = LanguageReference ?? throw new ArgumentNullException(nameof(LanguageReference));
        }
    }
}