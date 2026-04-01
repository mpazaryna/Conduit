# Class Diagram: Output Writers

The three output writer interfaces and their filesystem implementations.

```mermaid
classDiagram
    class IOutputWriter {
        <<interface>>
        +WriteAsync(items, sourceType, sourceName) Task
    }

    class ITransformedOutputWriter {
        <<interface>>
        +WriteAsync(items, sourceType, sourceName) Task
        +ReadPreviousIdsAsync(sourceType) Task
    }

    class IRejectedOutputWriter {
        <<interface>>
        +WriteAsync(items, sourceType, sourceName) Task
    }

    class JsonOutputWriter {
        -string _outputDir
        +WriteAsync(items, sourceType, sourceName) Task
    }

    class JsonTransformedOutputWriter {
        -string _outputDir
        +WriteAsync(items, sourceType, sourceName) Task
        +ReadPreviousIdsAsync(sourceType) Task
    }

    class JsonRejectedOutputWriter {
        -string _outputDir
        +WriteAsync(items, sourceType, sourceName) Task
    }

    IOutputWriter <|.. JsonOutputWriter
    ITransformedOutputWriter <|.. JsonTransformedOutputWriter
    IRejectedOutputWriter <|.. JsonRejectedOutputWriter

    JsonOutputWriter ..> "data/raw/" : writes
    JsonTransformedOutputWriter ..> "data/curated/" : writes
    JsonRejectedOutputWriter ..> "data/rejected/" : writes
```
