# ADR-002: Production Readiness is Continuous, Not a Milestone

**Date:** 2026-03-29
**Status:** Active
**Decision:** Resilience, observability, security, and deployment readiness are standing requirements across all milestones, not a separate phase.

## Context

The original roadmap had "Production Hardening" as Milestone 4 -- a final phase where retry policies, health checks, monitoring, and containerization would be added after all features were built.

This is a waterfall pattern. It assumes you can build features first and make them production-grade later. In practice, this leads to:
- Features shipped without error handling because "we'll add it in hardening"
- Observability bolted on after the fact, missing context that should have been there from the start
- A growing backlog of "hardening" work that never feels urgent until something breaks

A dev spike audit of the current codebase identified 25 production readiness gaps, ranging from critical (race condition in file writes, no API exception handling) to nice-to-have (metrics, rate limiting). These gaps exist because production concerns were deferred to a future milestone.

## Decision

Production readiness is a standing requirement, not a milestone. Every milestone must ship with:

1. **Error handling** -- failures are recovered gracefully, logged, and don't crash the pipeline
2. **Input validation** -- external inputs are validated at system boundaries
3. **Timeouts** -- external calls have configured timeouts
4. **Health checks** -- runnable services expose health endpoints
5. **Configuration** -- settings are overridable via environment variables
6. **Testability** -- new code ships with tests, coverage doesn't decline

The following are addressed as needed rather than in a dedicated phase:

- **Retry policies** -- added when an adapter calls an external service
- **Correlation IDs** -- added when distributed tracing becomes necessary
- **Containerization** -- added when deployment to a container host is needed
- **Metrics/observability** -- added when the pipeline runs long enough to need monitoring
- **Rate limiting** -- added when the API is exposed beyond localhost

## Current Gaps (from dev spike)

The audit identified these as the highest priority fixes to fold into the next milestone:

**Critical:**
- Race condition in JsonOutputWriter (concurrent writes can lose data)
- No global exception handler in API
- No HttpClient timeout configuration
- API reads files synchronously (thread pool starvation under load)
- API deserializes all records as FeedItem (breaks for 834/Zotero)

**Important:**
- No retry policies on external HTTP calls
- No health check endpoints
- Configuration not environment-variable driven
- Worker schedule hardcoded to 5 minutes

These will be addressed as part of the Data Transformation milestone or as standalone fixes, not as a separate "hardening" phase.

## Consequences

- The "Production Hardening" milestone is removed from the roadmap
- Each milestone's PRD must include production readiness in its success criteria
- The dev spike gaps become a backlog of fixes to address incrementally
- Code review should catch missing error handling, validation, and timeouts before merge
