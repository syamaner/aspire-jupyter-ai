using AspireJupyterAI.AppHost.Configuration;

namespace AspireJupyterAI.AppHost.Extensions;

public static class JupyterExtensions
{
   public static void SetApiProviderKey(this IDistributedApplicationBuilder builder, ModelProvider modelProvider,
        IResourceBuilder<ContainerResource> jupyterResource)
    {
        var openAiKey = builder.AddParameter("OpenAIKey", secret: true);
        var hfApiKey = builder.AddParameter("HuggingFaceKey", secret: true);
        switch (modelProvider)
        {
            case ModelProvider.OpenAI when string.IsNullOrWhiteSpace(openAiKey.Resource.Value):
                throw new InvalidOperationException("OpenAI API Key is required for OpenAI model provider.");
            case ModelProvider.HuggingFace when string.IsNullOrWhiteSpace(hfApiKey.Resource.Value):
                throw new InvalidOperationException(
                    "Hugging Face API Key is required for Hugging Face model provider.");
            case ModelProvider.Ollama:
            case ModelProvider.OllamaHost:
            default:
                break;
        }

        var (envName, envValue) = modelProvider switch
        {
            ModelProvider.Ollama => ("", ""),
            ModelProvider.OllamaHost => ("", ""),
            ModelProvider.OpenAI => ("OPENAI_API_KEY", openAiKey.Resource.Value),
            ModelProvider.HuggingFace => ("HUGGINGFACEHUB_API_TOKEN", hfApiKey.Resource.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(modelProvider), modelProvider, null)
        };
        if (!string.IsNullOrWhiteSpace(envName))
        {
            jupyterResource.WithEnvironment(envName, envValue);
        }
    }

    public static void InjectOllamaConnectionStringToJupyter(this IDistributedApplicationBuilder builder, IResourceBuilder<OllamaModelResource>? resourceBuilder, 
        IResourceBuilder<OllamaResource>? ollama1,
        IResourceBuilder<ContainerResource> jupyter1,
        ChatConfiguration chatConfiguration1, 
        string destinationConnectionStringName)
    {
        if(resourceBuilder != null && ollama1 != null)
        {
            jupyter1.WithReference(resourceBuilder).WaitFor(resourceBuilder); 
        }
        else
        {
            var connectionString = chatConfiguration1.CodeModelProvider switch
            {
                ModelProvider.Ollama or ModelProvider.OllamaHost => chatConfiguration1.ExternalOllamaConnectionString,
                ModelProvider.OpenAI or ModelProvider.HuggingFace => string.Empty,
                _ => throw new ArgumentOutOfRangeException(nameof(chatConfiguration1.CodeModelProvider),
                    chatConfiguration1.CodeModelProvider, null)
            };
            jupyter1.WithEnvironment(destinationConnectionStringName, connectionString);
        }
    }
}