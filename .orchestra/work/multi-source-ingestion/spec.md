# Multi-Source Ingestion -- Execution Spec

**PRD:** [prd.md](prd.md)
**Gherkin:** [gherkin.md](gherkin.md)
**Status:** Complete

## Approach

Four tracks executed in order, each building on the previous.

**Track 1: Generalize the pipeline record type.** Introduced `IPipelineRecord` base interface so the pipeline isn't locked to `FeedItem`. The output writer and pipeline loop operate on `IPipelineRecord`; domain-specific types remain in their adapter projects.

**Track 2: Adapter routing.** Replaced hardcoded `RssSourceAdapter` in DI with keyed services. Each adapter is registered by key and resolved at runtime from `SourceSettings.Type`.

**Track 3: EDI 834 adapter.** New source project with its own domain model (`EnrollmentRecord`), X12 segment parser, and test fixtures.

**Track 4: Atom + Zotero.** Added Atom auto-detection to the feed adapter and built a Zotero CSV adapter with arxiv API enrichment and access level detection.

## Steps

1. [x] Create `IPipelineRecord` interface in Conduit.Core
2. [x] Update `ISourceAdapter` to return `List<IPipelineRecord>`
3. [x] Update `IOutputWriter` to accept `List<IPipelineRecord>`
4. [x] Update `JsonOutputWriter` to serialize any `IPipelineRecord` implementation
5. [x] Update pipeline loop in Console, Worker, and Api
6. [x] Implement adapter routing via keyed DI services
7. [x] Update existing tests -- all original tests still pass
8. [x] Create `Conduit.Sources.Edi834` project
9. [x] Define `EnrollmentRecord` model implementing `IPipelineRecord`
10. [x] Implement `Edi834SourceAdapter` with X12 segment parser
11. [x] Create sample 834 test fixture
12. [x] Create `Conduit.Sources.Edi834.Tests` (12 tests)
13. [x] Add 834 source to appsettings.json
14. [x] Run full pipeline with multiple sources, verify output in data/{type}/
15. [x] Add concurrent processing via Task.WhenAll + SemaphoreSlim
16. [x] Add Atom feed support with auto-detection in FeedSourceAdapter
17. [x] Create `Conduit.Sources.Zotero` with ZoteroSourceAdapter
18. [x] Create `ResearchRecord` model with AccessLevel enum
19. [x] Implement arxiv API enrichment for Zotero entries
20. [x] Create `Conduit.Sources.Zotero.Tests` (22 tests)
21. [x] Integrate real Zotero CSV export (trimmed to 20 entries)
22. [x] Reorganize src/ into Core/, Adapters/, App/ folders

## Deliverables

| Deliverable | Location | Status |
|-------------|----------|--------|
| IPipelineRecord interface | src/Core/Conduit.Core/Models/ | Done |
| Adapter routing (keyed DI) | src/App/Conduit/Program.cs (and Worker, Api) | Done |
| Edi834SourceAdapter | src/Adapters/Conduit.Sources.Edi834/ | Done |
| EnrollmentRecord | src/Adapters/Conduit.Sources.Edi834/Models/ | Done |
| FeedSourceAdapter (RSS + Atom) | src/Adapters/Conduit.Sources.Rss/ | Done |
| ZoteroSourceAdapter | src/Adapters/Conduit.Sources.Zotero/ | Done |
| ResearchRecord + AccessLevel | src/Adapters/Conduit.Sources.Zotero/Models/ | Done |
| Concurrent processing | src/App/Conduit/Program.cs | Done |
| Sample fixtures | samples/edi834/, samples/zotero/ | Done |
| Sample output | data/rss/, data/edi834/, data/zotero/ | Done |
| 52 tests across 4 projects | tests/ | Done |

## Risks (resolved)

- **X12 parsing complexity** -- built a minimal parser for common scenarios. Production would use a library.
- **IPipelineRecord generalization** -- all original tests continued passing after generalization.
- **JSON serialization of polymorphic types** -- solved by casting to object before serializing, preserving domain-specific properties.
