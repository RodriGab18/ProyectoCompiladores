using System;
using System.Collections.Generic;
using System.Linq;
using BioSphereIDE.Core;

// All AST node types live in AST.cs (same namespace).
namespace BioSphereIDE.Analizadores
{
    // ════════════════════════════════════════════════════════════════════════════
    //  SYMBOL TABLE  —  Scoped, stack-based.
    //  Uses Core.Symbol (which already has Usado, NumParams, Columna).
    // ════════════════════════════════════════════════════════════════════════════
    public sealed class SymbolTable
    {
        private readonly Stack<Dictionary<string, Symbol>> _scopes = new();
        private readonly Stack<string>                     _names  = new();

        public string CurrentScope => _names.Count > 0 ? _names.Peek() : "global";

        public void PushScope(string name)
        {
            _scopes.Push(new Dictionary<string, Symbol>(StringComparer.Ordinal));
            _names.Push(name);
        }

        public IReadOnlyDictionary<string, Symbol> PopScope()
        {
            _names.Pop();
            return _scopes.Pop();
        }

        /// Search from innermost to outermost scope.
        /// Returns (symbol, depth) where depth=0 is the innermost.
        public (Symbol? sym, int depth) LookUp(string name)
        {
            int d = 0;
            foreach (var scope in _scopes)
            {
                if (scope.TryGetValue(name, out var s)) return (s, d);
                d++;
            }
            return (null, -1);
        }

        /// Add to the current (innermost) scope. Returns false if already present.
        public bool TryAdd(Symbol sym)
        {
            if (_scopes.Count == 0) return false;
            var top = _scopes.Peek();
            if (top.ContainsKey(sym.Nombre)) return false;
            top[sym.Nombre] = sym;
            return true;
        }

        /// Update a symbol at a known depth.
        public void Update(string name, Symbol updated, int depth)
        {
            var arr = _scopes.ToArray();
            if (depth >= 0 && depth < arr.Length)
                arr[depth][name] = updated;
        }

        /// All symbols across all active scopes (for unused-variable warnings).
        public IEnumerable<Symbol> AllSymbols() => _scopes.SelectMany(s => s.Values);
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  SEMANTIC ANALYSER  —  Visitor over the AST produced by Parser.
    //
    //  Key design choices:
    //   • stopOnFirstError = false → ALL errors collected per compilation.
    //   • NodoIf / NodoWhile conditions are NodoValor (full expressions).
    //   • Source tokens on AST nodes give accurate line / column numbers.
    //   • After analysis, unused variables emit Severity.Warning.
    // ════════════════════════════════════════════════════════════════════════════
    public sealed class SemanticAnalyzer
    {
        private readonly SymbolTable   _table  = new();
        private readonly List<ErrorInfo> _errors = new();
        private int _loopDepth = 0;   // for continuar / romper validation

        // ── Public surface ───────────────────────────────────────────────────────
        public List<Symbol> Symbols { get; private set; } = new();

        public (bool success, List<ErrorInfo> errors) Analyze(NodoPrograma programa)
        {
            _errors.Clear();
            Symbols.Clear();
            _loopDepth = 0;

            _table.PushScope("global");
            VisitPrograma(programa);
            var global = _table.PopScope();

            // Collect symbols and warn about unused variables (exclude functions).
            foreach (var sym in global.Values)
            {
                Symbols.Add(sym);
                if (!sym.Usado && sym.Tipo != "funcion")
                    AddWarning("SEM-090",
                        $"Variable '{sym.Nombre}' declarada pero nunca utilizada.",
                        null, sym.Linea,
                        "Elimine la variable o úsela en el programa.");
            }

            bool ok = _errors.All(e => e.Severity != Severity.Error);
            return (ok, _errors);
        }

