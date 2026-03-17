using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BioSphereIDE
{

    public enum TokenType { PALABRA_RESERVADA, IDENTIFICADOR, NUMERO, OPERADOR, SIMBOLO, CADENA, ERROR_LEXICO, EOF }

    public class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public int Line { get; }
        public int Column { get; }
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

    public class Lexer
    {
        private readonly string _sourceCode;
        private int _position = 0, _line = 1, _column = 1;
        private readonly List<TokenDefinition> _tokenDefinitions;
        public List<string> Errores { get; } = new List<string>();

        public Lexer(string sourceCode)
        {
            _sourceCode = sourceCode;
            _tokenDefinitions = new List<TokenDefinition>
            {
                // Palabras reservadas unificadas
                new TokenDefinition(TokenType.PALABRA_RESERVADA, @"(simulacion|planeta|atmosfera|agua|vida|inicio|fin|entero|imprimir|si|sino|mientras|iterar|continuar|romper|reporte|mostrar|verdadero|falso|gravedad|radiacion|temperatura|presion|co2|oxigeno|masa|radio|volumen|estado_liquido)\b"),
                
                // ATRAPA EL ERROR "3edad" (Número pegado a letras)
                new TokenDefinition(TokenType.ERROR_LEXICO, @"[0-9]+[a-zA-Z_][a-zA-Z0-9_]*"),
                
                // Identificadores válidos
                new TokenDefinition(TokenType.IDENTIFICADOR, @"[a-zA-Z][a-zA-Z0-9_]*"),
                
                // Núm
                // eros
                new TokenDefinition(TokenType.NUMERO, @"[0-9]+(\.[0-9]+)?"),
                
                // Cadenas de texto (Todo lo que esté entre comillas)
                new TokenDefinition(TokenType.CADENA, "\"[^\"]*\""),
                
                // Operadores protegidos
                new TokenDefinition(TokenType.OPERADOR, @"<=|>=|==|!=|<|>|\+|-|\*|/|=|(\^)"),
                
                // Símbolos del lenguaje (Incluyendo el punto \.)
                new TokenDefinition(TokenType.SIMBOLO, @"\(|\)|\{|\}|\[|\]|;|,|%|°|\.")
            };
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            while (_position < _sourceCode.Length)
            {
                // Ignorar espacios en blanco
                if (char.IsWhiteSpace(_sourceCode[_position])) 
                { 
                    AdvancePosition(_sourceCode[_position]); 
                    continue; 
                }

                // Ignorar comentarios (//)
                if (_position + 1 < _sourceCode.Length && _sourceCode[_position] == '/' && _sourceCode[_position + 1] == '/')
                {
                    while (_position < _sourceCode.Length && _sourceCode[_position] != '\n') 
                    {
                        AdvancePosition(_sourceCode[_position]);
                    }
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
                            Errores.Add($"[Error Léxico] Identificador mal formado '{match.Value}' en Lín: {_line}, Col: {_column}");
                        }
                        else
                        {
                            tokens.Add(new Token(def.Type, match.Value, _line, _column));
                        }
                        
                        foreach (char c in match.Value) AdvancePosition(c);
                        matchFound = true; 
                        break;
                    }
                }

               
                if (!matchFound)
                {
                    Errores.Add($"[Error Léxico] Símbolo no reconocido '{_sourceCode[_position]}' en Lín: {_line}, Col: {_column}");
                    AdvancePosition(_sourceCode[_position]);
                }
            }
            return tokens;
        }

        private void AdvancePosition(char c)
        {
            if (c == '\n') { _line++; _column = 1; } else { _column++; }
            _position++;
        }
    }

   
    public class BioSphereEditor : Form
    {
        private RichTextBox txtCodigo;
        private DataGridView gridTokens;
        private RichTextBox txtConsola;
        private Button btnCompilar;

        public BioSphereEditor()
        {
            this.Text = "BioSphere DSL - Analizador Léxico";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            SplitContainer splitContainer = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 600 };
            SplitContainer rightSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 500 };

            
            Panel headerPanel = new Panel 
            { 
                Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(45, 45, 48) 
            };

            // 2. BOTÓN VISTOSO
            btnCompilar = new Button
            {
                Text = " Analizar", 
                Font = new Font("Segoe UI Emoji", 16, FontStyle.Bold),
                Dock = DockStyle.Right, Width = 160,
                BackColor = Color.SeaGreen, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnCompilar.FlatAppearance.BorderSize = 0;
            btnCompilar.Click += BtnCompilar_Click;
            headerPanel.Controls.Add(btnCompilar);

            // 3. EDITOR DE CÓDIGO (Cargado con la prueba de Terraformación)
            txtCodigo = new RichTextBox
            {
                Dock = DockStyle.Fill, 
                Font = new Font("Consolas", 14),
                BackColor = Color.FromArgb(30, 30, 30), 
                ForeColor = Color.White, 
                AcceptsTab = true,
                Text = ""
            };

         
            Panel leftPanel = new Panel { Dock = DockStyle.Fill };
            leftPanel.Controls.Add(txtCodigo);    
            leftPanel.Controls.Add(headerPanel);  
            splitContainer.Panel1.Controls.Add(leftPanel);

            // 5. TABLA DE TOKENS
            gridTokens = new DataGridView
            {
                Dock = DockStyle.Fill, ColumnCount = 4, ReadOnly = true, AllowUserToAddRows = false,
                RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.WhiteSmoke
            };
            gridTokens.Columns[0].Name = "Token"; 
            gridTokens.Columns[1].Name = "Lexema";
            gridTokens.Columns[2].Name = "Línea"; 
            gridTokens.Columns[3].Name = "Columna";

            // 6. CONSOLA DE SALIDA
            txtConsola = new RichTextBox 
            { 
                Dock = DockStyle.Fill, 
                Font = new Font("Consolas", 11), 
                BackColor = Color.Black, 
                ForeColor = Color.LightGray, 
                ReadOnly = true 
            };

            rightSplit.Panel1.Controls.Add(gridTokens); 
            rightSplit.Panel2.Controls.Add(txtConsola);
            splitContainer.Panel2.Controls.Add(rightSplit);
            this.Controls.Add(splitContainer);
        }

        
        private void BtnCompilar_Click(object? sender, EventArgs e)
        {
            gridTokens.Rows.Clear();
            txtConsola.Clear();

            Lexer lexer = new Lexer(txtCodigo.Text);
            List<Token> tokens = lexer.Tokenize();

            foreach (var token in tokens)
            {
                string tokenName = token.Type.ToString().Replace("_", " ");
                gridTokens.Rows.Add(tokenName, token.Lexeme, token.Line, token.Column);
            }

            if (lexer.Errores.Count > 0)
            {
                txtConsola.SelectionColor = Color.Red;
                txtConsola.AppendText(" Análisis fallido. Errores léxicos encontrados:\n\n");
                foreach (string error in lexer.Errores)
                {
                    txtConsola.SelectionColor = Color.Salmon;
                    txtConsola.AppendText(error + "\n");
                }
            }
            else
            {
                txtConsola.SelectionColor = Color.LimeGreen;
                txtConsola.AppendText(" Análisis léxico completado sin errores. Listo para análisis sintáctico.\n");
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BioSphereEditor());
        }
    }
}