using AspireJupyterAI.AppHost;
using AspireJupyterAI.AppHost.Configuration;
using AspireJupyterAI.AppHost.Extensions;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);
ChatConfiguration chatConfiguration = new();


// container ports we will be using
Dictionary<string, int> applicationPorts = new()
{
    { Constants.ConnectionStringNames.JupyterService, 8888 }
};

// It is possible to use a model provider as following:
//  - Ollama using Aspire (might be good option if using an Nvidia Docker Compatible Docker host).
//  - Ollama on  host machine. Could be an option if using something like
//       a MacBook Pro with dedicated GPU where such features are not supported in Docker natively
//  - Also possible to use Open AI and Hugging Faces too.
// Downstream projects will resect the choices too so we only make the changes in app host configuration.
builder.AddModelProvider(chatConfiguration, out var codeModel, 
    out var embeddingModel, 
    out var ollama);

// For the ingestion pipeline and evaluation, we will be using Python and Jupyter.
var jupyter = builder
    .AddDockerfile(Constants.ConnectionStringNames.JupyterService, "./Jupyter")
    .WithBuildArg("PORT", applicationPorts[Constants.ConnectionStringNames.JupyterService])
    .WithBindMount("./Jupyter/Notebooks/", "/home/jovyan/work")
    .WithHttpEndpoint(targetPort: applicationPorts[Constants.ConnectionStringNames.JupyterService], env: "PORT", name:"http")
    .WithLifetime(ContainerLifetime.Session)
    .WithOtlpExporter()
    .WithEnvironment("OTEL_SERVICE_NAME", "jupyterdemo")
    .WithEnvironment("OTEL_EXPORTER_OTLP_INSECURE", "true")
    .WithEnvironment("PYTHONUNBUFFERED", "0")
    .WithEnvironment("CODE_MODEL", chatConfiguration.CodeModel)
    .WithEnvironment("EMBEDDING_MODEL", chatConfiguration.EmbeddingModel);

builder.SetApiProviderKey(chatConfiguration.CodeModelProvider, jupyter);

builder.InjectOllamaConnectionStringToJupyter(codeModel, ollama, jupyter, chatConfiguration, 
    "ConnectionStrings__codemodel");
builder.InjectOllamaConnectionStringToJupyter(embeddingModel, ollama, jupyter, chatConfiguration, 
    "ConnectionStrings__embeddingmodel");

builder.Build().Run();

 
