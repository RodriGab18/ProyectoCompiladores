using System.Text.RegularExpressions;

namespace BioSphereIDE.Core
{
    // ─── Tipos de token ─────────────────────────────────────────────────────────
    public enum TokenType
    {
        PALABRA_RESERVADA,
        IDENTIFICADOR,
        NUMERO,
        OPERADOR,
        SIMBOLO,
        CADENA,
        UNIDAD,        // unidades físicas: km, kg, °C, atm, ppm, etc.
        ERROR_LEXICO,
        EOF
    }

    // ─── Token ──────────────────────────────────────────────────────────────────
    public sealed class Token
    {
        public TokenType Type   { get; }
        public string    Lexeme { get; }
        public int       Line   { get; }
        public int       Column { get; }
        public int       Length => Lexeme.Length;

        public Token(TokenType type, string lexeme, int line, int column)
        {
            Type = type; Lexeme = lexeme; Line = line; Column = column;
        }

        public override string ToString() => $"[{Type} '{Lexeme}' L{Line}:C{Column}]";
    }

    // ─── Definición de token (scanner por regex) ────────────────────────────────
    public sealed class TokenDefinition
    {
        public TokenType Type  { get; }
        public Regex     Regex { get; }

        public TokenDefinition(TokenType type, string pattern)
        {
            Type  = type;
            Regex = new Regex(@"\G(?:" + pattern + ")",
                              RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }
    }

    // ─── Sistema de errores ─────────────────────────────────────────────────────
    public enum ErrorType { Lexico, Estructural, Semantico }
    public enum Severity  { Error, Warning, Info }

    public sealed class ErrorInfo
    {
        public ErrorType Type       { get; set; }
        public Severity  Severity   { get; set; } = Severity.Error;
        public int       Line       { get; set; }
        public int       Column     { get; set; }
        public int       Length     { get; set; } = 1;
        public string    Code       { get; set; } = "";   // e.g. "LEX-001", "SEM-014"
        public string    Message    { get; set; } = "";
        public string    Suggestion { get; set; } = "";   // sugerencia accionable al usuario

        public string FullMessage =>
            (string.IsNullOrEmpty(Code) ? "" : $"[{Code}] ") +
            Message +
            (string.IsNullOrEmpty(Suggestion) ? "" : $"\n  → Sugerencia: {Suggestion}");

        public override string ToString() =>
            $"[{Severity}/{Type} {Code}] L{Line}:C{Column} — {Message}";
    }

    // ─── Símbolo (compartido entre analizador semántico y visor de tabla) ────────
    public sealed class Symbol
    {
        public string  Nombre    { get; set; } = "";
        public string  Tipo      { get; set; } = "";   // "numero","texto","booleano","lista","nulo","funcion"
        public object? Valor     { get; set; }
        public string? Unidad    { get; set; }
        public int     Linea     { get; set; }
        public int     Columna   { get; set; }
        public string  Ambito    { get; set; } = "";
        public bool    Usado     { get; set; }         // para warnings de variable no usada
        public int     NumParams { get; set; }         // para funciones: cantidad de parámetros

        public override string ToString() =>
            $"{Nombre} : {Tipo}" +
            (Unidad != null ? $" [{Unidad}]" : "") +
            $" = {Valor}  ({Ambito}, L{Linea})";
    }
}
