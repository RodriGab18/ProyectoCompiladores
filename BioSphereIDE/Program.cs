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
        public List<string> ErroresEstructurales { get; } = new List<string>();

        public Lexer(string sourceCode)
        {
            _sourceCode = sourceCode;
            _tokenDefinitions = new List<TokenDefinition>
            {
                // PRIMERO: Palabras reservadas
                new TokenDefinition(TokenType.PALABRA_RESERVADA, @"\b(simulacion|planeta|atmosfera|agua|vida|inicio|fin|entero|imprimir|si|sino|mientras|iterar|continuar|romper|reporte|mostrar|verdadero|falso|gravedad|radiacion|temperatura|presion|co2|oxigeno|masa|radio|volumen|estado_liquido|y)\b"),
                
                // SEGUNDO: Detectar números pegados a letras (ERROR)
                new TokenDefinition(TokenType.ERROR_LEXICO, @"\d+[a-zA-Z_][a-zA-Z0-9_]*"),
                
                // TERCERO: Símbolos no permitidos
                new TokenDefinition(TokenType.ERROR_LEXICO, @"[@#$%^&*!?<>|\\~`]"),
                
                // CUARTO: Números
                new TokenDefinition(TokenType.NUMERO, @"-?\d+(\.\d+)?"),
                
                // QUINTO: Cadenas de texto
                new TokenDefinition(TokenType.CADENA, "\"[^\"]*\""),
                
                // SEXTO: Operadores
                new TokenDefinition(TokenType.OPERADOR, @"<=|>=|==|!=|<|>|\+|-|\*|/|=|\^"),
                
                // SÉPTIMO: Símbolos del lenguaje (cada uno individualmente)
                new TokenDefinition(TokenType.SIMBOLO, @"\(|\)|\{|\}|\[|\]|;|,|\.|°"),
                
                // OCTAVO: Identificadores válidos
                new TokenDefinition(TokenType.IDENTIFICADOR, @"[a-zA-Z][a-zA-Z0-9_]*"),
                
                // NOVENO: Unidades de medida como ERROR
                new TokenDefinition(TokenType.ERROR_LEXICO, @"[a-zA-Z]+_[a-zA-Z]+|[a-zA-Z]{1,3}\b(?![a-zA-Z0-9_])")
            };
        }

        // Verificador básico de estructura (llaves y paréntesis)
        private void VerificarEstructuraBasica(List<Token> tokens)
        {
            Stack<Token> pilaLlaves = new Stack<Token>();
            Stack<Token> pilaParentesis = new Stack<Token>();
            Stack<Token> pilaCorchetes = new Stack<Token>();
            
            foreach (var token in tokens)
            {
                if (token.Type != TokenType.SIMBOLO) continue;
                
                switch (token.Lexeme)
                {
                    case "{":
                        pilaLlaves.Push(token);
                        break;
                    case "}":
                        if (pilaLlaves.Count == 0)
                        {
                            ErroresEstructurales.Add($"[Error Estructural] Llave de cierre '}}' sin abrir en Lín: {token.Line}, Col: {token.Column}");
                        }
                        else
                        {
                            pilaLlaves.Pop();
                        }
                        break;
                        
                    case "(":
                        pilaParentesis.Push(token);
                        break;
                    case ")":
                        if (pilaParentesis.Count == 0)
                        {
                            ErroresEstructurales.Add($"[Error Estructural] Paréntesis de cierre ')' sin abrir en Lín: {token.Line}, Col: {token.Column}");
                        }
                        else
                        {
                            pilaParentesis.Pop();
                        }
                        break;
                        
                    case "[":
                        pilaCorchetes.Push(token);
                        break;
                    case "]":
                        if (pilaCorchetes.Count == 0)
                        {
                            ErroresEstructurales.Add($"[Error Estructural] Corchete de cierre ']' sin abrir en Lín: {token.Line}, Col: {token.Column}");
                        }
                        else
                        {
                            pilaCorchetes.Pop();
                        }
                        break;
                }
            }
            
            // Verificar si quedaron símbolos sin cerrar
            while (pilaLlaves.Count > 0)
            {
                var token = pilaLlaves.Pop();
                ErroresEstructurales.Add($"[Error Estructural] Llave '{{' abierta en Lín: {token.Line}, Col: {token.Column} no tiene cierre");
            }
            
            while (pilaParentesis.Count > 0)
            {
                var token = pilaParentesis.Pop();
                ErroresEstructurales.Add($"[Error Estructural] Paréntesis '(' abierto en Lín: {token.Line}, Col: {token.Column} no tiene cierre");
            }
            
            while (pilaCorchetes.Count > 0)
            {
                var token = pilaCorchetes.Pop();
                ErroresEstructurales.Add($"[Error Estructural] Corchete '[' abierto en Lín: {token.Line}, Col: {token.Column} no tiene cierre");
            }
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            Errores.Clear();
            ErroresEstructurales.Clear();
            
            while (_position < _sourceCode.Length)
            {
                // Ignorar espacios en blanco
                if (char.IsWhiteSpace(_sourceCode[_position])) 
                { 
                    AdvancePosition(_sourceCode[_position]); 
                    continue; 
                }

                // Ignorar comentarios
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
                            Errores.Add($"[Error Léxico] '{match.Value}' no permitido en Lín: {_line}, Col: {_column}");
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
            
            tokens.Add(new Token(TokenType.EOF, "EOF", _line, _column));
            
            // Verificar estructura después de tokenizar
            VerificarEstructuraBasica(tokens);
            
            return tokens;
        }

        private void AdvancePosition(char c)
        {
            if (c == '\n') { _line++; _column = 1; } 
            else if (c == '\t') { _column += 4; } 
            else { _column++; }
            _position++;
        }
    }

    public class BioSphereEditor : Form
    {
        private RichTextBox txtCodigo;
        private DataGridView gridTokens;
        private RichTextBox txtConsola;
        private Button btnCompilar;
        private CheckBox chkModoPrueba;

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

            // Checkbox para modo de prueba con errores
            chkModoPrueba = new CheckBox
            {
                Text = " Modo Prueba (con errores)",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Location = new Point(10, 15),
                AutoSize = true,
                Checked = false
            };
            chkModoPrueba.CheckedChanged += ChkModoPrueba_CheckedChanged;
            headerPanel.Controls.Add(chkModoPrueba);

            btnCompilar = new Button
            {
                Text = " Analizar", 
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Dock = DockStyle.Right, Width = 160,
                BackColor = Color.SeaGreen, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnCompilar.FlatAppearance.BorderSize = 0;
            btnCompilar.Click += BtnCompilar_Click;
            headerPanel.Controls.Add(btnCompilar);

            // Código correcto por defecto
            string codicodigoCorrecto = @"// Simulacion basica correcta
simulacion {
    planeta {
        temperatura = 25;
        gravedad = 9.8;
    }
    agua {
        estado_liquido = verdadero;
    }
}";

            // Código con errores para pruebas
            string codigoConErrores = @"// Simulacion con ERRORES
simulacion {{  // Error: llaves dobles
    planeta {
        temperatura = 25;
        // Falta cerrar esta llave
    agua {
        estado_liquido = verdadero;
    // Falta cerrar parentesis
    si (temperatura > 20 {
        mostrar (""Calor"");
    }
}  // Esta llave sobra?

    // Identificador mal formado
    123variable = 50;
    
    // Simbolos no permitidos
    @ $ #
";

            txtCodigo = new RichTextBox
            {
                Dock = DockStyle.Fill, 
                Font = new Font("Consolas", 14),
                BackColor = Color.FromArgb(30, 30, 30), 
                ForeColor = Color.White, 
                AcceptsTab = true,
                Text = codigoConErrores
            };

            Panel leftPanel = new Panel { Dock = DockStyle.Fill };
            leftPanel.Controls.Add(txtCodigo);    
            leftPanel.Controls.Add(headerPanel);  
            splitContainer.Panel1.Controls.Add(leftPanel);

            gridTokens = new DataGridView
            {
                Dock = DockStyle.Fill, 
                ColumnCount = 4, 
                ReadOnly = true, 
                AllowUserToAddRows = false,
                RowHeadersVisible = false, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.WhiteSmoke
            };
            gridTokens.Columns[0].Name = "Token"; 
            gridTokens.Columns[1].Name = "Lexema";
            gridTokens.Columns[2].Name = "Línea"; 
            gridTokens.Columns[3].Name = "Columna";

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

        private void ChkModoPrueba_CheckedChanged(object? sender, EventArgs e)
        {
            if (chkModoPrueba.Checked)
            {
                txtCodigo.Text = @"// ========== MODO PRUEBA CON ERRORES ==========
// 1. ERROR: Llaves multiples sin cerrar
simulacion {{{
    planeta {
        temperatura = 25;
        // 2. ERROR: Parentesis sin cerrar
        gravedad = (9.8 * 2;
        
    // 3. ERROR: Falta cerrar bloque planeta
    agua {
        estado_liquido = verdadero;
        // 4. ERROR: Corchete sin cerrar
        datos[0 = 100;
    
    // 5. ERROR: Llave de cierre sin abrir
}

    // 6. ERROR: Identificador con numero
    123variable = 50;
    
    // 7. ERROR: Simbolos no permitidos
    @ $ #
    
    // 8. ERROR: Cierre de bloque simulacion faltante
// ==========================================";
            }
            else
            {
                txtCodigo.Text = @"// Simulacion correcta
simulacion {
    planeta {
        temperatura = 25;
        gravedad = 9.8;
    }
    agua {
        estado_liquido = verdadero;
    }
}";
            }
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
                Color? color = null;
                
                // Colorear filas de error en la tabla
                if (token.Type == TokenType.ERROR_LEXICO)
                    color = Color.LightCoral;
                    
                int rowIndex = gridTokens.Rows.Add(tokenName, token.Lexeme, token.Line, token.Column);
                if (color.HasValue)
                {
                    gridTokens.Rows[rowIndex].DefaultCellStyle.BackColor = color.Value;
                }
            }

            // Mostrar errores en consola
            if (lexer.Errores.Count > 0 || lexer.ErroresEstructurales.Count > 0)
            {
                txtConsola.SelectionColor = Color.Red;
                txtConsola.AppendText("❌ ANÁLISIS FALLIDO\n\n");
                
                if (lexer.Errores.Count > 0)
                {
                    txtConsola.SelectionColor = Color.Orange;
                    txtConsola.AppendText("🔴 ERRORES LÉXICOS:\n");
                    foreach (string error in lexer.Errores)
                    {
                        txtConsola.SelectionColor = Color.Salmon;
                        txtConsola.AppendText("  • " + error + "\n");
                    }
                }
                
                if (lexer.ErroresEstructurales.Count > 0)
                {
                    txtConsola.SelectionColor = Color.Orange;
                    txtConsola.AppendText("\n🔵 ERRORES ESTRUCTURALES (llaves/parentesis):\n");
                    foreach (string error in lexer.ErroresEstructurales)
                    {
                        txtConsola.SelectionColor = Color.LightBlue;
                        txtConsola.AppendText("  • " + error + "\n");
                    }
                }
                
                txtConsola.SelectionColor = Color.Yellow;
                txtConsola.AppendText($"\n📊 TOTAL: {lexer.Errores.Count + lexer.ErroresEstructurales.Count} errores");
            }
            else
            {
                txtConsola.SelectionColor = Color.LimeGreen;
                txtConsola.AppendText("✅ Análisis completado SIN ERRORES\n");
                txtConsola.AppendText("📝 Total tokens: " + tokens.Count + "\n");
                txtConsola.AppendText("🔍 Listo para análisis sintáctico");
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