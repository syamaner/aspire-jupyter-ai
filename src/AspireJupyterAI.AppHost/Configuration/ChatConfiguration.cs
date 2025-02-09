namespace AspireJupyterAI.AppHost.Configuration;

public record ChatConfiguration
{
    public ChatConfiguration()
    {  
        CodeModel = Environment.GetEnvironmentVariable("CODE_MODEL") ?? "ollama:codellama:7b";
        ExternalOllamaConnectionString = Environment.GetEnvironmentVariable("EXTERNAL_OLLAMA_CONNECTION_STRING") ?? "";
        if(Enum.TryParse<ModelProvider>(Environment.GetEnvironmentVariable("CODE_MODEL_PROVIDER"), 
               out var codeModelProvider))
        {
            CodeModelProvider = codeModelProvider;
        } 
        EmbeddingModel = Environment.GetEnvironmentVariable("EMBEDDING_MODEL") ?? "ollama:nomic-embed-text";
        if(Enum.TryParse<ModelProvider>(Environment.GetEnvironmentVariable("EMBEDDING_MODEL_PROVIDER"), 
               out var embeddingModelProvider))
        {
            EmbeddingModelProvider = embeddingModelProvider;
        } 
    }  
    
    /// <summary>
    /// Name of the Chat Model to be used.
    /// </summary>
    public string CodeModel { get; init; }    

    /// <summary>
    /// Whether or not using Ollama via Docker, Ollama on Host machine, OpenAI or Hugging Face.
    /// </summary>
    public ModelProvider CodeModelProvider { get; init; }
    
    /// <summary>
    /// Name of the Chat Model to be used.
    /// </summary>
    public string EmbeddingModel { get; init; }    

    /// <summary>
    /// Whether or not using Ollama via Docker, Ollama on Host machine, OpenAI or Hugging Face.
    /// </summary>
    public ModelProvider EmbeddingModelProvider { get; init; }

    public string ExternalOllamaConnectionString { get; set; }
}