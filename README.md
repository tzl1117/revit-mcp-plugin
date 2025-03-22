# revit-mcp-plugin

## 简介

revit-mcp-plugin 允许你使用claude客户端通过 MCP 协议与 Revit 进行交互。

本项目是revit客户端（接收信息，操作revit），还需要配合[revit-mcp](https://github.com/revit-mcp/revit-mcp)（向AI提供tools）使用。

## 安装

### 环境要求

- revit 2019

### 安装流程

## 项目文件组织

```
revit_mcp/
├── Core/
│   ├── Application.cs                     # 应用程序入口点
│   ├── MCPServiceConnection.cs            # Revit外部命令
│   ├── SocketService.cs                   # Socket服务实现
│   └── JsonRPC/                           # JSON-RPC相关类
│       ├── JsonRPCRequest.cs              # 请求模型
│       ├── JsonRPCResponse.cs             # 响应模型
│       ├── JsonRPCErrorCodes.cs           # 错误代码常量
│       └── JsonRPCSerializer.cs           # 序列化/反序列化帮助类
│
├── Commands/
│   ├── Interfaces/                        # 命令接口
│   │   ├── IRevitCommand.cs               # 命令接口
│   │   └── IWaitableExternalEventHandler.cs # 事件等待处理器接口
│   │
│   ├── Base/
│   │   └── ExternalEventCommandBase.cs    # 基于外部事件的命令基类
│   │
│   ├── Registry/
│   │   └── RevitCommandRegistry.cs        # 命令注册
│   │
│   ├── Wall/                              # 墙相关命令
│   │   ├── CreateWallEventHandler.cs      # 创建墙事件处理器
│   │   └── CreateWallCommand.cs           # 创建墙命令
│   │
│   └── Code/                              # 代码执行相关命令
│       ├── ExecuteCodeEventHandler.cs     # 执行代码事件处理器
│       └── ExecuteCodeCommand.cs          # 执行代码命令
│
├── Utils/                                 # 工具类
│
└── Models/                                # 数据模型
```

### Core 目录

1. **Application.cs**: 应用程序入口点，负责初始化插件
2. **MCPServiceConnection.cs**: Revit外部命令，用于启动服务
3. **SocketService.cs**: Socket服务实现，负责与外部客户端通信
4. **JsonRPC目录**: 包含JSON-RPC协议相关的类和实现

### Commands 目录

1. **Interfaces目录**: 命令相关接口定义
2. **Base目录**: 包含命令的基类实现
3. **Registry目录**: 命令注册和管理
4. **功能性目录** (Wall, Code等): 包含特定功能的命令和事件处理器

### Utils 目录

包含各种工具类，帮助处理重复使用的功能。

### Models 目录

包含数据模型类，用于在系统各部分之间传递数据。

## 添加命令的流程

1. 在`Commands`目录下创建新的功能子目录（例如`Commands/window/`）
2. 添加事件处理器（例如`DeleteWindowEventHandler.cs`），事件处理器基于Revit的外部事件
3. 添加命令类（例如`DeleteWindowCommand.cs`），解析来着mcp服务器的信息，调用处理器实现
4. 在`SocketService`的`RegisterCommands`方法中注册新命令
