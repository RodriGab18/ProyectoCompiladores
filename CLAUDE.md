# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**BioSphere IDE** is an integrated development environment for **ASTRA** (Astrobiology Simulation and Terrain Analysis), a domain-specific language (DSL) designed for modeling and simulating biosferes. The project implements a complete compiler with lexical, syntactic, and semantic analysis phases, plus a real-time code editor with syntax highlighting and error detection.

## Build and Run

### Prerequisites
- **.NET 8.0 SDK** (Windows only, requires native Windows Forms support)
- **Visual Studio 2022 or VS Code** (optional, for editing)
- **AvalonEdit 6.3.0.90** (automatically installed via NuGet)

### Build and Execute
```bash
# Restore NuGet packages (AvalonEdit dependency)
dotnet restore

# Build and run the IDE
dotnet run --project BioSphereIDE\BioSphereIDE.csproj

# Build without running
dotnet build BioSphereIDE\BioSphereIDE.csproj

# Release build
dotnet build -c Release BioSphereIDE\BioSphereIDE.csproj
```

## Architecture

The compiler implements a classical multi-stage pipeline. All analyzers and core types are in the `BioSphereIDE` project.

### Stage 1: Lexical Analysis (Lexer)
**File**: `BioSphereIDE/Analizadores/Lexer.cs`

