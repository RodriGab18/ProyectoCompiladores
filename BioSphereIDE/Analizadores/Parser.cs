using System;
using System.Collections.Generic;
using BioSphereIDE.Core;

// All AST node types live in AST.cs (same namespace — no import needed).
namespace BioSphereIDE.Analizadores
{
    // ════════════════════════════════════════════════════════════════════════════
    //  PARSER  —  Recursive-descent parser for the ASTRA DSL.
    //
    //  Expression grammar (highest precedence first):
    //   Factor      →  '-' Factor | '(' Expr ')' | Literal | Identifier | Call
    //   Potencia    →  Factor  ('^' Potencia)?         (right-assoc)
    //   Term        →  Potencia (('*'|'/') Potencia)*
    //   Suma        →  Term    (('+'|'-') Term)*
    //   Comparacion →  Suma    (RelOp Suma)?
    //   And         →  Comparacion ('y' Comparacion)*
    //   Or          →  And     ('o' And)*
    //   Expr        →  Or                               (= ParseValor)
    //
    //  Conditions in 'si' and 'mientras' are full expressions (no separate
    //  NodoCondicion node), validated as boolean by the semantic analyser.
    // ════════════════════════════════════════════════════════════════════════════
    public sealed class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;
        private readonly List<ErrorInfo> _errors = new();

        // Tracks functions defined so far for arity checks at call sites.
        private readonly Dictionary<string, int> _funcArity = new();

        // ── Helpers ─────────────────────────────────────────────────────────────
        private Token? Current  => _pos < _tokens.Count ? _tokens[_pos] : null;
        private Token? LookAhead(int offset = 1) =>
            (_pos + offset) < _tokens.Count ? _tokens[_pos + offset] : null;

        public Parser(List<Token> tokens) { _tokens = tokens; }

        // ── Error reporting ──────────────────────────────────────────────────────
        private void AddError(string code, string msg, Token? at, string sugg = "")
        {
            _errors.Add(new ErrorInfo
            {
                Type       = ErrorType.Estructural,
                Code       = code,
                Line       = at?.Line   ?? 1,
                Column     = at?.Column ?? 1,
                Length     = at?.Length ?? 1,
                Message    = msg,
                Suggestion = sugg
            });
        }

        // ── Navigation primitives ────────────────────────────────────────────────

        /// Peek: true if current token matches type (and optionally lexeme).
        private bool Peek(TokenType type, string? lexeme = null) =>
            Current?.Type == type && (lexeme == null || Current.Lexeme == lexeme);

        /// Consume: advance and return token if it matches; add error and return
        /// null otherwise.  Does NOT advance on mismatch (let callers decide).
        private Token? Consume(TokenType type, string? lexeme = null)
        {
            if (Current?.Type == type && (lexeme == null || Current.Lexeme == lexeme))
            {
                var tok = Current;
                _pos++;
                return tok;
            }
            string expected = lexeme != null ? $"'{lexeme}'" : $"<{type}>";
            string found    = Current != null
                ? $"'{Current.Lexeme}' ({Current.Type})"
                : "fin de archivo";
            AddError("SIN-001",
                $"Se esperaba {expected} pero se encontró {found}.",
                Current,
                $"Verifique la sintaxis cerca de la línea {Current?.Line ?? 0}.");
            return null;
        }

        /// Eat: like Consume but advances even on mismatch so parsing can continue.
        private bool Eat(TokenType type, string? lexeme = null)
        {
            if (Current?.Type == type && (lexeme == null || Current.Lexeme == lexeme))
            { _pos++; return true; }

            string expected = lexeme != null ? $"'{lexeme}'" : $"<{type}>";
            string found    = Current != null
                ? $"'{Current.Lexeme}' ({Current.Type})"
                : "fin de archivo";
            AddError("SIN-001",
                $"Se esperaba {expected} pero se encontró {found}.",
                Current,
                $"Verifique la sintaxis cerca de la línea {Current?.Line ?? 0}.");

            // Advance so we don't loop forever on the same bad token.
            if (Current != null && Current.Type != TokenType.EOF) _pos++;
            return false;
        }

