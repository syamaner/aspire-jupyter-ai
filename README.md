# Running Jupyter Notebooks with Jupyter AI support with .NET Aspire and Ollama

Brief demonstration of how to use Local Coding Assistant models with Jupyter Notebook.

System components:

- Ollama
  - Running as a container or running as an executable on host machine or accessible from local network. 
  - Models:
    - qwen2.5-coder:32b when Ollama is running on host
    - command-r7b when Ollama is running as container without acceleration
    - chatgpt-4o-latest when using OpenAI
- Juyter Notebooks
  - With support for .NET Interactive as a Jupyter kernel
  
- Aspire Solution

![Setup overview - locally hosted Ollama](./doc/aspire-docker-network-2.png)


## Sources:

- [](https://github.com/dotnet/interactive/blob/main/docs/NotebookswithJupyter.md)
- [](https://github.com/jupyterlab/jupyter-ai)