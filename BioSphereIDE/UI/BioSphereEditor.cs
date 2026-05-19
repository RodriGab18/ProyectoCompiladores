using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BioSphereIDE.Core;
using BioSphereIDE.Analizadores;

namespace BioSphereIDE.UI
{
    // ==================================================================================
    // PANEL DE DOCUMENTACIÓN
    // ==================================================================================
    public class PanelDocumentacion : Panel
    {
        public PanelDocumentacion()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(9, 6, 22);
            this.ForeColor = Color.FromArgb(240, 244, 255);
            this.Font = new Font("Segoe UI", 11);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill, Appearance = TabAppearance.Normal };
            tabs.Padding = new Point(15, 5);
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.DrawItem += (s, e) =>
            {
                Graphics g = e.Graphics;
                TabPage page = tabs.TabPages[e.Index];
                Rectangle tabBounds = tabs.GetTabRect(e.Index);
                Color backColor = e.State == DrawItemState.Selected ? Color.White : Color.FromArgb(240, 240, 240);
                Color foreColor = Color.Black;
                using (var brush = new SolidBrush(backColor))
                    g.FillRectangle(brush, tabBounds);
                TextRenderer.DrawText(g, page.Text, tabs.Font, tabBounds, foreColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            tabs.TabPages.Add(CrearPaginaRtf("1. Introducción", GetInfoGeneral()));
            tabs.TabPages.Add(CrearPaginaRtf("2. Sintaxis Básica", GetSintaxisBasica()));
            tabs.TabPages.Add(CrearPaginaRtf("3. Palabras Reservadas", GetPalabrasReservadas()));
            tabs.TabPages.Add(CrearPaginaRtf("4. Ejemplos", GetEjemplos()));

            this.Controls.Add(tabs);
        }

        private TabPage CrearPaginaRtf(string titulo, string rtfContent)
        {
            TabPage page = new TabPage(titulo) { BackColor = Color.FromArgb(9, 6, 22) };
            RichTextBox rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Rtf = rtfContent,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(9, 6, 22),
                ForeColor = Color.FromArgb(240, 244, 255),
                Padding = new Padding(20)
            };
            page.Controls.Add(rtb);
            return page;
        }

        private string GetInfoGeneral() => @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
\b\fs24 ASTRA – Lenguaje para Simulación de Biosferas\b0\fs20\par
\par
\b ¿Qué es ASTRA?\b0\par
ASTRA (Astronomical Simulation & Terrain Research Algorithm) es un lenguaje de dominio específico (DSL) diseñado para modelar y simular biosferas virtuales. Permite definir planetas, atmósferas, cuerpos de agua y formas de vida, así como establecer reglas de evolución, cálculos y reportes.\par
\par
\b Propósito del compilador\b0\par
El compilador de ASTRA (implementado en C#) realiza cinco fases:\par
\tab • \b Análisis léxico\b0: reconoce palabras reservadas, identificadores, números, operadores, cadenas, símbolos y detecta errores (caracteres ilegales, números pegados a letras, cadenas sin cerrar).\par
\tab • \b Análisis sintáctico\b0: construye un árbol sintáctico (AST) según la gramática del lenguaje y valida la estructura (llaves balanceadas, presencia de ''inicio'' y ''fin'', etc.).\par
\tab • \b Análisis semántico\b0: verifica reglas de tipo, declara variables, comprueba unidades físicas, exige variables obligatorias (masa, radio, presion, co2, estado_liquido), detecta uso de variables no definidas y operaciones inválidas.\par
\tab • \b Tabla de símbolos\b0: almacena nombre, tipo, valor, unidad, línea y ámbito de cada variable y función.\par
\tab • \b Generación / ejecución\b0: interpreta el AST y muestra resultados en consola.\par
\par
\b Ejemplo mínimo funcional\b0\par
\f1\fs18 inicio\par
    simulacion {\par
        planeta {\par
            masa = 5.97e24 kg;\par
            radio = 6371 km;\par
        }\par
        atmosfera {\par
            presion = 1 atm;\par
            co2 = 400 ppm;\par
        }\par
        agua {\par
            estado_liquido = verdadero;\par
        }\par
        vida {\par
            // reglas de vida aquí\par
        }\par
    }\par
fin\f0\fs20\par
\par
Con ASTRA puedes simular desde un ecosistema simple hasta biosferas complejas con funciones definidas por el usuario, condicionales y bucles.\par
}";

