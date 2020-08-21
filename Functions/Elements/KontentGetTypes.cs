using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Core.KenticoKontent.Models.Management.References;
using Core.KenticoKontent.Models.Management.Types;
using Core.KenticoKontent.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Functions.Elements
{
    public partial class KontentGetTypes : BaseFunction
    {
        private readonly IKontentRepository kontentRepository;

        public KontentGetTypes(
            ILogger<KontentChangeType> logger,
            IKontentRepository kontentRepository
            ) : base(logger)
        {
            this.kontentRepository = kontentRepository;
        }

        [FunctionName(nameof(KontentGetTypes))]
        public async Task<IActionResult> Run(
            [HttpTrigger(
                "get",
                Route = Routes.KontentGetTypes
            )] string body,
            string itemCodename
            )
        {
            try
            {
                var itemReference = new CodenameReference(itemCodename);

                var allTypes = await kontentRepository.ListContentTypes();

                foreach (var type in allTypes)
                {
                    if (type.Elements == null)
                    {
                        throw new NotImplementedException("Item type does not have elements.");
                    }

                    var newTypeElements = new List<ElementType>(type.Elements);

                    foreach (var typeElement in type.Elements)
                    {
                        if (typeElement.Snippet != null)
                        {
                            var itemTypeSnippet = await kontentRepository.RetrieveContentTypeSnippet(typeElement.Snippet);

                            if (itemTypeSnippet.Elements == null)
                            {
                                throw new NotImplementedException("Item type snippet does not have elements.");
                            }

                            foreach (var typeSnippetElement in itemTypeSnippet.Elements)
                            {
                                newTypeElements.Add(typeSnippetElement);
                            }
                        }
                    }

                    var supportingElements = new[] { "snippet", "guidelines" };

                    type.Elements = newTypeElements.Where(element => !supportingElements.Contains(element.Type)).ToList();
                }

                var item = await kontentRepository.RetrieveContentItem(itemReference);

                return LogOkObject(new
                {
                    CurrentType = allTypes.First(type => type.Id == item.TypeReference?.Value),
                    OtherTypes = allTypes.Where(type => type.Id != item.TypeReference?.Value)
                });
            }
            catch (Exception ex)
            {
                return LogException(ex);
            }
        }
    }
}