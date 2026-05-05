using System.Text.RegularExpressions;

namespace BioSphereIDE.Core
{
          public enum TokenType { PALABRA_RESERVADA, IDENTIFICADOR, NUMERO, OPERADOR, SIMBOLO, CADENA, ERROR_LEXICO, EOF }

          public class Token
          {
                    public TokenType Type { get; }
                    public string Lexeme { get; }
                    public int Line { get; }
                    public int Column { get; }
                    public int Length => Lexeme.Length;

                    public Token(TokenType type, string lexeme, int line, int column)
                    {
                              Type = type; Lexeme = lexeme; Line = line; Column = column;
                    }
          }

          public class TokenDefinition
          {
                    public TokenType Type { get; }
                    public Regex Regex { get; }
                    public TokenDefinition(TokenType type, string pattern)
                    {
                              Regex = new Regex(@"\G(?:" + pattern + ")", RegexOptions.Compiled);
                              Type = type;
                    }
          }

          public enum ErrorType { Lexico, Estructural, Semantico }

          public class ErrorInfo
          {
                    public int Line { get; set; }
                    public int Column { get; set; }
                    public int Length { get; set; }
                    public string Message { get; set; } = "";
                    public ErrorType Type { get; set; }
          }
}