Tokenizes source code into a stream of tokens. Key responsibilities:
- Recognizes reserved words (`simulacion`, `planeta`, `si`, `mientras`, `verdadero`, `falso`, etc.)
- Identifies identifiers, numbers (integers and floats), strings, operators, and symbols
- Detects lexical errors: invalid character sequences (e.g., numbers starting an identifier like `5000colonos`), unclosed strings, illegal characters (@, #, $, etc.)
- Tracks line and column position for error reporting
- Skips comments (lines starting with `//`)

**Token Types** (from `Core/Comunes.cs`): `PALABRA_RESERVADA`, `IDENTIFICADOR`, `NUMERO`, `OPERADOR`, `SIMBOLO`, `CADENA`, `ERROR_LEXICO`, `EOF`

The lexer defines token patterns in order of precedence. Error patterns must appear before normal patterns (e.g., invalid identifiers before valid ones) to capture them correctly.

### Stage 2: Syntactic Analysis (Parser)
**File**: `BioSphereIDE/Analizadores/Parser.cs`

Builds an Abstract Syntax Tree (AST) from tokens, validating grammatical structure.

**Key Grammar Rules**:
- Program: `inicio simulacion { ... } fin`
- Simulation block must contain: `planeta { ... }`, `atmosfera { ... }`, `agua { ... }`, optionally `vida { ... }` and function definitions
- Statements: assignments, conditionals (`si`/`sino`), loops (`mientras`), function calls, print/report operations
- Expressions: arithmetic operators (`+`, `-`, `*`, `/`, `^`), comparison operators, logical operators (`y`, `o`)
- Parentheses, braces, and brackets must be balanced

**AST Node Hierarchy**:
- Abstract base: `NodoAST`
- Programs: `NodoPrograma` containing `NodoBloqueSimulacion`
- Blocks: `NodoBloquePlaneta`, `NodoBloqueAtmosfera`, `NodoBloqueAgua`, `NodoBloqueVida`
- Statements: `NodoAsignacion`, `NodoIf`, `NodoWhile`, `NodoMostrar`, `NodoReporte`, `NodoDefinicionFuncion`
- Values/Expressions: `NodoBooleano`, `NodoTexto`, `NodoNulo`, `NodoLista`, `NodoCantidad`, `NodoExprBinaria`, `NodoExprPotencia`, `NodoExprParentesis`, `NodoLlamadaFuncion`

Parser uses recursive descent with lookahead (`Peek`, `Match`, `Eat` methods). Errors are collected but parsing continues (error recovery mode) to report multiple issues per compilation.

### Stage 3: Semantic Analysis (SemanticAnalyzer)
**File**: `BioSphereIDE/Analizadores/Semantico.cs`

Validates type correctness, variable declarations, mandatory blocks, and physical units. Uses a visitor pattern traversing the AST.

**Symbol Table** (`SymbolTable` class):
- Manages nested scopes (stack-based)
- Stores symbols with: name, type, value, optional unit, line, scope name
- Supports scope push/pop for functions and blocks
- Lookup searches from innermost to outermost scope

**Validation Rules**:
- All three main blocks (`planeta`, `atmosfera`, `agua`) are mandatory
- `planeta` requires: `masa` (numeric), `radio` (numeric)
- `atmosfera` requires: `presion` (numeric), `co2` (numeric)
- `agua` requires: `estado_liquido` (boolean)
- Type compatibility on assignment and operations
- Arithmetic operators require numeric operands; no operations on `nulo`
- Comparison operators return boolean; equality works on all types
- Logical operators (`y`, `o`) require boolean operands
- Physical units must match in addition/subtraction; exponents cannot have units
- Undefined variable detection
- Function definitions tracked; duplicate definitions rejected

Error codes follow pattern `SEM-NNN` (e.g., `SEM-001` for undefined variables, `SEM-041` for missing required variables).

### Stage 4: UI and Real-Time Feedback
**Files**:
- `BioSphereIDE/UI/BioSphereEditor.cs` – Main IDE window with code editor panel, token table (DataGridView), console output
- `BioSphereIDE/UI/AvalonEditExtension.cs` – Syntax highlighting and error squiggles using AvalonEdit
- `BioSphereIDE/UI/Visualizador.cs` – AST visualization panel

**Features**:
- Real-time compilation as user types
- Syntax coloring by token type (VS Code-style colors)
- Red dashed underlines for lexical/structural/semantic errors
- Symbol table display (Token table showing type, lexeme, line, column)
- Console-like output panel for `mostrar` and `reporte` statements
- Built-in documentation viewer (`FrmDocumentacion`) with ASTRA syntax and examples

## Data Flow

1. **User Input** → Lexer → Token stream
2. **Token stream** → Parser → AST (+ structural errors)
3. **AST** → SemanticAnalyzer → Semantic errors + Symbol table
4. **Errors** → UI error display (syntax coloring, squiggles, console)
5. **AST** → Interpreter (evaluates `mostrar`/`reporte`, executes assignments and control flow)

## Core Types
**File**: `BioSphereIDE/Core/Comunes.cs`

- `Token` – Lexeme, type, line, column, length
- `TokenType` enum – Type classifications
- `ErrorInfo` – Line, column, length, message, error type (Lexico/Estructural/Semantico)
- `Symbol` – Variable/function metadata in symbol table

## Entry Point
**File**: `BioSphereIDE/Program.cs`

Standard Windows Forms entry point. Creates and runs `BioSphereEditor` form.

## Key Implementation Notes

1. **Lexer Position Tracking**: Line increments on `\n`, column resets; tabs expand to 4 spaces.
2. **Parser Error Recovery**: Continues parsing after errors via `Eat` and `Match` methods, enabling multiple error reporting.
3. **Semantic Analysis**: Performs type inference where possible; variables created on first assignment. Function parameters default to numeric type.
4. **Symbol Table Scopes**: Each block (function, planeta, etc.) creates a new scope; variables searched from innermost outward.
5. **UI Updates**: Real-time tokenization/parsing triggered on text changes; errors displayed immediately with red underlines and console messages.
6. **Physical Units**: Optional; tracked as string property on symbols. Used for validation but not computation.

## Common Development Tasks

### Adding a New Reserved Word
1. Add to keyword pattern in `Lexer` constructor (line 21)
2. Update `Parser` to handle in `ParseSentencia` method if it introduces a statement
3. Update `SemanticAnalyzer` visitor methods if it affects validation

### Adding a New Error Code
Error codes follow semantic analyzer convention. Add to `AddError` calls with format `[SEM-NNN]` (e.g., `SEM-099`).

### Modifying Token Types
Update `TokenType` enum in `Core/Comunes.cs` and add corresponding patterns in `Lexer` (respecting pattern order for conflict resolution).

### Testing the Compiler
Use the built-in editor: type ASTRA code and click the compile button. Observe token table, error panel, and console output. The README includes a test program with intentional errors.

### Adding a New Statement Type
1. Define AST node class in `Parser.cs` extending `NodoSentencia`
2. Add parsing method in `Parser` (e.g., `ParseNewStatement`)
3. Call from `ParseSentencia` via `Peek` check
4. Add visitor method in `SemanticAnalyzer.VisitSentencia` switch
5. Update UI display if needed

## Solution Structure
```
ProyectoCompiladores/
  Compi.sln                    # Solution file
  README.md                    # Project documentation
  contexto_proyecto.txt        # Project context (large)
  BioSphereIDE/                # Main project (net8.0-windows)
    BioSphereIDE.csproj
    Program.cs                 # Entry point
    Analizadores/
      Lexer.cs                 # Stage 1: Tokenization
      Parser.cs                # Stage 2: AST construction
      Semantico.cs             # Stage 3: Type checking & validation
    Core/
      Comunes.cs               # Shared types & enums
    UI/
      BioSphereEditor.cs       # Main window form
      AvalonEditExtension.cs   # Syntax highlighting & error display
      Visualizador.cs          # AST visualization
    bin/Debug/                 # Build output
    obj/                       # Build artifacts
  .vscode/                     # Editor settings
  .git/                        # Git repository
```

## Dependencies
- **AvalonEdit 6.3.0.90**: WPF text editor control embedded in Windows Forms via `ElementHost`. Provides syntax highlighting engine and error indicator rendering.
- **.NET 8.0 runtime libraries**: Windows Forms, WPF interop

## Notes for Future Work

- **Interpreter Phase**: Currently, statements like assignments, conditionals, and `mostrar`/`reporte` are parsed but not fully executed as a runtime interpreter.
- **Code Generation**: No bytecode or machine code generation phase yet (planned for later phases).
- **Unit Handling**: Physical units are validated but not used in arithmetic calculations.
- **Line Tracking**: Semantic analyzer currently uses placeholder line numbers (line 1) for generated symbols; could extract actual lines from parser tokens.
