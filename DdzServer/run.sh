#!/bin/bash

# 检查.NET Core是否安装
if ! command -v dotnet &> /dev/null; then
    echo "错误: 未找到.NET Core SDK，请先安装.NET Core 8.0"
    exit 1
fi

# 检查MySQL连接
echo "正在检查MySQL连接..."
# 这里可以添加MySQL连接检查逻辑

# 恢复NuGet包
echo "正在恢复NuGet包..."
dotnet restore

# 构建项目
echo "正在构建项目..."
dotnet build

# 运行数据库迁移（如果需要）
echo "正在运行数据库迁移..."
dotnet ef database update

# 启动应用
echo "正在启动斗地主游戏服务器..."
dotnet run --urls "http://localhost:5000" 