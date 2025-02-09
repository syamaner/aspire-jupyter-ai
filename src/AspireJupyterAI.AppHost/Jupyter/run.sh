#!/bin/bash
CODEMODELURL=$(echo "${ConnectionStrings__codemodel}" | cut -d'=' -f2 | cut -d';' -f1)
EMBEDDINGMODELURL=$(echo "${ConnectionStrings__embeddingmodel}" | cut -d'=' -f2 | cut -d';' -f1)
 
# Base command
CMD="jupyter lab --NotebookApp.token=''"
echo "code model: $CODEMODELURL"
echo "embedding model: $EMBEDDINGMODELURL"

# Add embedding model
CMD="$CMD --AiExtension.default_embeddings_model=$EMBEDDING_MODEL"
# Add code model
CMD="$CMD --AiExtension.default_language_model=$CODE_MODEL"
CMD="$CMD --AiExtension.default_api_keys='{\"OPENAI_API_KEY\":\"${OPENAI_API_KEY}\", \"HUGGINGFACEHUB_API_TOKEN\":\"$HUGGINGFACEHUB_API_TOKEN\"}'"
CMD="$CMD --AiExtension.default_max_chat_history=12"
#,

# Add embedding model URL if specified
if [ ! -z "$EMBEDDINGMODELURL" ]; then
    CMD="$CMD --AiExtension.model_parameters $EMBEDDING_MODEL='{\"base_url\":\"$EMBEDDINGMODELURL\"}'"
fi

# Add code model URL if specified
if [ ! -z "$CODEMODELURL" ]; then
    CMD="$CMD --AiExtension.model_parameters $CODE_MODEL='{\"base_url\":\"$CODEMODELURL\"}'"
fi
 
# Execute the command
eval "$CMD"
 