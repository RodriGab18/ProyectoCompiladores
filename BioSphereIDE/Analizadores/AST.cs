using System.Collections.Generic;
using System.Text;
using BioSphereIDE.Core;

namespace BioSphereIDE.Analizadores
{
    // ════════════════════════════════════════════════════════════════════════════
    //  NODOS DEL ÁRBOL SINTÁCTICO ABSTRACTO (AST)
    //
    //  Diseño:
    //   • NodoAST  ← base de todo
    //   • NodoValor ← cualquier expresión evaluable
    //   • NodoExpr  ← subconjunto: expresiones numéricas/lógicas compuestas
    //   • NodoSentencia ← instrucciones ejecutables
    //
    //  Cada nodo almacena SourceToken para reportar errores con línea/columna
    //  reales en lugar del placeholder "línea 1".
    // ════════════════════════════════════════════════════════════════════════════

    // ─── Base ──────────────────────────────────────────────────────────────────
    public abstract class NodoAST
    {
        /// Token del código fuente que originó este nodo (para reporte de errores).
        public Token? SourceToken { get; set; }

        public abstract string ToTreeString(string indent, bool isLast);

        protected static string Pfx(string indent, bool isLast) =>
            indent + (isLast ? "└── " : "├── ");

        protected static string NextIndent(string indent, bool isLast) =>
            indent + (isLast ? "    " : "│   ");
    }