        /// Skip tokens until we reach a statement boundary or EOF.
        private void Synchronize()
        {
            if (Current != null && Current.Type != TokenType.EOF) _pos++;

            while (Current != null && Current.Type != TokenType.EOF)
            {
                if (Peek(TokenType.SIMBOLO, ";") || Peek(TokenType.SIMBOLO, "}"))
                { _pos++; return; }
                if (Current.Type == TokenType.PALABRA_RESERVADA) return;
                _pos++;
            }
        }

        // ════════════════════════════════════════════════════════════════════════
        //  ENTRY POINT
        // ════════════════════════════════════════════════════════════════════════
        public (NodoPrograma? Programa, List<ErrorInfo> Errores) ParsePrograma()
        {
            _pos = 0;
            _errors.Clear();
            _funcArity.Clear();

            // Skip leading lexical-error tokens
            SkipErrorTokens();

            var inicioTok = Consume(TokenType.PALABRA_RESERVADA, "inicio");
            if (inicioTok == null)
            {
                AddError("SIN-010", "El programa debe comenzar con 'inicio'.", Current,
                    "Agregue 'inicio' al principio del archivo.");
                return (null, _errors);
            }

            var bloque = ParseBloqueSimulacion();
            if (bloque == null) return (null, _errors);

            Consume(TokenType.PALABRA_RESERVADA, "fin");

            // Accept even with errors so the IDE can display partial results.
            return (new NodoPrograma { BloqueSimulacion = bloque, SourceToken = inicioTok }, _errors);
        }

