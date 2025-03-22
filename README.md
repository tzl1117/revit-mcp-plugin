# revit-mcp-plugin

## 简介

revit-mcp-plugin 允许你使用claude客户端通过 MCP 协议与 Revit 进行交互。

本项目是revit客户端（接收信息，操作revit），还需要配合[revit-mcp](https://github.com/revit-mcp/revit-mcp)（向AI提供tools）使用。

## 环境要求

- revit 2019

## 使用方法

### 注册插件

注册插件，重启Revit

```
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>revit-mcp</Name>
    <Assembly>revit-mcp-plugin.dll</Assembly>
    <FullClassName>revit_mcp_plugin.Core.Application</FullClassName>
    <ClientId>090A4C8C-61DC-426D-87DF-E4BAE0F80EC1</ClientId>
    <VendorId>revit-mcp</VendorId>
    <VendorDescription>https://github.com/revit-mcp/revit-mcp-plugin</VendorDescription>
  </AddIn>
</RevitAddIns>
```

### 启用服务

附加模块->mcp->开启mcp服务监听

## 添加命令

对于添加命令，仅需要聚焦Commands目录（命令的具体实现）以及Core/SocketService.cs文件（注册命令）

Commands中的每一个命令分为两个部分：

- `XXXCommand`负责解析参数并触发事件处理程序，同时处理超时和错误
- `XXXEventHandler`负责实际的操作，使用事务确保操作的原子性

命令需要在SocketService中的RegisterCommands注册后才能被mcp服务调用

**流程**

1. 在`Commands`目录下创建新的功能子目录（例如`Commands/window/`）
2. 添加事件处理器（例如`CreateWindowEventHandler.cs`），事件处理器基于Revit的外部事件
3. 添加命令类（例如`CreateWindowCommand.cs`），解析来自mcp服务器的信息，调用处理器实现
4. 在`SocketService`的`RegisterCommands`方法中注册新命令

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
