var app = WebApplication.Create();

app.MapGet("", async context =>
{
    context.Response.Headers.Append("content-type", "text/html;charset=utf-8");

    var body = $@"
    <html>
    <head>
        <link href='https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css' rel='stylesheet' />
        <style>
            body {{
                background-color: #f4f4f4;
                height: 100vh;
                display: flex;
                justify-content: center;
                align-items: center;
            }}
            .upload-container {{
                background-color: white;
                padding: 40px;
                border-radius: 10px;
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                width: 400px;
            }}
            .custom-file-input {{
                cursor: pointer;
            }}
        </style>
    </head>
    <body>
        <div class='upload-container'>
            <h1 class='text-center mb-4'>上传文件</h1>
            <form action='Upload' method='post' enctype='multipart/form-data'>
                <div class='form-group'>
                    <div class='custom-file'>
                        <input type='file' class='custom-file-input' id='customFile' name='file'>
                        <label class='custom-file-label' for='customFile'>选择文件</label>
                    </div>
                </div>
                <button type='submit' class='btn btn-primary btn-block'>上传</button>
            </form>
        </div>
        <script src='https://code.jquery.com/jquery-3.5.1.slim.min.js'></script>
        <script src='https://cdn.jsdelivr.net/npm/bootstrap@4.5.2/dist/js/bootstrap.bundle.min.js'></script>
        <script>
            // Update the label of the file input with the selected file name
            $('.custom-file-input').on('change', function (e) {{
                var fileName = e.target.files[0].name;
                $(this).next('.custom-file-label').html(fileName);
            }});
        </script>
    </body>
    </html>
";

    await context.Response.WriteAsync(body);
});

app.MapPost("Upload", async context =>
{
    if (context.Request.HasFormContentType)
    {
        var form = await context.Request.ReadFormAsync();

        foreach (var f in form.Files)
        {
            using (var body = f.OpenReadStream())
            {
                var fileName = Path.Combine(app.Environment.ContentRootPath, f.FileName);
                File.WriteAllBytes(fileName, ReadFully(body));
                context.Response.Headers.Append("content-type", "text/html;charset=utf-8");
                await context.Response.WriteAsync($"上传文件被写入到 {fileName}");
            }
        }
    }
    await context.Response.WriteAsync("");
});

app.Run();

static byte[] ReadFully(Stream input)
{
    byte[] buffer = new byte[16 * 1024];
    using var ms = new MemoryStream();
    int read;
    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
    {
        ms.Write(buffer, 0, read);
    }
    return ms.ToArray();
}