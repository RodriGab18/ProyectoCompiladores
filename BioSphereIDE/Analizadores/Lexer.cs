using System.Collections.Generic;
using System.Text.RegularExpressions;
using BioSphereIDE.Core;

namespace BioSphereIDE.Analizadores
{
          public class Lexer
          {
                    private readonly string _sourceCode;
                    private int _position = 0, _line = 1, _column = 1;
                    private readonly List<TokenDefinition> _tokenDefinitions;
                    public List<ErrorInfo> Errores { get; } = new List<ErrorInfo>();
                    public List<Token> Tokens { get; private set; } = new List<Token>();

                    public Lexer(string sourceCode)
                    {
                              _sourceCode = sourceCode;
                              _tokenDefinitions = new List<TokenDefinition>
                              {
                                        // Palabras reservadas (estructurales y de control)
                                        new TokenDefinition(TokenType.PALABRA_RESERVADA, @"\b(simulacion|planeta|atmosfera|agua|vida|inicio|fin|si|sino|mientras|iterar|continuar|romper|reporte|mostrar|verdadero|falso|nulo|y|o|funcion)\b"),

                                        // ERRORES LÉXICOS: números pegados a letras (DEBEN IR ANTES QUE NUMERO)
                                        new TokenDefinition(TokenType.ERROR_LEXICO, @"\d+[a-zA-Z_][a-zA-Z0-9_]*"),
                                        new TokenDefinition(TokenType.ERROR_LEXICO, @"[@#$%&!?|\\~`]"),

                                        // NÚMEROS
                                        new TokenDefinition(TokenType.NUMERO, @"\d+\.\d+"),
                                        new TokenDefinition(TokenType.NUMERO, @"\d+"),

                                        // OPERADORES
                                        new TokenDefinition(TokenType.OPERADOR, @"<=|>=|==|!=|<|>|\+|-|\*|/|=|\^"),

                                        // Cadenas
                                        new TokenDefinition(TokenType.CADENA, "\"[^\"\r\n]*\""),
                                        new TokenDefinition(TokenType.ERROR_LEXICO, "\"[^\"\r\n]*$"),

                                        // Símbolos del lenguaje
                                        new TokenDefinition(TokenType.SIMBOLO, @"\(|\)|\{|\}|\[|\]|;|,|\.|°"),

                                        // Identificadores válidos
                                        new TokenDefinition(TokenType.IDENTIFICADOR, @"[a-zA-Z_][a-zA-Z0-9_]*"),
                              };                
                                  }

                    private void AdvancePosition(char c)
                    {
                              if (c == '\n') { _line++; _column = 1; }
                              else if (c == '\t') { _column += 4; }
                              else { _column++; }
                              _position++;
                    }

                    public List<Token> Tokenize()
                    {
                              Tokens.Clear();
                              Errores.Clear();
                              _position = 0; _line = 1; _column = 1;

                              while (_position < _sourceCode.Length)
                              {
                                        if (char.IsWhiteSpace(_sourceCode[_position]))
                                        {
                                                  AdvancePosition(_sourceCode[_position]);
                                                  continue;
                                        }

                                        // Comentarios de línea
                                        if (_position + 1 < _sourceCode.Length && _sourceCode[_position] == '/' && _sourceCode[_position + 1] == '/')
                                        {
                                                  int startCol = _column, startLine = _line;
                                                  while (_position < _sourceCode.Length && _sourceCode[_position] != '\n')
                                                            AdvancePosition(_sourceCode[_position]);
                                                  // No se agrega token de comentario para no interferir
                                                  continue;
                                        }

                                        bool matchFound = false;
                                        foreach (var def in _tokenDefinitions)
                                        {
                                                  Match match = def.Regex.Match(_sourceCode, _position);
                                                  if (match.Success)
                                                  {
                                                            if (def.Type == TokenType.ERROR_LEXICO)
                                                            {
                                                                      string msg;
                                                                      if (Regex.IsMatch(match.Value, @"^\d+[a-zA-Z_]"))
                                                                                msg = $"Identificador inválido '{match.Value}': no puede comenzar con un número";
                                                                      else if (match.Value.StartsWith("\""))
                                                                                msg = $"Cadena de texto sin cerrar: {match.Value}";
                                                                      else
                                                                                msg = $"Carácter ilegal '{match.Value}' no permitido en ASTRA";
                                                                      Errores.Add(new ErrorInfo { Line = _line, Column = _column, Length = match.Value.Length, Message = msg, Type = ErrorType.Lexico });
                                                            }
                                                            Tokens.Add(new Token(def.Type, match.Value, _line, _column));
                                                            foreach (char c in match.Value) AdvancePosition(c);
                                                            matchFound = true;
                                                            break;
                                                  }
                                        }

                                        if (!matchFound)
                                        {
                                                  Errores.Add(new ErrorInfo { Line = _line, Column = _column, Length = 1, Message = $"Símbolo '{_sourceCode[_position]}' no reconocido", Type = ErrorType.Lexico });
                                                  Tokens.Add(new Token(TokenType.ERROR_LEXICO, _sourceCode[_position].ToString(), _line, _column));
                                                  AdvancePosition(_sourceCode[_position]);
                                        }
                              }
                              Tokens.Add(new Token(TokenType.EOF, "EOF", _line, _column));
                              return Tokens;
                    }
          }
}