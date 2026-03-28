---
_layout: landing
---

# Feedpipe

A data pipeline that fetches, parses, and stores content from RSS feeds. Built with .NET 10.

## Projects

| Project | Description |
|---------|-------------|
| [Feedpipe.Core](api/Feedpipe.Core.Models.html) | Shared models and interfaces |
| [Feedpipe](api/Feedpipe.Services.html) | Console pipeline runner |
| [Feedpipe.Worker](api/Feedpipe.Worker.html) | Background service (timer-based) |
| [Feedpipe.Api](api/Feedpipe.Api.html) | REST API (ASP.NET minimal APIs) |
| [Feedpipe.Cli](api/Feedpipe.Cli.html) | Command-line tool |

## Quick Start

```bash
dotnet restore
dotnet run --project src/Feedpipe
dotnet test
```

## API Reference

Browse the full [API documentation](api/index.md).