        private string GetSintaxisBasica() => @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
\b\fs24 Sintaxis Básica de ASTRA\b0\fs20\par
\par
\b 1. Estructura general\b0\par
\f1 inicio\par
    simulacion {\par
        // Bloques obligatorios\par
        planeta { ... }\par
        atmosfera { ... }\par
        agua { ... }\par
        vida { ... }\par
        // También pueden haber funciones y sentencias globales\par
    }\par
fin\f0\par
\par
\b 2. Bloques obligatorios y variables requeridas\b0\par
\f1\tab • planeta\f0: \f1 masa\f0 (numérico, unidad ej. kg) y \f1 radio\f0 (numérico, km).\par
\f1\tab • atmosfera\f0: \f1 presion\f0 (numérico, atm) y \f1 co2\f0 (numérico, ppm).\par
\f1\tab • agua\f0: \f1 estado_liquido\f0 (booleano: verdadero/falso).\par
\f1\tab • vida\f0: sin variables obligatorias.\par
\par
\b 3. Tipos de datos\b0\par
\tab • \b Números\b0: enteros o decimales (42, 3.1416, 5.97e24).\par
\tab • \b Textos\b0: entre comillas dobles (""Hola mundo"").\par
\tab • \b Booleanos\b0: \f1 verdadero\f0, \f1 falso\f0.\par
\tab • \b Listas\b0: [1, 2, 3], [""a"", ""b""].\par
\tab • \b Nulo\b0: \f1 nulo\f0.\par
\par
\b 4. Operadores\b0\par
\tab • Aritméticos: \f1 +   -   *   /   ^\f0 (potencia).\par
\tab • Comparación: \f1 ==   !=   <   >   <=   >=\f0.\par
\tab • Lógicos: \f1 y   o\f0.\par
\par
\b 5. Unidades físicas (opcionales)\b0\par
Se pueden añadir a números: \f1 km, m, g, kg, atm, ppm, Sv, %, °C, h, s\f0.\par
Ejemplo: \f1 temperatura = 25 °C;\f0\par
\par
\b 6. Sentencias de control\b0\par
\tab • \b Asignación\b0: \f1 identificador = valor;\f0\par
\tab • \b Condicional\b0:\par
\f1\tab     si (condicion) {\par
\tab         ...\par
\tab     } sino {\par
\tab         ...\par
\tab     }\f0\par
\tab • \b Bucle mientras\b0:\par
\f1\tab     mientras (condicion) {\par
\tab         ...\par
\tab     }\f0\par
\tab • \b Mostrar\b0 (salida): \f1 mostrar(expresion);\f0 o \f1 mostrar expresion;\f0\par
\tab • \b Reporte\b0 (estadística): \f1 reporte(expresion);\f0\par
\tab • \b Definición de función\b0:\par
\f1\tab     funcion nombre(param1, param2) {\par
\tab         ...\par
\tab     }\f0\par
\tab • \b Llamada a función\b0: \f1 nombre(argumento1, argumento2);\f0\par
\par
\b 7. Comentarios\b0\par
\f1 // comentario de línea\par
/* comentario\par
   de bloque */\f0\par
}";

        private string GetPalabrasReservadas() => @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
\b\fs24 Palabras Reservadas de ASTRA\b0\fs20\par
\par
Estas palabras tienen un significado especial y \b no pueden usarse como identificadores\b0 (variables o funciones).\par
\par
\b Lista completa (según el analizador léxico)\b0\par
\f1\fs18
\b • Estructura de programa:\b0   inicio, fin, simulacion\par
\b • Bloques temáticos:\b0        planeta, atmosfera, agua, vida\par
\b • Control de flujo:\b0         si, sino, mientras, iterar, continuar, romper\par
\b • Salida:\b0                    mostrar, reporte\par
\b • Valores lógicos y nulo:\b0   verdadero, falso, nulo\par
\b • Operadores lógicos:\b0       y, o\par
\b • Funciones:\b0                 funcion\f0\fs20\par
\par
\b ⚠️ Notas importantes\b0\par
\tab • Todas las palabras reservadas deben escribirse en \b minúsculas\b0. Ejemplo: \f1 simulacion\f0 (válido), \f1 Simulacion\f0 (no reconocido).\par
\tab • No se pueden usar como identificadores:\par
\f1\tab     planeta = 5;\f0   \b ❌\b0\par
\f1\tab     miPlaneta = 5;\f0 \b ✅\b0\par
\par
\b Ejemplo de uso correcto\b0\par
\f1\fs18 inicio\par
    simulacion {\par
        planeta { masa = 1000 kg; }\par
    }\par
fin\f0\fs20\par
\par
\b Ejemplo de error léxico por mayúsculas\b0\par
\f1\fs18 Inicio\par
    Simulacion {\par
        Planeta { masa = 1000 kg; }  /* Error: 'Planeta' no es palabra reservada */\par
    }\par
