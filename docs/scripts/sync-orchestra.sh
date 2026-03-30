#!/bin/bash
# Copies .orchestra/ content into docs/docs/ with Docusaurus frontmatter.
# Run from the repo root: bash docs/scripts/sync-orchestra.sh

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
DOCS="$REPO_ROOT/docs/docs"

mkdir -p "$DOCS/milestones" "$DOCS/decisions" "$DOCS/devlog" "$DOCS/learning"

add_frontmatter() {
  local file="$1"
  local title="$2"
  local position="$3"
  local dest="$4"

  echo "---" > "$dest"
  echo "sidebar_position: $position" >> "$dest"
  echo "---" >> "$dest"
  echo "" >> "$dest"
  cat "$file" >> "$dest"
}

# Intro/landing
cat > "$DOCS/index.md" << 'INTRO'
---
sidebar_position: 1
slug: /
---

# Conduit

A domain-agnostic data pipeline for multi-source content processing. Built with .NET 10.

Browse the sidebar for project documentation: roadmap, milestone PRDs, architecture decisions, development journal, and learning notes.

All content is sourced from the [.orchestra/](https://github.com/mpazaryna/Conduit/tree/main/.orchestra) directory in the repository.
INTRO

# Roadmap
add_frontmatter "$REPO_ROOT/.orchestra/roadmap.md" "Roadmap" 2 "$DOCS/roadmap.md"

# Milestones
add_frontmatter "$REPO_ROOT/.orchestra/work/foundation/prd.md" "Foundation" 1 "$DOCS/milestones/foundation.md"
add_frontmatter "$REPO_ROOT/.orchestra/work/multi-source-ingestion/prd.md" "Multi-Source Ingestion" 2 "$DOCS/milestones/multi-source-ingestion.md"
add_frontmatter "$REPO_ROOT/.orchestra/work/data-transformation/prd.md" "Data Transformation" 3 "$DOCS/milestones/data-transformation.md"

# Decisions
add_frontmatter "$REPO_ROOT/.orchestra/adr/ADR-000-the-score.md" "ADR-000" 1 "$DOCS/decisions/adr-000-the-score.md"
add_frontmatter "$REPO_ROOT/.orchestra/adr/ADR-001-domain-agnostic-pipeline.md" "ADR-001" 2 "$DOCS/decisions/adr-001-domain-agnostic-pipeline.md"
add_frontmatter "$REPO_ROOT/.orchestra/adr/ADR-002-production-readiness-is-continuous.md" "ADR-002" 3 "$DOCS/decisions/adr-002-production-readiness-is-continuous.md"
add_frontmatter "$REPO_ROOT/.orchestra/adr/ADR-003-no-docs-site.md" "ADR-003" 4 "$DOCS/decisions/adr-003-no-docs-site.md"

# Devlog
pos=1
for f in "$REPO_ROOT"/.orchestra/devlog/2026-Q1/2026-03-28-*.md "$REPO_ROOT"/.orchestra/devlog/2026-Q1/2026-03-29-{pivot,source-isolation,multi-source,zotero,dropping}*.md; do
  [ -f "$f" ] || continue
  name=$(basename "$f" .md)
  add_frontmatter "$f" "$name" $pos "$DOCS/devlog/$name.md"
  pos=$((pos + 1))
done

# Learning
pos=1
for f in "$REPO_ROOT"/.orchestra/devlog/2026-Q1/*learning*.md; do
  [ -f "$f" ] || continue
  name=$(basename "$f" .md | sed 's/^2026-03-[0-9]*-learning-//')
  add_frontmatter "$f" "$name" $pos "$DOCS/learning/$name.md"
  pos=$((pos + 1))
done

echo "Synced .orchestra/ to docs/docs/"
