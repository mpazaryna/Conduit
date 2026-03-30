#!/bin/bash
# Copies .orchestra/ content into docs/docs/ with Docusaurus frontmatter.
# Run from the repo root: bash docs/scripts/sync-orchestra.sh

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
DOCS="$REPO_ROOT/docs/docs"

mkdir -p "$DOCS/milestones" "$DOCS/decisions" "$DOCS/devlog" "$DOCS/learning"

add_frontmatter() {
  local file="$1"
  local label="$2"
  local position="$3"
  local dest="$4"

  echo "---" > "$dest"
  echo "sidebar_label: \"$label\"" >> "$dest"
  echo "sidebar_position: $position" >> "$dest"
  echo "---" >> "$dest"
  echo "" >> "$dest"
  cat "$file" >> "$dest"
}

# Intro/landing
cat > "$DOCS/index.md" << 'INTRO'
---
sidebar_label: Home
sidebar_position: 1
slug: /
---

# Conduit

A domain-agnostic data pipeline for multi-source content processing. Built with .NET 10.

## What's Here

| Section | Description |
|---------|-------------|
| **Roadmap** | Project vision, milestones, and current status |
| **Milestones** | PRDs for each phase -- what we're building and why |
| **Decisions** | Architecture decision records -- the choices that shape the system |
| **Devlog** | Development journal -- what happened, what changed, what we learned |
| **Learning** | .NET study notes -- fundamentals, DI, testing, architecture, coverage |

## Source Adapters

| Adapter | Format | Status |
|---------|--------|--------|
| RSS / Atom | XML feeds (auto-detected) | Complete |
| EDI 834 | Healthcare enrollment | Complete |
| Zotero | Research library CSV + arxiv | Complete |

## Quick Start

```bash
dotnet restore
dotnet run --project src/App/Conduit
dotnet test
```

All content is sourced from [.orchestra/](https://github.com/mpazaryna/Conduit/tree/main/.orchestra) in the repository.
INTRO

# Roadmap
add_frontmatter "$REPO_ROOT/.orchestra/roadmap.md" "Roadmap" 2 "$DOCS/roadmap.md"

# Milestones
add_frontmatter "$REPO_ROOT/.orchestra/work/foundation/prd.md" "Foundation" 1 "$DOCS/milestones/foundation.md"
add_frontmatter "$REPO_ROOT/.orchestra/work/multi-source-ingestion/prd.md" "Multi-Source Ingestion" 2 "$DOCS/milestones/multi-source-ingestion.md"
add_frontmatter "$REPO_ROOT/.orchestra/work/data-transformation/prd.md" "Data Transformation" 3 "$DOCS/milestones/data-transformation.md"

# Decisions
add_frontmatter "$REPO_ROOT/.orchestra/adr/ADR-000-the-score.md" "ADR-000: The Score" 1 "$DOCS/decisions/adr-000-the-score.md"
add_frontmatter "$REPO_ROOT/.orchestra/adr/ADR-001-domain-agnostic-pipeline.md" "ADR-001: Domain-Agnostic" 2 "$DOCS/decisions/adr-001-domain-agnostic-pipeline.md"
add_frontmatter "$REPO_ROOT/.orchestra/adr/ADR-002-production-readiness-is-continuous.md" "ADR-002: Continuous Readiness" 3 "$DOCS/decisions/adr-002-production-readiness-is-continuous.md"
add_frontmatter "$REPO_ROOT/.orchestra/adr/ADR-003-no-docs-site.md" "ADR-003: Docs Strategy" 4 "$DOCS/decisions/adr-003-no-docs-site.md"

# Devlog
add_frontmatter "$REPO_ROOT/.orchestra/devlog/2026-Q1/2026-03-28-project-kickoff.md" "Project Kickoff" 1 "$DOCS/devlog/2026-03-28-project-kickoff.md"
add_frontmatter "$REPO_ROOT/.orchestra/devlog/2026-Q1/2026-03-29-pivot-to-conduit.md" "Pivot to Conduit" 2 "$DOCS/devlog/2026-03-29-pivot-to-conduit.md"
add_frontmatter "$REPO_ROOT/.orchestra/devlog/2026-Q1/2026-03-29-source-isolation.md" "Source Isolation" 3 "$DOCS/devlog/2026-03-29-source-isolation.md"
add_frontmatter "$REPO_ROOT/.orchestra/devlog/2026-Q1/2026-03-29-multi-source-ingestion.md" "Multi-Source Ingestion" 4 "$DOCS/devlog/2026-03-29-multi-source-ingestion.md"
add_frontmatter "$REPO_ROOT/.orchestra/devlog/2026-Q1/2026-03-29-zotero-adapter.md" "Zotero Adapter" 5 "$DOCS/devlog/2026-03-29-zotero-adapter.md"
add_frontmatter "$REPO_ROOT/.orchestra/devlog/2026-Q1/2026-03-29-dropping-docs-site.md" "Dropping the Docs Site" 6 "$DOCS/devlog/2026-03-29-dropping-docs-site.md"

# Learning
add_frontmatter "$REPO_ROOT/.orchestra/devlog/2026-Q1/2026-03-29-learning-dotnet-fundamentals.md" ".NET Fundamentals" 1 "$DOCS/learning/dotnet-fundamentals.md"
add_frontmatter "$REPO_ROOT/.orchestra/devlog/2026-Q1/2026-03-29-learning-di-and-solid.md" "DI and SOLID" 2 "$DOCS/learning/di-and-solid.md"
add_frontmatter "$REPO_ROOT/.orchestra/devlog/2026-Q1/2026-03-29-learning-testing-patterns.md" "Testing Patterns" 3 "$DOCS/learning/testing-patterns.md"
add_frontmatter "$REPO_ROOT/.orchestra/devlog/2026-Q1/2026-03-29-learning-project-architecture.md" "Project Architecture" 4 "$DOCS/learning/project-architecture.md"
add_frontmatter "$REPO_ROOT/.orchestra/devlog/2026-Q1/2026-03-29-learning-code-coverage.md" "Code Coverage" 5 "$DOCS/learning/code-coverage.md"

echo "Synced .orchestra/ to docs/docs/"
