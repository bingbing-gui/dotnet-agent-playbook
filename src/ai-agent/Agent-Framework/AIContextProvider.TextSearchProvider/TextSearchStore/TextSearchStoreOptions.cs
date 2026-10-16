using System;
using System.Collections.Generic;
using System.Text;

namespace AIContextProvider.TextSearchProvider
{
    public sealed class TextSearchStoreOptions
    {
        /// <summary>
        /// 获取或设置一个可选的命名空间，用于在进行向量搜索时预先筛选可能的记录。
        /// </summary>
        public string? SearchNamespace { get; init; }

        /// <summary>
        /// 获取或设置一个值，该值指示是否使用源 ID 作为记录的主键。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 使用源 ID 作为主键可以轻松地从源更新任何已更改的记录，因为这些记录可以再次进行 upsert，并将覆盖同一记录的先前版本。
        /// </para>
        /// <para>
        /// 此设置仅在所选键类型为字符串时可用。
        /// </para>
        /// </remarks>
        /// <value>
        /// Defaults to <c>false</c> if not set.
        /// </value>
        public bool? UseSourceIdAsPrimaryKey { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether to use hybrid search if it is available for the provided vector store.
        /// </summary>
        /// <value>
        /// Defaults to <c>true</c> if not set.
        /// </value>
        public bool? UseHybridSearch { get; init; }

        /// <summary>
        /// 获取或设置一个单词分割器函数，用于将搜索文本拆分为单独的单词，以便进行混合搜索。
        /// 如果 <see cref="UseHybridSearch"/> 设置为 <c>false</c>，则不会使用此函数。
        /// </summary>
        /// <remarks>
        /// 默认为一个简单的基于文本字符的分割器，该分割器按任何非文本字符拆分文本。
        /// </remarks>
        public Func<string, ICollection<string>>? WordSegmenter { get; init; }

        /// <summary>
        /// 获取或设置文本搜索存储中记录的键类型。
        /// </summary>
        /// <remarks>
        /// 确保选择底层向量存储支持的键类型。
        /// 请注意，当使用 <see cref="UseSourceIdAsPrimaryKey"/> 时，必须选择 <see cref="string"/>。
        /// </remarks>
        /// <value>如果未设置，默认为 <see cref="string"/>。当前仅支持 <see cref="string"/> 和 <see cref="Guid"/>。</value>
        public Type? KeyType { get; init; }

        /// <summary>
        /// 获取或设置一个可选的回调，以使用源 ID 或源链接加载源文本
        /// 如果源文本未保存在数据库中。
        /// </summary>
        /// <remarks>
        /// 响应应包括请求中提供的源 ID 或源链接，以及从源加载的源文本。
        /// </remarks>
        public Func<List<SourceRetrievalRequest>, Task<IEnumerable<SourceRetrievalResponse>>>? SourceRetrievalCallback { get; init; }

        /// <summary>
        /// Represents a request to the <see cref="SourceRetrievalCallback"/>.
        /// </summary>
        public sealed class SourceRetrievalRequest
        {
            /// <summary>
            /// 初始化 <see cref="SourceRetrievalRequest"/> 类的新实例。
            /// </summary>
            /// <param name="sourceId">要检索的文档的源 ID。</param>
            /// <param name="sourceLink">要检索的文档的源链接。</param>
            public SourceRetrievalRequest(string? sourceId, string? sourceLink)
            {
                this.SourceId = sourceId;
                this.SourceLink = sourceLink;
            }

            /// <summary>
            /// 获取或设置要检索的文档的源 ID。
            /// </summary>
            public string? SourceId { get; set; }

            /// <summary>
            /// 获取或设置要检索的文档的源链接。
            /// </summary>
            public string? SourceLink { get; set; }
        }

        /// <summary>
        /// 表示来自 <see cref="SourceRetrievalCallback"/> 的响应。
        /// </summary>
        public sealed class SourceRetrievalResponse
        {
            /// <summary>
            /// 初始化 <see cref="SourceRetrievalResponse"/> 类的新实例。
            /// </summary>
            /// <param name="request">与此响应匹配的请求。</param>
            /// <param name="text">检索到的源文本。</param>
            public SourceRetrievalResponse(SourceRetrievalRequest request, string text)
            {
                ArgumentNullException.ThrowIfNull(request);
                ArgumentNullException.ThrowIfNull(text);

                this.SourceId = request.SourceId;
                this.SourceLink = request.SourceLink;
                this.Text = text;
            }

            /// <summary>
            /// 获取或设置已检索文档的源 ID。
            /// </summary>
            public string? SourceId { get; set; }

            /// <summary>
            /// 获取或设置已检索文档的源链接。
            /// </summary>
            public string? SourceLink { get; set; }

            /// <summary>
            /// 获取或设置已检索文档的源文本。
            /// </summary>
            public string Text { get; set; }
        }
    }
}
