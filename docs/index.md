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

- **API Reference** -- generated from XML doc comments (see API Reference tab)
- **Roadmap** -- project vision, milestones, and status (see Project tab)
- **Decisions** -- architecture decision records
- **Devlog** -- development journal
