---
_layout: landing
---

# Feedpipe API Reference

A production-ready data pipeline that fetches, parses, transforms, and serves content from multiple sources. Built with .NET 10.

## Namespaces

| Namespace | Description |
|-----------|-------------|
| Feedpipe.Core.Models | Shared data models (FeedItem) |
| Feedpipe.Core.Services | Service contracts (IFeedFetcher, IFeedWriter) |
| Feedpipe.Services | Implementations (RssFeedFetcher, JsonFeedWriter) |
| Feedpipe.Models | Application configuration (AppSettings, FeedSettings) |
| Feedpipe.Worker | Background service for scheduled pipeline runs |

## Quick Start

```bash
dotnet restore
dotnet run --project src/Feedpipe
dotnet test
```
