# ADNC 文档

ADNC 是一套基于 .NET 8 的应用框架与示例系统，用于构建分布式、服务化的业务应用。

本文档站使用 Docsify 构建，并按主题组织。新用户建议先阅读快速上手与 Docker 部署文档；已有项目经验的读者可以直接进入开发、数据访问、分布式能力或运维相关章节。

## 入门

- [项目导览](wiki/zh-cn/01-adnc-intro-zh)
- [快速上手](wiki/zh-cn/02-quickstart-zh)
- [快速 Docker 部署](wiki/zh-cn/03-quickly-docker-deploy-zh)

## 开发指南

- [配置节点说明](wiki/zh-cn/04-appsettings-zh)
- [完整开发流程](wiki/zh-cn/05-feature-dev-guide-zh)
- [Repository 层开发指引](wiki/zh-cn/06-repository-dev-guide-zh)
- [Service 层开发指引](wiki/zh-cn/07-service-dev-guide)
- [API 层开发指引](wiki/zh-cn/08-api-dev-guide-zh)
- [认证与授权](wiki/zh-cn/09-claims-based-authentication-zh)

## 数据访问

- [仓储基础功能](wiki/zh-cn/10-efcore-pemelo-curd-zh)
- [Code First](wiki/zh-cn/11-efcore-pemelo-codefirst-zh)
- [切换数据库类型](wiki/zh-cn/12-efcore-pemelo-sqlserver-zh)
- [事务与 Unit of Work](wiki/zh-cn/13-efcore-pemolo-unitofwork-zh)
- [执行原生 SQL](wiki/zh-cn/14-efcore-pemelo-sql-zh)
- [读写分离](wiki/zh-cn/15-maxsale-readwritesplit-zh)

## 分布式能力

- [雪花 ID 生成器](wiki/zh-cn/16-snowflake-max_value-wokerid-zh)
- [Cache、Redis、分布式锁与布隆过滤器](wiki/zh-cn/17-cache-redis-distributedlock-bloomfilter-zh)
- [HTTP 服务调用](wiki/zh-cn/18-service-http-call-zh)
- [gRPC 服务调用](wiki/zh-cn/19-service-grpc-call-zh)
- [事件服务调用](wiki/zh-cn/20-service-event-call-zh)

## 运维

- [SkyAPM 链路追踪](wiki/zh-cn/21-skyapm-tracing-zh)
- [Consul 配置中心](wiki/zh-cn/22-config-center-zh)
- [Consul 注册中心](wiki/zh-cn/23-registry-center-zh)

## 架构

- [整体架构图](architecture/adnc-overall-architecture.svg)
- [ADR-001: 优先采用模块化单体架构](architecture/adr/ADR-001-modular-monolith-first)

## Language

- [English Documentation](README)
