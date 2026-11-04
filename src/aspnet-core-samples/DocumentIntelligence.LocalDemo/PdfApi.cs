using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.AspNetCore.Http.Features;

namespace DocumentIntelligence.LocalDemo;

public sealed record PdfCell(int RowIndex, int ColumnIndex, int RowSpan, int ColumnSpan, string Content);
public sealed record PdfTable(int RowCount, int ColumnCount, IReadOnlyList<PdfCell> Cells);
public sealed record PdfAnalysis(int PageCount, string Content, IReadOnlyList<string> Paragraphs, IReadOnlyList<PdfTable> Tables);

public interface IPdfAnalyzer
{
    Task<PdfAnalysis> AnalyzeAsync(BinaryData data, CancellationToken cancellationToken);
}

public static class PdfApi
{
    public const int MaxPdfBytes = 10 * 1024 * 1024;
    public static void MapPdfApi(this WebApplication app)
    {
        app.MapPost("/api/analyze", async (HttpContext context, IPdfAnalyzer analyzer, CancellationToken token) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            if (context.Request.Headers["X-Demo-Request"] != "1")
                return Results.Json(new { error = "请求不被允许。" }, statusCode: 403);
            var origin = context.Request.Headers.Origin.ToString();
            if (!string.IsNullOrEmpty(origin) &&
                (!Uri.TryCreate(origin, UriKind.Absolute, out var source)
                 || !string.Equals(source.Authority, context.Request.Host.Value, StringComparison.OrdinalIgnoreCase)
                 || !string.Equals(source.Scheme, context.Request.Scheme, StringComparison.OrdinalIgnoreCase)))
                return Results.Json(new { error = "请求来源不被允许。" }, statusCode: 403);
            if (!context.Request.HasFormContentType)
                return Results.BadRequest(new { error = "请上传 PDF 文件。" });
            if (context.Request.ContentLength > MaxPdfBytes + 65536)
                return Results.Json(new { error = "文件不能超过 10 MB。" }, statusCode: 413);
            try
            {
                var form = await context.Request.ReadFormAsync(new FormOptions
                {
                    MemoryBufferThreshold = MaxPdfBytes + 65536,
                    MultipartBodyLengthLimit = MaxPdfBytes
                }, token);
                var file = form.Files.GetFile("file");
                if (form.Files.Count != 1 || file is null || file.Length < 5
                    || !string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
                    return Results.BadRequest(new { error = "请选择一个有效的 PDF 文件。" });
                if (file.Length > MaxPdfBytes)
                    return Results.Json(new { error = "文件不能超过 10 MB。" }, statusCode: 413);
                await using var stream = file.OpenReadStream();
                using var buffer = new MemoryStream();
                await stream.CopyToAsync(buffer, token);
                var bytes = buffer.ToArray();
                if (!bytes.AsSpan().StartsWith("%PDF-"u8))
                    return Results.BadRequest(new { error = "文件不是有效的 PDF。" });

                var result = await analyzer.AnalyzeAsync(BinaryData.FromBytes(bytes), token);
                return Results.Ok(result);
            }
            catch (InvalidDataException)
            {
                return Results.BadRequest(new { error = "上传内容无效或超过大小限制。" });
            }
            catch (DemoConfigurationException exception)
            {
                return Results.Json(new { error = exception.Message }, statusCode: 503);
            }
            catch (RequestFailedException exception)
            {
                app.Logger.LogWarning("Document Intelligence 请求失败，状态码 {Status}。", exception.Status);
                return Results.Json(new
                {
                    error = exception.Status == 429
                        ? "Azure 请求过于频繁，请稍后重试。"
                        : $"Azure 解析失败（状态码 {exception.Status}），请检查资源配置和文档。"
                }, statusCode: 502);
            }
            catch (Exception exception) when (exception is not OperationCanceledException || !token.IsCancellationRequested)
            {
                app.Logger.LogError("PDF 解析失败，错误类型：{ErrorType}。", exception.GetType().Name);
                return Results.Json(new { error = "解析失败，请检查服务配置后重试。" }, statusCode: 500);
            }
        });
    }
}

public sealed class DemoConfigurationException(string message) : Exception(message);

public sealed class AzurePdfAnalyzer(IConfiguration configuration) : IPdfAnalyzer
{
    public async Task<PdfAnalysis> AnalyzeAsync(BinaryData data, CancellationToken cancellationToken)
    {
        var endpoint = configuration["AZURE_DOCUMENT_INTELLIGENCE_ENDPOINT"];
        var key = configuration["AZURE_DOCUMENT_INTELLIGENCE_KEY"];
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrWhiteSpace(key))
            throw new DemoConfigurationException("请先设置有效的 Azure Endpoint 和 Key 环境变量，然后重启服务。");

        var client = new DocumentIntelligenceClient(uri, new AzureKeyCredential(key));

        var operation = await client.AnalyzeDocumentAsync(
            WaitUntil.Completed, "prebuilt-layout", data,cancellationToken);

        var document = operation.Value;
        var paragraphs = document.Paragraphs
            .Select(paragraph => paragraph.Content)
            .Where(content => !string.IsNullOrWhiteSpace(content))
            .ToArray();
        var tables = document.Tables.Select(table => new PdfTable(
            table.RowCount,
            table.ColumnCount,
            table.Cells.Select(cell => new PdfCell(
                cell.RowIndex, cell.ColumnIndex, cell.RowSpan ?? 1, cell.ColumnSpan ?? 1,
                cell.Content ?? string.Empty)).ToArray())).ToArray();
        return new PdfAnalysis(document.Pages.Count, document.Content ?? string.Empty, paragraphs, tables);
    }
}