    // ─── Programa ──────────────────────────────────────────────────────────────
    public class NodoPrograma : NodoAST
    {
        public NodoBloqueSimulacion BloqueSimulacion { get; set; } = null!;

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "PROGRAMA");
            var ni = NextIndent(indent, isLast);
            sb.Append(ni + "├── inicio\n");
            sb.Append(BloqueSimulacion.ToTreeString(ni, false));
            sb.Append(ni + "└── fin\n");
            return sb.ToString();
        }
    }

    // ─── Bloque de simulación ──────────────────────────────────────────────────
    public class NodoBloqueSimulacion : NodoAST
    {
        public List<NodoSentencia> Sentencias { get; set; } = new();

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "BLOQUE_SIMULACION");
            var ni = NextIndent(indent, isLast);
            sb.Append(ni + "├── simulacion {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(ni + "│   ", i == Sentencias.Count - 1));
            sb.Append(ni + "└── }\n");
            return sb.ToString();
        }
    }

    // ─── Sentencia base ────────────────────────────────────────────────────────
    public abstract class NodoSentencia : NodoAST { }

    // ─── Bloques temáticos ─────────────────────────────────────────────────────
    public class NodoBloquePlaneta : NodoSentencia
    {
        public List<NodoSentencia> Sentencias { get; set; } = new();

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "BLOQUE_PLANETA");
            var ni = NextIndent(indent, isLast);
            sb.Append(ni + "├── planeta {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(ni + "│   ", i == Sentencias.Count - 1));
            sb.Append(ni + "└── }\n");
            return sb.ToString();
        }
    }

    public class NodoBloqueAtmosfera : NodoSentencia
    {
        public List<NodoSentencia> Sentencias { get; set; } = new();

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "BLOQUE_ATMOSFERA");
            var ni = NextIndent(indent, isLast);
            sb.Append(ni + "├── atmosfera {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(ni + "│   ", i == Sentencias.Count - 1));
            sb.Append(ni + "└── }\n");
            return sb.ToString();
        }
    }

    public class NodoBloqueAgua : NodoSentencia
    {
        public List<NodoSentencia> Sentencias { get; set; } = new();

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "BLOQUE_AGUA");
            var ni = NextIndent(indent, isLast);
            sb.Append(ni + "├── agua {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(ni + "│   ", i == Sentencias.Count - 1));
            sb.Append(ni + "└── }\n");
            return sb.ToString();
        }
    }

    public class NodoBloqueVida : NodoSentencia
    {
        public List<NodoSentencia> Sentencias { get; set; } = new();

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "BLOQUE_VIDA");
            var ni = NextIndent(indent, isLast);
            sb.Append(ni + "├── vida {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(ni + "│   ", i == Sentencias.Count - 1));
            sb.Append(ni + "└── }\n");
            return sb.ToString();
        }
    }

    public class NodoOrbitaYEscala : NodoSentencia
    {
        public List<NodoSentencia> Instrucciones { get; set; } = new();

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "BLOQUE_ORBITA_Y_ESCALA");
            var ni = NextIndent(indent, isLast);
            sb.Append(ni + "├── orbita_y_escala {\n");
            for (int i = 0; i < Instrucciones.Count; i++)
                sb.Append(Instrucciones[i].ToTreeString(ni + "│   ", i == Instrucciones.Count - 1));
            sb.Append(ni + "└── }\n");
            return sb.ToString();
        }
    }

    // ─── Sentencias concretas ──────────────────────────────────────────────────

    public class NodoAsignacion : NodoSentencia
    {
        public string    Identificador { get; set; } = "";
        public NodoValor Valor         { get; set; } = null!;

        public override string ToTreeString(string indent, bool isLast) =>
            Pfx(indent, isLast) + $"ASIGNACION: {Identificador} = {Valor}\n";
    }

    public class NodoMostrar : NodoSentencia
    {
        public NodoValor Valor { get; set; } = null!;

        public override string ToTreeString(string indent, bool isLast) =>
            Pfx(indent, isLast) + $"MOSTRAR: {Valor}\n";
    }

    public class NodoReporte : NodoSentencia
    {
        public NodoValor Valor { get; set; } = null!;

        public override string ToTreeString(string indent, bool isLast) =>
            Pfx(indent, isLast) + $"REPORTE: {Valor}\n";
    }

    /// <summary>continuar; — equivalente a "continue" en otros lenguajes.</summary>
    public class NodoContinuar : NodoSentencia
    {
        public override string ToTreeString(string indent, bool isLast) =>
            Pfx(indent, isLast) + "CONTINUAR\n";
    }

    /// <summary>romper; — equivalente a "break" en otros lenguajes.</summary>
    public class NodoRomper : NodoSentencia
    {
        public override string ToTreeString(string indent, bool isLast) =>
            Pfx(indent, isLast) + "ROMPER\n";
    }

    public class NodoExpresionSentencia : NodoSentencia
    {
        public NodoExpr Expresion { get; set; } = null!;

        public override string ToTreeString(string indent, bool isLast) =>
            Expresion.ToTreeString(indent, isLast);
    }

    // ─── Condicional: condición es cualquier NodoValor (expresión booleana) ─────
    public class NodoIf : NodoSentencia
    {
        public NodoValor           Condicion      { get; set; } = null!;
        public List<NodoSentencia> ThenSentencias { get; set; } = new();
        public List<NodoSentencia> ElseSentencias { get; set; } = new();

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "SI");
            var ni = NextIndent(indent, isLast);
            sb.Append(ni + "├── condicion:\n");
            sb.Append(Condicion.ToTreeString(ni + "│   ", true));
            sb.Append(ni + "├── entonces {\n");
            for (int i = 0; i < ThenSentencias.Count; i++)
                sb.Append(ThenSentencias[i].ToTreeString(ni + "│   ", i == ThenSentencias.Count - 1));
            sb.Append(ni + "│   └── }\n");
            if (ElseSentencias.Count > 0)
            {
                sb.Append(ni + "└── sino {\n");
                for (int i = 0; i < ElseSentencias.Count; i++)
                    sb.Append(ElseSentencias[i].ToTreeString(ni + "    ", i == ElseSentencias.Count - 1));
                sb.Append(ni + "    └── }\n");
            }
            return sb.ToString();
        }
    }

    // ─── Bucle mientras ────────────────────────────────────────────────────────
    public class NodoWhile : NodoSentencia
    {
        public NodoValor           Condicion  { get; set; } = null!;
        public List<NodoSentencia> Sentencias { get; set; } = new();

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "MIENTRAS");
            var ni = NextIndent(indent, isLast);
            sb.Append(ni + "├── condicion:\n");
            sb.Append(Condicion.ToTreeString(ni + "│   ", true));
            sb.Append(ni + "└── cuerpo {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(ni + "    ", i == Sentencias.Count - 1));
            sb.Append(ni + "    └── }\n");
            return sb.ToString();
        }
    }

    // ─── Función ───────────────────────────────────────────────────────────────
    public class NodoDefinicionFuncion : NodoSentencia
    {
        public string              Nombre     { get; set; } = "";
        public List<string>        Parametros { get; set; } = new();
        public List<NodoSentencia> Sentencias { get; set; } = new();

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + $"FUNCION: {Nombre}({string.Join(", ", Parametros)})");
            var ni = NextIndent(indent, isLast);
            sb.Append(ni + "└── cuerpo {\n");
            for (int i = 0; i < Sentencias.Count; i++)
                sb.Append(Sentencias[i].ToTreeString(ni + "    ", i == Sentencias.Count - 1));
            sb.Append(ni + "    └── }\n");
            return sb.ToString();
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    //  VALORES Y EXPRESIONES
    //
    //  Jerarquía:
    //   NodoValor (abstracto)
    //   └── NodoExpr (abstracto) — toda expresión evaluable
    //       ├── NodoExprBinaria    a OP b     (aritmético, relacional, lógico y/o)
    //       ├── NodoExprUnaria     -a
    //       ├── NodoExprPotencia   a ^ b
    //       ├── NodoExprNumero     5, 3.14, 5.97e24
    //       ├── NodoExprIdentificador  varName
    //       ├── NodoExprParentesis  (expr)
    //       ├── NodoLlamadaFuncion  f(args)
    //       ├── NodoBooleano        verdadero / falso
    //       ├── NodoTexto           "cadena"
    //       ├── NodoNulo            nulo
    //       ├── NodoLista           [1, 2, 3]
    //       └── NodoCantidad        5.97 kg   (número + unidad física)
    // ════════════════════════════════════════════════════════════════════════════

    public abstract class NodoValor : NodoAST
    {
        public abstract override string ToString();
    }

    // Toda expresión ES un valor
    public abstract class NodoExpr : NodoValor { }

    // ─── Expresión binaria (arithmetic, relational AND logical y/o) ────────────
    public class NodoExprBinaria : NodoExpr
    {
        public NodoExpr Izquierda { get; set; } = null!;
        public string   Operador  { get; set; } = "";
        public NodoExpr Derecha   { get; set; } = null!;

        public override string ToString() => $"({Izquierda} {Operador} {Derecha})";

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + $"EXPR_BINARIA: {Operador}");
            var ni = NextIndent(indent, isLast);
            sb.Append(Izquierda.ToTreeString(ni, false));
            sb.Append(Derecha.ToTreeString(ni, true));
            return sb.ToString();
        }
    }

    // ─── Expresión unaria  (-expr) ─────────────────────────────────────────────
    public class NodoExprUnaria : NodoExpr
    {
        public string   Operador { get; set; } = "";
        public NodoExpr Operando { get; set; } = null!;

        public override string ToString() => $"({Operador}{Operando})";

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + $"EXPR_UNARIA: {Operador}");
            var ni = NextIndent(indent, isLast);
            sb.Append(Operando.ToTreeString(ni, true));
            return sb.ToString();
        }
    }

    // ─── Potencia (a ^ b, derecha-asociativa) ─────────────────────────────────
    public class NodoExprPotencia : NodoExpr
    {
        public NodoExpr Base      { get; set; } = null!;
        public NodoExpr Exponente { get; set; } = null!;

        public override string ToString() => $"({Base}^{Exponente})";

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "POTENCIA");
            var ni = NextIndent(indent, isLast);
            sb.Append(Base.ToTreeString(ni, false));
            sb.Append(Exponente.ToTreeString(ni, true));
            return sb.ToString();
        }
    }

    // ─── Literal numérico ──────────────────────────────────────────────────────
    public class NodoExprNumero : NodoExpr
    {
        public string Valor { get; set; } = "";

        public override string ToString() => Valor;

        public override string ToTreeString(string indent, bool isLast) =>
            Pfx(indent, isLast) + $"NUMERO: {Valor}\n";
    }

    // ─── Referencia a identificador ────────────────────────────────────────────
    public class NodoExprIdentificador : NodoExpr
    {
        public string Nombre { get; set; } = "";

        public override string ToString() => Nombre;

        public override string ToTreeString(string indent, bool isLast) =>
            Pfx(indent, isLast) + $"IDENTIFICADOR: {Nombre}\n";
    }

    // ─── Expresión entre paréntesis ────────────────────────────────────────────
    public class NodoExprParentesis : NodoExpr
    {
        public NodoExpr Expr { get; set; } = null!;

        public override string ToString() => $"({Expr})";

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "PARENTESIS");
            var ni = NextIndent(indent, isLast);
            sb.Append(Expr.ToTreeString(ni, true));
            return sb.ToString();
        }
    }

    // ─── Llamada a función ─────────────────────────────────────────────────────
    public class NodoLlamadaFuncion : NodoExpr
    {
        public string          Nombre     { get; set; } = "";
        public List<NodoValor> Argumentos { get; set; } = new();

        public override string ToString() => $"{Nombre}({string.Join(", ", Argumentos)})";

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + $"LLAMADA: {Nombre}");
            var ni = NextIndent(indent, isLast);
            if (Argumentos.Count > 0)
            {
                sb.Append(ni + "└── argumentos:\n");
                for (int i = 0; i < Argumentos.Count; i++)
                    sb.Append(Argumentos[i].ToTreeString(ni + "    ", i == Argumentos.Count - 1));
            }
            else
                sb.Append(ni + "└── (sin argumentos)\n");
            return sb.ToString();
        }
    }

    // ─── Literal booleano ──────────────────────────────────────────────────────
    public class NodoBooleano : NodoExpr
    {
        public bool Valor { get; set; }

        public override string ToString() => Valor ? "verdadero" : "falso";

        public override string ToTreeString(string indent, bool isLast) =>
            Pfx(indent, isLast) + $"BOOLEANO: {this}\n";
    }

    // ─── Literal de texto ──────────────────────────────────────────────────────
    public class NodoTexto : NodoExpr
    {
        public string Texto { get; set; } = "";

        public override string ToString() => $"\"{Texto}\"";

        public override string ToTreeString(string indent, bool isLast) =>
            Pfx(indent, isLast) + $"TEXTO: {this}\n";
    }

    // ─── Valor nulo ────────────────────────────────────────────────────────────
    public class NodoNulo : NodoExpr
    {
        public override string ToString() => "nulo";

        public override string ToTreeString(string indent, bool isLast) =>
            Pfx(indent, isLast) + "NULO\n";
    }

    // ─── Lista literal [v1, v2, ...] ───────────────────────────────────────────
    public class NodoLista : NodoExpr
    {
        public List<NodoValor> Valores { get; set; } = new();

        public override string ToString() => "[" + string.Join(", ", Valores) + "]";

        public override string ToTreeString(string indent, bool isLast)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Pfx(indent, isLast) + "LISTA");
            var ni = NextIndent(indent, isLast);
            for (int i = 0; i < Valores.Count; i++)
                sb.Append(Valores[i].ToTreeString(ni, i == Valores.Count - 1));
            return sb.ToString();
        }
    }

    // ─── Cantidad física (número + unidad opcional) ────────────────────────────
    public class NodoCantidad : NodoExpr
    {
        public NodoExpr Expr   { get; set; } = null!;
        public string?  Unidad { get; set; }

        public override string ToString() =>
            Expr?.ToString() + (Unidad != null ? " " + Unidad : "");

        public override string ToTreeString(string indent, bool isLast) =>
            Pfx(indent, isLast) + $"CANTIDAD: {this}\n";
    }
}
