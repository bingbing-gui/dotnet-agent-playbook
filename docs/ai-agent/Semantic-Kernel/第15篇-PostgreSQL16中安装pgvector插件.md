# PostgreSQL 16.9 安装 pgvector

## PGVector 简介与应用场景

PGVector 是 PostgreSQL 的扩展，支持原生存储、索引和检索向量（Vector Embeddings），广泛用于 AI 相关场景。它可高效管理文本、图像、音频等生成的向量表示，常见应用包括：

- **RAG（检索增强生成）**：向量化存储文档，通过语义搜索为大模型（LLM）检索相关内容。  
- **推荐系统**：利用用户与内容向量的相似度，实现个性化推荐。  
- **多媒体检索**：支持以图搜图、以文搜文等跨模态检索。  
- **相似度检测**：用于文本、图片等内容的相似性匹配与去重。  

> ⚠️ 官方提供了部分平台的预编译安装包，但并非所有环境都有，需要时可通过源码编译安装。

## pgvector 官方资源

- GitHub 地址：[https://github.com/pgvector/pgvector](https://github.com/pgvector/pgvector)

![pgvector](/docs/智能体开发框架/SemanticKernel/Materials/pgvector.png)


## 安装编译工具 & PostgreSQL 开发头文件

```bash
# 更新包索引
sudo apt update
# 安装编译必需工具、PG16 开发包、git（仅安装缺少的包）
sudo apt install -y --no-install-recommends \
    build-essential \
    postgresql-server-dev-16 \
    git
```

**说明：**

- `build-essential`：包含 gcc、g++、make 等编译工具
- `postgresql-server-dev-16`：PostgreSQL 16 的开发文件（不会重装 PostgreSQL）
- `git`：用于下载 pgvector 源码

---

## 验证工具是否可用

```bash
make --version
gcc --version
pg_config --version
```

- **make**  
  作用：自动化构建工具，常用于根据 Makefile 文件中的规则自动编译和构建项目。  
  用途：在编译 pgvector 这类 C/C++ 项目时，make 会自动处理源代码的编译、链接等步骤。

- **gcc**  
  作用：GNU 项目的 C/C++ 编译器。  
  用途：用于将 C/C++ 源代码编译为可执行文件或库，是 Linux 下最常用的编译器之一。

- **pg_config**  
  作用：PostgreSQL 提供的工具，用于显示 PostgreSQL 安装的相关信息（如版本、头文件路径、库文件路径等）。  
  用途：在编译 PostgreSQL 插件（如 pgvector）时，pg_config 能帮助编译器找到正确的 PostgreSQL 头文件和库文件。

---

## 5. 下载并编译安装 pgvector

```bash
# 切到临时目录
cd /tmp

# 删除可能已有的 pgvector 源码目录
rm -rf pgvector

# 克隆指定版本源码（这里是 v0.8.0，可换最新）
git clone --branch v0.8.0 https://github.com/pgvector/pgvector.git

cd pgvector

# 编译
make

# 安装（需要 sudo）
sudo make install
```

---

## 6. 在数据库中启用 pgvector

```bash
# 登录 PostgreSQL
psql -U postgres -d 你的数据库名

# 在数据库中创建扩展
CREATE EXTENSION IF NOT EXISTS vector;
```

---

## 7. 验证安装

```sql
SELECT extname, extversion 
FROM pg_extension 
WHERE extname = 'vector';
```

---

## 总结

通过上述步骤，您可以在 PostgreSQL 16 环境下成功安装并启用 pgvector 插件，为数据库添加高效的向量存储与检索能力。pgvector 能助力 AI、推荐系统、语义搜索等多种场景，提升数据处理与分析的智能化水平。同时pgvector也提供了针对dotnet的SDK, 建议后续结合实际业务需求，深入探索 pgvector 的高级功能与最佳实践。