        private void SkipErrorTokens()
        {
            while (Current?.Type == TokenType.ERROR_LEXICO) _pos++;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  BLOCKS
        // ════════════════════════════════════════════════════════════════════════
        private NodoBloqueSimulacion? ParseBloqueSimulacion()
        {
            var tok = Consume(TokenType.PALABRA_RESERVADA, "simulacion");
            if (tok == null) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;

            var stmts = ParseStmtList(stopOnSino: false);
            Eat(TokenType.SIMBOLO, "}");
            return new NodoBloqueSimulacion { Sentencias = stmts, SourceToken = tok };
        }

        private NodoBloquePlaneta? ParseBloquePlaneta()
        {
            var tok = Consume(TokenType.PALABRA_RESERVADA, "planeta"); if (tok == null) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var stmts = ParseStmtList(stopOnSino: false);
            Eat(TokenType.SIMBOLO, "}");
            return new NodoBloquePlaneta { Sentencias = stmts, SourceToken = tok };
        }

        private NodoBloqueAtmosfera? ParseBloqueAtmosfera()
        {
            var tok = Consume(TokenType.PALABRA_RESERVADA, "atmosfera"); if (tok == null) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var stmts = ParseStmtList(stopOnSino: false);
            Eat(TokenType.SIMBOLO, "}");
            return new NodoBloqueAtmosfera { Sentencias = stmts, SourceToken = tok };
        }

        private NodoBloqueAgua? ParseBloqueAgua()
        {
            var tok = Consume(TokenType.PALABRA_RESERVADA, "agua"); if (tok == null) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var stmts = ParseStmtList(stopOnSino: false);
            Eat(TokenType.SIMBOLO, "}");
            return new NodoBloqueAgua { Sentencias = stmts, SourceToken = tok };
        }

        private NodoBloqueVida? ParseBloqueVida()
        {
            var tok = Consume(TokenType.PALABRA_RESERVADA, "vida"); if (tok == null) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var stmts = ParseStmtList(stopOnSino: false);
            Eat(TokenType.SIMBOLO, "}");
            return new NodoBloqueVida { Sentencias = stmts, SourceToken = tok };
        }

        private NodoOrbitaYEscala? ParseOrbitaYEscala()
        {
            var tok = Consume(TokenType.PALABRA_RESERVADA, "orbita_y_escala"); if (tok == null) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var stmts = ParseStmtList(stopOnSino: false);
            Eat(TokenType.SIMBOLO, "}");
            return new NodoOrbitaYEscala { Instrucciones = stmts, SourceToken = tok };
        }

        // ════════════════════════════════════════════════════════════════════════
        //  STATEMENT LIST
        // ════════════════════════════════════════════════════════════════════════
        private List<NodoSentencia> ParseStmtList(bool stopOnSino)
        {
            var list = new List<NodoSentencia>();
            while (Current != null && Current.Type != TokenType.EOF)
            {
                if (Peek(TokenType.SIMBOLO, "}")) break;
                if (stopOnSino && Peek(TokenType.PALABRA_RESERVADA, "sino")) break;

                SkipErrorTokens();
                if (Current == null || Current.Type == TokenType.EOF) break;
                if (Peek(TokenType.SIMBOLO, "}")) break;

                var stmt = ParseSentencia();
                if (stmt != null) { list.Add(stmt); continue; }

                // Could not parse — synchronize to avoid infinite loop.
                Synchronize();
            }
            return list;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  STATEMENTS
        // ════════════════════════════════════════════════════════════════════════
        private NodoSentencia? ParseSentencia()
        {
            if (Current == null || Current.Type == TokenType.EOF) return null;

            if (Peek(TokenType.PALABRA_RESERVADA, "planeta"))    return ParseBloquePlaneta();
            if (Peek(TokenType.PALABRA_RESERVADA, "atmosfera"))  return ParseBloqueAtmosfera();
            if (Peek(TokenType.PALABRA_RESERVADA, "agua"))       return ParseBloqueAgua();
            if (Peek(TokenType.PALABRA_RESERVADA, "vida"))       return ParseBloqueVida();
            if (Peek(TokenType.PALABRA_RESERVADA, "orbita_y_escala")) return ParseOrbitaYEscala();
            if (Peek(TokenType.PALABRA_RESERVADA, "funcion"))    return ParseFuncion();
            if (Peek(TokenType.PALABRA_RESERVADA, "si"))         return ParseSi();
            if (Peek(TokenType.PALABRA_RESERVADA, "mientras"))   return ParseMientras();
            if (Peek(TokenType.PALABRA_RESERVADA, "mostrar"))    return ParseMostrar();
            if (Peek(TokenType.PALABRA_RESERVADA, "reporte"))    return ParseReporte();
            if (Peek(TokenType.PALABRA_RESERVADA, "continuar"))
            {
                var t = Current!; _pos++;
                Eat(TokenType.SIMBOLO, ";");
                return new NodoContinuar { SourceToken = t };
            }
            if (Peek(TokenType.PALABRA_RESERVADA, "romper"))
            {
                var t = Current!; _pos++;
                Eat(TokenType.SIMBOLO, ";");
                return new NodoRomper { SourceToken = t };
            }
            if (Peek(TokenType.IDENTIFICADOR)) return ParseAsignacionOLlamada();

            AddError("SIN-008", $"Sentencia inválida: token '{Current?.Lexeme}' inesperado.", Current,
                "Verifique que la instrucción sea válida en ASTRA (asignación, si, mientras, mostrar, funcion…).");
            return null;
        }

        // ── Function definition ──────────────────────────────────────────────────
        private NodoDefinicionFuncion? ParseFuncion()
        {
            var tok = Consume(TokenType.PALABRA_RESERVADA, "funcion"); if (tok == null) return null;
            var nameTok = Consume(TokenType.IDENTIFICADOR);            if (nameTok == null) return null;
            if (!Eat(TokenType.SIMBOLO, "(")) return null;

            var pars = new List<string>();
            if (!Peek(TokenType.SIMBOLO, ")"))
            {
                var p = Consume(TokenType.IDENTIFICADOR);
                if (p != null) pars.Add(p.Lexeme);
                while (Peek(TokenType.SIMBOLO, ","))
                {
                    _pos++;
                    var pn = Consume(TokenType.IDENTIFICADOR);
                    if (pn != null) pars.Add(pn.Lexeme);
                }
            }
            if (!Eat(TokenType.SIMBOLO, ")")) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var body = ParseStmtList(stopOnSino: false);
            if (!Eat(TokenType.SIMBOLO, "}")) return null;

            // Register for call-site arity validation.
            if (_funcArity.ContainsKey(nameTok.Lexeme))
                AddError("SIN-007", $"La función '{nameTok.Lexeme}' ya está definida.", nameTok,
                    "Cambie el nombre de una de las definiciones.");
            else
                _funcArity[nameTok.Lexeme] = pars.Count;

            return new NodoDefinicionFuncion
            {
                Nombre     = nameTok.Lexeme,
                Parametros = pars,
                Sentencias = body,
                SourceToken = tok
            };
        }

        // ── If ───────────────────────────────────────────────────────────────────
        private NodoIf? ParseSi()
        {
            var tok = Consume(TokenType.PALABRA_RESERVADA, "si"); if (tok == null) return null;
            if (!Eat(TokenType.SIMBOLO, "(")) return null;
            var cond = ParseExpr(); if (cond == null) return null;
            if (!Eat(TokenType.SIMBOLO, ")")) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var thenStmts = ParseStmtList(stopOnSino: true);
            if (!Eat(TokenType.SIMBOLO, "}")) return null;

            var elseStmts = new List<NodoSentencia>();
            if (Peek(TokenType.PALABRA_RESERVADA, "sino"))
            {
                _pos++;
                if (!Eat(TokenType.SIMBOLO, "{")) return null;
                elseStmts = ParseStmtList(stopOnSino: false);
                Eat(TokenType.SIMBOLO, "}");
            }

            return new NodoIf
            {
                Condicion      = cond,
                ThenSentencias = thenStmts,
                ElseSentencias = elseStmts,
                SourceToken    = tok
            };
        }

        // ── While ────────────────────────────────────────────────────────────────
        private NodoWhile? ParseMientras()
        {
            var tok = Consume(TokenType.PALABRA_RESERVADA, "mientras"); if (tok == null) return null;
            if (!Eat(TokenType.SIMBOLO, "(")) return null;
            var cond = ParseExpr(); if (cond == null) return null;
            if (!Eat(TokenType.SIMBOLO, ")")) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var body = ParseStmtList(stopOnSino: false);
            if (!Eat(TokenType.SIMBOLO, "}")) return null;

            return new NodoWhile { Condicion = cond, Sentencias = body, SourceToken = tok };
        }

        // ── Mostrar / Reporte ────────────────────────────────────────────────────
        private NodoMostrar? ParseMostrar()
        {
            var tok = Consume(TokenType.PALABRA_RESERVADA, "mostrar"); if (tok == null) return null;
            NodoExpr? val;
            if (Peek(TokenType.SIMBOLO, "("))
            {
                _pos++;
                val = ParseExpr();
                Eat(TokenType.SIMBOLO, ")");
            }
            else val = ParseExpr();

            if (val == null) return null;
            Eat(TokenType.SIMBOLO, ";");
            return new NodoMostrar { Valor = val, SourceToken = tok };
        }

        private NodoReporte? ParseReporte()
        {
            var tok = Consume(TokenType.PALABRA_RESERVADA, "reporte"); if (tok == null) return null;
            NodoExpr? val;
            if (Peek(TokenType.SIMBOLO, "("))
            {
                _pos++;
                val = ParseExpr();
                Eat(TokenType.SIMBOLO, ")");
            }
            else val = ParseExpr();

            if (val == null) return null;
            Eat(TokenType.SIMBOLO, ";");
            return new NodoReporte { Valor = val, SourceToken = tok };
        }

        // ── Assignment / Function-call statement ─────────────────────────────────
        private NodoSentencia? ParseAsignacionOLlamada()
        {
            var idTok = Current!;
            // Is this a function call?  id '(' ...
            if (LookAhead()?.Type == TokenType.SIMBOLO && LookAhead()?.Lexeme == "(")
            {
                _pos++; // consume identifier
                var call = ParseLlamadaFuncion(idTok.Lexeme, idTok);
                if (call == null) return null;
                Eat(TokenType.SIMBOLO, ";");
                return new NodoExpresionSentencia { Expresion = call, SourceToken = idTok };
            }
            return ParseAsignacion();
        }

        private NodoAsignacion? ParseAsignacion()
        {
            var idTok = Consume(TokenType.IDENTIFICADOR); if (idTok == null) return null;
            if (!Eat(TokenType.OPERADOR, "="))             return null;
            var val = ParseExpr();                         if (val == null) return null;
            Eat(TokenType.SIMBOLO, ";");
            return new NodoAsignacion { Identificador = idTok.Lexeme, Valor = val, SourceToken = idTok };
        }

        // ════════════════════════════════════════════════════════════════════════
        //  EXPRESSION GRAMMAR
        //  ParseExpr = ParseOr  (entry point for any expression)
        // ════════════════════════════════════════════════════════════════════════
        private NodoExpr? ParseExpr() => ParseOr();

        // Or: lowest precedence
        private NodoExpr? ParseOr()
        {
            var left = ParseAnd();
            while (left != null && Peek(TokenType.PALABRA_RESERVADA, "o"))
            {
                var tok = Current!; _pos++;
                var right = ParseAnd();
                if (right == null) break;
                left = new NodoExprBinaria { Izquierda = left, Operador = "o", Derecha = right, SourceToken = tok };
            }
            return left;
        }

        private NodoExpr? ParseAnd()
        {
            var left = ParseComparacion();
            while (left != null && Peek(TokenType.PALABRA_RESERVADA, "y"))
            {
                var tok = Current!; _pos++;
                var right = ParseComparacion();
                if (right == null) break;
                left = new NodoExprBinaria { Izquierda = left, Operador = "y", Derecha = right, SourceToken = tok };
            }
            return left;
        }

        private NodoExpr? ParseComparacion()
        {
            var left = ParseSuma();
            if (left != null && IsRelOp())
            {
                var tok = Current!; string op = tok.Lexeme; _pos++;
                var right = ParseSuma();
                if (right == null) return left;
                return new NodoExprBinaria { Izquierda = left, Operador = op, Derecha = right, SourceToken = tok };
            }
            return left;
        }

        private NodoExpr? ParseSuma()
        {
            var left = ParseTerm();
            while (left != null &&
                   (Peek(TokenType.OPERADOR, "+") || Peek(TokenType.OPERADOR, "-")))
            {
                var tok = Current!; string op = tok.Lexeme; _pos++;
                var right = ParseTerm();
                if (right == null) break;
                left = new NodoExprBinaria { Izquierda = left, Operador = op, Derecha = right, SourceToken = tok };
            }
            return left;
        }

        private NodoExpr? ParseTerm()
        {
            var left = ParsePotencia();
            while (left != null &&
                   (Peek(TokenType.OPERADOR, "*") || Peek(TokenType.OPERADOR, "/")))
            {
                var tok = Current!; string op = tok.Lexeme; _pos++;
                var right = ParsePotencia();
                if (right == null) break;
                left = new NodoExprBinaria { Izquierda = left, Operador = op, Derecha = right, SourceToken = tok };
            }
            return left;
        }

        private NodoExpr? ParsePotencia()
        {
            var left = ParseFactor();
            if (left != null && Peek(TokenType.OPERADOR, "^"))
            {
                var tok = Current!; _pos++;
                var right = ParsePotencia(); // right-associative
                if (right == null) return left;
                return new NodoExprPotencia { Base = left, Exponente = right, SourceToken = tok };
            }
            return left;
        }

        private NodoExpr? ParseFactor()
        {
            // Unary minus
            if (Peek(TokenType.OPERADOR, "-"))
            {
                var tok = Current!; _pos++;
                var operand = ParseFactor();
                if (operand == null) return null;
                return new NodoExprUnaria { Operador = "-", Operando = operand, SourceToken = tok };
            }

            // Parenthesized expression — may contain full Or-level expressions
            if (Peek(TokenType.SIMBOLO, "("))
            {
                var tok = Current!; _pos++;
                var expr = ParseExpr();
                if (expr == null) return null;
                Eat(TokenType.SIMBOLO, ")");
                return new NodoExprParentesis { Expr = expr, SourceToken = tok };
            }

            // Boolean literals
            if (Peek(TokenType.PALABRA_RESERVADA, "verdadero") ||
                Peek(TokenType.PALABRA_RESERVADA, "falso"))
            {
                var tok = Current!; _pos++;
                return new NodoBooleano { Valor = tok.Lexeme == "verdadero", SourceToken = tok };
            }

            // Null literal
            if (Peek(TokenType.PALABRA_RESERVADA, "nulo"))
            {
                var tok = Current!; _pos++;
                return new NodoNulo { SourceToken = tok };
            }

            // String literal
            if (Peek(TokenType.CADENA))
            {
                var tok = Current!; _pos++;
                string text = tok.Lexeme.Length >= 2
                    ? tok.Lexeme.Substring(1, tok.Lexeme.Length - 2)
                    : tok.Lexeme;
                return new NodoTexto { Texto = text, SourceToken = tok };
            }

            // List literal  [ v1, v2, … ]
            if (Peek(TokenType.SIMBOLO, "[")) return ParseLista();

            // Numeric literal, optionally followed by a physical unit
            if (Peek(TokenType.NUMERO))
            {
                var tok = Current!; _pos++;
                NodoExpr num = new NodoExprNumero { Valor = tok.Lexeme, SourceToken = tok };

                if (IsUnit())
                {
                    var unitTok = Current!; _pos++;
                    return new NodoCantidad { Expr = num, Unidad = unitTok.Lexeme, SourceToken = tok };
                }
                return num;
            }

            // Identifier or function call
            if (Peek(TokenType.IDENTIFICADOR))
            {
                var idTok = Current!;
                if (LookAhead()?.Type == TokenType.SIMBOLO && LookAhead()?.Lexeme == "(")
                {
                    _pos++; // consume identifier
                    return ParseLlamadaFuncion(idTok.Lexeme, idTok);
                }
                _pos++;
                return new NodoExprIdentificador { Nombre = idTok.Lexeme, SourceToken = idTok };
            }

            AddError("SIN-009",
                $"Se esperaba un valor o expresión pero se encontró '{Current?.Lexeme ?? "EOF"}'.",
                Current,
                "Verifique que la expresión sea un número, variable, llamada a función, cadena o booleano.");
            return null;
        }

        // ── List literal ─────────────────────────────────────────────────────────
        private NodoLista? ParseLista()
        {
            var tok = Current!; _pos++;          // consume '['
            var vals = new List<NodoValor>();
            if (!Peek(TokenType.SIMBOLO, "]"))
            {
                var first = ParseExpr(); if (first != null) vals.Add(first);
                while (Peek(TokenType.SIMBOLO, ","))
                {
                    _pos++;
                    var v = ParseExpr(); if (v != null) vals.Add(v);
                }
            }
            Eat(TokenType.SIMBOLO, "]");
            return new NodoLista { Valores = vals, SourceToken = tok };
        }

        // ── Function call  name '(' args ')' ────────────────────────────────────
        private NodoLlamadaFuncion? ParseLlamadaFuncion(string nombre, Token nameTok)
        {
            if (!Eat(TokenType.SIMBOLO, "(")) return null;
            var args = new List<NodoValor>();
            if (!Peek(TokenType.SIMBOLO, ")"))
            {
                var first = ParseExpr(); if (first != null) args.Add(first);
                while (Peek(TokenType.SIMBOLO, ","))
                {
                    _pos++;
                    var a = ParseExpr(); if (a != null) args.Add(a);
                }
            }
            if (!Eat(TokenType.SIMBOLO, ")")) return null;

            // Arity check against known definitions.
            if (_funcArity.TryGetValue(nombre, out int expected))
            {
                if (args.Count != expected)
                    AddError("SIN-006",
                        $"La función '{nombre}' espera {expected} argumento(s), se recibieron {args.Count}.",
                        nameTok,
                        $"Ajuste el número de argumentos a {expected}.");
            }
            // Unknown functions are validated by the semantic analyser.

            return new NodoLlamadaFuncion { Nombre = nombre, Argumentos = args, SourceToken = nameTok };
        }

        // ── Helpers ──────────────────────────────────────────────────────────────
        private bool IsRelOp() =>
            Current?.Type == TokenType.OPERADOR &&
            Current.Lexeme is "<" or ">" or "<=" or ">=" or "==" or "!=";

        private static readonly HashSet<string> _units = new(StringComparer.Ordinal)
                { "km", "m", "g", "kg", "atm", "ppm", "Sv", "h", "s", "mol", "K", "Pa", "UA", "km3", "m3" };

        private bool IsUnit() =>
            Current?.Type == TokenType.IDENTIFICADOR && _units.Contains(Current.Lexeme);
    }
}
