using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Core;
using Core.KenticoKontent.Models;
using Core.KenticoKontent.Models.Management;
using Core.KenticoKontent.Models.Management.Elements;
using Core.KenticoKontent.Models.Management.Items;
using Core.KenticoKontent.Models.Management.References;
using Core.KenticoKontent.Models.Management.Types;
using Core.KenticoKontent.Services;

using Newtonsoft.Json.Serialization;

namespace KenticoKontent
{
    public class KontentRepository : IKontentRepository
    {
        private readonly IKontentRateLimiter kontentRateLimiter;
        private readonly HttpClient httpClient;
        private readonly Settings settings;

        public IDictionary<string, object> CacheDictionary { get; } = new ConcurrentDictionary<string, object>();

        public KontentRepository(
            IKontentRateLimiter kontentRateLimiter,
            HttpClient httpClient,
            Settings settings
            )
        {
            this.kontentRateLimiter = kontentRateLimiter;
            this.httpClient = httpClient;
            this.settings = settings;

            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", settings.ManagementApiKey);
        }

        public async Task PrepareChangeType(PrepareItemParameters prepareItemParameters)
        {
            var ((item, variant), languageReference, newItemReference, newItemVariants) = prepareItemParameters;

            var allTypes = await ListContentTypes();

            foreach (var type in allTypes)
            {
                var allTypeVariants = await ListVariantsByType(new IdReference(type.Id!));

                foreach (var typeVariant in allTypeVariants)
                {
                    var matched = false;

                    foreach (var element in typeVariant.Elements!)
                    {
                        switch (element)
                        {
                            case RichTextElement richTextElement:

                                void ReplaceReferences(ref string richTextValue, string pattern)
                                {
                                    var matches = Regex.Matches(richTextValue, pattern);

                                    foreach (Match? match in matches)
                                    {
                                        if (match?.Success == true)
                                        {
                                            var matchValue = match.Groups[1];

                                            richTextValue = richTextValue.Replace(matchValue.Value, newItemReference!.Value);

                                            matched = true;
                                        }
                                    }
                                }

                                if (richTextElement.Value != null)
                                {
                                    var newRichTextValue = richTextElement.Value;

                                    ReplaceReferences(ref newRichTextValue, $"(?<=data-type=\"item\" *)data-id=\"{item.Id}\"");
                                    ReplaceReferences(ref newRichTextValue, $"(?<=data-type=\"item\" *)data-external-id=\"{item.ExternalId}\"");

                                    newRichTextValue = Regex.Replace(newRichTextValue, "(?<=data-type=\"item\" *)data-id", "data-external-id");

                                    ReplaceReferences(ref newRichTextValue, $"data-item-id=\"{item.Id}\"");
                                    ReplaceReferences(ref newRichTextValue, $"data-item-external-id=\"{item.ExternalId}\"");

                                    newRichTextValue = newRichTextValue.Replace("data-item-id", "data-item-external-id");

                                    richTextElement.Value = newRichTextValue;
                                }

                                break;

                            case LinkedItemsElement linkedItemsElement:

                                Reference ReplaceReference(Reference reference)
                                {
                                    if (reference.Value == variant.ItemReference?.Value)
                                    {
                                        matched = true;

                                        return newItemReference;
                                    }

                                    return reference;
                                }

                                if (linkedItemsElement.Value != null)
                                {
                                    linkedItemsElement.Value = linkedItemsElement.Value.Select(ReplaceReference).ToList();
                                }

                                break;
                        }
                    }

                    if (matched)
                    {
                        newItemVariants.Add(typeVariant.ItemReference!, typeVariant);
                    }
                }
            }
        }

        public async Task<ItemVariant> GetItemVariant(GetItemVariantParameters getItemVariantParameters)
        {
            var (itemReference, languageReference) = getItemVariantParameters;

            var item = await RetrieveContentItem(itemReference);

            var variant = await RetrieveLanguageVariant(new RetrieveLanguageVariantParameters
            {
                ItemReference = itemReference,
                TypeReference = item.TypeReference,
                LanguageReference = languageReference
            });

            if (variant == null)
            {
                throw new NotImplementedException("Variant could not be retrieved.");
            }

            return new ItemVariant { Item = item, Variant = variant };
        }

        public async Task<IEnumerable<ContentItem>> ListContentItems()
        {
            if (CacheDictionary.TryGetValue(nameof(ListContentItems), out var item) && item is IEnumerable<ContentItem> cachedItems)
            {
                return cachedItems;
            }

            var allItems = new List<ContentItem>();

            await EnumerateListing<ListContentItemsResponse>(() => Get($"items"), responseObject =>
            {
                if (responseObject.Items != null)
                {
                    allItems.AddRange(responseObject.Items);
                }
            });

            CacheDictionary.Add(nameof(ListContentItems), allItems);

            return allItems;
        }

