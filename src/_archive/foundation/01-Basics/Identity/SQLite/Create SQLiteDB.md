# 创建 SQLite 数据库步骤

1. **安装 SQLite 工具**
    - 下载并安装 SQLite 命令行工具 [SQLite Download Page](https://www.sqlite.org/download.html)。
    ![SQLite Download Page](/src/01-Basics/Identity/SQLite/Materials/SQLite-download.jpg)

2. **Window系统配置 SQLite**
    - 下载对应的 sqlite-dll (x86 或者 x64) 以及 tools
    - 在 C 盘创建 SQLite 文件夹，将下载的文件复制到 SQLite 文件夹下
    - 配置环境变量

    ![SQLite Environment Variable Configuration](/src/01-Basics/Identity/SQLite/Materials/1.jpg)
3. **验证**
    ![SQLite verification screenshot](/src/01-Basics/Identity/SQLite/Materials/2.jpg)
4. **Migrations命令**
    - 打开命令行工具，导航到你想要创建数据库文件的目录。
    - 执行以下命令创建数据库文件：

      ```sh
      dotnet ef migrations add InitDBCommand //删除之前的migrations，重新运行。之前的是SQL Server版本
      dotnet ef database update /*直接运行这个命令*/
      ```

5. **SQLite Tool**
    - [SQLite数据库管理工具](https://sqlitestudio.pl/)

6. **插入数据**
    - 使用以下 SQL 语句插入数据：

      ```sql
      INSERT INTO users (username, password) VALUES ('user1', 'password1');
      INSERT INTO users (username, password) VALUES ('user2', 'password2');
      ```

7. **查询数据**
    - 使用以下 SQL 语句查询数据：

      ```sql
      SELECT * FROM users;
      ```

8. **退出 SQLite 命令行**
    - 输入 `.exit` 或按 `Ctrl + D` 退出 SQLite 命令行。

以上步骤将帮助你创建一个简单的 SQLite 数据库并进行基本操作。
