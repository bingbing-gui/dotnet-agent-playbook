using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SK.VectorStores.Models
{
    public sealed class AzureEmbeddingOptions
    {
        /// <summary>Azure OpenAI 部署名（embedding 模型的部署名）。</summary>
        public string DeploymentName { get; set; } = default!;

        /// <summary>期望向量维度（需与所选模型一致，例如 1536/3072）。</summary>
        public int ExpectedDimension { get; set; } = 1536;
    }

}