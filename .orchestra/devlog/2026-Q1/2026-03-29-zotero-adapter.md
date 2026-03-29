# 2026-03-29: Zotero Source Adapter

## What Happened

Added a third source adapter for Zotero CSV exports. This demonstrates a hybrid ingestion pattern: read a local file, then selectively enrich from external APIs based on what's accessible.

## What Was Built

**ZoteroSourceAdapter** -- reads Zotero CSV exports and produces ResearchRecord instances. For entries with arxiv URLs, calls the arxiv API to fetch full abstracts. Entries with DOIs but no abstract are flagged as Paywalled. Entries with abstracts are marked Open.

**ResearchRecord model** -- Title, Authors, Doi, Url, Abstract, Tags, AccessLevel (Open/Paywalled/Unknown), ArxivId. Implements IPipelineRecord.

**Three ingestion patterns now proven:**
1. RSS/Atom -- fetch from URL, parse XML, auto-detect format
2. EDI 834 -- read local file, parse structured segments
3. Zotero -- read local file, selectively enrich from APIs, flag access level

The third pattern is the most realistic for production pipelines. Not every source is fully accessible. The adapter does what it can and flags what it can't.

## Test Coverage

- 52 total tests across 4 test projects
- Conduit.Sources.Zotero.Tests: 22 new tests covering CSV parsing, field extraction, access level detection, arxiv enrichment (mocked), and error handling
- All tests mock HttpClient -- no network calls in tests

## Data Directory

```
data/
  rss/hacker-news_*.json           <- 20 RSS items
  edi834/benefits-enrollment_*.json <- 4 enrollment records
  zotero/research-papers_*.json    <- 4 research records
```

## What's Next

Multi-Source Ingestion milestone is fully complete with four source types. Data Transformation is next.