        public async Task<ContentItem> RetrieveContentItem(Reference itemReference)
        {
            return await Cache(
                () => kontentRateLimiter.WithRetry(() => Get($"items/{itemReference}")),
                itemReference,
                async response =>
            {
                await ThrowIfNotSuccessStatusCode(response);

                return await response.Content.ReadAsAsync<ContentItem>();
            });
        }

        public async Task<IEnumerable<LanguageVariant>> ListVariantsByType(Reference typeReference)
        {
            if (CacheDictionary.TryGetValue(nameof(ListVariantsByType) + typeReference, out var item) && item is IEnumerable<LanguageVariant> cachedVariants)
            {
                return cachedVariants;
            }

            var allVariants = new List<LanguageVariant>();

            await EnumerateListing<ListLanguageVariantsResponse>(() => Get($"types/{typeReference}/variants"), responseObject =>
            {
                if (responseObject.Variants != null)
                {
                    allVariants.AddRange(responseObject.Variants);
                }
            });

            foreach (var variant in allVariants)
            {
                await BindVariantElementTypes(typeReference, variant);
            }

            CacheDictionary.Add(nameof(ListVariantsByType) + typeReference, allVariants);

            return allVariants;
        }

        public async Task<LanguageVariant?> RetrieveLanguageVariant(RetrieveLanguageVariantParameters retrieveLanguageVariantParameters)
        {
            var (itemReference, typeReference, languageReference) = retrieveLanguageVariantParameters;

            var variant = await Cache(
                () => kontentRateLimiter.WithRetry(() => Get($"items/{itemReference}/variants/{languageReference}")),
                itemReference + languageReference,
                async response =>
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }

                    await ThrowIfNotSuccessStatusCode(response);

                    return await response.Content.ReadAsAsync<LanguageVariant>();
                });

            if (variant == null)
            {
                return null;
            }

            await BindVariantElementTypes(typeReference, variant);

