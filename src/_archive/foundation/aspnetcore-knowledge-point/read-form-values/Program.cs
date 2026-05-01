var app = WebApplication.Create();

app.MapGet("", async context =>
{
    context.Response.Headers.Append("content-type", "text/html;charset=utf-8");
    var page = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>提交个人信息</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            width: 50%;
            margin: 50px auto;
            background-color: #fff;
            padding: 20px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            border-radius: 8px;
        }}
        h1 {{
            text-align: center;
            color: #333;
        }}
        form {{
            display: flex;
            flex-direction: column;
        }}
        input[type=""text""], input[type=""password""], textarea, select {{
            width: 100%;
            padding: 10px;
            margin-bottom: 20px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }}
        input[type=""checkbox""] {{
            margin-right: 10px;
        }}
        label {{
            margin-bottom: 10px;
            color: #555;
        }}
        input[type=""submit""] {{
            background-color: #28a745;
            color: #fff;
            border: none;
            padding: 10px 20px;
            cursor: pointer;
            border-radius: 4px;
            font-size: 16px;
        }}
        input[type=""submit""]:hover {{
            background-color: #218838;
        }}
    </style>
</head>
<body>
<div class=""container"">
    <h1>提交表单</h1>
    <form action=""person-info"" method=""post"" enctype=""multipart/form-data"">
        <label for=""name"">姓名</label>
        <input type=""text"" name=""name"" id=""name"" placeholder=""姓名"" required />

        <label for=""password"">密码</label>
        <input type=""password"" name=""password"" id=""password"" placeholder=""密码"" required />

        <label for=""description"">描述</label>
        <textarea name=""description"" id=""description"" placeholder=""描述""></textarea>

        <label for=""gender"">性别</label>
        <select name=""gender"" id=""gender"">
            <option value=""M"">公</option>
            <option value=""F"">母</option>
        </select>

        <label for=""hobbies"">爱好</label>
        <select name=""hobbies"" id=""hobbies"" multiple=""multiple"">
            <option>写小说</option>
            <option>看电影</option>
            <option>打篮球</option>
        </select>

        <div>
            <input type=""checkbox"" name=""married"" value=""Yes"" id=""married"" />
            <label for=""married"">是否已婚?</label>
        </div>

        <div>
            <input type=""checkbox"" name=""adventurous"" value=""Yes"" id=""adventurous"" />
            <label for=""adventurous"">是否敢于尝试新事物?</label>
        </div>

        <input type=""hidden"" name=""secretHiddenValue"" value=""保密信息"" />

        <input type=""submit"" value=""提交个人信息"" />
    </form>
</div>
</body>
</html>
";

    await context.Response.WriteAsync(page);
});

app.MapPost("person-info", async context =>
{
    context.Response.Headers.Append("content-type", "text/html;charset=utf-8");

    if (context.Request.HasFormContentType)
    {
        var form = await context.Request.ReadFormAsync();

        foreach (var v in form.Keys)
        {
            await context.Response.WriteAsync($"{v} = {form[v]} <br/>");
        }

    }
    await context.Response.WriteAsync("");
});

app.Run();