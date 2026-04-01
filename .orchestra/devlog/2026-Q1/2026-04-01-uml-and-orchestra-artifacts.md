# 2026-04-01: UML Diagrams and Orchestra Artifact Cleanup

## What Happened

Added Mermaid UML diagrams to `.orchestra/uml/` and did a round of cleanup on the work item folder structure — renaming ticket-prefixed folders to semantic names and adding gherkin acceptance scenarios across all milestones.

## Key Decisions

### UML as an agentic context artifact

The argument for bringing UML back isn't that diagrams are good practice in the abstract — it's that agents don't build mental models the way humans do. A human navigating an unfamiliar codebase reads files, follows imports, and accumulates intuition. An agent working within a context window reads what it's given and reasons from it.

A sequence diagram of the pipeline run communicates in one pass what would take a dozen file reads to reconstruct — and it communicates things the code doesn't say at all: the intended stage order, which writer handles which tier, where the concurrency boundary sits. Same for the class diagram: the interface hierarchy and the `TransformedRecord<T>` envelope pattern are visible at a glance instead of scattered across eight files.

This is a genuinely new reason to maintain diagrams that didn't exist five years ago. The README in `.orchestra/uml/` captures this argument so future contributors (and agents) understand why the artifacts are there.

### Mermaid over PlantUML

Mermaid renders natively in GitHub and Docusaurus without a build step or external tool. The diagrams live as plain text in `.md` files, update in the same commit as the feature that changes the design, and render wherever the project is read. A diagram that requires a separate tool to view is a diagram that won't stay current.

### Three diagrams to start

- **Component** (`component/architecture.md`) — projects, layers, and which interfaces each project implements. The structural map.
- **Sequence** (`sequence/pipeline-run.md`) — a single pipeline run from ingestion through validation, deduplication, enrichment, and output. The runtime story.
- **Class** (`class/domain-model.md`) — `IPipelineRecord`, `ITransform`, `IRecordValidator`, the envelope types, and the three concrete record types. The type hierarchy.

These three cover the questions most likely to come up when someone (or something) is trying to understand or extend the pipeline.

### Folder structure by diagram type, not subject

`.orchestra/uml/sequence/`, `component/`, `class/` — organized by UML taxonomy rather than by feature area. This means a new diagram for, say, the Storage Backends milestone goes in the right folder immediately, without a naming decision. It also makes the collection legible as a diagram library rather than a pile of files.

### Gherkin in every work item folder

Added `gherkin.md` to all seven work item folders. For completed milestones the scenarios document what was built. For the not-started milestones (Source Depth: EDI 834, RSS, Zotero) they define the acceptance bar that a future spec needs to satisfy — concrete enough to anchor implementation, not so prescriptive that they constrain the approach.

Gherkin belongs in the orchestra methodology for the same reason UML does: it's compressed, high-signal, and gives an agent (or a developer) an unambiguous definition of done before implementation starts.

### Dropping ticket IDs from folder names

The two folders prefixed with ClickUp IDs (`86e0mhar6-validation-transform`, `86e0mhara-rejected-data-tier`) were renamed to their semantic equivalents. Ticket IDs in folder names are noise for anyone navigating the repo and break if the ticket ID changes. The IDs now live in `spec.md` frontmatter where they're findable but not in the way.

## What We Shipped

- `.orchestra/uml/README.md` — explains the history of UML and its role in agentic development
- `component/architecture.md` — full project dependency graph with interface implementations
- `sequence/pipeline-run.md` — complete runtime sequence for a single pipeline run
- `class/domain-model.md` — type hierarchy covering all core interfaces, models, and transforms
- `gherkin.md` in all 7 work item folders
- Folder renames: `86e0mhara-rejected-data-tier` → `rejected-data-tier`, `86e0mhar6-validation-transform` → `validation-transform`
- Cross-references updated in `data-transformation/prd.md` and `validation-transform/prd.md`

## What's Next

The orchestra artifacts are now complete for the milestones shipped so far. The natural next move is to pick one of the three Source Depth milestones and write a spec — the gherkin acceptance scenarios are already in place to anchor it.
