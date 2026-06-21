; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category            | Severity | Notes
--------|---------------------|----------|---------------------------------------------------
CHT001  | Cheetah.Modularity | Error    | ModuleDependencyAnalyzer, missing [DependsOn]
CHT002  | Cheetah.Modularity | Warning  | ModuleDependencyAnalyzer, unused [DependsOn]
