# <div align="center"><img src="https://aspdotnetcore.net/assets/images/adnc-github.png" alt="ADNC-支持模块化单体平滑演进为分布式微服务的 .NET 8 开源框架" style="zoom:50%;" /></div>
<div align='center'>
<a href="./LICENSE">
<img alt="GitHub license" src="https://img.shields.io/github/license/AlphaYu/Adnc"/>
</a>
<a href="https://github.com/AlphaYu/Adnc/stargazers">
<img alt="GitHub stars" src="https://img.shields.io/github/stars/AlphaYu/Adnc"/>
</a>
<a href="https://github.com/AlphaYu/Adnc/network">
<img alt="GitHub forks" src="https://img.shields.io/github/forks/AlphaYu/Adnc"/>
</a>
<img alt="Visitors" src="https://komarev.com/ghpvc/?username=alphayu&color=red&label=Visitors"/>
</div>
<p align="center">
  一套可落地的 .NET 8 框架，支持模块化单体平滑演进为分布式微服务。
</p>
<p align="center">
  <a href="./README.md">English</a> · 简体中文
</p>



## 简介

### ADNC 是什么？

`ADNC` 是一个基于 `.NET 8` 的开源框架，适合构建边界清晰的模块化业务系统，并支持从模块化单体逐步演进为分布式微服务。它围绕网关路由、服务注册与发现、配置中心、认证授权、服务间通信、事件驱动集成、持久化、缓存、可观测性、弹性治理和部署支持等常见能力，提供了一套可直接落地的基础设施与工程实践。

这个仓库不仅包含框架代码，也提供了一套可以直接运行的示例系统。它包含可复用的 `Adnc.Infra.*` 基础设施包、`Adnc.Shared.*` 服务通用包、Ocelot 网关、多服务 Demo、数据库脚本、Docker Compose 部署资产，以及中英文配套文档。

ADNC 并不是一套“一刀切”的模板。它展示了不同服务形态如何在同一套基础设施和约定下共存：经典分层服务、紧凑型单项目服务，以及带明确领域层的 DDD 风格服务。

### 为什么选择 ADNC？

**分布式系统最容易失败的地方，往往不是控制器和仓储，而是边界。**
服务归属、数据一致性、不同环境之间的配置不一致、认证授权、调用链路和服务之间的接口约定，才是真正消耗工程成本的地方。ADNC 关注这些边界问题，而不是只生成简单的 CRUD 代码。

ADNC 建立在三个核心支柱之上：

- **模块化单体优先**：从第一天开始设计清晰边界，避免系统过早退化为“大泥球”。
- **战略性演进**：只有当规模、团队或运维收益证明值得时，再拆分为独立服务。分布式系统是一种成本，不是功能本身。
- **企业级基础设施**：围绕 .NET 8 提供生产级基础设施积木，让业务代码专注于业务行为，而不是重复处理技术管道。

#### 当你需要这些能力时，可以选择 ADNC：

- **战略性基线**：一套在简单性和可扩展性之间取得平衡的 .NET 8 框架。
- **平滑演进路径**：帮助模块化单体逐步演进为分布式微服务，避免大规模重写。
- **架构纪律**：在缓存、消息、认证、仓储、远程调用等横切能力上保持一致抽象。
- **贴近生产的示例**：Demo 覆盖网关、身份认证、CAP 事件总线、SkyWalking 链路追踪、缓存、日志和部署资产。

#### ADNC 不是：

- 隐藏 .NET 应用架构细节的黑盒平台。
- 没有架构意图的代码生成器。
- 强制每个限界上下文从第一天开始独立部署的纯微服务模板。
- 性能基准报告。性能说明只是特定场景下的参考，不代表通用承诺。

## 设计原则

**模块化优先，必要时再分布式化**

在系统还没有被拆成多个独立服务之前，就应该先把业务模块和职责边界划清楚。ADNC 支持先建立清晰的模块边界，再根据实际需要逐步引入分布式部署。