Fin\f0\fs20\par
\par
El analizador léxico trataría \f1 Inicio\f0, \f1 Simulacion\f0, \f1 Planeta\f0 y \f1 Fin\f0 como identificadores normales, lo que provocaría errores sintácticos porque se esperaban las palabras reservadas en minúsculas.\par
}";

        private string GetEjemplos() => @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
\b\fs24 Ejemplos Completos en ASTRA\b0\fs20\par
\par
\b Ejemplo 1: Simulación básica con condicional\b0\par
\f1\fs18 inicio\par
    simulacion {\par
        planeta {\par
            masa = 1000 kg;\par
            radio = 100 km;\par
        }\par
        atmosfera {\par
            presion = 0.5 atm;\par
            co2 = 800 ppm;\par
        }\par
        agua {\par
            estado_liquido = falso;\par
        }\par
        vida {\par
            temperatura = 30;\par
            si (temperatura > 25) {\par
                mostrar(""Alerta: temperatura alta"");\par
            }\par
            reporte(""Condiciones extremas"");\par
        }\par
    }\par
fin\f0\fs20\par
\par
\b Ejemplo 2: Uso de bucle y función definida por usuario\b0\par
\f1\fs18 inicio\par
    simulacion {\par
        planeta { masa = 1.0 kg; radio = 1.0 km; }\par
        atmosfera { presion = 1.0 atm; co2 = 300 ppm; }\par
        agua { estado_liquido = verdadero; }\par
        vida {\par
            funcion cuadrado(x) {\par
                retorno = x * x;\par
            }\par
            contador = 0;\par
            mientras (contador < 5) {\par
                mostrar(cuadrado(contador));\par
                contador = contador + 1;\par
            }\par
        }\par
    }\par
fin\f0\fs20\par
\par
\b Ejemplo 3: Listas y operaciones con unidades\b0\par
\f1\fs18 inicio\par
    simulacion {\par
        planeta { masa = 5.97e24 kg; radio = 6371 km; }\par
        atmosfera { presion = 1.0 atm; co2 = 400 ppm; }\par
        agua { estado_liquido = verdadero; }\par
        vida {\par
            temperaturas = [15, 20, 25] °C;\par
            promedio = (temperaturas[0] + temperaturas[1] + temperaturas[2]) / 3;\par
            mostrar(""Promedio: "", promedio, ""°C"");\par
            si (promedio > 20) {\par
                reporte(""Clima cálido"");\par
            }\par
        }\par
    }\par
fin\f0\fs20\par
\par
\b Ejemplo 4: Errores semánticos (para probar el analizador)\b0\par
\f1\fs18 inicio\par
    simulacion {\par
        planeta {\par
            masa = 10 kg;\par
            // Falta 'radio' → error SEM-041\par
        }\par
        atmosfera {\par
            presion = 1 atm;\par
            // Falta 'co2' → error SEM-041\par
        }\par
        agua {\par
            estado_liquido = 42;   // error: debe ser booleano\par
        }\par
        vida {\par
            x = y + 2;   // error: 'y' no declarada (SEM-001)\par
        }\par
    }\par
