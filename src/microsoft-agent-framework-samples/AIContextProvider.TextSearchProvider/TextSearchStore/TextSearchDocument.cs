using System;
using System.Collections.Generic;
using System.Text;

public class TextSearchDocument
{
    /// <summary>
    /// 获取或设置文档所属的可选命名空间列表。
    /// </summary>
    /// <remarks>
    /// 命名空间是文档的逻辑分组，例如，可以包含组 ID，以将文档限定到特定用户组。
    /// </remarks>
    public IList<string> Namespaces { get; set; } = [];

    /// <summary>
    /// 获取或设置文本形式的内容。
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 获取或设置文档的可选源 ID。
    /// </summary>
    /// <remarks>
    /// 此 ID 在存储文档的集合中应当是唯一的，并可用于映射回该文档对应的源项目。
    /// 如果以后需要进行更新，或者源文档已被删除且此文档也需要删除，
    /// 可以使用此 ID 再次找到该文档。
    /// </remarks>
    public string? SourceId { get; set; }

    /// <summary>
    /// 获取或设置源文档的可选名称。
    /// </summary>
    /// <remarks>
    /// 当文档作为查询响应的一部分被引用时，此名称可用于提供引用链接的显示名称。
    /// </remarks>
    public string? SourceName { get; set; }
    /// <summary>
    /// 获取或设置指向文档来源的可选链接。
    /// </summary>
    /// <remarks>
    /// 当文档作为查询响应的一部分被引用时，此链接可用于提供引用链接。
    /// </remarks>
    public string? SourceLink { get; set; }
}