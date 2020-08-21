using System.Collections.Generic;
using System.Threading.Tasks;

using Core.KenticoKontent.Models;
using Core.KenticoKontent.Models.Management.Items;
using Core.KenticoKontent.Models.Management.References;
using Core.KenticoKontent.Models.Management.Types;

namespace Core.KenticoKontent.Services
{
    public interface IKontentRepository
    {
        Task PrepareChangeType(PrepareItemParameters prepareItemParameters);

        Task<ItemVariant> GetItemVariant(GetItemVariantParameters getItemVariantParameters);

        Task<IEnumerable<ContentItem>> ListContentItems();

        Task<ContentItem> RetrieveContentItem(Reference itemReference);

        Task<IEnumerable<LanguageVariant>> ListVariantsByType(Reference typeReference);

        Task<LanguageVariant?> RetrieveLanguageVariant(RetrieveLanguageVariantParameters retrieveLanguageVariantParameters);

        string GetExternalId();

        Task<ContentItem> UpsertContentItem(ContentItem contentItem);

        Task UpsertLanguageVariant(UpsertLanguageVariantParameters upsertLanguageVariantParameters);

        Task<IEnumerable<ContentType>> ListContentTypes();

        Task<ContentType> RetrieveContentType(Reference typeReference);

        Task<ContentType> RetrieveContentTypeSnippet(Reference typeReference);

        Task<IEnumerable<WorkflowStep>> RetrieveWorkflowSteps();

        Task CreateNewVersionLanguageVariant(UpsertLanguageVariantParameters upsertLanguageVariantParameters);

        Task PublishLanguageVariant(UpsertLanguageVariantParameters upsertLanguageVariantParameters);

        Task ChangeWorkflowStepLanguageVariant(ChangeWorkflowStepParameters changeWorkflowStepParameters);
    }
}