fin\f0\fs20\par
\par
\b ¿Cómo probarlos?\b0\par
1. Copia cualquiera de los ejemplos en el editor.\par
2. Haz clic en \b Compilar\b0.\par
3. Revisa la ventana de \b tokens\b0, \b consola de salida\b0 y los errores (si los hay).\par
4. Si todo es correcto, se mostrará el árbol sintáctico y la tabla de símbolos.\par
}";
    }

    // ==================================================================================
    // EDITOR PRINCIPAL CON NUEVA INTERFAZ GRÁFICA (Properly Studio)
    // ==================================================================================
    public class BioSphereEditor : Form
    {
        private System.Windows.Forms.Integration.ElementHost editorHost = null!;
        private ICSharpCode.AvalonEdit.TextEditor txtCodigo = null!;
        private SyntaxColorizer syntaxColorizer = null!;
        private CommentColorizer commentColorizer = null!;
        private ErrorSquiggleRenderer errorRenderer = null!;
        private System.Windows.Forms.Timer parseTimer = null!;
        private ToolTip errorToolTip = null!;
        private List<ErrorInfo> currentErrors = new List<ErrorInfo>();

        private RichTextBox txtConsola = null!;

        // Pestañas UI
        private Panel panelContenidoPestañas = null!;
        private Panel btnTabCompilador = null!;
        private Panel btnTabArboles = null!;
        private Panel btnTabDocumentacion = null!;
        private Panel panelCompilador = null!;
        private VisualizadorASTPanel panelArboles = null!;
        private PanelDocumentacion panelDocumentacion = null!;

        private CheckBox chkModoPrueba = null!;
        private Panel statusPanel = null!;
        private Label lblErrorCount = null!;
        private System.Windows.Forms.Timer errorTimer = null!;

        private NodoPrograma? ultimoAST;

        // Paleta Nebulosa (Basada en la imagen proporcionada)
        private static readonly Color ColRichBlack = Color.FromArgb(9, 6, 22);
        private static readonly Color ColDarkGreen = Color.FromArgb(19, 14, 38);
        private static readonly Color ColBangladesh = Color.FromArgb(30, 26, 69);
        private static readonly Color ColMeadow = Color.FromArgb(255, 140, 46);
        private static readonly Color ColCaribbean = Color.FromArgb(46, 230, 255);
        private static readonly Color ColAntiFlash = Color.FromArgb(240, 244, 255);
        private static readonly Color ColPine = Color.FromArgb(15, 12, 30);
        private static readonly Color ColBasil = Color.FromArgb(25, 20, 50);
        private static readonly Color ColForest = Color.FromArgb(230, 42, 77);
        private static readonly Color ColStone = Color.FromArgb(162, 169, 196);
        private static readonly Color ColPistachio = Color.FromArgb(211, 216, 239);

        public BioSphereEditor()
        {
            this.Text = "ASTRA IDE — Astronomical Simulation & Terrain Research Algorithm";
            this.Size = new Size(1440, 900);
            this.MinimumSize = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColRichBlack;
            this.ForeColor = ColAntiFlash;
            this.DoubleBuffered = true;

            // ════════ TOPBAR ════════
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = ColDarkGreen };
            topPanel.Paint += (s, e) =>
            {
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    topPanel.ClientRectangle, ColDarkGreen, ColBangladesh, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, topPanel.ClientRectangle);
                using var pen = new Pen(ColCaribbean, 2);
                e.Graphics.DrawLine(pen, 0, topPanel.Height - 2, topPanel.Width, topPanel.Height - 2);
            };

            var lblTitle = new Label { Text = "ASTRA", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = ColCaribbean, Location = new Point(14, 10), Size = new Size(135, 44), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            var lblSub = new Label { Text = "IDE", Font = new Font("Segoe UI", 8, FontStyle.Regular), ForeColor = ColPistachio, Location = new Point(116, 26), Size = new Size(32, 18), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            var sep = new Panel { Location = new Point(152, 14), Size = new Size(2, 36), BackColor = ColBangladesh };

            chkModoPrueba = new CheckBox { Text = "  Modo Prueba", ForeColor = ColPistachio, Font = new Font("Segoe UI", 10), Location = new Point(170, 20), AutoSize = true, Checked = false };
            chkModoPrueba.CheckedChanged += ChkModoPrueba_CheckedChanged;

            topPanel.Controls.Add(lblTitle); topPanel.Controls.Add(lblSub); topPanel.Controls.Add(sep); topPanel.Controls.Add(chkModoPrueba);

            // ════════ STATUSBAR ════════
            statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = ColPine };
            statusPanel.Paint += (s, e) =>
            {
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(statusPanel.ClientRectangle, ColPine, ColBasil, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, statusPanel.ClientRectangle);
                using var pen = new Pen(ColBangladesh, 1);
                e.Graphics.DrawLine(pen, 0, 0, statusPanel.Width, 0);
            };
            lblErrorCount = new Label { Text = "●  Sin errores", ForeColor = ColCaribbean, Font = new Font("Segoe UI", 9, FontStyle.Regular), Location = new Point(12, 6), AutoSize = true };
            var lblVersion = new Label { Text = "ASTRA DSL v1.0 — Compiladores", ForeColor = ColStone, Font = new Font("Segoe UI", 8), Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0), AutoSize = true };
            statusPanel.Controls.Add(lblErrorCount); statusPanel.Controls.Add(lblVersion);

            // ════════ SPLIT PRINCIPAL ════════
            var mainSplit = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 750, Orientation = Orientation.Vertical, BackColor = ColBangladesh, SplitterWidth = 4 };
            var rightSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 350, BackColor = ColBangladesh, SplitterWidth = 4 };

            // == DERECHA: Simulación y Consola ==
            var simulacionPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColRichBlack, Padding = new Padding(8) };
            var simInner = new Panel { Dock = DockStyle.Fill, BackColor = ColDarkGreen };
            AplicarBordesRedondeados(simInner, 15, ColDarkGreen);
            simInner.Controls.Add(new Label { Text = "Área reservada para motor 3D", ForeColor = ColStone, Font = new Font("Segoe UI", 12, FontStyle.Italic), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
            simInner.Controls.Add(new Label { Text = "  ◈  simulación del planeta", ForeColor = ColMeadow, Font = new Font("Consolas", 10, FontStyle.Bold), Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.MiddleLeft });
            simulacionPanel.Controls.Add(simInner);

            var consolaPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColRichBlack, Padding = new Padding(8) };
            var consolaInner = new Panel { Dock = DockStyle.Fill, BackColor = ColDarkGreen };
            AplicarBordesRedondeados(consolaInner, 15, ColDarkGreen);
            txtConsola = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10), BackColor = ColDarkGreen, ForeColor = ColAntiFlash, ReadOnly = true, BorderStyle = BorderStyle.None };
            var consolaTxtContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15, 0, 15, 15) };
            consolaTxtContainer.Controls.Add(txtConsola);
            consolaInner.Controls.Add(consolaTxtContainer);
            consolaInner.Controls.Add(new Label { Text = "  ◈  consola de salida", ForeColor = ColMeadow, Font = new Font("Consolas", 10, FontStyle.Bold), Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.MiddleLeft });
            consolaPanel.Controls.Add(consolaInner);

            rightSplit.Panel1.Controls.Add(simulacionPanel);
            rightSplit.Panel2.Controls.Add(consolaPanel);

            // == IZQUIERDA: Pestañas en la parte inferior ==
            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColRichBlack, Padding = new Padding(0) };

            // Barra de pestañas en el borde INFERIOR
            var tabsHeaderPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = ColRichBlack,
                WrapContents = false,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Panel de contenido con padding ajustado (sin padding abajo para fusión visual)
            panelContenidoPestañas = new Panel { Dock = DockStyle.Fill, BackColor = ColRichBlack, Padding = new Padding(10, 10, 10, 0) };
            var contenidoInner = new Panel { Dock = DockStyle.Fill, BackColor = ColDarkGreen };
            AplicarBordesRedondeados(contenidoInner, 15, ColDarkGreen);
            panelContenidoPestañas.Controls.Add(contenidoInner);

            // Orden correcto: Fill primero, Bottom después
            leftPanel.Controls.Add(panelContenidoPestañas);
            leftPanel.Controls.Add(tabsHeaderPanel);
            tabsHeaderPanel.BringToFront();

            // Pestaña 1: Código
            panelCompilador = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            editorHost = new System.Windows.Forms.Integration.ElementHost { Dock = DockStyle.Fill };
            txtCodigo = new ICSharpCode.AvalonEdit.TextEditor
            {
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 14,
                ShowLineNumbers = true,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(19, 14, 38)),
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 244, 255))
            };
            txtCodigo.TextArea.TextView.CurrentLineBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 46, 230, 255));
            txtCodigo.TextArea.TextView.CurrentLineBorder = new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(60, 45, 123, 255)), 1);

            syntaxColorizer = new SyntaxColorizer(); commentColorizer = new CommentColorizer();
            txtCodigo.TextArea.TextView.LineTransformers.Add(syntaxColorizer); txtCodigo.TextArea.TextView.LineTransformers.Add(commentColorizer);
            errorRenderer = new ErrorSquiggleRenderer(txtCodigo); txtCodigo.TextArea.TextView.BackgroundRenderers.Add(errorRenderer);

            parseTimer = new System.Windows.Forms.Timer { Interval = 600 }; parseTimer.Tick += ParseTimer_Tick;
            txtCodigo.TextChanged += (s, e) => { parseTimer.Stop(); parseTimer.Start(); };
            errorToolTip = new ToolTip { AutoPopDelay = 8000, InitialDelay = 0, ReshowDelay = 0, ShowAlways = true, BackColor = Color.FromArgb(230, 42, 77), ForeColor = Color.FromArgb(255, 255, 255) };
            txtCodigo.TextArea.TextView.MouseHover += TextView_MouseHover; txtCodigo.TextArea.TextView.MouseHoverStopped += TextView_MouseHoverStopped;
            editorHost.Child = txtCodigo; panelCompilador.Controls.Add(editorHost);

            // Pestaña 2: Árboles
            panelArboles = new VisualizadorASTPanel { Dock = DockStyle.Fill, Padding = new Padding(5) };

            // Pestaña 3: Documentación
            panelDocumentacion = new PanelDocumentacion { Dock = DockStyle.Fill, Padding = new Padding(5) };

            contenidoInner.Controls.Add(panelCompilador);
            contenidoInner.Controls.Add(panelArboles);
            contenidoInner.Controls.Add(panelDocumentacion);

            btnTabCompilador = CrearBotonTab("💻 Código", panelCompilador);
            btnTabArboles = CrearBotonTab("🌳 Árboles", panelArboles);
            btnTabDocumentacion = CrearBotonTab("📖 Documentación", panelDocumentacion);

            tabsHeaderPanel.Controls.Add(btnTabCompilador);
            tabsHeaderPanel.Controls.Add(btnTabArboles);
            tabsHeaderPanel.Controls.Add(btnTabDocumentacion);

            ActivarTab(btnTabCompilador, panelCompilador);
            mainSplit.Panel1.Controls.Add(leftPanel); mainSplit.Panel2.Controls.Add(rightSplit);

            mainSplit.Paint += (s, e) => { e.Graphics.FillRectangle(new SolidBrush(ColBangladesh), mainSplit.SplitterRectangle); };
            rightSplit.Paint += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(ColBangladesh), rightSplit.SplitterRectangle);
                int cx = rightSplit.SplitterRectangle.X + rightSplit.SplitterRectangle.Width / 2, cy = rightSplit.SplitterRectangle.Y + rightSplit.SplitterRectangle.Height / 2;
                using var pen = new Pen(ColStone, 1); e.Graphics.DrawLine(pen, cx - 20, cy, cx + 20, cy);
            };
            mainSplit.SplitterMoved += (s, e) => mainSplit.Invalidate(); rightSplit.SplitterMoved += (s, e) => rightSplit.Invalidate();

            this.Controls.Add(topPanel); this.Controls.Add(statusPanel); this.Controls.Add(mainSplit);

            // =====================================================================
            // CORRECCIÓN DE DOCKING Y Z-ORDER (Añadir al final del constructor)
            // =====================================================================

            // 1. Corregir el panel izquierdo (para que se vean las pestañas abajo)
            tabsHeaderPanel.SendToBack();          // Obliga a la barra inferior a recortar su espacio primero
            panelContenidoPestañas.BringToFront(); // Obliga al editor a ocupar solo lo que sobra

            // 2. Corregir el formulario principal (para que no se tape la línea 1)
            topPanel.SendToBack();                 // La barra superior (ASTRA IDE) toma su espacio
            statusPanel.SendToBack();              // La barra de estado toma su espacio
            mainSplit.BringToFront();              // El divisor principal respeta a ambas y se ajusta al centro

            CargarCodigoCorrecto();
            errorTimer = new System.Windows.Forms.Timer { Interval = 600 }; errorTimer.Tick += (s, e) => ActualizarContadorErrores(); errorTimer.Start();
        }

        // ==================== METODOS DE INTERFAZ ====================

        private Panel CrearBotonTab(string texto, Panel targetPanel)
        {
            var btn = new Panel
            {
                Text = texto,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(175, 44),
                BackColor = ColRichBlack,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 4, 0)
            };

            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(ColRichBlack);

                bool isActive = btn.BackColor == ColDarkGreen;
                Color tabBg = isActive ? ColDarkGreen : Color.FromArgb(22, 17, 45);
                Color textCol = isActive ? ColAntiFlash : ColStone;

                int r = 12;
                int w = btn.Width;
                int h = btn.Height;

                // Path para pestañas inferiores: techo recto, base redondeada
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddLine(0, 0, w - 1, 0); // Línea superior recta
                path.AddArc(w - r * 2 - 1, h - r * 2 - 1, r * 2, r * 2, 0, 90); // Esquina inferior derecha
                path.AddArc(1, h - r * 2 - 1, r * 2, r * 2, 90, 90); // Esquina inferior izquierda
                path.CloseFigure();

                using var brush = new SolidBrush(tabBg);
                g.FillPath(brush, path);

                // Acento en la base para pestaña activa
                if (isActive)
                {
                    using var accentPen = new Pen(ColCaribbean, 2.5f);
                    g.DrawLine(accentPen, r + 1, h - 1, w - r - 1, h - 1);
                }

                TextRenderer.DrawText(g, btn.Text, btn.Font, new Rectangle(0, 0, w, h), textCol,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            btn.Click += (s, e) => ActivarTab(btn, targetPanel);
            return btn;
        }

        private void ActivarTab(Panel botonActivo, Panel panelActivo)
        {
            btnTabCompilador.BackColor = ColRichBlack;
            btnTabArboles.BackColor = ColRichBlack;
            btnTabDocumentacion.BackColor = ColRichBlack;

            botonActivo.BackColor = ColDarkGreen;

            btnTabCompilador.Invalidate();
            btnTabArboles.Invalidate();
            btnTabDocumentacion.Invalidate();

            panelCompilador.Visible = false;
            panelArboles.Visible = false;
            panelDocumentacion.Visible = false;

            panelActivo.Visible = true;
            panelActivo.BringToFront();
        }

        private void AplicarBordesRedondeados(Control ctrl, int radius, Color backColor)
        {
            ctrl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var brush = new SolidBrush(backColor);
                e.Graphics.FillRoundRectangle(brush, new RectangleF(0, 0, ctrl.Width, ctrl.Height), radius);
            };
        }

        // ==================== MÉTODOS Y LOGICA DE EVENTOS ====================

        private void ActualizarContadorErrores()
        {
            if (currentErrors != null)
                if (currentErrors.Count > 0)
                { lblErrorCount.Text = $"●  {currentErrors.Count} error(es) detectado(s)"; lblErrorCount.ForeColor = Color.FromArgb(255, 100, 80); }
                else
                { lblErrorCount.Text = "●  Sin errores"; lblErrorCount.ForeColor = ColCaribbean; }
        }

        private void ParseTimer_Tick(object? sender, EventArgs e)
        {
            parseTimer.Stop();
            try
            {
                Lexer lexer = new Lexer(txtCodigo.Text);
                var tokens = lexer.Tokenize();
                var erroresLexicos = lexer.Errores ?? new List<ErrorInfo>();

                syntaxColorizer.UpdateTokens(tokens);

                txtConsola.Clear();

                if (erroresLexicos.Count > 0)
                {
                    currentErrors = erroresLexicos;
                    errorRenderer.UpdateErrors(currentErrors);
                    txtCodigo.TextArea.TextView.Redraw();
                    ultimoAST = null;

                    txtConsola.SelectionColor = Color.FromArgb(255, 100, 80);
                    txtConsola.AppendText($"❌ {erroresLexicos.Count} error(es) léxico(s)\n\n");
                    foreach (var err in erroresLexicos)
                    {
                        txtConsola.SelectionColor = Color.FromArgb(255, 140, 100);
                        txtConsola.AppendText($"  • Línea {err.Line}, Col {err.Column}: {err.Message}\n");
                    }
                    return;
                }

                var parser = new Parser(tokens);
                var (programa, erroresSintacticos) = parser.ParsePrograma();

                if (erroresSintacticos.Count > 0)
                {
                    currentErrors = erroresSintacticos;
                    errorRenderer.UpdateErrors(currentErrors);
                    txtCodigo.TextArea.TextView.Redraw();
                    ultimoAST = null;

                    txtConsola.SelectionColor = Color.FromArgb(255, 100, 80);
                    txtConsola.AppendText($"❌ {erroresSintacticos.Count} error(es) sintáctico(s)\n\n");
                    foreach (var err in erroresSintacticos)
                    {
                        txtConsola.SelectionColor = Color.FromArgb(255, 140, 100);
                        txtConsola.AppendText($"  • Línea {err.Line}, Col {err.Column}: {err.Message}\n");
                    }
                    return;
                }

                ultimoAST = programa;

                var semantic = new SemanticAnalyzer();
                var (semSuccess, semanticErrors) = semantic.Analyze(programa!);

                if (!semSuccess)
                {
                    currentErrors = semanticErrors;
                    errorRenderer.UpdateErrors(currentErrors);
                    txtCodigo.TextArea.TextView.Redraw();

                    txtConsola.SelectionColor = Color.FromArgb(255, 100, 80);
                    txtConsola.AppendText($"❌ {semanticErrors.Count} error(es) semántico(s)\n\n");
                    foreach (var err in semanticErrors)
                    {
                        txtConsola.SelectionColor = Color.FromArgb(255, 140, 100);
                        txtConsola.AppendText($"  • Línea {err.Line}, Col {err.Column}: {err.Message}\n");
                    }
                    return;
                }

                currentErrors = new List<ErrorInfo>();
                errorRenderer.UpdateErrors(currentErrors);
                txtCodigo.TextArea.TextView.Redraw();

                txtConsola.SelectionColor = ColMeadow;
                txtConsola.AppendText("══════════════════════════════════════\n");
                txtConsola.SelectionColor = ColCaribbean;
                txtConsola.AppendText("  ✅ COMPILACIÓN EXITOSA\n");
                txtConsola.SelectionColor = ColPistachio;
                txtConsola.AppendText("  Léxico ✓  │  Sintáctico ✓  │  Semántico ✓\n");
                txtConsola.SelectionColor = ColMeadow;
                txtConsola.AppendText("══════════════════════════════════════\n\n");

                txtConsola.SelectionColor = ColMeadow;
                txtConsola.AppendText("📖 ÁRBOL SINTÁCTICO:\n\n");
                txtConsola.SelectionColor = ColAntiFlash;
                txtConsola.AppendText(programa!.ToTreeString("", true));

                var arbolesVisual = new List<NodoASTVisual>();
                var arbolGlobal = ConvertirAST(programa);
                if (arbolGlobal != null) arbolesVisual.Add(arbolGlobal);
                var subArboles = ExtraerSubArboles(programa);
                arbolesVisual.AddRange(subArboles);
                panelArboles.ActualizarArboles(arbolesVisual);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error parseando: {ex.Message}"); }
        }

        private void TextView_MouseHover(object sender, System.Windows.Input.MouseEventArgs e)
        {
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
                    errorToolTip.ToolTipTitle = found.Type == ErrorType.Lexico ? "Error léxico" : found.Type == ErrorType.Semantico ? "Error semántico" : "Error estructural";
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
            txtCodigo.Text = @"inicio
simulacion {
    gravedad = 0;
    planeta {
        masa = 5;
        radio = 6371;
        gravedad = (6 * masa) / (radio ^ 2);
    }
    atmosfera {
        presion = 0.006;
        co2 = 95;
    }
    agua {
        estado_liquido = falso;
    }
    vida {
        si (gravedad > 8 y gravedad < 12) {
            mostrar(""Gravedad óptima para terraformación"");
        } sino {
            reporte(""Gravedad extrema, ajustar simulación"");
        }
        mientras (co2 > 0) {
            co2 = co2 - 1;
            mostrar(""Reduciendo CO2"");
        }
    }
}
fin";
        }

        private void CargarCodigoConErrores()
        {
            txtCodigo.Text = @"inicio
simulacion {
    // 1. ERROR LÉXICO: identificador que comienza con número
    123planeta = 5;

    planeta {
        masa = 5;
        radio = 6371;
        // ERROR LÉXICO: símbolo no permitido
        @$# = 10;
    }

    atmosfera {
        presion = 0.006
        co2 = 95;    // ERROR SINTÁCTICO: falta ';' después de 0.006
    }

    agua {
        estado_liquido = falso;
    }

    vida {
        // ERROR SEMÁNTICO: variable 'gravedad' no declarada
        si (gravedad > 8) {
            mostrar(""Condición correcta"");
        }
        // ERROR SEMÁNTICO: operador aritmético con texto
        resultado = ""Hola"" + 5;
    }
}
fin";
        }

        private void ChkModoPrueba_CheckedChanged(object? sender, EventArgs e)
        {
            if (chkModoPrueba.Checked) CargarCodigoConErrores();
            else CargarCodigoCorrecto();
        }

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
                    if (par.Expr != null)
                        visual.Hijos.Add(ConvertirAST(par.Expr)!);
                    break;
                case NodoCondicion cond:
                    if (cond.Izquierda != null)
                        visual.Hijos.Add(ConvertirAST(cond.Izquierda)!);
                    if (!string.IsNullOrEmpty(cond.Operador) && cond.Derecha != null)
                    {
                        visual.Hijos.Add(new NodoASTVisual(cond.Operador, "Operador"));
                        visual.Hijos.Add(ConvertirAST(cond.Derecha)!);
                    }
                    foreach (var (op, sub) in cond.OperadoresLogicos)
                    {
                        visual.Hijos.Add(new NodoASTVisual(op, "Operador"));
                        var subVisual = ConvertirAST(sub);
                        if (subVisual != null) visual.Hijos.Add(subVisual);
                    }
                    break;
                case NodoCantidad cant:
                    if (cant.Expr != null)
                        visual.Hijos.Add(ConvertirAST(cant.Expr)!);
                    if (cant.Unidad != null)
                        visual.Hijos.Add(new NodoASTVisual(cant.Unidad, "Identificador"));
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

            void Recorrer(NodoAST nodo)
            {
                if (nodo == null) return;

                NodoASTVisual? subVisual = null;
                if (nodo is NodoExprBinaria binaria && EsExpresionCompleja(binaria))
                    subVisual = ConvertirAST(binaria);
                else if (nodo is NodoExprPotencia potencia && EsExpresionCompleja(potencia))
                    subVisual = ConvertirAST(potencia);
                else if (nodo is NodoExprParentesis parentesis && EsExpresionCompleja(parentesis.Expr))
                    subVisual = ConvertirAST(parentesis);
                else if (nodo is NodoIf nodoIf)
                    subVisual = ConvertirAST(nodoIf);
                else if (nodo is NodoWhile nodoWhile)
                    subVisual = ConvertirAST(nodoWhile);

                if (subVisual != null)
                    subArboles.Add(subVisual);

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
                        contador++;
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
}