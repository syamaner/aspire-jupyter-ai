using AspireJupyterAI.AppHost.Configuration;

namespace AspireJupyterAI.AppHost.Extensions;

public static class OllamaExtensions
{
    /// <summary>
    /// There are times running Ollama via Aspire is not produtive.
    /// For instance:
    ///   - If running Ollama locally on the host can make better use of GPU resources.
    ///     - This can be the case when running on Windows and have not condifured WSL-2 to utilise NVIDIA Docker.
    ///     - This can also be the case when running on a macOS Device with dedicated GPU where such features are not supported in Docker.
    ///   - Also might choose to use Open AI and Hugging Faces too.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="chatConfiguration">This is driven by Properties/launchSettings.json</param>
    /// <param name="codeModel">If Ollama is run by Aspire, this will point to the relevant chat model resource. Null otherwise.</param>
    /// <param name="embeddingModel">If Ollama is run by Aspire, this will point to the relevant embedding model resource. Null otherwise.</param>
    /// <param name="ollama"></param>
    public static void AddModelProvider(this IDistributedApplicationBuilder builder,
        ChatConfiguration chatConfiguration,
        out IResourceBuilder<OllamaModelResource>? codeModel,
        out IResourceBuilder<OllamaModelResource>? embeddingModel,
        out IResourceBuilder<OllamaResource>? ollama)
    {
        codeModel = null;
        ollama = null;
        embeddingModel = null;

        if (chatConfiguration.CodeModelProvider != ModelProvider.Ollama) return;

        // Following image is based off ollama 0.5.7 TAG and runs as non-root user.
        ollama = builder.AddOllama(Constants.ConnectionStringNames.Ollama)
            // by default Ollama container runs as root so here is a custom image that runs as non-root user.
            // see ./Ollama/Dockerfile for more details.
            // build.sh script will build this image supprting multiple CPU architectures.
            .WithImage("syamaner/ollama-nonroot")
            .WithImageTag("0.5.7")
            .WithBindMount("./Ollama/data", "/home/ollama/.ollama")
            .WithLifetime(ContainerLifetime.Session);

        if (chatConfiguration.CodeModelProvider == ModelProvider.Ollama)
        {
            codeModel = ollama.AddModel(name: Constants.ConnectionStringNames.CodeModel,
                chatConfiguration.CodeModel.Replace("ollama:", ""));
        }

        if (chatConfiguration.EmbeddingModelProvider == ModelProvider.Ollama)
        {
            embeddingModel = ollama.AddModel(
                name: Constants.ConnectionStringNames.EmbeddingModel,
                chatConfiguration.EmbeddingModel.Replace("ollama:", ""));
        }
 
    }
}