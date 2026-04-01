# Component Diagram: Layer Overview

The four layers and their dependency direction. All dependencies flow downward — no layer references anything above it.

```mermaid
graph TD
    subgraph App["App Layer"]
        Console["Conduit (Console)"]
        Worker["Conduit.Worker"]
        API["Conduit.Api"]
        CLI["Conduit.Cli"]
    end

    subgraph Transforms["Conduit.Transforms"]
        T["Validators + Enrichment Transforms\nValidationTransform · DeduplicationTransform\nRssEnrichmentTransform · Edi834EnrichmentTransform · ZoteroEnrichmentTransform"]
    end

    subgraph Adapters["Adapters"]
        RSS["Conduit.Sources.Rss"]
        EDI["Conduit.Sources.Edi834"]
        Zotero["Conduit.Sources.Zotero"]
    end

    subgraph Core["Conduit.Core"]
        C["Interfaces + Models\nIPipelineRecord · ISourceAdapter · ITransform\nIRecordValidator · IOutputWriter · TransformPipeline"]
    end

    App -->|depends on| Transforms
    App -->|depends on| Adapters
    App -->|depends on| Core
    Transforms -->|depends on| Adapters
    Transforms -->|depends on| Core
    Adapters -->|depends on| Core
```

See [interface-map.md](interface-map.md) for which classes implement which interfaces.
