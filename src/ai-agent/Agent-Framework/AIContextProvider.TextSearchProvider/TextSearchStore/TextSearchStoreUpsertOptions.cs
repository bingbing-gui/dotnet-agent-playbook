using System;
using System.Collections.Generic;
using System.Text;

namespace AIContextProvider.TextSearchProvider
{
    public sealed class TextSearchStoreUpsertOptions
    {
        /// <summary>
        /// 获取或设置一个值，该值指示是否应在数据库中保留源文本。
        /// </summary>
        /// <value>
        /// Defaults to <see langword="false"/> if not set.
        /// </value>
        public bool DoNotPersistSourceText { get; init; }
    }
}
