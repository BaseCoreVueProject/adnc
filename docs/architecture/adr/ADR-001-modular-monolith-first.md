# ADR-001: Prefer Modular Monolith as the Initial Architectural Style

## Status

Accepted

---

## Context

Microservices are frequently adopted as a default architectural choice for modern distributed systems due to their scalability and team autonomy benefits.

However, a microservice architecture introduces substantial complexity, including but not limited to:

- Distributed transaction and consistency challenges
- Network latency and partial failure handling
- Service discovery and configuration management
- Cross-service authentication and authorization
- Distributed tracing and observability requirements
- CI/CD pipeline and deployment orchestration overhead
- Increased infrastructure and operational cost

For many enterprise applications, these concerns outweigh the benefits during early and intermediate stages of system growth.

ADNC aims to provide a pragmatic architectural foundation for building maintainable, scalable enterprise systems while avoiding premature distribution.

---

## Decision

ADNC adopts **Modular Monolith** as the preferred initial architectural style.

Applications built with ADNC are expected to:

- Deploy as a single process/unit initially
- Organize business capabilities into domain-oriented modules
- Enforce explicit boundaries between modules
- Communicate across modules via contracts and domain/integration events
- Prevent direct coupling across unrelated domains

### ADNC Implementation

To enforce modular boundaries, ADNC provides the following structural constraints:

- Dedicated module startup and registration contracts
- Explicit inter-module dependency declarations
- Internal application service boundaries per module
- Event-driven inter-module communication patterns

---

## Rationale

### Reduced Operational Complexity

A modular monolith avoids the distributed infrastructure burden associated with microservices, including service discovery, network resiliency, and cross-service observability.

### Transactional Simplicity

Business workflows can leverage local ACID transactions without introducing distributed transaction coordination or eventual consistency concerns prematurely.

### Improved Developer Productivity

Single-process deployment simplifies:

- Local development and debugging
- Integration testing
- Deployment workflows
- Operational troubleshooting

### Evolutionary Architecture

Architectural distribution should be driven by demonstrated scaling or organizational needs rather than assumed future requirements.

A well-structured modular monolith preserves the option to extract modules into independent services when justified.

---

## Non-Goals

ADNC does not attempt to eliminate the need for microservices.

Rather, it encourages teams to defer distribution until clear operational,
organizational, or scalability requirements justify the added complexity.

## Consequences

### Positive

- Lower infrastructure and operational overhead
- Faster development and iteration cycles
- Simpler debugging and testing workflows
- Stronger consistency guarantees within process boundaries
- Clearer path for future service decomposition

### Negative

- Independent deployment per module is not available initially
- Runtime scaling remains process-level rather than module-level
- Architectural discipline is required to preserve module boundaries
- Poor governance may degrade the system into a tightly coupled monolith

---

## Alternatives Considered

### Microservices from Day One

Rejected.

While microservices provide deployment and scaling independence, they introduce significant operational and architectural complexity that is unjustified for many systems at inception.

### Traditional Layered Monolith

Rejected.

Traditional layered monoliths often lack explicit domain boundaries, leading to tight coupling and poor evolvability.

---

## Migration Considerations

If future requirements justify service decomposition, modules may be extracted incrementally into standalone services.

The modular boundaries, contracts, and event-based communication patterns established by ADNC are designed to provide natural seams for such evolution.

---

## References

- Martin Fowler — Monolith First
- Sam Newman — Building Microservices
- Eric Evans — Domain-Driven Design