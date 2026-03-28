---
_layout: landing
---

# Feedpipe

A production-ready data pipeline that fetches, parses, transforms, and serves content from multiple sources. Built with .NET 10.

## Projects

| Project | Description |
|---------|-------------|
| Feedpipe.Core | Shared models and interfaces |
| Feedpipe | Console pipeline runner |
| Feedpipe.Worker | Background service (timer-based) |
| Feedpipe.Api | REST API (ASP.NET minimal APIs) |
| Feedpipe.Cli | Command-line tool |

## Quick Start

```bash
dotnet restore
dotnet run --project src/Feedpipe
dotnet test
```

## Documentation

- [Roadmap](../.orchestra/roadmap.md) -- project vision, milestones, and status
- [Foundation Milestone](../.orchestra/work/foundation/prd.md) -- project structure and conventions (Done)
- [Multi-Source Ingestion](../.orchestra/work/multi-source-ingestion/prd.md) -- pluggable source adapters
- [Data Transformation](../.orchestra/work/data-transformation/prd.md) -- enrichment and dedup
- [Production Hardening](../.orchestra/work/production-hardening/prd.md) -- resilience, observability, deployment
- [ADR-000: The Score](../.orchestra/adr/ADR-000-the-score.md) -- founding architecture decision
- [Project Kickoff Devlog](../.orchestra/devlog/2026-Q1/2026-03-28-project-kickoff.md) -- development journal
