using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BioSphereIDE.Core;

namespace BioSphereIDE.Analizadores
{
    public sealed class Lexer
    {
        private readonly string _source;
        private int _pos, _line, _col;
        private readonly List<TokenDefinition> _defs;

        public List<Token>     Tokens  { get; } = new();
        public List<ErrorInfo> Errores { get; } = new();

        // Keywords classified as PALABRA_RESERVADA during post-processing after identifier match
        private static readonly HashSet<string> _keywords = new(StringComparer.Ordinal)
        {
            "simulacion", "planeta", "atmosfera", "agua", "vida", "orbita_y_escala", "inicio", "fin",
            "si", "sino", "mientras", "iterar", "continuar", "romper", "reporte",
            "mostrar", "verdadero", "falso", "nulo", "y", "o", "funcion"
        };

        public Lexer(string source) { _source = source ?? ""; _defs = BuildDefinitions(); }

        // ────────────────────────────────────────────────────────────────────────
        // Token patterns  (ORDER MATTERS — first match wins)
        // ────────────────────────────────────────────────────────────────────────
        private static List<TokenDefinition> BuildDefinitions() => new()
        {
            // ERRORS must come before their valid counterparts
            new(TokenType.ERROR_LEXICO, @"\G\d+[a-zA-Z_][a-zA-Z0-9_]*"),   // 123abc
            new(TokenType.ERROR_LEXICO, @"\G[@#\$%&!?\|\\~`]"),             // illegal chars
            new(TokenType.ERROR_LEXICO, "\\G\"[^\"\r\n]*$"),                  // unclosed string

            // Numbers — scientific first, then float, then integer
            new(TokenType.NUMERO, @"\G\d+\.?\d*[eE][+\-]?\d+"),
            new(TokenType.NUMERO, @"\G\d+\.\d+"),
            new(TokenType.NUMERO, @"\G\d+"),

            // Operators — multi-char before single-char
            new(TokenType.OPERADOR, @"\G(?:<=|>=|==|!=|<|>|\+|-|\*|/|=|\^)"),

            // Strings with basic escape sequences
            new(TokenType.CADENA, "\\G\"(?:[^\"\\\\\\r\\n]|\\\\.)*\""),

            // Symbols
            new(TokenType.SIMBOLO, @"\G(?:\(|\)|\{|\}|\[|\]|;|,|\.|°)"),

            // Identifiers — keywords promoted to PALABRA_RESERVADA in Tokenize()
            new(TokenType.IDENTIFICADOR, @"\G[a-zA-ZáéíóúÁÉÍÓÚñÑ_][a-zA-ZáéíóúÁÉÍÓÚñÑ0-9_]*"),
        };

        // ────────────────────────────────────────────────────────────────────────
        public List<Token> Tokenize()
        {
            Tokens.Clear();
            Errores.Clear();
            _pos = 0; _line = 1; _col = 1;

            while (_pos < _source.Length)
            {
                char c = _source[_pos];

                // Whitespace
                if (char.IsWhiteSpace(c)) { Advance(c); continue; }

                // Single-line comment  //
                if (c == '/' && Peek1() == '/')
                {
                    while (_pos < _source.Length && _source[_pos] != '\n')
                        Advance(_source[_pos]);
                    continue;
                }

                // Block comment  /* ... */
                if (c == '/' && Peek1() == '*') { ParseBlockComment(); continue; }

                bool matched = false;
                int tokLine = _line, tokCol = _col;

                foreach (var def in _defs)
                {
                    Match m = def.Regex.Match(_source, _pos);
                    if (!m.Success) continue;

                    if (def.Type == TokenType.ERROR_LEXICO)
                        EmitLexError(m.Value, tokLine, tokCol);

                    var tokType = def.Type;
                    if (tokType == TokenType.IDENTIFICADOR && _keywords.Contains(m.Value))
                        tokType = TokenType.PALABRA_RESERVADA;

                    Tokens.Add(new Token(tokType, m.Value, tokLine, tokCol));
                    foreach (char ch in m.Value) Advance(ch);
                    matched = true;
                    break;
                }

                if (!matched)
                {
                    char bad = _source[_pos];
                    Errores.Add(new ErrorInfo
                    {
                        Type = ErrorType.Lexico, Code = "LEX-001",
                        Line = _line, Column = _col, Length = 1,
                        Message    = $"Carácter ilegal '{bad}' (U+{(int)bad:X4}) no permitido en ASTRA.",
                        Suggestion = "Elimine o reemplace los caracteres especiales no permitidos."
                    });
                    Tokens.Add(new Token(TokenType.ERROR_LEXICO, bad.ToString(), _line, _col));
                    Advance(bad);
                }
            }

            Tokens.Add(new Token(TokenType.EOF, "EOF", _line, _col));
            return Tokens;
        }

        // ────────────────────────────────────────────────────────────────────────
        private void ParseBlockComment()
        {
            int startLine = _line, startCol = _col;
            Advance('/'); Advance('*');
            while (_pos < _source.Length)
            {
                if (_source[_pos] == '*' && Peek1() == '/') { Advance('*'); Advance('/'); return; }
                Advance(_source[_pos]);
            }
            Errores.Add(new ErrorInfo
            {
                Type = ErrorType.Lexico, Code = "LEX-005",
                Line = startLine, Column = startCol, Length = 2,
                Message    = $"Comentario de bloque sin cerrar (iniciado en línea {startLine}, columna {startCol}).",
                Suggestion = "Agregue '*/' para cerrar el comentario."
            });
        }

        private void EmitLexError(string value, int line, int col)
        {
            string code, msg, sugg;

            if (Regex.IsMatch(value, @"^\d+[a-zA-Z_]"))
            {
                code = "LEX-002";
                msg  = $"Identificador inválido '{value}': los identificadores no pueden comenzar con un dígito.";
                sugg = $"Use un prefijo alfabético, p. ej. 'n{value}' o 'var{value[0]}…'.";
            }
            else if (value.StartsWith("\""))
            {
                code = "LEX-003";
                string preview = value.Length > 25 ? value.Substring(0, 22) + "..." : value;
                msg  = $"Cadena de texto sin cerrar: {preview}";
                sugg = "Agregue '\"' al final de la cadena de texto.";
            }
            else
            {
                code = "LEX-001";
                msg  = $"Carácter ilegal '{value}' no permitido en ASTRA.";
                sugg = "Elimine el carácter especial o use una cadena de texto para incluirlo.";
            }

            Errores.Add(new ErrorInfo
            {
                Type = ErrorType.Lexico, Code = code,
                Line = line, Column = col, Length = value.Length,
                Message = msg, Suggestion = sugg
            });
        }

        private char Peek1() => _pos + 1 < _source.Length ? _source[_pos + 1] : '\0';

        private void Advance(char c)
        {
            _pos++;
            if      (c == '\n') { _line++; _col = 1; }
            else if (c == '\t') { _col += 4; }
            else                { _col++; }
        }
    }
}
