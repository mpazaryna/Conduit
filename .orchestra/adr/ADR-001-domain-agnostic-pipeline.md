# ADR-001: Domain-Agnostic Pipeline Architecture

**Date:** 2026-03-29
**Status:** Active
**Decision:** Conduit is a domain-agnostic data pipeline. Source types and domain models are pluggable; the pipeline infrastructure is shared.

## Context

The project started as "hello-dot-net" -- a .NET learning exercise that fetched RSS feeds. Over the course of a single working session it evolved through several identity shifts:

1. **HelloDotNet** -- a tutorial console app
2. **Feedpipe** -- an RSS-specific pipeline with multi-project structure
3. **Conduit** -- a domain-agnostic pipeline lab

The trigger for the final rename was learning that the upcoming work on Jon's team involves EDI 834 healthcare enrollment data. The question became: should we build a separate 834-only project, or extend the existing pipeline? The answer was to extend -- because the pipeline pattern (ingest -> transform -> store) is identical regardless of whether the data is RSS articles, healthcare enrollments, log files, or PDFs.

Keeping two separate projects would mean rebuilding DI, logging, testing, CI/CD, and project structure from scratch. Keeping one project forces the abstractions to be genuinely pluggable rather than theoretically pluggable.

## Decision

### The separation

```
Shared (Conduit.Core)          Domain-specific (adapters)
├── ISourceAdapter             ├── RssFeedAdapter
├── IPipelineRecord            ├── AtomFeedAdapter
├── ITransformStage            ├── Edi834Adapter
├── IStorageBackend            ├── FeedItem (content domain)
└── Pipeline infrastructure    └── EnrollmentRecord (healthcare domain)
```

1. **Pipeline infrastructure is domain-agnostic.** The DI container, logging, configuration, worker, API, CLI, and pipeline orchestration know nothing about RSS, Atom, or 834. They operate on interfaces.

2. **Source adapters are domain-specific.** Each source type implements `ISourceAdapter` and produces its own domain model. RSS produces `FeedItem`. 834 produces `EnrollmentRecord`. The pipeline doesn't care which.

3. **Domain types do not contaminate each other.** `FeedItem` and `EnrollmentRecord` share a common interface (`IPipelineRecord`) but no fields. Healthcare field names never appear in content models and vice versa.

4. **The project name reflects this.** "Conduit" means data flows through it -- it doesn't specify what kind of data.

### What this enables

The same codebase serves as a working lab for:
- RSS/Atom content aggregation
- EDI 834 healthcare enrollment processing
- Any future source type (log data, credit card transactions, research PDFs, FHIR APIs)

Adding a new domain requires implementing an adapter and a domain model. It does not require changing the pipeline, the worker, the API, the CLI, or the tests for other domains.

## Rationale

- **One well-maintained project beats two half-finished ones.** The foundation (DI, CI, docs, testing) only needs to be built once.
- **Forced generalization produces better abstractions.** If the adapter interface only needs to handle RSS, it's easy to accidentally bake in RSS assumptions. Adding 834 forces the interface to be genuinely source-agnostic.
- **The lab grows with the developer.** New patterns learned on Jon's team can be brought back and implemented cleanly. Six months of .NET learning compounds in one place.

## Consequences

- Source adapter interfaces must be designed for the lowest common denominator -- they can't assume URLs, XML, or any specific transport.
- Each domain's test fixtures are isolated. RSS test data lives alongside RSS tests, 834 test data alongside 834 tests.
- The `Conduit.Core` library must remain dependency-free beyond the .NET base class library. Domain-specific NuGet packages belong in the adapter projects, not in Core.
- When evaluating new features, the question is always: "Does this belong in the pipeline infrastructure or in a domain adapter?"
