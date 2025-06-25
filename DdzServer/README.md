# 斗地主游戏服务器 (.NET Core)

## 项目概述

这是一个基于 ASP.NET Core 8.0 和 SignalR 的斗地主游戏服务器，从原始的 Node.js 版本迁移而来。

## 技术栈

- **框架**: ASP.NET Core 8.0
- **实时通信**: SignalR
- **数据库**: MySQL 8.0
- **ORM**: Entity Framework Core
- **架构**: 分层架构 (Models, Services, Hubs, Controllers)

## 系统要求

- .NET Core 8.0 SDK
- MySQL 8.0 或更高版本
- 支持 WebSocket 的现代浏览器

## 安装和运行

### 1. 安装依赖

```bash
# 进入服务器目录
cd DdzServer

# 恢复NuGet包
dotnet restore
```

### 2. 配置数据库

编辑 `appsettings.json` 文件，修改数据库连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ddz_game;User=root;Password=your_password;"
  }
}
```

### 3. 创建数据库

```bash
# 创建数据库迁移
dotnet ef migrations add InitialCreate

# 应用迁移到数据库
dotnet ef database update
```

### 4. 运行服务器

```bash
# 方法1: 使用启动脚本
./run.sh

# 方法2: 直接运行
dotnet run --urls "http://localhost:5000"
```

服务器将在 `http://localhost:5000` 启动。

## API 接口

### SignalR Hub

- **端点**: `/gamehub`
- **协议**: WebSocket (SignalR)

### HTTP API

- **健康检查**: `GET /api/game/health`
- **房间列表**: `GET /api/game/rooms`
- **房间信息**: `GET /api/game/rooms/{roomId}`

## 游戏流程

1. **连接**: 客户端连接到 SignalR Hub
2. **登录**: 通过 `wxlogin` 命令进行用户认证
3. **创建/加入房间**: 使用 `createroom_req` 或 `joinroom_req`
4. **游戏开始**: 房主使用 `player_start_notify` 开始游戏
5. **抢地主**: 玩家使用 `player_rob_notify` 抢地主
6. **出牌**: 使用 `chu_card_req` 或 `chu_bu_card_req` 出牌
7. **游戏结束**: 系统自动结算

## 项目结构

```
DdzServer/
├── Controllers/          # API控制器
├── Data/                # 数据访问层
├── Hubs/                # SignalR Hub
├── Models/              # 数据模型
├── Services/            # 业务服务层
├── Program.cs           # 应用程序入口
├── appsettings.json     # 配置文件
└── DdzServer.csproj     # 项目文件
```

## 故障排除

### 常见问题

1. **数据库连接失败**
   - 检查MySQL服务是否运行
   - 验证连接字符串中的用户名和密码
   - 确保数据库已创建

2. **SignalR连接失败**
   - 检查防火墙设置
   - 确保客户端使用正确的WebSocket协议
   - 验证CORS配置

3. **编译错误**
   - 确保安装了.NET Core 8.0 SDK
   - 运行 `dotnet restore` 恢复包
   - 检查NuGet包版本兼容性

## 开发说明

### 添加新功能

1. 在 `Models/` 中添加数据模型
2. 在 `Services/` 中实现业务逻辑
3. 在 `Hubs/` 中添加SignalR方法
4. 在 `Controllers/` 中添加HTTP API（如需要）

### 数据库迁移

```bash
# 创建新迁移
dotnet ef migrations add MigrationName

# 应用迁移
dotnet ef database update

# 回滚迁移
dotnet ef database update PreviousMigrationName
```

## 许可证

本项目基于原始Node.js版本迁移，遵循相同的许可证。 