        // ── Error / Warning helpers ──────────────────────────────────────────────
        private void AddError(string code, string msg, Token? tok,
                              string sugg = "", int lineOverride = -1)
        {
            int ln  = tok?.Line   ?? (lineOverride > 0 ? lineOverride : 1);
            int col = tok?.Column ?? 1;
            int len = tok?.Length ?? 1;
            _errors.Add(new ErrorInfo
            {
                Type       = ErrorType.Semantico,
                Severity   = Severity.Error,
                Code       = code,
                Line       = ln, Column = col, Length = len,
                Message    = msg,
                Suggestion = sugg
            });
        }

        private void AddWarning(string code, string msg, Token? tok,
                                int lineOverride = -1, string sugg = "")
        {
            int ln  = tok?.Line   ?? (lineOverride > 0 ? lineOverride : 1);
            int col = tok?.Column ?? 1;
            _errors.Add(new ErrorInfo
            {
                Type       = ErrorType.Semantico,
                Severity   = Severity.Warning,
                Code       = code,
                Line       = ln, Column = col, Length = 1,
                Message    = msg,
                Suggestion = sugg
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        //  VISITORS
        // ════════════════════════════════════════════════════════════════════════
        private void VisitPrograma(NodoPrograma node) =>
            VisitBloqueSimulacion(node.BloqueSimulacion);

        private void VisitBloqueSimulacion(NodoBloqueSimulacion node)
        {
            _table.PushScope("simulacion");
            bool hasPlaneta = false, hasAtmosfera = false, hasAgua = false;
            foreach (var stmt in node.Sentencias)
            {
                if (stmt is NodoBloquePlaneta)   hasPlaneta   = true;
                if (stmt is NodoBloqueAtmosfera) hasAtmosfera = true;
                if (stmt is NodoBloqueAgua)      hasAgua      = true;
                VisitSentencia(stmt);
            }

            if (!hasPlaneta)
                AddError("SEM-040", "El bloque obligatorio 'planeta' está faltante.", node.SourceToken,
                    "Agregue 'planeta { masa = …; radio = …; }' dentro de 'simulacion { }'.");
            if (!hasAtmosfera)
                AddError("SEM-040", "El bloque obligatorio 'atmosfera' está faltante.", node.SourceToken,
                    "Agregue 'atmosfera { presion = …; co2 = …; }' dentro de 'simulacion { }'.");
            if (!hasAgua)
                AddError("SEM-040", "El bloque obligatorio 'agua' está faltante.", node.SourceToken,
                    "Agregue 'agua { estado_liquido = verdadero; }' dentro de 'simulacion { }'.");

            // Collect inner symbols for the public Symbols list.
            var scope = _table.PopScope();
            foreach (var sym in scope.Values) Symbols.Add(sym);
        }

        private void VisitBloquePlaneta(NodoBloquePlaneta node)
        {
            _table.PushScope("planeta");
            foreach (var s in node.Sentencias) VisitSentencia(s);

            var (masa,  _) = _table.LookUp("masa");
            var (radio, _) = _table.LookUp("radio");
            var tok = node.SourceToken;

            if (masa == null)
                AddError("SEM-041", "Variable 'masa' requerida en bloque 'planeta'.", tok,
                    "Agregue 'masa = <número> kg;' dentro del bloque 'planeta'.");
            else if (masa.Tipo != "numero")
                AddError("SEM-014", $"'masa' debe ser numérica, no '{masa.Tipo}'.", tok,
                    "Asigne un valor numérico a 'masa', p. ej. 'masa = 5.97e24 kg;'.");

            if (radio == null)
                AddError("SEM-041", "Variable 'radio' requerida en bloque 'planeta'.", tok,
                    "Agregue 'radio = <número> km;' dentro del bloque 'planeta'.");
            else if (radio.Tipo != "numero")
                AddError("SEM-014", $"'radio' debe ser numérico, no '{radio.Tipo}'.", tok,
                    "Asigne un valor numérico a 'radio', p. ej. 'radio = 6371 km;'.");

            var scope = _table.PopScope();
            foreach (var sym in scope.Values) Symbols.Add(sym);
        }

        private void VisitBloqueAtmosfera(NodoBloqueAtmosfera node)
        {
            _table.PushScope("atmosfera");
            foreach (var s in node.Sentencias) VisitSentencia(s);

            var (presion, _) = _table.LookUp("presion");
            var (co2,    _) = _table.LookUp("co2");
            var tok = node.SourceToken;

            if (presion == null)
                AddError("SEM-041", "Variable 'presion' requerida en bloque 'atmosfera'.", tok,
                    "Agregue 'presion = <número> atm;' dentro del bloque 'atmosfera'.");
            else if (presion.Tipo != "numero")
                AddError("SEM-014", $"'presion' debe ser numérica, no '{presion.Tipo}'.", tok);

            if (co2 == null)
                AddError("SEM-041", "Variable 'co2' requerida en bloque 'atmosfera'.", tok,
                    "Agregue 'co2 = <número> ppm;' dentro del bloque 'atmosfera'.");
            else if (co2.Tipo != "numero")
                AddError("SEM-014", $"'co2' debe ser numérica, no '{co2.Tipo}'.", tok);

            var scope = _table.PopScope();
            foreach (var sym in scope.Values) Symbols.Add(sym);
        }

        private void VisitBloqueAgua(NodoBloqueAgua node)
        {
            _table.PushScope("agua");
            foreach (var s in node.Sentencias) VisitSentencia(s);

            var (estado, _) = _table.LookUp("estado_liquido");
            var tok = node.SourceToken;

            if (estado == null)
                AddError("SEM-041", "Variable 'estado_liquido' requerida en bloque 'agua'.", tok,
                    "Agregue 'estado_liquido = verdadero;' o 'estado_liquido = falso;'.");
            else if (estado.Tipo != "booleano")
                AddError("SEM-014",
                    $"'estado_liquido' debe ser booleana (verdadero/falso), no '{estado.Tipo}'.", tok,
                    "Use 'estado_liquido = verdadero;' o 'estado_liquido = falso;'.");

            var scope = _table.PopScope();
            foreach (var sym in scope.Values) Symbols.Add(sym);
        }

        private void VisitBloqueVida(NodoBloqueVida node)
        {
            _table.PushScope("vida");
            foreach (var s in node.Sentencias) VisitSentencia(s);
            var scope = _table.PopScope();
            foreach (var sym in scope.Values) Symbols.Add(sym);
        }

        private void VisitDefinicionFuncion(NodoDefinicionFuncion node)
        {
            // Register the function symbol in the *outer* scope.
            var funcSym = new Symbol
            {
                Nombre   = $"func_{node.Nombre}",
                Tipo     = "funcion",
                Valor    = node.Parametros.Count,
                NumParams= node.Parametros.Count,
                Linea    = node.SourceToken?.Line   ?? 1,
                Columna  = node.SourceToken?.Column ?? 1,
                Ambito   = _table.CurrentScope,
                Usado    = false
            };
            if (!_table.TryAdd(funcSym))
                AddError("SEM-003",
                    $"La función '{node.Nombre}' ya está definida en este ámbito.", node.SourceToken,
                    "Elija un nombre distinto para la función.");

            // Visit body in a child scope.
            _table.PushScope($"funcion:{node.Nombre}");
            foreach (var param in node.Parametros)
            {
                var pSym = new Symbol
                {
                    Nombre  = param,
                    Tipo    = "numero",  // default param type in ASTRA
                    Linea   = node.SourceToken?.Line ?? 1,
                    Ambito  = _table.CurrentScope,
                    Usado   = false
                };
                _table.TryAdd(pSym);
            }
            foreach (var s in node.Sentencias) VisitSentencia(s);
            var scope = _table.PopScope();
            foreach (var sym in scope.Values) Symbols.Add(sym);
        }

        // ── Statement dispatch ───────────────────────────────────────────────────
        private void VisitSentencia(NodoSentencia stmt)
        {
            switch (stmt)
            {
                case NodoAsignacion         a:  VisitAsignacion(a);         break;
                case NodoMostrar            m:  VisitValor(m.Valor, m.SourceToken); break;
                case NodoReporte            r:  VisitValor(r.Valor, r.SourceToken); break;
                case NodoIf                 i:  VisitIf(i);                 break;
                case NodoWhile              w:  VisitWhile(w);              break;
                case NodoDefinicionFuncion  f:  VisitDefinicionFuncion(f);  break;
                case NodoBloquePlaneta      p:  VisitBloquePlaneta(p);      break;
                case NodoBloqueAtmosfera    at: VisitBloqueAtmosfera(at);   break;
                case NodoBloqueAgua         ag: VisitBloqueAgua(ag);        break;
                case NodoBloqueVida         v:  VisitBloqueVida(v);         break;
                case NodoContinuar          c:  VisitContinuar(c);          break;
                case NodoRomper             ro: VisitRomper(ro);            break;
                case NodoExpresionSentencia es: VisitValor(es.Expresion, es.SourceToken); break;
                default: break;
            }
        }

        private void VisitAsignacion(NodoAsignacion node)
        {
            var (tipo, valor, unidad) = VisitValor(node.Valor, node.SourceToken);
            var (existing, depth)     = _table.LookUp(node.Identificador);

            if (existing == null)
            {
                var sym = new Symbol
                {
                    Nombre  = node.Identificador,
                    Tipo    = tipo,
                    Valor   = valor,
                    Unidad  = unidad,
                    Linea   = node.SourceToken?.Line   ?? 1,
                    Columna = node.SourceToken?.Column ?? 1,
                    Ambito  = _table.CurrentScope,
                    Usado   = false
                };
                _table.TryAdd(sym);
            }
            else
            {
                // Type-change check (excluding numeric widening)
                if (existing.Tipo != tipo &&
                    !(existing.Tipo == "numero" && tipo == "numero"))
                {
                    AddError("SEM-014",
                        $"Incompatibilidad de tipos: '{node.Identificador}' es '{existing.Tipo}' " +
                        $"pero se le intenta asignar '{tipo}'.",
                        node.SourceToken,
                        "Verifique que el tipo del valor coincida con el de la variable, " +
                        "o declare una nueva variable con otro nombre.");
                }
                existing.Valor  = valor;
                existing.Unidad = unidad;
                _table.Update(node.Identificador, existing, depth);
            }
        }

        private void VisitIf(NodoIf node)
        {
            var (tipoCond, _, _) = VisitValor(node.Condicion, node.SourceToken);
            if (tipoCond != "booleano" && tipoCond != "nulo")
                AddError("SEM-020",
                    $"La condición de 'si' debe ser booleana, no '{tipoCond}'.",
                    node.SourceToken,
                    "Use un operador relacional (==, !=, <, >, <=, >=) o un valor verdadero/falso.");

            _table.PushScope("si:entonces");
            foreach (var s in node.ThenSentencias) VisitSentencia(s);
            var thenScope = _table.PopScope();
            foreach (var sym in thenScope.Values) Symbols.Add(sym);

            if (node.ElseSentencias.Count > 0)
            {
                _table.PushScope("si:sino");
                foreach (var s in node.ElseSentencias) VisitSentencia(s);
                var elseScope = _table.PopScope();
                foreach (var sym in elseScope.Values) Symbols.Add(sym);
            }
        }

        private void VisitWhile(NodoWhile node)
        {
            var (tipoCond, _, _) = VisitValor(node.Condicion, node.SourceToken);
            if (tipoCond != "booleano" && tipoCond != "nulo")
                AddError("SEM-020",
                    $"La condición de 'mientras' debe ser booleana, no '{tipoCond}'.",
                    node.SourceToken,
                    "Use un operador relacional o un valor booleano como condición.");

            _loopDepth++;
            _table.PushScope("mientras");
            foreach (var s in node.Sentencias) VisitSentencia(s);
            var scope = _table.PopScope();
            foreach (var sym in scope.Values) Symbols.Add(sym);
            _loopDepth--;
        }

        private void VisitContinuar(NodoContinuar node)
        {
            if (_loopDepth == 0)
                AddError("SEM-050", "'continuar' debe estar dentro de un bloque 'mientras'.", node.SourceToken,
                    "Mueva 'continuar;' al interior de un bucle 'mientras (…) { … }'.");
        }

        private void VisitRomper(NodoRomper node)
        {
            if (_loopDepth == 0)
                AddError("SEM-051", "'romper' debe estar dentro de un bloque 'mientras'.", node.SourceToken,
                    "Mueva 'romper;' al interior de un bucle 'mientras (…) { … }'.");
        }

        // ════════════════════════════════════════════════════════════════════════
        //  VALUE / EXPRESSION VISITOR
        //  Returns (tipo, valor?, unidad?)
        // ════════════════════════════════════════════════════════════════════════
        private (string tipo, object? valor, string? unidad) VisitValor(NodoValor nodo, Token? ctx)
        {
            switch (nodo)
            {
                case NodoExprNumero num:
                    return ("numero", num.Valor, null);

                case NodoBooleano boo:
                    return ("booleano", boo.Valor, null);

                case NodoTexto txt:
                    return ("texto", txt.Texto, null);

                case NodoNulo _:
                    return ("nulo", null, null);

                case NodoLista lst:
                    foreach (var v in lst.Valores) VisitValor(v, ctx);
                    return ("lista", null, null);

                case NodoCantidad cant:
                    var (tC, vC, uC) = VisitValor(cant.Expr, ctx);
                    string unitFinal  = cant.Unidad ?? uC ?? "";
                    return (tC, vC, unitFinal);

                case NodoExprIdentificador id:
                    var (sym, symDepth) = _table.LookUp(id.Nombre);
                    if (sym == null)
                    {
                        AddError("SEM-001",
                            $"Variable '{id.Nombre}' no está declarada en este ámbito.",
                            id.SourceToken ?? ctx,
                            $"Declare '{id.Nombre}' asignándole un valor antes de usarla.");
                        return ("nulo", null, null);
                    }
                    sym.Usado = true;
                    _table.Update(id.Nombre, sym, symDepth);
                    return (sym.Tipo, sym.Valor, sym.Unidad);

                case NodoExprUnaria una:
                    var (tU, _, uU) = VisitValor(una.Operando, una.SourceToken ?? ctx);
                    if (una.Operador == "-" && tU != "numero")
                        AddError("SEM-010",
                            $"El operador unario '-' requiere un valor numérico, no '{tU}'.",
                            una.SourceToken ?? ctx,
                            "Aplique '-' solo a variables o expresiones numéricas.");
                    return ("numero", null, uU);

                case NodoExprParentesis par:
                    return VisitValor(par.Expr, ctx);

                case NodoExprBinaria bin:
                    return VisitBinaria(bin, ctx);

                case NodoExprPotencia pot:
                    return VisitPotencia(pot, ctx);

                case NodoLlamadaFuncion call:
                    return VisitLlamada(call, ctx);

                default:
                    return ("nulo", null, null);
            }
        }

        // ── Binary expression ────────────────────────────────────────────────────
        private (string tipo, object? valor, string? unidad) VisitBinaria(
            NodoExprBinaria bin, Token? ctx)
        {
            var tok = bin.SourceToken ?? ctx;
            var (tL, _, uL) = VisitValor(bin.Izquierda, tok);
            var (tR, _, uR) = VisitValor(bin.Derecha,   tok);
            string op = bin.Operador;

            // Arithmetic operators
            if (op is "+" or "-" or "*" or "/")
            {
                if (tL == "nulo" || tR == "nulo")
                    AddError("SEM-013", $"Operación '{op}' no permitida con valor 'nulo'.", tok,
                        "Asegúrese de que ambos operandos estén inicializados.");
                else if (tL != "numero" || tR != "numero")
                    AddError("SEM-010",
                        $"El operador '{op}' requiere operandos numéricos, " +
                        $"se obtuvo '{tL}' y '{tR}'.", tok,
                        "Use solo variables numéricas en expresiones aritméticas.");

                if ((op is "+" or "-") && uL != null && uR != null && uL != uR)
                    AddError("SEM-030",
                        $"Unidades incompatibles en '{op}': '{uL}' y '{uR}'.", tok,
                        "Solo puede sumar o restar magnitudes con la misma unidad física.");

                string? resultUnit = (op is "+" or "-") ? (uL ?? uR) : null;
                return ("numero", null, resultUnit);
            }

            // Power handled by NodoExprPotencia; shouldn't reach here, but just in case.
            if (op == "^")
            {
                if (tL != "numero" || tR != "numero")
                    AddError("SEM-010", $"El operador '^' requiere operandos numéricos.", tok);
                if (uR != null)
                    AddError("SEM-031", "El exponente no puede tener unidad física.", tok,
                        "Use un número puro como exponente.");
                return ("numero", null, uL);
            }

            // Relational operators
            if (op is "<" or ">" or "<=" or ">=")
            {
                if (tL != "numero" || tR != "numero")
                    AddError("SEM-011",
                        $"El operador '{op}' requiere operandos numéricos, " +
                        $"se obtuvo '{tL}' y '{tR}'.", tok,
                        "Compare solo variables o expresiones numéricas.");
                return ("booleano", null, null);
            }

            // Equality operators — any type pair is valid
            if (op is "==" or "!=")
                return ("booleano", null, null);

            // Logical operators
            if (op is "y" or "o")
            {
                if (tL != "booleano")
                    AddError("SEM-021",
                        $"El operando izquierdo de '{op}' debe ser booleano, no '{tL}'.", tok,
                        "Use una comparación o un literal verdadero/falso.");
                if (tR != "booleano")
                    AddError("SEM-021",
                        $"El operando derecho de '{op}' debe ser booleano, no '{tR}'.", tok,
                        "Use una comparación o un literal verdadero/falso.");
                return ("booleano", null, null);
            }

            AddError("SEM-010", $"Operador desconocido '{op}'.", tok);
            return ("nulo", null, null);
        }

        // ── Power expression ─────────────────────────────────────────────────────
        private (string tipo, object? valor, string? unidad) VisitPotencia(
            NodoExprPotencia pot, Token? ctx)
        {
            var tok = pot.SourceToken ?? ctx;
            var (tB, _, uB) = VisitValor(pot.Base,     tok);
            var (tE, _, uE) = VisitValor(pot.Exponente, tok);

            if (tB != "numero" || tE != "numero")
                AddError("SEM-010", $"El operador '^' requiere operandos numéricos, " +
                    $"se obtuvo '{tB}' y '{tE}'.", tok,
                    "Use solo valores numéricos en operaciones de potencia.");
            if (uE != null && uE != "")
                AddError("SEM-031", "El exponente no puede tener unidad física.", tok,
                    "Use un número puro como exponente.");

            return ("numero", null, uB);
        }

        // ── Function call ────────────────────────────────────────────────────────
        private (string tipo, object? valor, string? unidad) VisitLlamada(
            NodoLlamadaFuncion call, Token? ctx)
        {
            var tok = call.SourceToken ?? ctx;
            var (funcSym, funcDepth) = _table.LookUp($"func_{call.Nombre}");

            if (funcSym == null)
            {
                AddError("SEM-002",
                    $"Función '{call.Nombre}' no está definida.",
                    tok,
                    $"Defina 'funcion {call.Nombre}(…) {{ … }}' antes de llamarla, " +
                    "o verifique el nombre.");
            }
            else
            {
                funcSym.Usado = true;
                _table.Update($"func_{call.Nombre}", funcSym, funcDepth);

                int expectedArity = funcSym.NumParams;
                if (call.Argumentos.Count != expectedArity)
                    AddError("SEM-060",
                        $"La función '{call.Nombre}' espera {expectedArity} argumento(s), " +
                        $"se recibieron {call.Argumentos.Count}.",
                        tok,
                        $"Ajuste el número de argumentos a {expectedArity}.");
            }

            foreach (var arg in call.Argumentos) VisitValor(arg, tok);
            return ("numero", null, null); // ASTRA functions return numeric by default
        }
    }
}
