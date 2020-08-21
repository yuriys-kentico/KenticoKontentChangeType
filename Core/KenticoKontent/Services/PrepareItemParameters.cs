using System;
using System.Collections.Generic;

using Core.KenticoKontent.Models;
using Core.KenticoKontent.Models.Management.Items;
using Core.KenticoKontent.Models.Management.References;

namespace Core.KenticoKontent.Services
{
    public class PrepareItemParameters
    {
        public ItemVariant? ItemVariant { get; set; }

        public Reference? LanguageReference { get; set; }

        public Reference? NewItemReference { get; set; }

        public IDictionary<Reference, LanguageVariant>? NewVariants { get; set; }

        public void Deconstruct(
            out ItemVariant itemVariant,
            out Reference languageReference,
            out Reference newItemReference,
            out IDictionary<Reference, LanguageVariant> newVariants
        )
        {
            itemVariant = ItemVariant ?? throw new ArgumentNullException(nameof(ItemVariant));
            languageReference = LanguageReference ?? throw new ArgumentNullException(nameof(LanguageReference));
            newItemReference = NewItemReference ?? throw new ArgumentNullException(nameof(NewItemReference));
            newVariants = NewVariants ?? throw new ArgumentNullException(nameof(NewVariants));
        }
    }
}