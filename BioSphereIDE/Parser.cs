using System;
using System.Collections.Generic;
using System.Text;

namespace BioSphereIDE
{
    // ==================== NODOS DEL ÁRBOL SINTÁCTICO ====================
    public abstract class NodoAST
    {
        public abstract string ToTreeString(string indent, bool isLast);
    }

    public class NodoPrograma : NodoAST
    {
        public NodoBloqueSimulacion BloqueSimulacion { get; set; } = null!;
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "PROGRAMA");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(newIndent + "├── inicio\n");
            sb.Append(BloqueSimulacion.ToTreeString(newIndent, false));
            sb.Append(newIndent + "└── fin\n");
            return sb.ToString();
        }
    }

    public class NodoBloqueSimulacion : NodoAST
    {
        public List<NodoSentencia> Sentencias { get; set; } = new List<NodoSentencia>();
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "BLOQUE_SIMULACION");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(newIndent + "├── simulacion\n");
            sb.Append(newIndent + "├── {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(newIndent + "│   ", i == Sentencias.Count - 1));
            sb.Append(newIndent + "└── }\n");
            return sb.ToString();
        }
    }

    public class NodoBloquePlaneta : NodoSentencia
    {
        public List<NodoSentencia> Sentencias { get; set; } = new List<NodoSentencia>();
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "BLOQUE_PLANETA");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(newIndent + "├── planeta\n");
            sb.Append(newIndent + "├── {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(newIndent + "│   ", i == Sentencias.Count - 1));
            sb.Append(newIndent + "└── }\n");
            return sb.ToString();
        }
    }

    public class NodoBloqueAtmosfera : NodoSentencia
    {
        public List<NodoSentencia> Sentencias { get; set; } = new List<NodoSentencia>();
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "BLOQUE_ATMOSFERA");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(newIndent + "├── atmosfera\n");
            sb.Append(newIndent + "├── {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(newIndent + "│   ", i == Sentencias.Count - 1));
            sb.Append(newIndent + "└── }\n");
            return sb.ToString();
        }
    }

    public class NodoBloqueAgua : NodoSentencia
    {
        public List<NodoSentencia> Sentencias { get; set; } = new List<NodoSentencia>();
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "BLOQUE_AGUA");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(newIndent + "├── agua\n");
            sb.Append(newIndent + "├── {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(newIndent + "│   ", i == Sentencias.Count - 1));
            sb.Append(newIndent + "└── }\n");
            return sb.ToString();
        }
    }

    public class NodoBloqueVida : NodoSentencia
    {
        public List<NodoSentencia> Sentencias { get; set; } = new List<NodoSentencia>();
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "BLOQUE_VIDA");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(newIndent + "├── vida\n");
            sb.Append(newIndent + "├── {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(newIndent + "│   ", i == Sentencias.Count - 1));
            sb.Append(newIndent + "└── }\n");
            return sb.ToString();
        }
    }

    public abstract class NodoSentencia : NodoAST { }

    public class NodoAsignacion : NodoSentencia
    {
        public string Identificador { get; set; } = "";
        public NodoValor Valor { get; set; } = null!;
        public override string ToTreeString(string indent, bool isLast)
        {
            return indent + (isLast ? "└── " : "├── ") + $"ASIGNACION: {Identificador} = {Valor?.ToString() ?? "?"}\n";
        }
    }

    public class NodoMostrar : NodoSentencia
    {
        public NodoValor Valor { get; set; } = null!;
        public override string ToTreeString(string indent, bool isLast)
        {
            return indent + (isLast ? "└── " : "├── ") + $"MOSTRAR: {Valor?.ToString() ?? "?"}\n";
        }
    }

    public class NodoReporte : NodoSentencia
    {
        public NodoValor Valor { get; set; } = null!;
        public override string ToTreeString(string indent, bool isLast)
        {
            return indent + (isLast ? "└── " : "├── ") + $"REPORTE: {Valor?.ToString() ?? "?"}\n";
        }
    }

    public class NodoIf : NodoSentencia
    {
        public NodoCondicion Condicion { get; set; } = null!;
        public List<NodoSentencia> ThenSentencias { get; set; } = new List<NodoSentencia>();
        public List<NodoSentencia> ElseSentencias { get; set; } = new List<NodoSentencia>();
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "IF");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(Condicion.ToTreeString(newIndent, false));
            sb.Append(newIndent + "├── then {\n");
            for (int i = 0; i < ThenSentencias.Count; i++)
                sb.Append(ThenSentencias[i].ToTreeString(newIndent + "│   ", i == ThenSentencias.Count - 1));
            sb.Append(newIndent + "│   └── }\n");
            if (ElseSentencias.Count > 0)
            {
                sb.Append(newIndent + "└── else {\n");
                for (int i = 0; i < ElseSentencias.Count; i++)
                    sb.Append(ElseSentencias[i].ToTreeString(newIndent + "    ", i == ElseSentencias.Count - 1));
                sb.Append(newIndent + "    └── }\n");
            }
            return sb.ToString();
        }
    }

    public class NodoWhile : NodoSentencia
    {
        public NodoCondicion Condicion { get; set; } = null!;
        public List<NodoSentencia> Sentencias { get; set; } = new List<NodoSentencia>();
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "WHILE");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(Condicion.ToTreeString(newIndent, false));
            sb.Append(newIndent + "└── {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(newIndent + "    ", i == Sentencias.Count - 1));
            sb.Append(newIndent + "    └── }\n");
            return sb.ToString();
        }
    }

    public class NodoCondicion : NodoAST
    {
        public NodoValor Izquierda { get; set; } = null!;
        public string? Operador { get; set; }
        public NodoValor? Derecha { get; set; }
        public List<(string op, NodoCondicion cond)> OperadoresLogicos { get; set; } = new List<(string, NodoCondicion)>();
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "CONDICION");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(newIndent + "├── " + (Izquierda?.ToString() ?? "?") + "\n");
            if (!string.IsNullOrEmpty(Operador))
                sb.Append(newIndent + "├── " + Operador + "\n" + newIndent + "└── " + (Derecha?.ToString() ?? "?") + "\n");
            else
                sb.Append(newIndent + "└── (único valor)\n");
            foreach (var (op, cond) in OperadoresLogicos)
            {
                sb.Append(newIndent + "├── " + op + "\n");
                sb.Append(cond.ToTreeString(newIndent + "│   ", false));
            }
            return sb.ToString();
        }
    }

    public abstract class NodoValor : NodoAST
    {
        public abstract override string ToString();
    }

    public class NodoCantidad : NodoValor
    {
        public NodoExpr Expr { get; set; } = null!;
        public string? Unidad { get; set; }
        public override string ToString() => Expr?.ToString() + (Unidad != null ? " " + Unidad : "");
        public override string ToTreeString(string indent, bool isLast)
        {
            return indent + (isLast ? "└── " : "├── ") + $"CANTIDAD: {ToString()}\n";
        }
    }

    public class NodoBooleano : NodoValor
    {
        public bool Valor { get; set; }
        public override string ToString() => Valor ? "verdadero" : "falso";
        public override string ToTreeString(string indent, bool isLast)
        {
            return indent + (isLast ? "└── " : "├── ") + $"BOOLEANO: {ToString()}\n";
        }
    }

    public class NodoNulo : NodoValor
    {
        public override string ToString() => "nulo";
        public override string ToTreeString(string indent, bool isLast)
        {
            return indent + (isLast ? "└── " : "├── ") + $"NULO\n";
        }
    }

    public class NodoTexto : NodoValor
    {
        public string Texto { get; set; } = "";
        public override string ToString() => $"\"{Texto}\"";
        public override string ToTreeString(string indent, bool isLast)
        {
            return indent + (isLast ? "└── " : "├── ") + $"TEXTO: {ToString()}\n";
        }
    }

    public class NodoLista : NodoValor
    {
        public List<NodoValor> Valores { get; set; } = new List<NodoValor>();
        public override string ToString() => "[" + string.Join(", ", Valores) + "]";
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "LISTA");
            string newIndent = indent + (isLast ? "    " : "│   ");
            for (int i = 0; i < Valores.Count; i++)
                sb.Append(Valores[i].ToTreeString(newIndent, i == Valores.Count - 1));
            return sb.ToString();
        }
    }

    public abstract class NodoExpr : NodoValor { }

    public class NodoExprBinaria : NodoExpr
    {
        public NodoExpr Izquierda { get; set; } = null!;
        public string Operador { get; set; } = "";
        public NodoExpr Derecha { get; set; } = null!;
        public override string ToString() => $"({Izquierda} {Operador} {Derecha})";
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + $"EXPR_BINARIA: {Operador}");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(Izquierda.ToTreeString(newIndent, false));
            sb.Append(Derecha.ToTreeString(newIndent, true));
            return sb.ToString();
        }
    }

    public class NodoExprPotencia : NodoExpr
    {
        public NodoExpr Base { get; set; } = null!;
        public NodoExpr Exponente { get; set; } = null!;
        public override string ToString() => $"({Base}^{Exponente})";
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "POTENCIA");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(Base.ToTreeString(newIndent, false));
            sb.Append(Exponente.ToTreeString(newIndent, true));
            return sb.ToString();
        }
    }

    public class NodoExprNumero : NodoExpr
    {
        public string Valor { get; set; } = "";
        public override string ToString() => Valor;
        public override string ToTreeString(string indent, bool isLast)
        {
            return indent + (isLast ? "└── " : "├── ") + $"NUMERO: {Valor}\n";
        }
    }

    public class NodoExprIdentificador : NodoExpr
    {
        public string Nombre { get; set; } = "";
        public override string ToString() => Nombre;
        public override string ToTreeString(string indent, bool isLast)
        {
            return indent + (isLast ? "└── " : "├── ") + $"IDENTIFICADOR: {Nombre}\n";
        }
    }

    public class NodoExprParentesis : NodoExpr
    {
        public NodoExpr Expr { get; set; } = null!;
        public override string ToString() => $"({Expr})";
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + "PARENTESIS");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(Expr.ToTreeString(newIndent, true));
            return sb.ToString();
        }
    }

    // Nodos para funciones
    public class NodoDefinicionFuncion : NodoSentencia
    {
        public string Nombre { get; set; } = "";
        public List<string> Parametros { get; set; } = new List<string>();
        public List<NodoSentencia> Sentencias { get; set; } = new List<NodoSentencia>();
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + $"FUNCION: {Nombre}({string.Join(", ", Parametros)})");
            string newIndent = indent + (isLast ? "    " : "│   ");
            sb.Append(newIndent + "├── {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(newIndent + "│   ", i == Sentencias.Count - 1));
            sb.Append(newIndent + "└── }\n");
            return sb.ToString();
        }
    }

    public class NodoLlamadaFuncion : NodoExpr
    {
        public string Nombre { get; set; } = "";
        public List<NodoValor> Argumentos { get; set; } = new List<NodoValor>();
        public override string ToString() => $"{Nombre}({string.Join(", ", Argumentos)})";
        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(indent + (isLast ? "└── " : "├── ") + $"LLAMADA: {Nombre}");
            string newIndent = indent + (isLast ? "    " : "│   ");
            if (Argumentos.Count > 0)
            {
                sb.Append(newIndent + "├── argumentos:\n");
                for (int i = 0; i < Argumentos.Count; i++)
                    sb.Append(Argumentos[i].ToTreeString(newIndent + "│   ", i == Argumentos.Count - 1));
            }
            else
                sb.Append(newIndent + "└── (sin argumentos)\n");
            return sb.ToString();
        }
    }

    public class NodoExpresionSentencia : NodoSentencia
    {
        public NodoExpr Expresion { get; set; } = null!;
        public override string ToTreeString(string indent, bool isLast)
        {
            return Expresion.ToTreeString(indent, isLast);
        }
    }

    // ==================== PARSER CON RECUPERACIÓN DE ERRORES ====================
    public class Parser
    {
        private List<Token> _tokens;
        private int _pos;
        private List<ErrorInfo> _errores;
        private Dictionary<string, int> _funcionesDefinidas = new Dictionary<string, int>();
        private Token? Current => _pos < _tokens.Count ? _tokens[_pos] : null;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
            _errores = new List<ErrorInfo>();
            SkipComments();
        }

        private void SkipComments()
        {
            while (Current != null && Current.Type == TokenType.SIMBOLO && Current.Lexeme.StartsWith("//"))
                _pos++;
        }

        private void AddError(string message, Token? token)
        {
            _errores.Add(new ErrorInfo
            {
                Line = token?.Line ?? 1,
                Column = token?.Column ?? 1,
                Length = token?.Length ?? 1,
                Message = message,
                Type = ErrorType.Estructural
            });
        }

        private bool Eat(TokenType type, string? lexeme = null)
        {
            if (Current != null && Current.Type == type && (lexeme == null || Current.Lexeme == lexeme))
            {
                _pos++;
                SkipComments();
                return true;
            }
            string expected = lexeme != null ? $"'{lexeme}'" : type.ToString();
            string found = Current != null ? $"{Current.Type} ('{Current.Lexeme}')" : "EOF";
            AddError($"Se esperaba {expected} pero se encontró {found}", Current);
            if (Current != null) _pos++;
            SkipComments();
            return false;
        }

        private Token? Match(TokenType type, string? lexeme = null)
        {
            if (Current != null && Current.Type == type && (lexeme == null || Current.Lexeme == lexeme))
            {
                var token = Current;
                _pos++;
                SkipComments();
                return token;
            }
            string expected = lexeme != null ? $"'{lexeme}'" : type.ToString();
            string found = Current != null ? $"{Current.Type} ('{Current.Lexeme}')" : "EOF";
            AddError($"Se esperaba {expected} pero se encontró {found}", Current);
            return null;
        }

        private bool Peek(TokenType type, string? lexeme = null)
        {
            return Current != null && Current.Type == type && (lexeme == null || Current.Lexeme == lexeme);
        }

        public (NodoPrograma? Programa, List<ErrorInfo> Errores) ParsePrograma()
        {
            _pos = 0;
            _errores.Clear();
            _funcionesDefinidas.Clear();
            SkipComments();

            // Obligatorio: "inicio"
            if (!Eat(TokenType.PALABRA_RESERVADA, "inicio"))
            {
                AddError("Se esperaba 'inicio' al comienzo del programa.", Current);
                return (null, _errores);
            }

            var bloqueSimulacion = ParseBloqueSimulacion();
            if (bloqueSimulacion == null)
                return (null, _errores);

            // Obligatorio: "fin"
            if (!Eat(TokenType.PALABRA_RESERVADA, "fin"))
            {
                AddError("Se esperaba 'fin' al final del programa.", Current);
                return (null, _errores);
            }

            if (_errores.Count > 0)
                return (null, _errores);

            return (new NodoPrograma { BloqueSimulacion = bloqueSimulacion }, _errores);
        }

        private NodoBloqueSimulacion? ParseBloqueSimulacion()
        {
            if (!Eat(TokenType.PALABRA_RESERVADA, "simulacion")) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var sentencias = ParseListaSentenciasHastaCierre();
            Eat(TokenType.SIMBOLO, "}");
            return new NodoBloqueSimulacion { Sentencias = sentencias };
        }

        private List<NodoSentencia> ParseListaSentenciasHastaCierre()
        {
            var list = new List<NodoSentencia>();
            while (!Peek(TokenType.SIMBOLO, "}") && Current != null && Current.Type != TokenType.EOF)
            {
                var sentencia = ParseSentencia();
                if (sentencia != null)
                    list.Add(sentencia);
                else
                {
                    if (Current != null) _pos++;
                    SkipComments();
                }
            }
            return list;
        }

        private List<NodoSentencia> ParseListaSentenciasHastaCierreLlave()
        {
            var list = new List<NodoSentencia>();
            while (!Peek(TokenType.SIMBOLO, "}") && !Peek(TokenType.PALABRA_RESERVADA, "sino") && Current != null && Current.Type != TokenType.EOF)
            {
                var sentencia = ParseSentencia();
                if (sentencia != null)
                    list.Add(sentencia);
                else
                {
                    if (Current != null) _pos++;
                    SkipComments();
                }
            }
            return list;
        }

        private NodoSentencia? ParseSentencia()
        {
            if (Peek(TokenType.PALABRA_RESERVADA, "funcion"))
                return ParseDefinicionFuncion();
            if (Peek(TokenType.PALABRA_RESERVADA, "planeta"))
                return ParseBloquePlaneta();
            if (Peek(TokenType.PALABRA_RESERVADA, "atmosfera"))
                return ParseBloqueAtmosfera();
            if (Peek(TokenType.PALABRA_RESERVADA, "agua"))
                return ParseBloqueAgua();
            if (Peek(TokenType.PALABRA_RESERVADA, "vida"))
                return ParseBloqueVida();
            if (Peek(TokenType.PALABRA_RESERVADA, "si"))
                return ParseSi();
            if (Peek(TokenType.PALABRA_RESERVADA, "mientras"))
                return ParseMientras();
            if (Peek(TokenType.PALABRA_RESERVADA, "mostrar"))
                return ParseMostrar();
            if (Peek(TokenType.PALABRA_RESERVADA, "reporte"))
                return ParseReporte();
            if (Peek(TokenType.IDENTIFICADOR))
                return ParseAsignacionOLlamada();
            AddError("Se esperaba una sentencia válida", Current);
            return null;
        }

        private NodoBloquePlaneta? ParseBloquePlaneta()
        {
            if (!Eat(TokenType.PALABRA_RESERVADA, "planeta")) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var sentencias = ParseListaSentenciasHastaCierreLlave();
            Eat(TokenType.SIMBOLO, "}");
            return new NodoBloquePlaneta { Sentencias = sentencias };
        }

        private NodoBloqueAtmosfera? ParseBloqueAtmosfera()
        {
            if (!Eat(TokenType.PALABRA_RESERVADA, "atmosfera")) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var sentencias = ParseListaSentenciasHastaCierreLlave();
            Eat(TokenType.SIMBOLO, "}");
            return new NodoBloqueAtmosfera { Sentencias = sentencias };
        }

        private NodoBloqueAgua? ParseBloqueAgua()
        {
            if (!Eat(TokenType.PALABRA_RESERVADA, "agua")) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var sentencias = ParseListaSentenciasHastaCierreLlave();
            Eat(TokenType.SIMBOLO, "}");
            return new NodoBloqueAgua { Sentencias = sentencias };
        }

        private NodoBloqueVida? ParseBloqueVida()
        {
            if (!Eat(TokenType.PALABRA_RESERVADA, "vida")) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var sentencias = ParseListaSentenciasHastaCierreLlave();
            Eat(TokenType.SIMBOLO, "}");
            return new NodoBloqueVida { Sentencias = sentencias };
        }

        private NodoDefinicionFuncion? ParseDefinicionFuncion()
        {
            if (!Eat(TokenType.PALABRA_RESERVADA, "funcion")) return null;
            var nombreToken = Match(TokenType.IDENTIFICADOR);
            if (nombreToken == null) return null;
            if (!Eat(TokenType.SIMBOLO, "(")) return null;
            var parametros = new List<string>();
            if (!Peek(TokenType.SIMBOLO, ")"))
            {
                var primerParam = Match(TokenType.IDENTIFICADOR);
                if (primerParam != null) parametros.Add(primerParam.Lexeme);
                while (Peek(TokenType.SIMBOLO, ","))
                {
                    Eat(TokenType.SIMBOLO, ",");
                    var param = Match(TokenType.IDENTIFICADOR);
                    if (param != null) parametros.Add(param.Lexeme);
                }
            }
            if (!Eat(TokenType.SIMBOLO, ")")) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var sentencias = ParseListaSentenciasHastaCierreLlave();
            if (!Eat(TokenType.SIMBOLO, "}")) return null;

            // Registrar función
            if (_funcionesDefinidas.ContainsKey(nombreToken.Lexeme))
                AddError($"La función '{nombreToken.Lexeme}' ya está definida", nombreToken);
            else
                _funcionesDefinidas.Add(nombreToken.Lexeme, parametros.Count);

            return new NodoDefinicionFuncion
            {
                Nombre = nombreToken.Lexeme,
                Parametros = parametros,
                Sentencias = sentencias
            };
        }

        private NodoSentencia? ParseAsignacionOLlamada()
        {
            var idToken = Current;
            if (idToken == null) return null;

            Token? next = (_pos + 1 < _tokens.Count) ? _tokens[_pos + 1] : null;
            if (next != null && next.Type == TokenType.SIMBOLO && next.Lexeme == "(")
            {
                _pos++;
                SkipComments();
                var llamada = ParseLlamadaFuncion(idToken.Lexeme);
                if (llamada != null)
                {
                    Eat(TokenType.SIMBOLO, ";");
                    return new NodoExpresionSentencia { Expresion = llamada };
                }
                return null;
            }
            else
            {
                return ParseAsignacion();
            }
        }

        private NodoAsignacion? ParseAsignacion()
        {
            var idToken = Match(TokenType.IDENTIFICADOR);
            if (idToken == null) return null;
            if (!Eat(TokenType.OPERADOR, "=")) return null;
            var valor = ParseValor();
            if (valor == null) return null;
            Eat(TokenType.SIMBOLO, ";");
            return new NodoAsignacion { Identificador = idToken.Lexeme, Valor = valor };
        }

        private NodoMostrar? ParseMostrar()
        {
            if (!Eat(TokenType.PALABRA_RESERVADA, "mostrar")) return null;
            NodoValor? valor;
            if (Peek(TokenType.SIMBOLO, "("))
            {
                Eat(TokenType.SIMBOLO, "(");
                valor = ParseValor();
                Eat(TokenType.SIMBOLO, ")");
            }
            else
            {
                valor = ParseValor();
            }
            if (valor == null) return null;
            Eat(TokenType.SIMBOLO, ";");
            return new NodoMostrar { Valor = valor };
        }

        private NodoReporte? ParseReporte()
        {
            if (!Eat(TokenType.PALABRA_RESERVADA, "reporte")) return null;
            NodoValor? valor;
            if (Peek(TokenType.SIMBOLO, "("))
            {
                Eat(TokenType.SIMBOLO, "(");
                valor = ParseValor();
                Eat(TokenType.SIMBOLO, ")");
            }
            else
            {
                valor = ParseValor();
            }
            if (valor == null) return null;
            Eat(TokenType.SIMBOLO, ";");
            return new NodoReporte { Valor = valor };
        }

        private NodoIf? ParseSi()
        {
            if (!Eat(TokenType.PALABRA_RESERVADA, "si")) return null;
            if (!Eat(TokenType.SIMBOLO, "(")) return null;
            var cond = ParseCondicion();
            if (cond == null) return null;
            if (!Eat(TokenType.SIMBOLO, ")")) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var thenSent = ParseListaSentenciasHastaCierreLlave();
            if (!Eat(TokenType.SIMBOLO, "}")) return null;
            var elseSent = new List<NodoSentencia>();
            if (Peek(TokenType.PALABRA_RESERVADA, "sino"))
            {
                Eat(TokenType.PALABRA_RESERVADA, "sino");
                if (Eat(TokenType.SIMBOLO, "{"))
                {
                    elseSent = ParseListaSentenciasHastaCierreLlave();
                    Eat(TokenType.SIMBOLO, "}");
                }
            }
            return new NodoIf { Condicion = cond, ThenSentencias = thenSent, ElseSentencias = elseSent };
        }

        private NodoWhile? ParseMientras()
        {
            if (!Eat(TokenType.PALABRA_RESERVADA, "mientras")) return null;
            if (!Eat(TokenType.SIMBOLO, "(")) return null;
            var cond = ParseCondicion();
            if (cond == null) return null;
            if (!Eat(TokenType.SIMBOLO, ")")) return null;
            if (!Eat(TokenType.SIMBOLO, "{")) return null;
            var sentencias = ParseListaSentenciasHastaCierreLlave();
            if (!Eat(TokenType.SIMBOLO, "}")) return null;
            return new NodoWhile { Condicion = cond, Sentencias = sentencias };
        }

        private NodoCondicion? ParseCondicion()
        {
            var leftVal = ParseValor();
            if (leftVal == null) return null;
            string? op = null;
            NodoValor? rightVal = null;
            if (IsRelationalOperator())
            {
                op = Current!.Lexeme;
                _pos++;
                SkipComments();
                rightVal = ParseValor();
                if (rightVal == null) return null;
            }
            var cond = new NodoCondicion { Izquierda = leftVal, Operador = op, Derecha = rightVal };
            while (Peek(TokenType.PALABRA_RESERVADA, "y") || Peek(TokenType.PALABRA_RESERVADA, "o"))
            {
                string logicalOp = Current!.Lexeme;
                _pos++;
                SkipComments();
                var nextCond = ParseCondicion();
                if (nextCond != null)
                    cond.OperadoresLogicos.Add((logicalOp, nextCond));
            }
            return cond;
        }

        private bool IsRelationalOperator()
        {
            return Current != null && Current.Type == TokenType.OPERADOR &&
                   (Current.Lexeme == "<" || Current.Lexeme == ">" || Current.Lexeme == "<=" ||
                    Current.Lexeme == ">=" || Current.Lexeme == "==" || Current.Lexeme == "!=");
        }

        private NodoValor? ParseValor()
        {
            if (Peek(TokenType.PALABRA_RESERVADA, "verdadero") || Peek(TokenType.PALABRA_RESERVADA, "falso"))
            {
                var token = Match(TokenType.PALABRA_RESERVADA);
                if (token == null) return null;
                return new NodoBooleano { Valor = token.Lexeme == "verdadero" };
            }
            if (Peek(TokenType.PALABRA_RESERVADA, "nulo"))
            {
                if (!Eat(TokenType.PALABRA_RESERVADA, "nulo")) return null;
                return new NodoNulo();
            }
            if (Peek(TokenType.CADENA))
            {
                var token = Match(TokenType.CADENA);
                if (token == null) return null;
                string text = token.Lexeme.Substring(1, token.Lexeme.Length - 2);
                return new NodoTexto { Texto = text };
            }
            if (Peek(TokenType.SIMBOLO, "["))
                return ParseLista();
            return ParseCantidad();
        }

        private NodoLista? ParseLista()
        {
            if (!Eat(TokenType.SIMBOLO, "[")) return null;
            var valores = new List<NodoValor>();
            if (!Peek(TokenType.SIMBOLO, "]"))
            {
                var primero = ParseValor();
                if (primero != null) valores.Add(primero);
                while (Peek(TokenType.SIMBOLO, ","))
                {
                    Eat(TokenType.SIMBOLO, ",");
                    var sig = ParseValor();
                    if (sig != null) valores.Add(sig);
                }
            }
            Eat(TokenType.SIMBOLO, "]");
            return new NodoLista { Valores = valores };
        }

        private NodoCantidad? ParseCantidad()
        {
            var expr = ParseExpr();
            if (expr == null) return null;
            string? unidad = null;
            if (IsUnidad())
            {
                unidad = Current!.Lexeme;
                _pos++;
                SkipComments();
            }
            return new NodoCantidad { Expr = expr, Unidad = unidad };
        }

        private bool IsUnidad()
        {
            if (Current == null || Current.Type != TokenType.IDENTIFICADOR) return false;
            string[] unidades = { "km", "m", "g", "kg", "atm", "ppm", "Sv", "%", "°C", "h", "s" };
            return Array.Exists(unidades, u => u == Current.Lexeme);
        }

        private NodoExpr? ParseExpr()
        {
            var left = ParseTerm();
            if (left == null) return null;
            while (Peek(TokenType.OPERADOR, "+") || Peek(TokenType.OPERADOR, "-"))
            {
                string op = Current!.Lexeme;
                _pos++;
                SkipComments();
                var right = ParseTerm();
                if (right == null) return null;
                left = new NodoExprBinaria { Izquierda = left, Operador = op, Derecha = right };
            }
            return left;
        }

        private NodoExpr? ParseTerm()
        {
            var left = ParsePotencia();
            if (left == null) return null;
            while (Peek(TokenType.OPERADOR, "*") || Peek(TokenType.OPERADOR, "/"))
            {
                string op = Current!.Lexeme;
                _pos++;
                SkipComments();
                var right = ParsePotencia();
                if (right == null) return null;
                left = new NodoExprBinaria { Izquierda = left, Operador = op, Derecha = right };
            }
            return left;
        }

        private NodoExpr? ParsePotencia()
        {
            var left = ParseFactor();
            if (left == null) return null;
            if (Peek(TokenType.OPERADOR, "^"))
            {
                _pos++;
                SkipComments();
                var right = ParsePotencia();
                if (right == null) return null;
                return new NodoExprPotencia { Base = left, Exponente = right };
            }
            return left;
        }

        private NodoExpr? ParseFactor()
        {
            if (Peek(TokenType.OPERADOR, "-"))
            {
                _pos++;
                SkipComments();
                var operand = ParseFactor();
                if (operand == null) return null;
                return new NodoExprBinaria { Izquierda = new NodoExprNumero { Valor = "0" }, Operador = "-", Derecha = operand };
            }
            if (Peek(TokenType.SIMBOLO, "("))
            {
                if (!Eat(TokenType.SIMBOLO, "(")) return null;
                var expr = ParseExpr();
                if (expr == null) return null;
                Eat(TokenType.SIMBOLO, ")");
                return new NodoExprParentesis { Expr = expr };
            }
            if (Peek(TokenType.NUMERO))
            {
                var token = Match(TokenType.NUMERO);
                if (token == null) return null;
                return new NodoExprNumero { Valor = token.Lexeme };
            }
            if (Peek(TokenType.IDENTIFICADOR))
            {
                var idToken = Current;
                Token? next = (_pos + 1 < _tokens.Count) ? _tokens[_pos + 1] : null;
                if (next != null && next.Type == TokenType.SIMBOLO && next.Lexeme == "(")
                {
                    _pos++;
                    SkipComments();
                    var llamada = ParseLlamadaFuncion(idToken!.Lexeme);
                    if (llamada != null) return llamada;
                    return null;
                }
                else
                {
                    var token = Match(TokenType.IDENTIFICADOR);
                    if (token == null) return null;
                    return new NodoExprIdentificador { Nombre = token.Lexeme };
                }
            }
            AddError("Se esperaba número, identificador, '(' o llamada a función", Current);
            return null;
        }

        private NodoLlamadaFuncion? ParseLlamadaFuncion(string nombre)
        {
            if (!Eat(TokenType.SIMBOLO, "(")) return null;
            var argumentos = new List<NodoValor>();
            if (!Peek(TokenType.SIMBOLO, ")"))
            {
                var primerArg = ParseValor();
                if (primerArg != null) argumentos.Add(primerArg);
                while (Peek(TokenType.SIMBOLO, ","))
                {
                    Eat(TokenType.SIMBOLO, ",");
                    var arg = ParseValor();
                    if (arg != null) argumentos.Add(arg);
                }
            }
            if (!Eat(TokenType.SIMBOLO, ")")) return null;

            // Validar número de argumentos contra definición
            if (_funcionesDefinidas.TryGetValue(nombre, out int numParams))
            {
                if (argumentos.Count != numParams)
                    AddError($"La función '{nombre}' espera {numParams} argumentos pero se recibieron {argumentos.Count}", Current);
            }
            else
            {
                AddError($"Función '{nombre}' no definida", Current);
            }

            return new NodoLlamadaFuncion { Nombre = nombre, Argumentos = argumentos };
        }
    }
}