            return variant;
        }

        private async Task BindVariantElementTypes(Reference typeReference, LanguageVariant variant)
        {
            var itemTypeElements = new HashSet<ElementType>();

            var itemType = (await ListContentTypes()).First(type => type.Id == typeReference.Value);

            if (itemType.Elements == null)
            {
                throw new NotImplementedException("Item type does not have elements.");
            }

            itemTypeElements.UnionWith(itemType.Elements);

            foreach (var typeElement in itemType.Elements)
            {
                if (typeElement.Snippet != null)
                {
                    var itemTypeSnippet = await RetrieveContentTypeSnippet(typeElement.Snippet);

                    if (itemTypeSnippet.Elements == null)
                    {
                        throw new NotImplementedException("Item type snippet does not have elements.");
                    }

                    itemTypeElements.UnionWith(itemTypeSnippet.Elements);
                }
            }

            variant.Elements = variant.Elements.Select(element =>
            {
                if (element is AbstractReferenceListElement listElement)
                {
                    var elementType = itemTypeElements.First(typeElement => typeElement.Id == element.Element?.Value);

                    switch (elementType.Type)
                    {
                        case "asset":
                            return new AssetElement(listElement);

                        case "multiple_choice":
                            return new MultipleChoiceElement(listElement);

                        case "taxonomy":
                            return new TaxonomyElement(listElement);

                        case "modular_content":
                            return new LinkedItemsElement(listElement);
                    }
                }

                return element;
            }).ToList();
        }

        public string GetExternalId() => Guid.NewGuid().ToString();

        public async Task<ContentItem> UpsertContentItem(ContentItem contentItem)
        {
            var response = await kontentRateLimiter.WithRetry(() => Put($"items/{new ExternalIdReference(contentItem.ExternalId!)}", contentItem));

            await ThrowIfNotSuccessStatusCode(response);

            return await response.Content.ReadAsAsync<ContentItem>();
        }

        public async Task UpsertLanguageVariant(UpsertLanguageVariantParameters upsertLanguageVariantParameters)
        {
            var (language, variant) = upsertLanguageVariantParameters;

            var response = await kontentRateLimiter.WithRetry(() => Put($"items/{variant.ItemReference}/variants/{language}", variant));

            await ThrowIfNotSuccessStatusCode(response);
        }

        public async Task<IEnumerable<ContentType>> ListContentTypes()
        {
            if (CacheDictionary.TryGetValue(nameof(ListContentTypes), out var item) && item is IEnumerable<ContentType> cachedTypes)
            {
                return cachedTypes;
            }

            var allTypes = new List<ContentType>();

            await EnumerateListing<ListContentTypesResponse>(() => Get($"types"), responseObject =>
            {
                if (responseObject.Types != null)
                {
                    allTypes.AddRange(responseObject.Types);
                }
            });

            CacheDictionary.Add(nameof(ListContentTypes), allTypes);

            return allTypes;
        }

        public async Task<ContentType> RetrieveContentType(Reference typeReference)
        {
            return await Cache(
                () => kontentRateLimiter.WithRetry(() => Get($"types/{typeReference}")),
                typeReference,
                async response =>
            {
                await ThrowIfNotSuccessStatusCode(response);

                return await response.Content.ReadAsAsync<ContentType>();
            });
        }

        public async Task<ContentType> RetrieveContentTypeSnippet(Reference typeReference)
        {
            return await Cache(
                () => kontentRateLimiter.WithRetry(() => Get($"snippets/{typeReference}")),
                typeReference,
                async response =>
            {
                await ThrowIfNotSuccessStatusCode(response);

                return await response.Content.ReadAsAsync<ContentType>();
            });
        }

        public async Task<IEnumerable<WorkflowStep>> RetrieveWorkflowSteps()
        {
            var response = await kontentRateLimiter.WithRetry(() => Get($"workflow"));

            await ThrowIfNotSuccessStatusCode(response);

            return await response.Content.ReadAsAsync<IEnumerable<WorkflowStep>>();
        }

        public async Task CreateNewVersionLanguageVariant(UpsertLanguageVariantParameters upsertLanguageVariantParameters)
        {
            var (languageReference, variant) = upsertLanguageVariantParameters;

            var response = await kontentRateLimiter.WithRetry(() => Put($"items/{variant.ItemReference}/variants/{languageReference}/new-version"));

            await ThrowIfNotSuccessStatusCode(response);
        }

        public async Task PublishLanguageVariant(UpsertLanguageVariantParameters upsertLanguageVariantParameters)
        {
            var (languageReference, variant) = upsertLanguageVariantParameters;

            var response = await kontentRateLimiter.WithRetry(() => Put($"items/{variant.ItemReference}/variants/{languageReference}/publish"));

            await ThrowIfNotSuccessStatusCode(response);
        }

        public async Task ChangeWorkflowStepLanguageVariant(ChangeWorkflowStepParameters changeWorkflowStepParameters)
        {
            var (workflowReference, languageReference, variant) = changeWorkflowStepParameters;

            var response = await kontentRateLimiter.WithRetry(() => Put($"items/{variant.ItemReference}/variants/{languageReference}/workflow/{workflowReference}"));

            await ThrowIfNotSuccessStatusCode(response);
        }

        private async Task EnumerateListing<T>(Func<Task<HttpResponseMessage>> doRequest, Action<T> getItems) where T : AbstractListingResponse
        {
            var continuationToken = "";

            do
            {
                httpClient.DefaultRequestHeaders.Add("x-continuation", continuationToken);

                var response = await kontentRateLimiter.WithRetry(doRequest);

                await ThrowIfNotSuccessStatusCode(response);

                var responseObject = await response.Content.ReadAsAsync<T>();

                getItems(responseObject);

                if (!string.IsNullOrWhiteSpace(responseObject.Pagination?.ContinuationToken))
                {
                    continuationToken = responseObject.Pagination?.ContinuationToken;
                }

                httpClient.DefaultRequestHeaders.Remove("x-continuation");
            } while (!string.IsNullOrWhiteSpace(continuationToken));
        }

        private async Task<TOut> Cache<TIn, TOut>(Func<Task<TIn>> doRequest, string key, Func<TIn, Task<TOut>> getItem) where TOut : class?
        {
            if (CacheDictionary.TryGetValue(key, out var item) && item is TOut tItem)
            {
                return tItem;
            }

            var newItem = await getItem(await doRequest());

            if (newItem != null)
            {
                CacheDictionary.Add(key, newItem);
            }

            return newItem;
        }

        private async Task<HttpResponseMessage> Get(string relativePath) => await httpClient.GetAsync(GetEndpoint(relativePath));

        private async Task<HttpResponseMessage> Put(string relativePath, object? value = default)
        {
            var response = await httpClient.PutAsync(GetEndpoint(relativePath), value, new JsonMediaTypeFormatter()
            {
                SerializerSettings =
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            });

            return response;
        }

        private string GetEndpoint(string? endpoint = default) => $@"https://manage.kontent.ai/v2/projects/{settings.ProjectId}/{endpoint}";

        private static async Task ThrowIfNotSuccessStatusCode(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsAsync<APIErrorResponse>();
                throw errorContent.GetException();
            }
        }
    }
}