**不同领域需要不同形态**

简单 CRUD 服务不应该被迫承担领域复杂服务的结构成本。ADNC 通过多种 Demo 结构帮助团队做出更合适的取舍。

**基础设施共享，业务逻辑归服务所有**

认证、缓存、仓储、事件、日志、健康检查等横切能力集中在可复用构建块中；业务逻辑保留在对应服务的 Application 和 Domain 层中。

**运维能力是一等架构关注点**

配置、注册发现、链路追踪、日志、网关路由、健康检查和部署资产都被视为架构的一部分，而不是事后补上的运维脚本。

**优先集成成熟组件，而不是重复造轮子**

ADNC 将 Ocelot、Consul、Refit、gRPC、EF Core、Dapper、CAP、RabbitMQ、Redis、Polly、NLog、SkyAPM 和 HealthChecks 等 .NET 生态组件组织在一致的工程约定之下。

## 快速开始

建议按下面顺序开始：

1. 先阅读 [快速开始文档](https://docs.aspdotnetcore.net/wiki/zh-cn/02-quickstart-zh)
2. 使用 `src/Adnc.sln` 或 `src/Demo/Adnc.Demo.sln` 打开解决方案
3. 如需前端项目，请查看文末前端链接
4. 如需初始化数据，请查看文末数据库脚本链接

运行 Demo 前请先准备 `.NET 8 SDK`，以及快速开始文档中提到的基础依赖。更完整的接入与本地运行说明请直接查看快速开始文档。

## 目录 / 架构

### 目录结构

```
adnc 
├── .github/                   GitHub Actions 工作流
├── database/                  数据库初始化脚本
├── deploy/                    Docker Compose 部署资产
├── docs/
│   ├── architecture/          架构决策记录
│   └── wiki/                  中英文文档源文件
├── src/
│   ├── Infrastructures/       ADNC 基础设施包
│   ├── ServiceShared/         服务通用层构建块
│   ├── Gateways/              Ocelot 网关
│   └── Demo/                  Demo 微服务
├── test/                      测试相关项目
├── README.md
├── README_ZH.md
└── LICENSE
```
### 重要文件
| 路径                                 | 描述                               |
| -------------------------------------| ---------------------------------- |
| `src/Adnc.sln`                      | 该解决方案包含`adnc`所有工程       |
| `src/Infrastructures/Adnc.Infra.sln`| 该解决方案仅包含基础架构层相关工程 |
| `src/ServiceShared/Adnc.Shared.sln` | 该解决方案仅包含服务通用层相关工程 |
| `src/Demo/Adnc.Demo.sln`            | 该解决方案仅包含`demo`相关工程     |
| `src/.editorconfig`            | 用于统一代码风格的跨编辑器配置文件，确保团队中无论谁用 VS、VS Code 还是 JetBrains Rider，写出的代码格式都是一致的     |
| `src/Directory.Build.props`             | 用于管理通用构建属性（如目标框架、语言版本、输出路径等）  |
| `src/Directory.Packages.props`          | 用于中央包管理 (CPM)，统一管理整个解决方案中 NuGet 包的版本号  |

### 总体架构图

<img src="https://aspdotnetcore.net/assets/images/adnc_framework-e1682145003197.png" alt="adnc_framework"/>

#### Adnc.Infra.*

[NuGet Gallery | Packages matching adnc.infra](https://www.nuget.org/packages?q=adnc.infra)

![adnc-framework-2](https://aspdotnetcore.net/assets/images/adnc-framework-2.png)

#### Adnc.Shared.*

[NuGet Gallery | Packages matching adnc.shared](https://www.nuget.org/packages?q=adnc.shared)

<img src="https://aspdotnetcore.net/assets/images/adnc-framework-3.png" alt="adnc-framework-3" style="zoom:80%;" />

### 技术栈

| 名称                                                         | 描述                                                         |
| ------------------------------------------------------------ | ------------------------------------------------------------ |
| <a target="_blank" href="https://github.com/ThreeMammals/Ocelot">Ocelot</a> | 基于 .NET 编写的开源网关                                     |
| <a target="_blank" href="https://github.com/hashicorp/consul">Consul</a> | 配置中心、注册中心组件                                       |
| <a target="_blank" href="https://github.com/reactiveui/refit">Refit</a> | 声明式、类型安全的 RESTful 服务调用组件                      |
| <a target="_blank" href="https://github.com/grpc/grpc-dotnet">Grpc.Net.ClientFactory</a><br />Grpc.Tools | gRPC 通讯框架                                                |
| <a target="_blank" href="https://github.com/SkyAPM/SkyAPM-dotnet">SkyAPM.Agent.AspNetCore</a> | SkyWalking .NET 探针，链路追踪与性能监测组件                 |
| <a target="_blank" href="https://github.com/castleproject/Core">Castle DynamicProxy</a> | 动态代理，AOP开源实现组件                                    |
| <a target="_blank" href="https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql">Pomelo.EntityFrameworkCore.MySql</a> | EF Core ORM 组件                                             |
| <a target="_blank" href="https://github.com/StackExchange/Dapper">Dapper</a> | 轻量级 ORM 组件                                              |
| <a target="_blank" href="https://github.com/NLog/NLog">NLog</a><br />NLog.MongoDB<br />NLog.Loki | 日志记录组件                                                 |
| <a target="_blank" href="https://github.com/AutoMapper/AutoMapper">AutoMapper</a> | 模型映射组件                                                 |
| <a target="_blank" href="https://github.com/domaindrivendev/Swashbuckle.AspNetCore">Swashbuckle.AspNetCore</a> | API 文档生成工具（Swagger）                                  |
| <a target="_blank" href="https://github.com/StackExchange/StackExchange.Redis">StackExchange.Redis</a> | Redis 客户端 SDK                                             |
| <a target="_blank" href="https://github.com/dotnetcore/CAP">CAP</a> | 事件总线与最终一致性（分布式事务）组件                       |
| <a target="_blank" href="https://github.com/rabbitmq/rabbitmq-dotnet-client">RabbitMQ</a> | 异步消息队列组件                                             |
| <a target="_blank" href="https://github.com/App-vNext/Polly">Polly</a> | .NET 弹性与瞬态故障处理库                                    |
| <a target="_blank" href="https://github.com/FluentValidation">FluentValidation</a> | .NET 验证框架                                                |
| <a target="_blank" href="https://github.com/mariadb-corporation/MaxScale">MaxScale</a> | MariaDB 开发的一款成熟、高性能、免费开源的数据库中间件       |
| <a target="_blank" href="https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks">AspNetCore.HealthChecks</a> | 健康检查组件，可与 Consul 健康检查配合使用                   |

## Demo 服务概览

Demo 提供了五个相互关联的微服务示例，分别展示了不同的服务拆分方式与工程组织形式。

| 服务  | 描述                                           | 架构风格                    |
| ----- | ---------------------------------------------- | --------------------------- |
| Admin | 系统管理（组织、用户、角色、权限、字典、配置） | 经典三层分离合约            |
| Maint | 运维管理（日志、审计）                         | 经典三层合并合约            |
| Cust  | 客户管理                                       | 单项目最小结构              |
| Ord   | 订单管理                                       | 领域驱动设计（DDD）带领域层 |
| Whse  | 仓库管理                                       | 领域驱动设计（DDD）带领域层 |

这些 Demo 展示了在保持整体框架一致性的前提下，如何按不同业务规模和复杂度组织代码。

##### :white_check_mark: Shared 

> Demo 公用工程，所有演示服务都会复用 `Shared` 目录中的通用组件。

```
Shared/
├── Remote.Event/ - 用于跨服务通信事件定义
├── Remote.Grpc/ - gRPC客户端定义
├── Remote.Http/ - HTTP客户端定义
├── protos/ - gRPC的协议文件定义
└── resources/ - 共享配置和资源
```

##### :white_check_mark: Adnc.Demo.Admin

> Admin 是系统管理服务，采用经典三层结构，并将应用服务契约定义拆分到 `Adnc.Demo.Admin.Application.Contracts` 层。这种组织方式层次清晰，适合边界明确、模块较多的后台管理场景。

```
Admin/
├── Api/ - 控制器和API端点
├── Application/ - 业务逻辑实现
├── Application.Contracts/ - DTO和服务接口
└── Repository/ - 数据访问层
```

##### :white_check_mark: Adnc.Demo.Maint

> Maint 是运维中心服务，采用更紧凑的三层结构，应用服务实现与契约定义都位于 `Adnc.Demo.Maint.Application` 层。这种结构减少了项目数量，同时保留了清晰的职责边界。

```
Maint/
├── Api/ - 控制器和端点
├── Application/ - 包含合约和实现
└── Repository/ - 数据访问层
```

##### :white_check_mark: Adnc.Demo.Cust

> Cust 是客户中心服务，采用单项目结构，控制器、应用服务、契约定义和仓储都位于同一个工程中。这种方式更适合职责单一、边界清晰的小型服务。

```
Cust/
└── Api/ - 包含控制器、应用逻辑和存储库
```

##### :white_check_mark: Adnc.Demo.Ord

> Ord 是订单中心服务，采用带独立领域层的 DDD 结构，用于突出业务规则与领域模型，并将其与应用层职责分离。

```
Ord/
├── Api/ - API端点
├── Application/ - 应用服务
├── Domain/ - 领域实体、聚合和领域服务
└── Migrations/ - 数据库迁移
```

##### :white_check_mark: Adnc.Demo.Whse

> Whse 是仓储中心服务，整体结构与 Ord 一致，同样采用带独立领域层的 DDD 组织方式。

```
Whse/
├── Api/ - API端点
├── Application/ - 应用服务
├── Domain/ - 领域实体、聚合和领域服务
└── Migrations/ - 数据库迁移
```

## 文档链接

| **序号** | 标题                                                         |
| -------- | ------------------------------------------------------------ |
| 1        | [ADNC 项目导览：一套可落地的 .NET 8 微服务/分布式工程实践](https://docs.aspdotnetcore.net/wiki/zh-cn/01-adnc-intro-zh) |
| 2        | [ADNC 快速上手指南](https://docs.aspdotnetcore.net/wiki/zh-cn/02-quickstart-zh) |
| 3        | [ADNC 快速 Docker 部署指南](https://docs.aspdotnetcore.net/wiki/zh-cn/03-quickly-docker-deploy-zh) |
| 4        | [ADNC 配置节点详细说明](https://docs.aspdotnetcore.net/wiki/zh-cn/04-appsettings-zh) |
| 5        | [ADNC 完整开发流程](https://docs.aspdotnetcore.net/wiki/zh-cn/05-feature-dev-guide-zh) |
| 6        | [ADNC Repository 层开发指引](https://docs.aspdotnetcore.net/wiki/zh-cn/06-repository-dev-guide-zh) |
| 7        | [ADNC Service层开发指引](https://docs.aspdotnetcore.net/wiki/zh-cn/07-service-dev-guide) |
| 8        | [ADNC API 层开发指引](https://docs.aspdotnetcore.net/wiki/zh-cn/08-api-dev-guide-zh) |
| 9        | [ADNC 如何认证与授权](https://docs.aspdotnetcore.net/wiki/zh-cn/09-claims-based-authentication-zh) |
| 10       | [ADNC 如何使用仓储 - 基础功能](https://docs.aspdotnetcore.net/wiki/zh-cn/10-efcore-pemelo-curd-zh) |
| 11       | [ADNC 如何使用仓储 - CodeFirst](https://docs.aspdotnetcore.net/wiki/zh-cn/11-efcore-pemelo-codefirst-zh) |
| 12       | [ADNC 如何使用仓储 - 切换数据库类型](https://docs.aspdotnetcore.net/wiki/zh-cn/12-efcore-pemelo-sqlserver-zh) |
| 13       | [ADNC 如何使用仓储 - 事务](https://docs.aspdotnetcore.net/wiki/zh-cn/13-efcore-pemolo-unitofwork-zh) |
| 14       | [ADNC  如何使用仓储 - 执行原生SQL](https://docs.aspdotnetcore.net/wiki/zh-cn/14-efcore-pemelo-sql-zh) |
| 15       | [ADNC 如何使用仓储 - 读写分离](https://docs.aspdotnetcore.net/wiki/zh-cn/15-maxsale-readwritesplit-zh) |
| 16       | [ADNC Id生成器(雪花算法)介绍](https://docs.aspdotnetcore.net/wiki/zh-cn/16-snowflake-max_value-wokerid-zh) |
| 17       | [ADNC 如何使用Cache/Redis/分布式锁/布隆过滤器](https://docs.aspdotnetcore.net/wiki/zh-cn/17-cache-redis-distributedlock-bloomfilter-zh) |
| 18       | [ADNC 服务之间如何通过 HTTP 调用（Refit）](https://docs.aspdotnetcore.net/wiki/zh-cn/18-service-http-call-zh) |
| 19       | [ADNC 服务之间如何通过 gRPC 调用](https://docs.aspdotnetcore.net/wiki/zh-cn/19-service-grpc-call-zh) |
| 20       | [ADNC 服务之间如何通过事件（CAP）通信](https://docs.aspdotnetcore.net/wiki/zh-cn/20-service-event-call-zh) |
| 21       | [ADNC 如何开启 SkyAPM（SkyWalking）链路追踪](https://docs.aspdotnetcore.net/wiki/zh-cn/21-skyapm-tracing-zh) |
| 22       | [ADNC 如何使用配置中心（Consul）](https://docs.aspdotnetcore.net/wiki/zh-cn/22-config-center-zh) |
| 23       | [ADNC 如何使用注册中心](https://docs.aspdotnetcore.net/wiki/zh-cn/23-registry-center-zh) |

## 截图 / JMeter / 官网

### JMeter测试

> 6个测试用例覆盖了网关、服务发现、配置中心、服务间同步调用、数据库 CRUD、本地事务、分布式事务、缓存、布隆过滤器、SkyAPM 链路、NLog 日志记录、操作日志记录。

- ECS服务器配置：4核8G，带宽8M。服务器上装了很多东西，剩余大约50%的CPU资源，50%的内存资源。
- 因为服务器带宽限制，吞吐率约1000/s左右。
- 模拟并发线程1200/s
- 读写比率7:3

### 前端

基于 Vue 3、Vite、TypeScript 和 Element Plus 的开箱即用后台管理前端模板。

#### 项目地址

- [adnc-vue3: ADNC's Vue3 front-end](https://github.com/alphayu/adnc-vue-elementplus)

#### 界面截图

![.NET微服务开源框架-异常日志界面](https://aspdotnetcore.net/assets/images/adnc-dashboard-nlog.png)
![.NET微服务开源框架-角色管理界面](https://aspdotnetcore.net/assets/images/adnc-dashboard-role.png)

### 相关链接

#### 项目官网

- [https://aspdotnetcore.net](https://aspdotnetcore.net)

#### 文档网址

- [https://docs.aspdotnetcore.net](https://docs.aspdotnetcore.net)

#### 演示网址

- [https://online.aspdotnetcore.net](https://online.aspdotnetcore.net)

#### 代码生成器

- [https://code.aspdotnetcore.net](https://code.aspdotnetcore.net)

#### 数据库脚本

- [database/mysql/adnc.sql](./database/mysql/adnc.sql)

### 问题交流

- QQ群号：780634162

- 都看到这里了，那就点个`star`吧！

## License

本项目基于 **MIT License** 开源，详见 [LICENSE](./LICENSE)。
