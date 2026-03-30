import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  orchestraSidebar: [
    'index',
    'roadmap',
    {
      type: 'category',
      label: 'Milestones',
      collapsed: false,
      items: [
        'milestones/foundation',
        'milestones/multi-source-ingestion',
        'milestones/data-transformation',
      ],
    },
    {
      type: 'category',
      label: 'Decisions',
      collapsed: true,
      items: [
        'decisions/adr-000-the-score',
        'decisions/adr-001-domain-agnostic-pipeline',
        'decisions/adr-002-production-readiness-is-continuous',
        'decisions/adr-003-no-docs-site',
      ],
    },
    {
      type: 'category',
      label: 'Devlog',
      collapsed: true,
      items: [
        'devlog/2026-03-28-project-kickoff',
        'devlog/2026-03-29-pivot-to-conduit',
        'devlog/2026-03-29-source-isolation',
        'devlog/2026-03-29-multi-source-ingestion',
        'devlog/2026-03-29-zotero-adapter',
        'devlog/2026-03-29-dropping-docs-site',
      ],
    },
    {
      type: 'category',
      label: 'Learning',
      collapsed: true,
      items: [
        'learning/dotnet-fundamentals',
        'learning/di-and-solid',
        'learning/testing-patterns',
        'learning/project-architecture',
        'learning/code-coverage',
      ],
    },
  ],
};

export default sidebars;
