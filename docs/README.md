# ADNC Documentation

ADNC is a pragmatic .NET 8 reference architecture for modular monoliths, distributed applications, and microservice systems.

This Docsify site is organized by topic. Start with the quick start guides if you want to run the project locally, or jump directly to a specific development area.

## Getting Started

- [Project tour](wiki/en/01-adnc-intro)
- [Quick start](wiki/en/02-quickstart)
- [Quick Docker deployment](wiki/en/03-quickly-docker-deploy)

## Developer Guides

- [Configuration nodes](wiki/en/04-appsettings)
- [Development workflow](wiki/en/05-feature-dev-guide)
- [Repository layer guide](wiki/en/06-repository-dev-guide)
- [Service layer guide](wiki/en/07-service-dev-guide)
- [API layer guide](wiki/en/08-api-dev-guide)
- [Authentication and authorization](wiki/en/09-claims-based-authentication)

## Data Access

- [Repository basics](wiki/en/10-efcore-pemelo-curd)
- [Code First](wiki/en/11-efcore-pemelo-codefirst)
- [Switching database types](wiki/en/12-efcore-pemelo-sqlserver)
- [Transactions and unit of work](wiki/en/13-efcore-pemolo-unitofwork)
- [Raw SQL](wiki/en/14-efcore-pemelo-sql)
- [Read/write splitting](wiki/en/15-maxsale-readwritesplit)

## Distributed Capabilities

- [Snowflake ID generator](wiki/en/16-snowflake-max_value-wokerid)
- [Cache, Redis, distributed locks, and Bloom filters](wiki/en/17-cache-redis-distributedlock-bloomfilter)
- [HTTP service calls](wiki/en/18-service-http-call)
- [gRPC service calls](wiki/en/19-service-grpc-call)
- [Event-based service calls](wiki/en/20-service-event-call)

## Operations

- [SkyAPM tracing](wiki/en/21-skyapm-tracing)
- [Consul configuration center](wiki/en/22-config-center)
- [Consul service registry](wiki/en/23-registry-center)

## Architecture

- [Overall architecture diagram](architecture/adnc-overall-architecture.svg)
- [ADR-001: Prefer Modular Monolith as the Initial Architectural Style](architecture/adr/ADR-001-modular-monolith-first)

## Language

- [中文文档](README_ZH)
