# Multi-Source Ingestion -- Acceptance Scenarios

**PRD:** [prd.md](prd.md)
**Spec:** [spec.md](spec.md)

## RSS Ingestion

```gherkin
Feature: RSS Feed Ingestion

  Scenario: Ingest a valid RSS feed
    Given an RSS feed URL is configured as a source
    When the pipeline runs
    Then items are parsed from the RSS XML
    And each item has a title, link, description, and published date
    And the output is written to data/rss/

  Scenario: Ingest a valid Atom feed
    Given an Atom feed URL is configured as a source with type "rss"
    When the pipeline runs
    Then the adapter auto-detects the Atom format
    And items are parsed from the Atom XML
    And each item has a title, link, description, and updated date

  Scenario: Auto-detect feed format
    Given a feed URL that returns RSS XML
    When the adapter ingests the source
    Then it detects the format from the root element without user configuration

  Scenario: Handle unreachable feed
    Given an RSS feed URL that returns a network error
    When the pipeline runs
    Then the adapter logs the error
    And returns an empty list
    And other sources continue processing

  Scenario: Handle malformed XML
    Given an RSS feed URL that returns invalid XML
    When the pipeline runs
    Then the adapter logs the parse error
    And returns an empty list

  Scenario: Handle unknown XML format
    Given a URL that returns valid XML but not RSS or Atom
    When the adapter ingests the source
    Then it logs a warning
    And returns an empty list

  Scenario: Handle missing fields in RSS items
    Given an RSS item with only a title element
    When the adapter parses the item
    Then the link defaults to empty string
    And the description defaults to empty string
    And the published date defaults to DateTime.MinValue

  Scenario: Strip HTML from descriptions
    Given an RSS item with HTML tags in the description
    When the adapter parses the item
    Then HTML tags are removed leaving plain text
```

## EDI 834 Ingestion

```gherkin
Feature: EDI 834 Healthcare Enrollment Ingestion

  Scenario: Ingest a valid 834 file
    Given an 834 file path is configured as a source with type "edi834"
    When the pipeline runs
    Then enrollment records are parsed from the X12 segments
    And each record has subscriber ID, member name, and coverage dates
    And the output is written to data/edi834/

  Scenario: Parse subscriber enrollment
    Given an 834 file with an INS segment where the first element is "Y"
    When the adapter parses the file
    Then the record has relationship code "18" (subscriber)
    And the maintenance type reflects the enrollment action

  Scenario: Parse dependent enrollment
    Given an 834 file with an INS segment where the first element is "N"
    When the adapter parses the file
    Then the record has the appropriate relationship code (spouse, child)

  Scenario: Parse coverage termination
    Given an 834 file with a maintenance type code indicating termination
    When the adapter parses the file
    Then the record has a coverage end date

  Scenario: Handle missing 834 file
    Given an 834 file path that does not exist
    When the pipeline runs
    Then the adapter logs the error
    And returns an empty list
    And other sources continue processing

  Scenario: Handle malformed 834 segments
    Given an 834 file with incomplete or missing segments
    When the adapter parses the file
    Then records with available data are still produced
    And missing fields default to empty values

  Scenario: Handle empty 834 file
    Given an 834 file with only envelope segments and no member data
    When the adapter parses the file
    Then an empty list is returned
```

## Zotero CSV Ingestion

```gherkin
Feature: Zotero CSV Research Library Ingestion

  Scenario: Ingest a Zotero CSV export
    Given a Zotero CSV file is configured as a source with type "zotero"
    When the pipeline runs
    Then research records are parsed from the CSV
    And each record has title, authors, and URL
    And the output is written to data/zotero/

  Scenario: Handle full Zotero export with 80+ columns
    Given a Zotero CSV with the full column set
    When the adapter parses the file
    Then it finds columns by header name
    And extracts Title, Author, DOI, Url, Abstract Note, and Manual Tags

  Scenario: Detect arxiv papers
    Given a Zotero entry with a URL containing "arxiv.org"
    When the adapter parses the entry
    Then the arxiv ID is extracted from the URL

  Scenario: Enrich arxiv entries
    Given a Zotero entry with an arxiv ID and no abstract
    When the adapter ingests the source
    Then it calls the arxiv API to fetch the abstract
    And the record is updated with the fetched abstract

  Scenario: Flag paywalled entries
    Given a Zotero entry with a DOI but no abstract
    When the adapter determines access level
    Then the record is marked as Paywalled

  Scenario: Flag open access entries
    Given a Zotero entry with an abstract present
    When the adapter determines access level
    Then the record is marked as Open

  Scenario: Handle missing Zotero file
    Given a Zotero CSV path that does not exist
    When the pipeline runs
    Then the adapter logs the error
    And returns an empty list

  Scenario: Skip empty rows
    Given a Zotero CSV with rows that have no title and no URL
    When the adapter parses the file
    Then those rows are skipped
```

## Pipeline Behavior

```gherkin
Feature: Multi-Source Pipeline Orchestration

  Scenario: Process multiple sources concurrently
    Given three sources are configured (RSS, EDI 834, Zotero)
    When the pipeline runs
    Then all three sources are ingested concurrently
    And output appears in data/rss/, data/edi834/, and data/zotero/

  Scenario: Isolate source failures
    Given three sources are configured and one has an invalid location
    When the pipeline runs
    Then the failing source logs an error and returns empty
    And the other two sources complete successfully

  Scenario: Route to correct adapter by type
    Given a source configured with type "edi834"
    When the pipeline resolves the adapter
    Then it receives an Edi834SourceAdapter instance
    And not an RssSourceAdapter or ZoteroSourceAdapter

  Scenario: Content and transaction types coexist
    Given RSS sources producing FeedItem records
    And EDI 834 sources producing EnrollmentRecord records
    And Zotero sources producing ResearchRecord records
    When all are processed in the same pipeline run
    Then each record type is serialized with its domain-specific fields
    And output is organized by source type in separate directories

  Scenario: Add a new source type
    Given a developer implements ISourceAdapter in a new project
    And registers it with a keyed DI service
    And adds a source entry to appsettings.json
    When the pipeline runs
    Then the new source is ingested without changes to the pipeline core
```
