# ADR-003: Docusaurus Deploys .orchestra/ to GitHub Pages; No API Reference

**Date:** 2026-03-29
**Updated:** 2026-03-30
**Status:** Active
**Decision:** The docs site renders `.orchestra/` markdown via Docusaurus on GitHub Pages. No auto-generated API reference is published.

## Context

The project uses Docusaurus to deploy `.orchestra/` project documentation (roadmap, ADRs, devlogs, PRDs) as a navigable site on GitHub Pages.

API reference documentation is intentionally omitted. Traditional API doc sites (e.g., DocFX-generated HTML from XML doc comments) were a useful artifact when developers had to navigate code without assistance. With agentic programming tools, the agent reads source code and XML doc comments directly — a published API reference adds maintenance overhead without adding value.

## Decision

1. **Docusaurus** builds and deploys `.orchestra/` markdown to GitHub Pages via `.github/workflows/docs.yml` on every push to `main`.
2. **No API reference site.** IDE IntelliSense and XML doc comments serve developers in-editor. Agents read source directly.
3. **`.orchestra/` is always the source of truth.** The site is a read-only convenience view — never hand-edited.

## Consequences

- The `docs/` folder contains the Docusaurus site configuration and a sync script (`docs/scripts/sync-orchestra.sh`) that copies `.orchestra/` into Docusaurus at build time.
- All documentation effort goes into `.orchestra/` markdown and XML doc comments in source.
- API reference is never published as a separate artifact.
