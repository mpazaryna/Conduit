# Multi-Source Ingestion -- Execution Spec

**PRD:** [prd.md](prd.md)
**Status:** Not Started

## Approach

Three tracks executed in order, each building on the previous.

**Track 1: Generalize the pipeline record type.** The current `ISourceAdapter` returns `List<FeedItem>`, which locks the pipeline to content data. We need a base interface (`IPipelineRecord`) that both `FeedItem` and `EnrollmentRecord` can implement. The output writer and pipeline loop operate on `IPipelineRecord`; domain-specific types remain in their adapter projects.

**Track 2: Adapter routing.** The pipeline currently hardcodes `RssSourceAdapter` in DI. We need a factory or keyed-service pattern that resolves the correct adapter based on `SourceSettings.Type` at runtime. This is a standard .NET DI pattern -- keyed services were added in .NET 8.

**Track 3: EDI 834 adapter.** New source project (`Conduit.Sources.Edi834`) with its own domain model, X12 parser, and test fixtures. The 834 file format uses fixed-width segments delimited by `~` (segment terminator) and `*` (element separator), organized in a hierarchical loop structure.

Atom support and auto-detection are deferred -- they don't prove anything the 834 adapter doesn't prove more forcefully.

## Steps

1. [ ] **Create `IPipelineRecord` interface in Conduit.Core** -- minimal marker interface with common metadata (Id, Timestamp, SourceType). FeedItem implements it.
2. [ ] **Update `ISourceAdapter` to return `List<IPipelineRecord>`** instead of `List<FeedItem>`.
3. [ ] **Update `IOutputWriter` to accept `List<IPipelineRecord>`** instead of `List<FeedItem>`.
4. [ ] **Update `JsonOutputWriter`** to serialize any `IPipelineRecord` implementation.
5. [ ] **Update pipeline loop** in Console, Worker, and Api to use `IPipelineRecord`.
6. [ ] **Implement adapter routing** -- register adapters by key (`"rss"`, `"edi834"`), resolve by `SourceSettings.Type` in the pipeline loop.
7. [ ] **Update existing tests** -- all 10 tests must still pass after generalization.
8. [ ] **Create `Conduit.Sources.Edi834` project** with reference to Conduit.Core.
9. [ ] **Define `EnrollmentRecord` model** implementing `IPipelineRecord` -- fields: SubscriberId, MemberName, RelationshipCode, MaintenanceTypeCode, CoverageStartDate, CoverageEndDate, PlanId.
10. [ ] **Implement `Edi834SourceAdapter`** -- reads an 834 file from disk, parses ISA/GS/ST envelope, extracts member loops (2000), maps INS/REF/DTP/NM1 segments to `EnrollmentRecord`.
11. [ ] **Create sample 834 test fixture** -- a valid 834 file with 3-5 member records covering common scenarios (new enrollment, termination, dependent).
12. [ ] **Create `Conduit.Sources.Edi834.Tests`** -- tests for parsing, missing segments, malformed files, empty files.
13. [ ] **Add 834 source to appsettings.json** pointing to a sample file in the data directory.
14. [ ] **Run full pipeline** with both RSS and 834 sources configured. Verify data lands in `data/rss/` and `data/edi834/` respectively.
15. [ ] **Add concurrent processing** -- process sources in parallel using `Task.WhenAll` with a semaphore to control concurrency.

## Deliverables

| Deliverable | Location | Acceptance Criteria |
|-------------|----------|---------------------|
| IPipelineRecord interface | src/Conduit.Core/Models/ | FeedItem and EnrollmentRecord both implement it |
| Adapter routing | src/Conduit/Program.cs (and Worker, Api) | Correct adapter resolves based on SourceSettings.Type |
| Edi834SourceAdapter | src/Conduit.Sources.Edi834/Services/ | Parses sample 834 file into EnrollmentRecord list |
| EnrollmentRecord | src/Conduit.Sources.Edi834/Models/ | Captures subscriber, coverage, and maintenance data |
| Sample 834 fixture | tests/Conduit.Sources.Edi834.Tests/fixtures/ | Valid X12 834 with 3-5 member records |
| 834 adapter tests | tests/Conduit.Sources.Edi834.Tests/ | Parsing, edge cases, error handling |
| Concurrent processing | src/Conduit/ | Multiple sources process in parallel |
| Sample 834 output | data/edi834/ | Committed example of processed 834 data |

## Risks

- **X12 parsing complexity.** 834 files have a deeply nested segment/loop structure. We'll build a minimal parser for the common case (subscriber + dependent enrollment) rather than trying to handle every possible 834 variation. A production parser would use a library like `OopFactory.X12` or `EdiFabric`.
- **IPipelineRecord generalization.** Changing the return type of `ISourceAdapter` from `List<FeedItem>` to `List<IPipelineRecord>` touches every caller. The existing test suite is the safety net -- all 10 tests must pass after each step.
- **JSON serialization of polymorphic types.** `System.Text.Json` needs configuration to serialize/deserialize types implementing an interface. We'll use the `JsonDerivedType` attribute or a type discriminator.

## Notes

### 834 file structure (simplified)

```
ISA*00*          *00*          *ZZ*SENDER         *ZZ*RECEIVER       *...~
GS*HP*SENDER*RECEIVER*20240115*1200*1*X*005010X220A1~
ST*834*0001*005010X220A1~
BGN*00*12345*20240115*1200****2~
  INS*Y*18*021*28*A*E**FT~           <- Subscriber, active enrollment
  REF*0F*123456789~                   <- SSN
  DTP*336*D8*20240101~                <- Coverage start date
  NM1*IL*1*DOE*JOHN****34*123456789~  <- Member name + ID
  HD*021**HLT*PLAN001~                <- Health coverage, plan ID
  DTP*348*D8*20240101~                <- Coverage effective date
SE*...*0001~
GE*1*1~
IEA*1*000000001~
```

Key segments:
- **INS** -- enrollment action (Y/N = subscriber/non-subscriber, maintenance type)
- **REF** -- reference identifiers (SSN, member ID, group number)
- **DTP** -- dates (coverage start, end, enrollment)
- **NM1** -- member name and identifiers
- **HD** -- health coverage details (plan, coverage type)

### Adapter routing pattern

```csharp
// Register adapters as keyed services
services.AddKeyedScoped<ISourceAdapter, RssSourceAdapter>("rss");
services.AddKeyedScoped<ISourceAdapter, Edi834SourceAdapter>("edi834");

// Resolve by type in the pipeline loop
foreach (var source in appSettings.Sources)
{
    var adapter = provider.GetRequiredKeyedService<ISourceAdapter>(source.Type);
    var items = await adapter.IngestAsync(source.Location);
    ...
}
```
