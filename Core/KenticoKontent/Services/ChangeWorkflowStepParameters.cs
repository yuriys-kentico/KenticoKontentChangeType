using System;

using Core.KenticoKontent.Models.Management.Items;
using Core.KenticoKontent.Models.Management.References;

namespace Core.KenticoKontent.Services
{
    public class ChangeWorkflowStepParameters
    {
        public Reference? WorkflowStepReference { get; set; }

        public Reference? LanguageReference { get; set; }

        public LanguageVariant? Variant { get; set; }

        public void Deconstruct(
            out Reference workflowStepReference,
            out Reference languageReference,
            out LanguageVariant variant
            )
        {
            workflowStepReference = WorkflowStepReference ?? throw new ArgumentNullException(nameof(WorkflowStepReference));
            languageReference = LanguageReference ?? throw new ArgumentNullException(nameof(LanguageReference));
            variant = Variant ?? throw new ArgumentNullException(nameof(Variant));
        }
    }
}