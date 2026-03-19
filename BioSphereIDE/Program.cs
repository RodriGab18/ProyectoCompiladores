using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;

namespace BioSphereIDE
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

    public class ErrorInfo
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public int Length { get; set; }
        public string Message { get; set; }
        public ErrorType Type { get; set; }
    }

    public enum ErrorType
    {
        Lexico,
        Estructural
    }

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
                // Palabras reservadas
                new TokenDefinition(TokenType.PALABRA_RESERVADA, @"\b(simulacion|planeta|atmosfera|agua|vida|inicio|fin|entero|imprimir|si|sino|mientras|iterar|continuar|romper|reporte|mostrar|verdadero|falso|gravedad|radiacion|temperatura|presion|co2|oxigeno|masa|radio|volumen|estado_liquido|y)\b"),

                // OPERADORES - PRIMERO para que no sean atrapados como errores
                new TokenDefinition(TokenType.OPERADOR, @"<=|>=|==|!=|<|>|\+|-|\*|/|=|\^"),

                // Detectar números pegados a letras (ERROR)
                new TokenDefinition(TokenType.ERROR_LEXICO, @"\d+[a-zA-Z_][a-zA-Z0-9_]*"),

                // Símbolos no permitidos (excluyendo < y > que ya son operadores)
                new TokenDefinition(TokenType.ERROR_LEXICO, @"[@#$%^&*!?|\\~`]"),

                // Números
                new TokenDefinition(TokenType.NUMERO, @"-?\d+(\.\d+)?"),
                new TokenDefinition(TokenType.NUMERO, @"\d+\.\d+"),
                new TokenDefinition(TokenType.NUMERO, @"\d+"),

                // Cadenas de texto
                // Cadena correctamente cerrada
                new TokenDefinition(TokenType.CADENA, "\"[^\"\r\n]*\""),
                // Cadena sin cerrar → ERROR LÉXICO
                new TokenDefinition(TokenType.ERROR_LEXICO, "\"[^\"\r\n]*$"),

                // Símbolos del lenguaje
                new TokenDefinition(TokenType.SIMBOLO, @"\(|\)|\{|\}|\[|\]|;|,|\.|°"),

                // Identificadores válidos
                new TokenDefinition(TokenType.IDENTIFICADOR, @"[a-zA-Z][a-zA-Z0-9_]*"),
                 // Física
                new TokenDefinition(TokenType.PALABRA_RESERVADA,
                    @"\b(gravedad|radiacion|temperatura|velocidad|densidad|composicion)\b"),
                // Control de flujo
                new TokenDefinition(TokenType.PALABRA_RESERVADA,
                    @"\b(si|sino|mientras|para|iterar|repetir|encontrar|esperar|continuar|interrumpir|romper)\b"),
                // Lógica y tipos
                new TokenDefinition(TokenType.PALABRA_RESERVADA,
                    @"\b(y|o|no|verdadero|falso|nulo|entero|booleano|decimal|texto)\b"),
                // Acciones
                new TokenDefinition(TokenType.PALABRA_RESERVADA,
                    @"\b(resultado|mostrar|guardar|reporte|analizar|configuracion)\b"),

            };
        }

        private void VerificarEstructuraBasica()
        {
            Stack<(Token token, int index)> pilaLlaves = new Stack<(Token, int)>();
            Stack<(Token token, int index)> pilaParentesis = new Stack<(Token, int)>();
            Stack<(Token token, int index)> pilaCorchetes = new Stack<(Token, int)>();
            
            for (int i = 0; i < Tokens.Count; i++)
            {
                var token = Tokens[i];
                if (token.Type != TokenType.SIMBOLO) continue;
                
                switch (token.Lexeme)
                {
                    case "{":
                        pilaLlaves.Push((token, i));
                        break;
                    case "}":
                        if (pilaLlaves.Count == 0)
                        {
                            Errores.Add(new ErrorInfo
                            {
                                Line = token.Line,
                                Column = token.Column,
                                Length = token.Length,
                                Message = $"Llave de cierre '}}' sin abrir",
                                Type = ErrorType.Estructural
                            });
                        }
                        else
                        {
                            pilaLlaves.Pop();
                        }
                        break;
                        
                    case "(":
                        pilaParentesis.Push((token, i));
                        break;
                    case ")":
                        if (pilaParentesis.Count == 0)
                        {
                            Errores.Add(new ErrorInfo
                            {
                                Line = token.Line,
                                Column = token.Column,
                                Length = token.Length,
                                Message = $"Paréntesis de cierre ')' sin abrir",
                                Type = ErrorType.Estructural
                            });
                        }
                        else
                        {
                            pilaParentesis.Pop();
                        }
                        break;
                        
                    case "[":
                        pilaCorchetes.Push((token, i));
                        break;
                    case "]":
                        if (pilaCorchetes.Count == 0)
                        {
                            Errores.Add(new ErrorInfo
                            {
                                Line = token.Line,
                                Column = token.Column,
                                Length = token.Length,
                                Message = $"Corchete de cierre ']' sin abrir",
                                Type = ErrorType.Estructural
                            });
                        }
                        else
                        {
                            pilaCorchetes.Pop();
                        }
                        break;
                }
            }
            
            // Verificar símbolos sin cerrar
            foreach (var item in pilaLlaves)
            {
                Errores.Add(new ErrorInfo
                {
                    Line = item.token.Line,
                    Column = item.token.Column,
                    Length = item.token.Length,
                    Message = $"Llave '{{' abierta no tiene cierre",
                    Type = ErrorType.Estructural
                });
            }
            
            foreach (var item in pilaParentesis)
            {
                Errores.Add(new ErrorInfo
                {
                    Line = item.token.Line,
                    Column = item.token.Column,
                    Length = item.token.Length,
                    Message = $"Paréntesis '(' abierto no tiene cierre",
                    Type = ErrorType.Estructural
                });
            }
            
            foreach (var item in pilaCorchetes)
            {
                Errores.Add(new ErrorInfo
                {
                    Line = item.token.Line,
                    Column = item.token.Column,
                    Length = item.token.Length,
                    Message = $"Corchete '[' abierto no tiene cierre",
                    Type = ErrorType.Estructural
                });
            }
        }

        public List<Token> Tokenize()
        {
            Tokens.Clear();
            Errores.Clear();
            _position = 0;
            _line = 1;
            _column = 1;
            
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
                    int startCol = _column;
                    int startLine = _line;
                    string comment = "";
                    
                    while (_position < _sourceCode.Length && _sourceCode[_position] != '\n') 
                    {
                        comment += _sourceCode[_position];
                        AdvancePosition(_sourceCode[_position]);
                    }
                    
                    // Agregar comentario como token especial (lo ignoramos pero lo guardamos para mantener índices)
                    Tokens.Add(new Token(TokenType.SIMBOLO, comment, startLine, startCol));
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
                            if (System.Text.RegularExpressions.Regex.IsMatch(match.Value, @"^\d+[a-zA-Z_]"))
                                msg = $"Identificador inválido '{match.Value}': no puede comenzar con un número";
                            else if (match.Value.StartsWith("\""))
                                msg = $"Cadena de texto sin cerrar: {match.Value}";
                            else
                                msg = $"Carácter ilegal '{match.Value}' no permitido en ASTRA";

                            Errores.Add(new ErrorInfo
                            {
                                Line = _line,
                                Column = _column,
                                Length = match.Value.Length,
                                Message = msg,
                                Type = ErrorType.Lexico
                            });
                        }
                        
                        Tokens.Add(new Token(def.Type, match.Value, _line, _column));
                        
                        foreach (char c in match.Value) AdvancePosition(c);
                        matchFound = true; 
                        break;
                    }
                }

                if (!matchFound)
                {
                    Errores.Add(new ErrorInfo
                    {
                        Line = _line,
                        Column = _column,
                        Length = 1,
                        Message = $"Símbolo '{_sourceCode[_position]}' no reconocido",
                        Type = ErrorType.Lexico
                    });
                    
                    Tokens.Add(new Token(TokenType.ERROR_LEXICO, _sourceCode[_position].ToString(), _line, _column));
                    AdvancePosition(_sourceCode[_position]);
                }
            }
            
            Tokens.Add(new Token(TokenType.EOF, "EOF", _line, _column));
            VerificarEstructuraBasica();
            
            return Tokens;
        }

        private void AdvancePosition(char c)
        {
            if (c == '\n') { _line++; _column = 1; } 
            else if (c == '\t') { _column += 4; } 
            else { _column++; }
            _position++;
        }
    }

    public class CodeEditorWithHighlighting : RichTextBox
    {
        private System.Windows.Forms.Timer highlightTimer;
        private Lexer currentLexer;
        private List<ErrorInfo> currentErrors = new List<ErrorInfo>();

        // Tooltip para mostrar mensajes de error al pasar el cursor
        private readonly ToolTip errorToolTip;
        private string lastTooltipMsg = "";   // evita redibujar si el mensaje no cambió
        private Point lastTooltipPos = Point.Empty;
        
        // Colores del tema
        private readonly Color FondoColor = Color.FromArgb(30, 30, 30);
        private readonly Color TextoColor = Color.FromArgb(173, 216, 230); // Azul claro
        private readonly Color PalabraReservadaColor = Color.FromArgb(25, 25, 112); // Azul oscuro midnight blue
        private readonly Color NumeroColor = Color.FromArgb(255, 215, 0); // Dorado
        private readonly Color CadenaColor = Color.FromArgb(152, 251, 152); // Verde claro
        private readonly Color OperadorColor = Color.FromArgb(255, 182, 193); // Rosa claro
        private readonly Color ErrorColor = Color.Red;
        private readonly Color ComentarioColor = Color.FromArgb(128, 128, 128); // Gris

        public CodeEditorWithHighlighting()
        {
            this.BackColor = FondoColor;
            this.ForeColor = TextoColor;
            this.Font = new Font("Consolas", 14);
            this.AcceptsTab = true;
            this.WordWrap = false;
            this.Dock = DockStyle.Fill;
            this.BorderStyle = BorderStyle.None;

            // ── Tooltip de errores ──────────────────────────────────────────
            errorToolTip = new ToolTip
            {
                AutoPopDelay  = 8000,   // permanece 8 s visible
                InitialDelay  = 0,      // aparece sin espera
                ReshowDelay   = 0,
                ShowAlways    = true,
                IsBalloon     = false,
                BackColor     = Color.FromArgb(45, 10, 10),
                ForeColor     = Color.FromArgb(255, 180, 180),
                ToolTipTitle  = "Error léxico",
            };
            
            highlightTimer = new System.Windows.Forms.Timer();
            highlightTimer.Interval = 500;
            highlightTimer.Tick += HighlightTimer_Tick;
            highlightTimer.Start();
            
            this.TextChanged += CodeEditorWithHighlighting_TextChanged;
            this.SelectionChanged += CodeEditorWithHighlighting_SelectionChanged;
            this.VScroll += CodeEditorWithHighlighting_Scroll;
            this.MouseMove += CodeEditorWithHighlighting_MouseMove;
            this.MouseLeave += CodeEditorWithHighlighting_MouseLeave;
        }

        // ── Hover: detectar si el cursor está sobre un error ───────────────
        private void CodeEditorWithHighlighting_MouseMove(object? sender, MouseEventArgs e)
        {
            if (currentErrors == null || currentErrors.Count == 0)
            {
                if (lastTooltipMsg != "")
                {
                    errorToolTip.Hide(this);
                    lastTooltipMsg = "";
                }
                return;
            }

            // Convertir posición del mouse a índice de carácter en el texto
            int charIndex = this.GetCharIndexFromPosition(e.Location);
            if (charIndex < 0 || charIndex >= this.Text.Length)
            {
                errorToolTip.Hide(this);
                lastTooltipMsg = "";
                return;
            }

            // Buscar si ese índice cae dentro de algún error registrado
            ErrorInfo? found = null;
            foreach (var error in currentErrors)
            {
                int errStart = GetPositionFromLineColumn(error.Line, error.Column);
                if (errStart < 0) continue;
                int errEnd = errStart + Math.Max(1, error.Length);
                if (charIndex >= errStart && charIndex < errEnd)
                {
                    found = error;
                    break;
                }
            }

            if (found == null)
            {
                if (lastTooltipMsg != "")
                {
                    errorToolTip.Hide(this);
                    lastTooltipMsg = "";
                }
                return;
            }

            // Construir mensaje enriquecido
            string tipoLabel = found.Type == ErrorType.Lexico ? "Error léxico" : "Error estructural";
            string msg = $"[Línea {found.Line}, Col {found.Column}]  {found.Message}";

            // Solo actualizar si cambió el mensaje o la posición (evita parpadeo)
            if (msg == lastTooltipMsg && e.Location == lastTooltipPos) return;

            lastTooltipMsg  = msg;
            lastTooltipPos  = e.Location;
            errorToolTip.ToolTipTitle = tipoLabel;

            // Mostrar el tooltip ligeramente por encima del cursor
            Point tipPos = new Point(e.X + 10, e.Y - 28);
            errorToolTip.Show(msg, this, tipPos, 8000);
        }

        private void CodeEditorWithHighlighting_MouseLeave(object? sender, EventArgs e)
        {
            errorToolTip.Hide(this);
            lastTooltipMsg = "";
        }

        private void CodeEditorWithHighlighting_Scroll(object? sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void CodeEditorWithHighlighting_SelectionChanged(object? sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void HighlightTimer_Tick(object? sender, EventArgs e)
        {
            highlightTimer.Stop();
            AplicarResaltado();
            highlightTimer.Start();
        }

        private void CodeEditorWithHighlighting_TextChanged(object? sender, EventArgs e)
        {
            highlightTimer.Stop();
            highlightTimer.Start();
        }

        private void AplicarResaltado()
        {
            if (this.Text.Length == 0) return;
            
            try
            {
                int selectionStart = this.SelectionStart;
                int selectionLength = this.SelectionLength;
                
                currentLexer = new Lexer(this.Text);
                var tokens = currentLexer.Tokenize();
                currentErrors = currentLexer.Errores;
                
                this.SuspendLayout();
                
                this.SelectAll();
                this.SelectionColor = TextoColor;
                
                foreach (var token in tokens)
                {
                    if (token.Type == TokenType.EOF) continue;
                    
                    int start = GetPositionFromLineColumn(token.Line, token.Column);
                    if (start < 0 || start >= this.Text.Length) continue;
                    
                    this.Select(start, token.Lexeme.Length);
                    
                    switch (token.Type)
                    {
                        case TokenType.PALABRA_RESERVADA:
                            this.SelectionColor = PalabraReservadaColor;
                            break;
                        case TokenType.NUMERO:
                            this.SelectionColor = NumeroColor;
                            break;
                        case TokenType.CADENA:
                            this.SelectionColor = CadenaColor;
                            break;
                        case TokenType.OPERADOR:
                            this.SelectionColor = OperadorColor;
                            break;
                        case TokenType.ERROR_LEXICO:
                            this.SelectionColor = ErrorColor;
                            break;
                    }
                }
                
                var commentMatches = Regex.Matches(this.Text, @"//.*$", RegexOptions.Multiline);
                foreach (Match match in commentMatches)
                {
                    this.Select(match.Index, match.Length);
                    this.SelectionColor = ComentarioColor;
                }
                
                this.Select(selectionStart, selectionLength);
                this.SelectionColor = TextoColor;
                
                this.ResumeLayout();
                this.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en resaltado: {ex.Message}");
            }
        }

        private int GetPositionFromLineColumn(int line, int column)
        {
            string[] lines = this.Text.Split('\n');
            int pos = 0;
            
            for (int i = 1; i < line; i++)
            {
                if (i - 1 < lines.Length)
                    pos += lines[i - 1].Length + 1;
                else
                    return -1;
            }
            
            return pos + column - 1;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            if (currentErrors != null && currentErrors.Count > 0)
            {
                using (Pen errorPen = new Pen(Color.Red, 2))
                {
                    errorPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    
                    foreach (var error in currentErrors)
                    {
                        try
                        {
                            int start = GetPositionFromLineColumn(error.Line, error.Column);
                            if (start < 0 || start >= this.Text.Length) continue;
                            
                            Point startPos = this.GetPositionFromCharIndex(start);
                            int endIndex = Math.Min(start + error.Length, this.Text.Length - 1);
                            if (endIndex <= start) continue;
                            
                            Point endPos = this.GetPositionFromCharIndex(endIndex);
                            
                            if (startPos.Y == endPos.Y)
                            {
                                int width = endPos.X - startPos.X;
                                if (width < 5) width = 5;
                                
                                e.Graphics.DrawLine(errorPen, 
                                    startPos.X, startPos.Y + this.Font.Height - 2,
                                    startPos.X + width, startPos.Y + this.Font.Height - 2);
                            }
                            else
                            {
                                e.Graphics.DrawLine(errorPen,
                                    startPos.X, startPos.Y + this.Font.Height - 2,
                                    this.Width - 20, startPos.Y + this.Font.Height - 2);
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        public List<ErrorInfo> GetErrores() => currentErrors ?? new List<ErrorInfo>();
    }

    public class BioSphereEditor : Form
    {
        private CodeEditorWithHighlighting txtCodigo;
        private DataGridView gridTokens;
        private RichTextBox txtConsola;
        private Button btnCompilar;
        private CheckBox chkModoPrueba;
        private Panel statusPanel;
        private Label lblErrorCount;
        private System.Windows.Forms.Timer errorTimer;

        public BioSphereEditor()
        {
            this.Text = "BioSphere DSL - IDE Profesional";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);

            // ===== PANEL SUPERIOR =====
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(60, 60, 70)
            };

            Label lblTitle = new Label
            {
                Text = "🌍 BioSphere DSL",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSize = true
            };
            topPanel.Controls.Add(lblTitle);

            btnCompilar = new Button
            {
                Text = "🔍 Analizar",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(250, 12),
                Size = new Size(130, 35),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCompilar.FlatAppearance.BorderSize = 0;
            btnCompilar.Click += BtnCompilar_Click;
            topPanel.Controls.Add(btnCompilar);

            chkModoPrueba = new CheckBox
            {
                Text = " Modo Prueba",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                Location = new Point(400, 18),
                AutoSize = true,
                Checked = false
            };
            chkModoPrueba.CheckedChanged += ChkModoPrueba_CheckedChanged;
            topPanel.Controls.Add(chkModoPrueba);

            // ===== PANEL INFERIOR (ESTADO) =====
            statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(50, 50, 55)
            };

            lblErrorCount = new Label
            {
                Text = "✅ Sin errores",
                ForeColor = Color.LightGreen,
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 6),
                AutoSize = true
            };
            statusPanel.Controls.Add(lblErrorCount);

            // ===== SPLIT CONTAINER PRINCIPAL (IZQUIERDA/DERECHA) =====
            SplitContainer mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 800,
                Orientation = Orientation.Vertical
            };

            // ===== PANEL IZQUIERDO (EDITOR) =====
            Panel leftPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            
            txtCodigo = new CodeEditorWithHighlighting();
            
            leftPanel.Controls.Add(txtCodigo);
            leftPanel.Controls.Add(topPanel); // topPanel está Dock.Top, txtCodigo está Dock.Fill

            // ===== PANEL DERECHO (TOKENS + CONSOLA) =====
            SplitContainer rightSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400
            };

            // Tabla de tokens
            gridTokens = new DataGridView
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.WhiteSmoke,
                Font = new Font("Consolas", 10)
            };
            gridTokens.Columns[0].Name = "Tipo";
            gridTokens.Columns[1].Name = "Lexema";
            gridTokens.Columns[2].Name = "Línea";
            gridTokens.Columns[3].Name = "Columna";

            // Consola
            txtConsola = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                BackColor = Color.Black,
                ForeColor = Color.LightGray,
                ReadOnly = true
            };

            rightSplit.Panel1.Controls.Add(gridTokens);
            rightSplit.Panel2.Controls.Add(txtConsola);

            // Asignar paneles al split principal
            mainSplit.Panel1.Controls.Add(leftPanel);
            mainSplit.Panel2.Controls.Add(rightSplit);

            // Agregar todo al formulario en el orden correcto
            this.Controls.Add(mainSplit);
            this.Controls.Add(statusPanel);

            // Cargar código inicial
            CargarCodigoCorrecto();

            // Timer para actualizar contador de errores
            errorTimer = new System.Windows.Forms.Timer();
            errorTimer.Interval = 600;
            errorTimer.Tick += (s, e) => ActualizarContadorErrores();
            errorTimer.Start();
        }

        private void ActualizarContadorErrores()
        {
            if (txtCodigo != null)
            {
                var errores = txtCodigo.GetErrores();
                if (errores != null && errores.Count > 0)
                {
                    lblErrorCount.Text = $"❌ {errores.Count} error(es) detectado(s)";
                    lblErrorCount.ForeColor = Color.Salmon;
                }
                else
                {
                    lblErrorCount.Text = "✅ Sin errores";
                    lblErrorCount.ForeColor = Color.LightGreen;
                }
            }
        }

        private void CargarCodigoCorrecto()
        {
            txtCodigo.Text = @"// Simulación de planeta Marte - CORRECTO
simulacion {
    planeta {
        temperatura = -60;
        gravedad = 3.7;
        radio = 3389;
    }
    
    atmosfera {
        presion = 0.006;
        co2 = 95;
    }
    
    agua {
        estado_liquido = falso;
    }
    
    si (temperatura > -50 y presion < 1) {
        mostrar(""Posible terraformación"");
    }
}";
        }

        private void CargarCodigoConErrores()
        {
            txtCodigo.Text = @"// ========== CÓDIGO CON ERRORES ==========
simulacion {{  // Error: llaves dobles
    planeta {
        temperatura = -60;
        // Error: falta cerrar este bloque
    atmosfera {
        presion = 0.006;
        co2 = 95;
    
    // Error: identificador con número
    123variable = 50;
    
    // Error: símbolos no permitidos
    @ $ #
    
    // Error: paréntesis sin cerrar
    si (temperatura > -50 {
        mostrar(""Error"");
    
// Error: falta cerrar simulacion";
        }

        private void ChkModoPrueba_CheckedChanged(object? sender, EventArgs e)
        {
            if (chkModoPrueba.Checked)
                CargarCodigoConErrores();
            else
                CargarCodigoCorrecto();
        }

        private void BtnCompilar_Click(object? sender, EventArgs e)
        {
            gridTokens.Rows.Clear();
            txtConsola.Clear();

            Lexer lexer = new Lexer(txtCodigo.Text);
            List<Token> tokens = lexer.Tokenize();
            var errores = lexer.Errores;

            foreach (var token in tokens)
            {
                string tokenName = token.Type.ToString().Replace("_", " ");
                int rowIndex = gridTokens.Rows.Add(tokenName, token.Lexeme, token.Line, token.Column);

                if (token.Type == TokenType.ERROR_LEXICO)
                {
                    gridTokens.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                    gridTokens.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }

            if (errores.Count > 0)
            {
                txtConsola.SelectionColor = Color.Red;
                txtConsola.AppendText("❌ ANÁLISIS COMPLETADO CON ERRORES\n\n");

                var lexicos = errores.Where(e => e.Type == ErrorType.Lexico).ToList();
                var estructurales = errores.Where(e => e.Type == ErrorType.Estructural).ToList();

                if (lexicos.Count > 0)
                {
                    txtConsola.SelectionColor = Color.Orange;
                    txtConsola.AppendText($"🔴 ERRORES LÉXICOS ({lexicos.Count}):\n");
                    foreach (var error in lexicos)
                    {
                        txtConsola.SelectionColor = Color.Salmon;
                        txtConsola.AppendText($"  • Línea {error.Line}, Col {error.Column}: {error.Message}\n");
                    }
                }

                if (estructurales.Count > 0)
                {
                    txtConsola.SelectionColor = Color.Orange;
                    txtConsola.AppendText($"\n🔵 ERRORES ESTRUCTURALES ({estructurales.Count}):\n");
                    foreach (var error in estructurales)
                    {
                        txtConsola.SelectionColor = Color.LightBlue;
                        txtConsola.AppendText($"  • Línea {error.Line}, Col {error.Column}: {error.Message}\n");
                    }
                }

                txtConsola.SelectionColor = Color.Yellow;
                txtConsola.AppendText($"\n📊 TOTAL: {errores.Count} errores");
            }
            else
            {
                txtConsola.SelectionColor = Color.LimeGreen;
                txtConsola.AppendText("✅ ANÁLISIS COMPLETADO SIN ERRORES\n");
                txtConsola.AppendText($"📝 Total de tokens: {tokens.Count}\n");
                txtConsola.SelectionColor = Color.Cyan;
                txtConsola.AppendText("\n🎯 El código es léxicamente correcto. Listo para análisis sintáctico.");
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