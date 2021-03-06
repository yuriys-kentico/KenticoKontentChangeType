# Kentico Kontent Change Type

Sample custom element and Azure Function code that uses the Kontent Management API to change the type of a content item. The change covers all content items that reference the item.

The code in this repository is configured for Azure Functions, however it can be lifted to any web application running ASP.NET Core 3.1.

## Setup

1. Deploy the Azure Function code in `Functions.sln` to an Azure Function. See the [Function Settings](#function-settings) section for additional configuration steps.
1. Deploy the custom element code in `Client/` to a secure public host.
   - See the [Deploying](#Deploying) section for a really quick option.
1. Follow the instructions in the [Kentico Kontent documentation](https://docs.kontent.ai/tutorials/develop-apps/integrate/integrating-your-own-content-editing-features#a-3--displaying-a-custom-element-in-kentico-kontent) to add the element to a content model.
   - The `Hosted code URL` is where you deployed to in step 1.
   - The `Parameters {JSON}` is a JSON object containing optional defaults. See the [JSON parameters](#json-parameters) section for details.

## Function Settings

The Azure Function code requires two settings: `ProjectId` and `ManagementApiKey`. You can either [set them using Visual Studio](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs#function-app-settings) or [use the options here](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings#settings).

## Deploying

Netlify has made this easy. If you click the deploy button below, it will guide you through the process of deploying it to Netlify and leave you with a copy of the repository in your GitHub account as well.

[![Deploy to Netlify](https://www.netlify.com/img/deploy/button.svg)](https://app.netlify.com/start/deploy?repository=https://github.com/yuriys-kentico/KenticoKontentChangeType)

## JSON Parameters

`changeTypeEndpoint` is a `string` defining the Azure Function URL appended with `/changeType`.
`getTypesEndpoint` is a `string` defining the Azure Function URL appended with `/getTypes`.

Example JSON parameters object:

```json
{
  "changeTypeEndpoint": "https://sample-kontent-change-type.azurewebsites.net/changeType",
  "getTypesEndpoint": "https://sample-kontent-change-type.azurewebsites.net/getTypes"
}
```
