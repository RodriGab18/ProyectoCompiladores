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

    public enum ErrorType { Lexico, Estructural }

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
    // Símbolos no permitidos
    new TokenDefinition(TokenType.ERROR_LEXICO, @"[@#$%&!?|\\~`]"),

    // NÚMEROS (incluyen negativos)
    new TokenDefinition(TokenType.NUMERO, @"-?\d+(\.\d+)?"),
    new TokenDefinition(TokenType.NUMERO, @"\d+\.\d+"),
    new TokenDefinition(TokenType.NUMERO, @"\d+"),

    // OPERADORES (incluye - pero los números negativos ya fueron capturados)
    new TokenDefinition(TokenType.OPERADOR, @"<=|>=|==|!=|<|>|\+|-|\*|/|=|\^"),

    // Cadenas de texto (correctas y con error si no se cierran)
    new TokenDefinition(TokenType.CADENA, "\"[^\"\r\n]*\""),
    new TokenDefinition(TokenType.ERROR_LEXICO, "\"[^\"\r\n]*$"),

    // Símbolos del lenguaje
    new TokenDefinition(TokenType.SIMBOLO, @"\(|\)|\{|\}|\[|\]|;|,|\.|°"),

    // Identificadores válidos (solo letra o guión bajo al inicio)
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
                    string comment = "";
                    while (_position < _sourceCode.Length && _sourceCode[_position] != '\n')
                    {
                        comment += _sourceCode[_position];
                        AdvancePosition(_sourceCode[_position]);
                    }
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

    // ==================================================================================
    // VENTANA DE DOCUMENTACIÓN (sin cambios, se mantiene igual)
    // ==================================================================================
    public class FrmDocumentacion : Form
    {
        public FrmDocumentacion()
        {
            this.Text = "Manual de Usuario - ASTRA DSL & IDE";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White; // Fondo Blanco
            this.ForeColor = Color.Black; // Texto Negro
            this.Font = new Font("Segoe UI", 11);
            this.ShowIcon = false;
            this.MinimizeBox = false;

            TabControl tabs = new TabControl { Dock = DockStyle.Fill, Appearance = TabAppearance.Normal };
            tabs.Padding = new Point(15, 5);

            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.DrawItem += (s, e) =>
            {
                Graphics g = e.Graphics;
                TabPage page = tabs.TabPages[e.Index];
                Rectangle tabBounds = tabs.GetTabRect(e.Index);

                // Tema Claro para las pestañas
                Color backColor = e.State == DrawItemState.Selected ? Color.White : Color.FromArgb(240, 240, 240);
                Color foreColor = Color.Black;

                g.FillRectangle(new SolidBrush(backColor), tabBounds);
                TextRenderer.DrawText(g, page.Text, tabs.Font, tabBounds, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            tabs.TabPages.Add(CrearPaginaRtf("1. Introducción", GetInfoGeneral()));
            tabs.TabPages.Add(CrearPaginaRtf("2. Sintaxis Básica", GetSintaxisBasica()));
            tabs.TabPages.Add(CrearPaginaRtf("3. Palabras Reservadas", GetPalabrasReservadas()));
            tabs.TabPages.Add(CrearPaginaRtf("4. Ejemplos", GetEjemplos()));

            this.Controls.Add(tabs);

            Button btnCerrar = new Button
            {
                Text = "Entendido",
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(230, 230, 230),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCerrar);
        }

        private TabPage CrearPaginaRtf(string titulo, string rtfContent)
        {
            TabPage page = new TabPage(titulo) { BackColor = Color.White };
            RichTextBox rtb = new RichTextBox { Dock = DockStyle.Fill, Rtf = rtfContent, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Color.White, ForeColor = Color.Black, Padding = new Padding(20) };
            page.Controls.Add(rtb);
            return page;
        }

        private string GetInfoGeneral() =>
            @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
            \viewkind4\uc1\pard\f0\fs36\b Manual de ASTRA DSL\b0\fs24\par
            \par
            Bienvenido al entorno de desarrollo de \b ASTRA\b0  (Astrobiology Simulation and Terrain Analysis).\par
            \par
            Este programa es un IDE (Entorno de Desarrollo Integrado) disenado para escribir, analizar y validar codigo escrito en el lenguaje \b ASTRA DSL\b0.\par
            \par
            \fs28\b El IDE consta de tres partes principales:\b0\fs24\par
            \b 1. Editor de Codigo (Izquierda):\b0  Donde escribes tu programa. Cuenta con resaltado de colores en tiempo real. Si cometes un error logico (como no cerrar una llave) o escribes un caracter invalido, veras un subrayado rojo discontinuo. Pasa el raton sobre el subrayado para ver la explicacion del error.\par
            \par
            \b 2. Tabla de Tokens (Arriba Derecha):\b0  Muestra como el compilador lee tu codigo, dividiendolo en piezas elementales clasificadas por tipo.\par
            \par
            \b 3. Consola de Salida (Abajo Derecha):\b0  Al presionar el boton de Analizar, aqui veras el resultado del analisis.\par
            }";

        private string GetSintaxisBasica() =>
            @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
            \viewkind4\uc1\pard\f0\fs32\b Conceptos Clave de ASTRA DSL\b0\fs24\par
            \par
            \b Comentarios:\b0\par
            Usa {\f1 //} para escribir comentarios de una sola linea.\par
            \par
            \b Bloques de Codigo:\b0\par
            ASTRA es un lenguaje estructurado por bloques delimitados por llaves \{ \}.\par
            \par
            \b Tipos de Datos:\b0\par
            \tab\bullet\b Identificadores:\b0  Nombres de variables. Deben empezar con una letra.\par
            \tab\bullet\b Numeros:\b0  Enteros o decimales.\par
            \tab\bullet\b Cadenas (Texto):\b0  Texto entre comillas dobles.\par
            }";

        private string GetPalabrasReservadas() =>
            @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
            \viewkind4\uc1\pard\f0\fs32\b Diccionario de Palabras Reservadas\b0\fs24\par
            \par
            \fs26\b Estructura y Control de Flujo:\b0\fs24\par
            \tab\bullet\b inicio / fin\par
            \tab\bullet\b si / sino / mientras / para / iterar / repetir\par
            \tab\bullet\b romper / continuar / interrumpir\par
            \tab\bullet\b y / o / no\par
            \par
            \fs26\b Propiedades y Logica:\b0\fs24\par
            \tab\bullet\b gravedad / radiacion / temperatura / velocidad / densidad / composicion\par
            \tab\bullet\b verdadero / falso / nulo / entero / booleano / decimal / texto\par
            \par
            \fs26\b Acciones:\b0\fs24\par
            \tab\bullet\b resultado / mostrar / guardar / reporte / analizar / configuracion\par
            }";

        private string GetEjemplos() =>
            @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Consolas;}{\f1\fnil\fcharset0 Segoe UI;}}
            \viewkind4\uc1\pard\f0\fs22 // Ejemplo de configuracion\line
            simulacion \{\line
            \tab planeta \{\line
            \tab\tab temperatura = -60;\line
            \tab\tab gravedad = 3.7;\line
            \tab\}\line
            \}\line
            \line
            \ pard\f1\fs24\par
            \f1\b Ejemplo de logica condicional:\b0\par
            \pard\f0\fs22 si (temperatura > -50 y presion < 1) \{\line
            \tab mostrar(""Posible terraformacion"");\line
            \}\line
            }";
    }

    // ==================================================================================
    // EDITOR PRINCIPAL
    // ==================================================================================
    public class BioSphereEditor : Form
    {
        private System.Windows.Forms.Integration.ElementHost editorHost;
        private ICSharpCode.AvalonEdit.TextEditor txtCodigo;
        private SyntaxColorizer syntaxColorizer;
        private CommentColorizer commentColorizer;
        private ErrorSquiggleRenderer errorRenderer;
        private System.Windows.Forms.Timer parseTimer;
        private ToolTip errorToolTip;
        private List<ErrorInfo> currentErrors = new List<ErrorInfo>();

        private DataGridView gridTokens;
        private RichTextBox txtConsola;
        private Button btnCompilar;
        private Button btnDocumentacion;
        private Button btnArboles;               // <--- NUEVO
        private CheckBox chkModoPrueba;
        private Panel statusPanel;
        private Label lblErrorCount;
        private System.Windows.Forms.Timer errorTimer;

        private NodoPrograma? ultimoAST;         // <--- Almacena el último árbol válido

        public BioSphereEditor()
        {
            this.Text = "ASTRA - IDE";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(45, 45, 48);

            // ===== PANEL SUPERIOR =====
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(60, 60, 70) };
            Label lblTitle = new Label { Text = "🌍 ASTRA DSL", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 15), AutoSize = true };
            topPanel.Controls.Add(lblTitle);

            // Botón Analizar
            btnCompilar = new Button { Text = "▷", Font = new Font("Segoe UI Symbol", 16), Location = new Point(250, 10), Size = new Size(40, 40), BackColor = Color.FromArgb(60, 60, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnCompilar.FlatAppearance.BorderSize = 0;
            btnCompilar.Click += BtnCompilar_Click;
            topPanel.Controls.Add(btnCompilar);

            // Botón Documentación
            btnDocumentacion = new Button { Text = "📖", Font = new Font("Segoe UI Emoji", 14), Location = new Point(295, 10), Size = new Size(40, 40), BackColor = Color.FromArgb(60, 60, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnDocumentacion.FlatAppearance.BorderSize = 0;
            btnDocumentacion.Click += BtnDocumentacion_Click;
            topPanel.Controls.Add(btnDocumentacion);

            // Botón Árbol Sintáctico (NUEVO)
            btnArboles = new Button { Text = "🌳", Font = new Font("Segoe UI Emoji", 12), Location = new Point(340, 10), Size = new Size(40, 40), BackColor = Color.FromArgb(60, 60, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnArboles.FlatAppearance.BorderSize = 0;
            btnArboles.Click += (s, e) => MostrarVentanaArboles();
            topPanel.Controls.Add(btnArboles);

            chkModoPrueba = new CheckBox { Text = " Modo Prueba", ForeColor = Color.White, Font = new Font("Segoe UI", 11), Location = new Point(400, 18), AutoSize = true, Checked = false };
            chkModoPrueba.CheckedChanged += ChkModoPrueba_CheckedChanged;
            topPanel.Controls.Add(chkModoPrueba);

            // ===== PANEL INFERIOR =====
            statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 30, BackColor = Color.FromArgb(50, 50, 55) };
            lblErrorCount = new Label { Text = "✅ Sin errores", ForeColor = Color.LightGreen, Font = new Font("Segoe UI", 10), Location = new Point(10, 6), AutoSize = true };
            statusPanel.Controls.Add(lblErrorCount);

            // ===== SPLIT CONTAINER =====
            SplitContainer mainSplit = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 800, Orientation = Orientation.Vertical };

            // Panel izquierdo (editor)
            Panel leftPanel = new Panel { Dock = DockStyle.Fill };
            editorHost = new System.Windows.Forms.Integration.ElementHost { Dock = DockStyle.Fill };
            txtCodigo = new ICSharpCode.AvalonEdit.TextEditor
            {
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 14,
                ShowLineNumbers = true,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)),
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(212, 212, 212))
            };
            syntaxColorizer = new SyntaxColorizer();
            commentColorizer = new CommentColorizer();
            txtCodigo.TextArea.TextView.LineTransformers.Add(syntaxColorizer);
            txtCodigo.TextArea.TextView.LineTransformers.Add(commentColorizer);
            errorRenderer = new ErrorSquiggleRenderer(txtCodigo);
            txtCodigo.TextArea.TextView.BackgroundRenderers.Add(errorRenderer);

            parseTimer = new System.Windows.Forms.Timer { Interval = 200 };
            parseTimer.Tick += ParseTimer_Tick;
            txtCodigo.TextChanged += (s, e) => { parseTimer.Stop(); parseTimer.Start(); };

            errorToolTip = new ToolTip { AutoPopDelay = 8000, InitialDelay = 0, ReshowDelay = 0, ShowAlways = true, BackColor = Color.FromArgb(45, 10, 10), ForeColor = Color.FromArgb(255, 180, 180) };
            txtCodigo.TextArea.TextView.MouseHover += TextView_MouseHover;
            txtCodigo.TextArea.TextView.MouseHoverStopped += TextView_MouseHoverStopped;

            editorHost.Child = txtCodigo;
            leftPanel.Controls.Add(editorHost);
            leftPanel.Controls.Add(topPanel);

            // Panel derecho (tabla + consola)
            SplitContainer rightSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 400 };
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

            txtConsola = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10), BackColor = Color.Black, ForeColor = Color.LightGray, ReadOnly = true };

            rightSplit.Panel1.Controls.Add(gridTokens);
            rightSplit.Panel2.Controls.Add(txtConsola);
            mainSplit.Panel1.Controls.Add(leftPanel);
            mainSplit.Panel2.Controls.Add(rightSplit);
            this.Controls.Add(mainSplit);
            this.Controls.Add(statusPanel);

            CargarCodigoCorrecto();
            errorTimer = new System.Windows.Forms.Timer { Interval = 600 };
            errorTimer.Tick += (s, e) => ActualizarContadorErrores();
            errorTimer.Start();
        }

        private void ActualizarContadorErrores()
        {
            if (currentErrors != null)
                if (currentErrors.Count > 0)
                { lblErrorCount.Text = $"❌ {currentErrors.Count} error(es) detectado(s)"; lblErrorCount.ForeColor = Color.Salmon; }
                else
                { lblErrorCount.Text = "✅ Sin errores"; lblErrorCount.ForeColor = Color.LightGreen; }
        }

        private void ParseTimer_Tick(object? sender, EventArgs e)
        {
            parseTimer.Stop();
            try
            {
                Lexer lexer = new Lexer(txtCodigo.Text);
                var tokens = lexer.Tokenize();
                currentErrors = lexer.Errores ?? new List<ErrorInfo>();
                syntaxColorizer.UpdateTokens(tokens);
                errorRenderer.UpdateErrors(currentErrors);
                txtCodigo.TextArea.TextView.Redraw();
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error parseando: {ex.Message}"); }
        }

        private void TextView_MouseHover(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // (Mismo código de tooltip que ya tenías, no lo modifico)
            if (currentErrors == null || currentErrors.Count == 0) return;
            var pos = txtCodigo.GetPositionFromPoint(e.GetPosition(txtCodigo));
            if (!pos.HasValue) return;
            try
            {
                int offset = txtCodigo.Document.GetOffset(pos.Value.Line, pos.Value.Column);
                ErrorInfo? found = null;
                foreach (var error in currentErrors)
                {
                    int errStart = txtCodigo.Document.GetOffset(error.Line, error.Column);
                    int errEnd = errStart + Math.Max(1, error.Length);
                    if (offset >= errStart && offset <= errEnd) { found = error; break; }
                }
                if (found != null)
                {
                    string msg = $"[Línea {found.Line}, Col {found.Column}]  {found.Message}";
                    var winPos = e.GetPosition(txtCodigo);
                    var formPos = editorHost.PointToScreen(new Point((int)winPos.X, (int)winPos.Y));
                    var clientPos = this.PointToClient(formPos);
                    errorToolTip.ToolTipTitle = found.Type == ErrorType.Lexico ? "Error léxico" : "Error estructural";
                    errorToolTip.Show(msg, this, clientPos.X + 10, clientPos.Y - 28, 8000);
                    e.Handled = true;
                }
                else errorToolTip.Hide(this);
            }
            catch { }
        }

        private void TextView_MouseHoverStopped(object sender, System.Windows.Input.MouseEventArgs e) => errorToolTip.Hide(this);

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
simulacion {{
    planeta {
        temperatura = -60;
    atmosfera {
        presion = 0.006;
    123variable = 50;
    @ $ #
    si (temperatura > -50 {
        mostrar(""Error"");
";
        }

        private void ChkModoPrueba_CheckedChanged(object sender, EventArgs e)
        {
            if (chkModoPrueba.Checked) CargarCodigoConErrores();
            else CargarCodigoCorrecto();
        }

        private void BtnDocumentacion_Click(object sender, EventArgs e)
        {
            using (FrmDocumentacion doc = new FrmDocumentacion()) doc.ShowDialog(this);
        }

        // ==================== MÉTODO DE ANÁLISIS PRINCIPAL (MODIFICADO) ====================
        private void BtnCompilar_Click(object sender, EventArgs e)
        {
            gridTokens.Rows.Clear();
            txtConsola.Clear();

            Lexer lexer = new Lexer(txtCodigo.Text);
            List<Token> tokens = lexer.Tokenize();
            var erroresLexicos = lexer.Errores;

            foreach (var token in tokens)
            {
                string tokenName = token.Type.ToString().Replace("_", " ");
                int rowIndex = gridTokens.Rows.Add(tokenName, token.Lexeme, token.Line, token.Column);
                if (token.Type == TokenType.ERROR_LEXICO)
                    gridTokens.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
            }

            if (erroresLexicos.Count > 0)
            {
                txtConsola.SelectionColor = Color.Red;
                txtConsola.AppendText("❌ ANÁLISIS LÉXICO CON ERRORES\n\n");
                foreach (var err in erroresLexicos)
                {
                    txtConsola.SelectionColor = Color.Salmon;
                    txtConsola.AppendText($"  • Línea {err.Line}, Col {err.Column}: {err.Message}\n");
                }
                errorRenderer.UpdateErrors(erroresLexicos);
                txtCodigo.TextArea.TextView.Redraw();
                ultimoAST = null;
                return;
            }

            txtConsola.AppendText("🔍 Análisis sintáctico...\n");
            var parser = new Parser(tokens);
            var (programa, erroresSintacticos) = parser.ParsePrograma();

            if (erroresSintacticos.Count > 0)
            {
                txtConsola.SelectionColor = Color.Red;
                txtConsola.AppendText($"❌ ANÁLISIS SINTÁCTICO CON {erroresSintacticos.Count} ERRORES\n\n");
                foreach (var err in erroresSintacticos)
                {
                    txtConsola.SelectionColor = Color.Salmon;
                    txtConsola.AppendText($"  • Línea {err.Line}, Col {err.Column}: {err.Message}\n");
                }
                errorRenderer.UpdateErrors(erroresSintacticos);
                txtCodigo.TextArea.TextView.Redraw();
                ultimoAST = null;
            }
            else
            {
                ultimoAST = programa;   // Guardamos el AST
                txtConsola.SelectionColor = Color.LimeGreen;
                txtConsola.AppendText("✅ ANÁLISIS SINTÁCTICO COMPLETADO SIN ERRORES\n\n");
                txtConsola.SelectionColor = Color.Cyan;
                txtConsola.AppendText("📖 ÁRBOL SINTÁCTICO (texto):\n\n");
                txtConsola.SelectionColor = Color.White;
                txtConsola.AppendText(programa!.ToTreeString("", true));
                errorRenderer.UpdateErrors(new List<ErrorInfo>());
                txtCodigo.TextArea.TextView.Redraw();
            }
        }

        // ==================== MÉTODO PARA MOSTRAR EL VISOR GRÁFICO ====================
        private void MostrarVentanaArboles()
        {
            if (ultimoAST == null)
            {
                MessageBox.Show("No hay un árbol sintáctico válido.\nPrimero analiza un código sin errores.",
                                "Visor de AST", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var arbolesVisual = new List<NodoASTVisual>();

            // 1. Árbol global del programa
            arbolesVisual.Add(ConvertirAST(ultimoAST));

            // 2. Subárboles extraídos (expresiones complejas, condicionales, bucles)
            var subArboles = ExtraerSubArboles(ultimoAST);
            arbolesVisual.AddRange(subArboles);

            using (var visor = new FrmVisualizadorArbol(arbolesVisual))
            {
                visor.ShowDialog(this);
            }
        }

        // ==================== CONVERSIÓN DE AST DEL PARSER A NODO VISUAL ====================
        private NodoASTVisual? ConvertirAST(NodoAST nodoParser)
        {
            if (nodoParser == null) return null;

            string etiqueta = "";
            string tipo = "Nodo";

            switch (nodoParser)
            {
                case NodoPrograma prog: etiqueta = "PROGRAMA"; tipo = "Programa"; break;
                case NodoBloqueSimulacion bloque: etiqueta = "simulacion { ... }"; tipo = "Bloque"; break;
                case NodoBloquePlaneta planeta: etiqueta = "planeta"; tipo = "PalabraReservada"; break;
                case NodoBloqueAtmosfera atmos: etiqueta = "atmosfera"; tipo = "PalabraReservada"; break;
                case NodoBloqueAgua agua: etiqueta = "agua"; tipo = "PalabraReservada"; break;
                case NodoBloqueVida vida: etiqueta = "vida"; tipo = "PalabraReservada"; break;
                case NodoAsignacion asig: etiqueta = "="; tipo = "Operador"; break;
                case NodoMostrar mostrar: etiqueta = "mostrar"; tipo = "PalabraReservada"; break;
                case NodoReporte reporte: etiqueta = "reporte"; tipo = "PalabraReservada"; break;
                case NodoIf condIf: etiqueta = "si"; tipo = "PalabraReservada"; break;
                case NodoWhile ciclo: etiqueta = "mientras"; tipo = "PalabraReservada"; break;
                case NodoDefinicionFuncion func: etiqueta = $"funcion {func.Nombre}"; tipo = "Funcion"; break;
                case NodoLlamadaFuncion llamada: etiqueta = $"llamada {llamada.Nombre}"; tipo = "Llamada"; break;
                case NodoExprNumero num: etiqueta = num.Valor; tipo = "Numero"; break;
                case NodoExprIdentificador id: etiqueta = id.Nombre; tipo = "Identificador"; break;
                case NodoExprBinaria bin: etiqueta = bin.Operador; tipo = "Operador"; break;
                case NodoExprPotencia pot: etiqueta = "^"; tipo = "Operador"; break;
                case NodoExprParentesis par: etiqueta = "( ... )"; tipo = "Parentesis"; break;
                case NodoCondicion cond: etiqueta = cond.Operador ?? "condicion"; tipo = "Condicion"; break;
                case NodoCantidad cant: etiqueta = cant.ToString(); tipo = "Cantidad"; break;
                case NodoBooleano boo: etiqueta = boo.Valor ? "verdadero" : "falso"; tipo = "Booleano"; break;
                case NodoTexto texto: etiqueta = texto.Texto; tipo = "Texto"; break;
                case NodoLista lista: etiqueta = "lista"; tipo = "Lista"; break;
                default: etiqueta = nodoParser.GetType().Name; tipo = "Desconocido"; break;
            }

            var visual = new NodoASTVisual(etiqueta, tipo);

            // Agregar hijos según el tipo concreto
            switch (nodoParser)
            {
                case NodoPrograma prog:
                    if (prog.BloqueSimulacion != null) visual.Hijos.Add(ConvertirAST(prog.BloqueSimulacion)!);
                    break;
                case NodoBloqueSimulacion bloque:
                    foreach (var s in bloque.Sentencias) visual.Hijos.Add(ConvertirAST(s)!);
                    break;
                case NodoBloquePlaneta planeta:
                    foreach (var s in planeta.Sentencias) visual.Hijos.Add(ConvertirAST(s)!);
                    break;
                case NodoBloqueAtmosfera atmos:
                    foreach (var s in atmos.Sentencias) visual.Hijos.Add(ConvertirAST(s)!);
                    break;
                case NodoBloqueAgua agua:
                    foreach (var s in agua.Sentencias) visual.Hijos.Add(ConvertirAST(s)!);
                    break;
                case NodoBloqueVida vida:
                    foreach (var s in vida.Sentencias) visual.Hijos.Add(ConvertirAST(s)!);
                    break;
                case NodoAsignacion asig:
                    visual.Hijos.Add(new NodoASTVisual(asig.Identificador, "Identificador"));
                    visual.Hijos.Add(ConvertirAST(asig.Valor)!);
                    break;
                case NodoMostrar mostrar:
                    visual.Hijos.Add(ConvertirAST(mostrar.Valor)!);
                    break;
                case NodoReporte reporte:
                    visual.Hijos.Add(ConvertirAST(reporte.Valor)!);
                    break;
                case NodoIf condIf:
                    visual.Hijos.Add(ConvertirAST(condIf.Condicion)!);
                    var thenBlock = new NodoASTVisual("then", "Bloque");
                    foreach (var s in condIf.ThenSentencias) thenBlock.Hijos.Add(ConvertirAST(s)!);
                    visual.Hijos.Add(thenBlock);
                    if (condIf.ElseSentencias.Count > 0)
                    {
                        var elseBlock = new NodoASTVisual("else", "Bloque");
                        foreach (var s in condIf.ElseSentencias) elseBlock.Hijos.Add(ConvertirAST(s)!);
                        visual.Hijos.Add(elseBlock);
                    }
                    break;
                case NodoWhile ciclo:
                    visual.Hijos.Add(ConvertirAST(ciclo.Condicion)!);
                    var bodyBlock = new NodoASTVisual("cuerpo", "Bloque");
                    foreach (var s in ciclo.Sentencias) bodyBlock.Hijos.Add(ConvertirAST(s)!);
                    visual.Hijos.Add(bodyBlock);
                    break;
                case NodoDefinicionFuncion func:
                    foreach (var p in func.Parametros)
                        visual.Hijos.Add(new NodoASTVisual(p, "Parametro"));
                    var funcBody = new NodoASTVisual("cuerpo", "Bloque");
                    foreach (var s in func.Sentencias) funcBody.Hijos.Add(ConvertirAST(s)!);
                    visual.Hijos.Add(funcBody);
                    break;
                case NodoLlamadaFuncion llamada:
                    foreach (var arg in llamada.Argumentos)
                        visual.Hijos.Add(ConvertirAST(arg)!);
                    break;
                case NodoExprBinaria bin:
                    visual.Hijos.Add(ConvertirAST(bin.Izquierda)!);
                    visual.Hijos.Add(ConvertirAST(bin.Derecha)!);
                    break;
                case NodoExprPotencia pot:
                    visual.Hijos.Add(ConvertirAST(pot.Base)!);
                    visual.Hijos.Add(ConvertirAST(pot.Exponente)!);
                    break;
                case NodoExprParentesis par:
                    etiqueta = "( )";
                    tipo = "Parentesis";
                    break;
                case NodoCondicion cond:
                    if (!string.IsNullOrEmpty(cond.Operador))
                        etiqueta = cond.Operador;   // muestra <, >, ==, etc.
                    else
                        etiqueta = "cond";
                    tipo = "Condicion";
                    break;
                case NodoCantidad cant:
                    etiqueta = "Cantidad";
                    tipo = "Cantidad";
                    break;
                case NodoLista lista:
                    foreach (var v in lista.Valores) visual.Hijos.Add(ConvertirAST(v)!);
                    break;
            }
            return visual;
        }
        private List<NodoASTVisual> ExtraerSubArboles(NodoPrograma programa)
        {
            var subArboles = new List<NodoASTVisual>();
            if (programa == null) return subArboles;

            // Función recursiva para recorrer cualquier nodo
            void Recorrer(NodoAST nodo)
            {
                if (nodo == null) return;

                // Detectar expresiones aritméticas complejas (con al menos dos operadores o paréntesis)
                if (nodo is NodoExprBinaria binaria && (EsExpresionCompleja(binaria)))
                {
                    subArboles.Add(ConvertirAST(binaria));
                }
                else if (nodo is NodoExprPotencia potencia && EsExpresionCompleja(potencia))
                {
                    subArboles.Add(ConvertirAST(potencia));
                }
                else if (nodo is NodoExprParentesis parentesis && EsExpresionCompleja(parentesis.Expr))
                {
                    subArboles.Add(ConvertirAST(parentesis));
                }
                // Detectar sentencias condicionales (si)
                else if (nodo is NodoIf nodoIf)
                {
                    subArboles.Add(ConvertirAST(nodoIf));
                }
                // Detectar bucles (mientras)
                else if (nodo is NodoWhile nodoWhile)
                {
                    subArboles.Add(ConvertirAST(nodoWhile));
                }

                // Recorrer hijos según el tipo de nodo (similar a ConvertirAST)
                switch (nodo)
                {
                    case NodoPrograma prog:
                        if (prog.BloqueSimulacion != null) Recorrer(prog.BloqueSimulacion);
                        break;
                    case NodoBloqueSimulacion bloque:
                        foreach (var s in bloque.Sentencias) Recorrer(s);
                        break;
                    case NodoBloquePlaneta planeta:
                        foreach (var s in planeta.Sentencias) Recorrer(s);
                        break;
                    case NodoBloqueAtmosfera atmos:
                        foreach (var s in atmos.Sentencias) Recorrer(s);
                        break;
                    case NodoBloqueAgua agua:
                        foreach (var s in agua.Sentencias) Recorrer(s);
                        break;
                    case NodoBloqueVida vida:
                        foreach (var s in vida.Sentencias) Recorrer(s);
                        break;
                    case NodoAsignacion asig:
                        Recorrer(asig.Valor);
                        break;
                    case NodoMostrar mostrar:
                        Recorrer(mostrar.Valor);
                        break;
                    case NodoReporte reporte:
                        Recorrer(reporte.Valor);
                        break;
                    case NodoIf condIf:
                        Recorrer(condIf.Condicion);
                        foreach (var s in condIf.ThenSentencias) Recorrer(s);
                        foreach (var s in condIf.ElseSentencias) Recorrer(s);
                        break;
                    case NodoWhile ciclo:
                        Recorrer(ciclo.Condicion);
                        foreach (var s in ciclo.Sentencias) Recorrer(s);
                        break;
                    case NodoDefinicionFuncion func:
                        foreach (var s in func.Sentencias) Recorrer(s);
                        break;
                    case NodoLlamadaFuncion llamada:
                        foreach (var arg in llamada.Argumentos) Recorrer(arg);
                        break;
                    case NodoExprBinaria bin:
                        Recorrer(bin.Izquierda);
                        Recorrer(bin.Derecha);
                        break;
                    case NodoExprPotencia pot:
                        Recorrer(pot.Base);
                        Recorrer(pot.Exponente);
                        break;
                    case NodoExprParentesis par:
                        Recorrer(par.Expr);
                        break;
                    case NodoCondicion cond:
                        Recorrer(cond.Izquierda);
                        if (cond.Derecha != null) Recorrer(cond.Derecha);
                        foreach (var (_, sub) in cond.OperadoresLogicos) Recorrer(sub);
                        break;
                    case NodoCantidad cant:
                        Recorrer(cant.Expr);
                        break;
                    case NodoLista lista:
                        foreach (var v in lista.Valores) Recorrer(v);
                        break;
                }
            }

            bool EsExpresionCompleja(NodoExpr expr)
            {
                // Consideramos compleja si tiene al menos dos operadores (binarios o potencia) o contiene paréntesis
                int contador = 0;
                void Contar(NodoExpr e)
                {
                    if (e == null) return;
                    if (e is NodoExprBinaria || e is NodoExprPotencia) contador++;
                    if (e is NodoExprBinaria bin)
                    {
                        Contar(bin.Izquierda);
                        Contar(bin.Derecha);
                    }
                    else if (e is NodoExprPotencia pot)
                    {
                        Contar(pot.Base);
                        Contar(pot.Exponente);
                    }
                    else if (e is NodoExprParentesis par)
                    {
                        contador++; // cuenta el paréntesis como complejidad
                        Contar(par.Expr);
                    }
                }
                Contar(expr);
                return contador >= 2;
            }

            Recorrer(programa);
            return subArboles;
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