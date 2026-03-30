---
layout: default
title: Home
---

# Conduit

A domain-agnostic data pipeline for multi-source content processing. Built with .NET 10.

## Roadmap

- [Project Roadmap](roadmap) -- vision, milestones, and status

## Milestones

- [Foundation](milestones/foundation) -- project structure, DI, testing, CI (Complete)
- [Multi-Source Ingestion](milestones/multi-source-ingestion) -- pluggable adapters, 834, Zotero (Complete)
- [Data Transformation](milestones/data-transformation) -- dedup, enrichment, storage (Not Started)

## Decisions

- [ADR-000: The Score](decisions/adr-000-the-score) -- PRDs all the way down
- [ADR-001: Domain-Agnostic Pipeline](decisions/adr-001-domain-agnostic-pipeline) -- why one codebase, not many
- [ADR-002: Production Readiness is Continuous](decisions/adr-002-production-readiness-is-continuous) -- no hardening phase
- [ADR-003: No External Documentation Site](decisions/adr-003-no-docs-site) -- docs live in the repo (ironic, yes)

## Devlog

- [Project Kickoff](devlog/2026-03-28-project-kickoff)
- [Pivot to Conduit](devlog/2026-03-29-pivot-to-conduit)
- [Source Adapter Isolation](devlog/2026-03-29-source-isolation)
- [Multi-Source Ingestion](devlog/2026-03-29-multi-source-ingestion)
- [Zotero Adapter](devlog/2026-03-29-zotero-adapter)
- [Dropping the Docs Site](devlog/2026-03-29-dropping-docs-site)

## Learning Notes

- [.NET Fundamentals](learning/dotnet-fundamentals) -- CLI, csproj, records, async, nullable
- [DI and SOLID](learning/di-and-solid) -- container, keyed services, five principles
- [Testing Patterns](learning/testing-patterns) -- xUnit, Moq, TDD, coverage
- [Project Architecture](learning/project-architecture) -- solution layout, adapter pattern
- [Code Coverage](learning/code-coverage) -- measuring, reporting, targets
