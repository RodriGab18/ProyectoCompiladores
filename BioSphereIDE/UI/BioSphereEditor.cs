using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using BioSphereIDE.Core;
using BioSphereIDE.Analizadores;

namespace BioSphereIDE.UI
{
    // ==================================================================================
    // VENTANA DE DOCUMENTACIÓN
    // ==================================================================================
    public class FrmDocumentacion : Form
    {
        public FrmDocumentacion()
        {
            this.Text = "Manual de Usuario - ASTRA DSL & IDE";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.ForeColor = Color.Black;
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
            tabs.TabPages.Add(CrearPaginaRtf("5. Integración Godot", GetIntegracionGodot()));

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

        internal static string GetInfoGeneral() => @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
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

        internal static string GetSintaxisBasica() => @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
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

        internal static string GetPalabrasReservadas() => @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
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

        internal static string GetEjemplos() => @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
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

        private string GetIntegracionGodot() => @"{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Consolas;}}
\b\fs24 Godot Planetary Simulation - API Integration Guide\b0\fs20\par
\par
Este documento detalla la interfaz de parámetros del simulador planetario en Godot.\par
\par
\b Esquema de Parámetros (JSON Schema)\b0\par
\tab • \b radius\b0: [100.0, 10000.0]\par
\tab • \b planet_mass\b0: [0.1, 20.0]\par
\tab • \b star_distance_au\b0: [0.1, 5.0]\par
\tab • \b rotation_period_hours\b0: [1.0, 1000.0]\par
\tab • \b planet_temp\b0: [-100.0, 500.0]\par
\tab • \b atm_pressure\b0: [0.0, 2.0]\par
\tab • \b atm_co2\b0: [0.0, 1.0]\par
\tab • \b atm_methane\b0: [0.0, 1.0]\par
\tab • \b atm_o2_n2\b0: [0.0, 1.0]\par
\tab • \b planet_water\b0: [0.0, 1.0]\par
\tab • \b tectonic_activity\b0: [0.0, 1.0]\par
\tab • \b composition_iron\b0: [0.0, 1.0]\par
\tab • \b planet_vegetation\b0: [0.0, 1.0]\par
\par
Para inyectar esto en el compilador, añade un bloque \f1 orbita_y_escala \{ ... \}\f0 dentro de simulacion.\par
}";
    }
    // ==================================================================================
    // EDITOR PRINCIPAL CON INTERFAZ UNIFICADA (Nebulosa + Pestañas Inferiores)
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

        private DataGridView gridTokens = null!;
        private RichTextBox txtConsola = null!;

        // Pestañas UI (Tu diseño)
        private Panel panelContenidoPestañas = null!;
        private Panel btnTabCodigo = null!;
        private Panel btnTabTokens = null!;
        private Panel btnTabDocumentacion = null!;
        private Panel panelCodigo = null!;
        private Panel panelTokens = null!;
        private Panel panelDocumentacion = null!;

        private CheckBox chkModoPrueba = null!;
        private Panel statusPanel = null!;
        private Label lblErrorCount = null!;
        private System.Windows.Forms.Timer errorTimer = null!;

        private NodoPrograma? ultimoAST;

        // ── Logo toolbar ──────────────────────────────────────────────────────────
        /// <summary>Bitmap del logo ASTRA cacheado; dibujado en topPanel.Paint.</summary>
        private Bitmap? _logoBmp;

        // ── Integración con Godot ─────────────────────────────────────────────
        /// <summary>Panel donde se incrustará la ventana de Godot.</summary>
        private Panel simRightInner = null!;
        /// <summary>Instancia del embedder Win32. Null hasta que se lanza la simulación.</summary>
        private GodotEmbedder? _godotEmbedder;
        // Rutas configuradas por el usuario
        private const string RutaEjeGodot =
            @"C:\Users\Botij\Downloads\Godot_v4.6.2-stable_win64.exe\Godot_v4.6.2-stable_win64.exe";
        private const string RutaProyectoGodot =
            @"C:/Users/Botij/OneDrive/Documents/godot-cuberact-planet-chunked-lod-main";

        // Paleta Nebulosa (Tus colores)
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
                // Gradiente actual
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    topPanel.ClientRectangle, ColDarkGreen, ColBangladesh,
                    System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, topPanel.ClientRectangle);
                using var pen = new Pen(ColCaribbean, 2);
                e.Graphics.DrawLine(pen, 0, topPanel.Height - 2, topPanel.Width, topPanel.Height - 2);

                // Dibujar logo (nuevo)
                Rectangle logoRect = new Rectangle(8, 0, 64, 64);
                if (_logoBmp != null)
                {
                    e.Graphics.DrawImage(_logoBmp, logoRect);
                }
                else
                {
                    // Fallback opcional
                    using var fallbackBrush = new SolidBrush(ColCaribbean);
                    e.Graphics.FillEllipse(fallbackBrush, logoRect);
                    using var font = new Font("Segoe UI", 16, FontStyle.Bold);
                    e.Graphics.DrawString("A", font, Brushes.White, logoRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
            };

            // ── Logo ASTRA ────────────────────────────────────────────────────────
            // Cargar logo desde ruta absoluta
            string rutaLogo = @"C:\Users\Botij\OneDrive\Documents\V SEMESTRE\PROYECTO_FINAL_COMPILADORES\ProyectoCompiladores\BioSphereIDE\Resources\Logo.png";
            try
            {
                if (File.Exists(rutaLogo))
                {
                    _logoBmp = new Bitmap(rutaLogo);
                    _logoBmp.MakeTransparent(Color.White);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Logo no encontrado: {rutaLogo}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando logo: {ex.Message}");
            }
            // Todo lo demás desplazado 38 px a la derecha para dar espacio al logo
            var lblTitle = new Label { Text = "ASTRA", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = ColCaribbean, Location = new Point(52, 10), Size = new Size(120, 44), AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
            var sep = new Panel { Location = new Point(190, 14), Size = new Size(2, 36), BackColor = ColBangladesh };

            // Botones de toolbar (herencia del colaborador)
            Button CrearBotonToolbar(string emoji, string tooltip, int x)
            {
                var btn = new Button
                {
                    Text = emoji,
                    Font = new Font("Segoe UI Emoji", 15),
                    Location = new Point(x, 10),
                    Size = new Size(44, 44),
                    BackColor = Color.Transparent,
                    ForeColor = ColAntiFlash,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    TabStop = false
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, ColMeadow);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, ColMeadow);
                new ToolTip().SetToolTip(btn, tooltip);
                topPanel.Controls.Add(btn);
                return btn;
            }

            var btnArboles       = CrearBotonToolbar("🌳",  "Ver árbol sintáctico (AST)",        200);
            var btnSimular       = CrearBotonToolbar("▶",  "Simular planeta en Godot",           248);
            // Dar al botón de simular un color especial para que destaque
            btnSimular.ForeColor        = ColMeadow;
            btnSimular.Font             = new Font("Segoe UI", 16, FontStyle.Bold);
            btnSimular.FlatAppearance.MouseOverBackColor  = Color.FromArgb(60, 46, 230, 255);
            btnSimular.FlatAppearance.MouseDownBackColor  = Color.FromArgb(120, 46, 230, 255);

            chkModoPrueba = new CheckBox { Text = "  Modo Prueba", ForeColor = ColPistachio, Font = new Font("Segoe UI", 10), Location = new Point(354, 20), AutoSize = true, Checked = false };
            chkModoPrueba.CheckedChanged += ChkModoPrueba_CheckedChanged;

            topPanel.Controls.Add(lblTitle); topPanel.Controls.Add(sep); topPanel.Controls.Add(chkModoPrueba);


            btnArboles.Click       += BtnArboles_Click;
            btnSimular.Click       += BtnSimular_ClickAsync;

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
            var mainSplit = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 750, Orientation = Orientation.Vertical, BackColor = ColRichBlack, SplitterWidth = 6 };

            // == PANEL DERECHO: Simulación (Arriba) + Consola (Abajo) ==
            var rightSplit = new SplitContainer 
            { 
                Dock = DockStyle.Fill, 
                Orientation = Orientation.Horizontal, 
                FixedPanel = FixedPanel.Panel2, // Mantiene la consola (abajo) fija al redimensionar, el planeta crece
                BackColor = ColRichBlack, 
                SplitterWidth = 6 
            };

            // Simulación (Planeta) — simRightInner guardado como campo para el embedder
            var simulacionRightPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColRichBlack, Padding = new Padding(8) };
            simRightInner = new Panel { Dock = DockStyle.Fill, BackColor = ColDarkGreen };
            AplicarBordesRedondeados(simRightInner, 15, ColDarkGreen);
            var lblSimPlaceholder = new Label
            {
                Text      = "Presiona  ▶  para iniciar la simulación en Godot",
                ForeColor = ColStone,
                Font      = new Font("Segoe UI", 12, FontStyle.Italic),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Name      = "lblSimPlaceholder"
            };
            var lblSimHeader = new Label
            {
                Text      = "  ◈  simulación de biósfera",
                ForeColor = ColMeadow,
                Font      = new Font("Consolas", 10, FontStyle.Bold),
                Dock      = DockStyle.Top,
                Height    = 35,
                TextAlign = ContentAlignment.MiddleLeft,
                Name      = "lblSimHeader"
            };
            simRightInner.Controls.Add(lblSimPlaceholder);
            simRightInner.Controls.Add(lblSimHeader);
            simulacionRightPanel.Controls.Add(simRightInner);
            // Limpiar al cerrar para no dejar el proceso de Godot huérfano
            this.FormClosing += (s, e) => _godotEmbedder?.Dispose();

            // Consola
            var consolaPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColRichBlack, Padding = new Padding(8) };
            var consolaInner = new Panel { Dock = DockStyle.Fill, BackColor = ColDarkGreen };
            AplicarBordesRedondeados(consolaInner, 15, ColDarkGreen);
            txtConsola = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10), BackColor = ColDarkGreen, ForeColor = ColAntiFlash, ReadOnly = true, BorderStyle = BorderStyle.None };
            var consolaTxtContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15, 0, 15, 15) };
            consolaTxtContainer.Controls.Add(txtConsola);
            consolaInner.Controls.Add(consolaTxtContainer);
            consolaInner.Controls.Add(new Label { Text = "  ◈  consola de salida", ForeColor = ColMeadow, Font = new Font("Consolas", 10, FontStyle.Bold), Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.MiddleLeft });
            consolaPanel.Controls.Add(consolaInner);

            rightSplit.Panel1.Controls.Add(simulacionRightPanel);
            rightSplit.Panel2.Controls.Add(consolaPanel);

            // == PANEL IZQUIERDO: Pestañas inferiores (Tu diseño) ==
            // Reemplaza TODA la sección "== PANEL IZQUIERDO: Pestañas inferiores (Tu diseño) =="
            // en el constructor de BioSphereEditor con este código:

            // == PANEL IZQUIERDO: Pestañas inferiores (CORREGIDO) ==
            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColRichBlack, Padding = new Padding(0) };

            // Barra de pestañas en el borde INFERIOR
            var tabsHeaderPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = ColRichBlack,
                WrapContents = false,
                Padding = new Padding(10, 0, 0, 0),
                Margin = new Padding(0)
            };

            // Panel de contenido con padding ajustado
            panelContenidoPestañas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColRichBlack,
                Padding = new Padding(10, 10, 10, 0),
                Margin = new Padding(0)
            };
            // Ensamblar las pestañas en el panel izquierdo principal
            leftPanel.Controls.Add(panelContenidoPestañas);
            leftPanel.Controls.Add(tabsHeaderPanel);

            // ── PANEL IZQUIERDO: Editor ─────────────────────────────────────
            var editorLayout = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                BackColor = ColRichBlack,
                RowCount = 2,
                ColumnCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            editorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            editorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panelCodigo = editorLayout;

            var editorHeader = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0), BackColor = ColDarkGreen };
            editorHeader.Paint += (s, e) =>
            {
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    editorHeader.ClientRectangle, ColDarkGreen, ColRichBlack,
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(brush, editorHeader.ClientRectangle);
            };
            var lblEditorTab = new Label
            {
                Text = "  ◈  editor.astra",
                ForeColor = ColMeadow,
                Font = new Font("Consolas", 9, FontStyle.Regular),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0)
            };
            editorHeader.Controls.Add(lblEditorTab);

            // AvalonEdit (Dock Fill después del header)
            editorHost = new System.Windows.Forms.Integration.ElementHost
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };

            txtCodigo = new ICSharpCode.AvalonEdit.TextEditor
            {
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 14,
                ShowLineNumbers = true,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(19, 14, 38)),
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 244, 255)),
                Padding = new System.Windows.Thickness(0) // Sin padding para no perder líneas
            };
            txtCodigo.TextArea.TextView.CurrentLineBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 46, 230, 255));
            txtCodigo.TextArea.TextView.CurrentLineBorder = new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(60, 45, 123, 255)), 1);

            syntaxColorizer = new SyntaxColorizer();
            commentColorizer = new CommentColorizer();
            txtCodigo.TextArea.TextView.LineTransformers.Add(syntaxColorizer);
            txtCodigo.TextArea.TextView.LineTransformers.Add(commentColorizer);
            errorRenderer = new ErrorSquiggleRenderer(txtCodigo);
            txtCodigo.TextArea.TextView.BackgroundRenderers.Add(errorRenderer);

            parseTimer = new System.Windows.Forms.Timer { Interval = 600 };
            parseTimer.Tick += ParseTimer_Tick;
            txtCodigo.TextChanged += (s, e) => { parseTimer.Stop(); parseTimer.Start(); };
            errorToolTip = new ToolTip
            {
                AutoPopDelay = 8000,
                InitialDelay = 0,
                ReshowDelay = 0,
                ShowAlways = true,
                BackColor = Color.FromArgb(230, 42, 77),
                ForeColor = Color.FromArgb(255, 255, 255)
            };
            txtCodigo.TextArea.TextView.MouseHover += TextView_MouseHover;
            txtCodigo.TextArea.TextView.MouseHoverStopped += TextView_MouseHoverStopped;
            editorHost.Child = txtCodigo;

            // Ensamblaje Panel Izquierdo
            editorHost.Dock = DockStyle.Fill;
            editorHost.Margin = new Padding(0);
            editorLayout.Controls.Add(editorHeader, 0, 0);
            editorLayout.Controls.Add(editorHost, 0, 1);

            // ── PANEL DERECHO: Tokens + Consola + Simulación ─────────────────
            var rightSplitTop = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 150, // Fijo a 150px para tokens
                BackColor = ColDarkGreen,
                SplitterWidth = 3
            };

            var rightSplitBottom = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 150, // Fijo a 150px para consola
                BackColor = ColDarkGreen,
                SplitterWidth = 3
            };

            // == 1. SECCIÓN TOKENS ==
            var tokensPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColRichBlack };
            var tokensHeader = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = ColDarkGreen };
            tokensHeader.Paint += (s, e) =>
            {
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(tokensHeader.ClientRectangle, ColBangladesh, ColDarkGreen, System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(brush, tokensHeader.ClientRectangle);
            };
            var lblTokensTab = new Label { Text = "  ◈  tokens", ForeColor = ColMeadow, Font = new Font("Consolas", 9), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            tokensHeader.Controls.Add(lblTokensTab);

            // Pestaña 2: Tokens
            panelTokens = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            gridTokens = new DataGridView
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = ColDarkGreen,
                GridColor = ColBangladesh,
                BorderStyle = BorderStyle.None,
                Font = new Font("Consolas", 9),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                DefaultCellStyle = { BackColor = ColDarkGreen, ForeColor = ColAntiFlash, SelectionBackColor = ColForest, SelectionForeColor = ColCaribbean, Font = new Font("Consolas", 9) },
                ColumnHeadersDefaultCellStyle = { BackColor = ColBangladesh, ForeColor = ColMeadow, Font = new Font("Segoe UI", 9, FontStyle.Bold), SelectionBackColor = ColBangladesh },
                EnableHeadersVisualStyles = false,
                AlternatingRowsDefaultCellStyle = { BackColor = ColBangladesh, ForeColor = ColAntiFlash, SelectionBackColor = ColForest, SelectionForeColor = ColCaribbean }
            };
            gridTokens.Columns[0].Name = "Tipo"; gridTokens.Columns[1].Name = "Lexema";
            gridTokens.Columns[2].Name = "Línea"; gridTokens.Columns[3].Name = "Columna";
            gridTokens.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            gridTokens.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            panelTokens.Controls.Add(gridTokens);

            // Pestaña 3: Documentación
            panelDocumentacion = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = ColRichBlack };
            
            Panel docInner = new Panel { Dock = DockStyle.Fill, BackColor = ColDarkGreen };
            AplicarBordesRedondeados(docInner, 12, ColDarkGreen);

            FlowLayoutPanel docHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = ColDarkGreen,
                Padding = new Padding(5, 5, 5, 0),
                WrapContents = false
            };

            Panel docContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = ColDarkGreen };

            RichTextBox rtb1 = new RichTextBox { Dock = DockStyle.Fill, Rtf = FrmDocumentacion.GetInfoGeneral(), ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = ColDarkGreen, ForeColor = ColAntiFlash };
            RichTextBox rtb2 = new RichTextBox { Dock = DockStyle.Fill, Rtf = FrmDocumentacion.GetSintaxisBasica(), ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = ColDarkGreen, ForeColor = ColAntiFlash, Visible = false };
            RichTextBox rtb3 = new RichTextBox { Dock = DockStyle.Fill, Rtf = FrmDocumentacion.GetPalabrasReservadas(), ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = ColDarkGreen, ForeColor = ColAntiFlash, Visible = false };
            RichTextBox rtb4 = new RichTextBox { Dock = DockStyle.Fill, Rtf = FrmDocumentacion.GetEjemplos(), ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = ColDarkGreen, ForeColor = ColAntiFlash, Visible = false };

            docContainer.Controls.Add(rtb1);
            docContainer.Controls.Add(rtb2);
            docContainer.Controls.Add(rtb3);
            docContainer.Controls.Add(rtb4);

            docInner.Controls.Add(docContainer);
            docInner.Controls.Add(docHeader);
            panelDocumentacion.Controls.Add(docInner);

            Panel? docBotonActivo = null;

            Panel CrearBotonDocTab(string label, RichTextBox target)
            {
                Panel btn = new Panel { Height = 26, Width = 140, BackColor = ColRichBlack, Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0) };
                btn.Paint += (s, e) =>
                {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    bool active = (docBotonActivo == btn);
                    Color bg = active ? ColBangladesh : ColRichBlack;
                    Color fg = active ? ColAntiFlash : ColStone;
                    using (SolidBrush b = new SolidBrush(bg))
                    {
                        g.FillRoundRectangle(b, new RectangleF(0, 0, btn.Width, btn.Height), 6);
                    }
                    TextRenderer.DrawText(g, label, new Font("Segoe UI", 9, FontStyle.Bold), btn.ClientRectangle, fg,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                };
                btn.Click += (s, e) =>
                {
                    docBotonActivo = btn;
                    foreach (Control ctrl in docHeader.Controls) ctrl.Invalidate();
                    
                    rtb1.Visible = (target == rtb1);
                    rtb2.Visible = (target == rtb2);
                    rtb3.Visible = (target == rtb3);
                    rtb4.Visible = (target == rtb4);
                };
                return btn;
            }

            Panel btn1 = CrearBotonDocTab("1. Introducción", rtb1);
            Panel btn2 = CrearBotonDocTab("2. Sintaxis", rtb2);
            Panel btn3 = CrearBotonDocTab("3. Palabras", rtb3);
            Panel btn4 = CrearBotonDocTab("4. Ejemplos", rtb4);

            docBotonActivo = btn1;

            docHeader.Controls.Add(btn1);
            docHeader.Controls.Add(btn2);
            docHeader.Controls.Add(btn3);
            docHeader.Controls.Add(btn4);

            // Agregar paneles al contenido
            panelContenidoPestañas.Controls.Add(panelCodigo);
            panelContenidoPestañas.Controls.Add(panelTokens);
            panelContenidoPestañas.Controls.Add(panelDocumentacion);

            // Crear botones de pestañas
            btnTabCodigo = CrearBotonTab("💻 Código", panelCodigo);
            btnTabTokens = CrearBotonTab("🔍 Tokens", panelTokens);
            btnTabDocumentacion = CrearBotonTab("📖 Documentación", panelDocumentacion);

            tabsHeaderPanel.Controls.Add(btnTabCodigo);
            tabsHeaderPanel.Controls.Add(btnTabTokens);
            tabsHeaderPanel.Controls.Add(btnTabDocumentacion);

            // Activar pestaña inicial y forzar repintado
            ActivarTab(btnTabCodigo, panelCodigo);

            // ⚡ FORZAR REPINTADO COMPLETO DEL LAYOUT
            this.Load += (s, e) =>
            {
                leftPanel.PerformLayout();
                panelContenidoPestañas.PerformLayout();
                tabsHeaderPanel.PerformLayout();
                this.PerformLayout();
                
                // Asegurar que la consola solo sobresalga un poco (~220px) en la parte inferior
                if (rightSplit.Height > 250)
                {
                    rightSplit.SplitterDistance = rightSplit.Height - 220;
                }
            };

            mainSplit.Panel1.Controls.Add(leftPanel);
            mainSplit.Panel2.Controls.Add(rightSplit);

            // ════════ CONFIGURACIÓN DE SPLITTERS ════════
            mainSplit.Paint += (s, e) => { e.Graphics.FillRectangle(new SolidBrush(ColRichBlack), mainSplit.SplitterRectangle); };
            rightSplit.Paint += (s, e) => { e.Graphics.FillRectangle(new SolidBrush(ColRichBlack), rightSplit.SplitterRectangle); };
            mainSplit.SplitterMoved += (s, e) => mainSplit.Invalidate();
            rightSplit.SplitterMoved += (s, e) => rightSplit.Invalidate();

            // ════════ AGREGAR CONTROLES AL FORMULARIO ════════
            // En WinForms, el control Dock.Fill debe agregarse PRIMERO al contenedor
            // para que ocupe el espacio restante sin superponerse a los controles Dock.Top/Bottom.
            this.Controls.Add(mainSplit);     // Dock Fill
            this.Controls.Add(statusPanel);   // Dock Bottom
            this.Controls.Add(topPanel);      // Dock Top

            CargarCodigoCorrecto();

            errorTimer = new System.Windows.Forms.Timer { Interval = 600 };
            errorTimer.Tick += (s, e) => ActualizarContadorErrores();
            errorTimer.Start();
        }

        // ==================== MÉTODOS DE INTERFAZ (Tu diseño de pestañas) ====================

        private Panel CrearBotonTab(string texto, Panel targetPanel)
        {
            var btn = new Panel
            {
                Text = texto,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(175, 44),
                BackColor = ColRichBlack,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 4, 0),
                MinimumSize = new Size(175, 44),
                MaximumSize = new Size(175, 44) // Tamaño fijo para evitar deformaciones
            };

            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Limpiar fondo
                using (var clearBrush = new SolidBrush(ColRichBlack))
                    g.FillRectangle(clearBrush, btn.ClientRectangle);

                bool isActive = btn.BackColor == ColDarkGreen;
                Color tabBg = isActive ? ColDarkGreen : Color.FromArgb(22, 17, 45);
                Color textCol = isActive ? ColAntiFlash : ColStone;

                int r = 12;
                int w = btn.Width - 1;
                int h = btn.Height - 1;

                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddLine(0, 0, w, 0);
                path.AddArc(w - r * 2, h - r * 2, r * 2, r * 2, 0, 90);
                path.AddArc(0, h - r * 2, r * 2, r * 2, 90, 90);
                path.CloseFigure();

                using var brush = new SolidBrush(tabBg);
                g.FillPath(brush, path);

                if (isActive)
                {
                    using var accentPen = new Pen(ColCaribbean, 2.5f);
                    g.DrawLine(accentPen, r + 1, h, w - r - 1, h);
                }

                TextRenderer.DrawText(g, btn.Text, btn.Font, new Rectangle(0, 0, w, h), textCol,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            };

            btn.Click += (s, e) => ActivarTab(btn, targetPanel);
            return btn;
        }

        private void ActivarTab(Panel botonActivo, Panel panelActivo)
        {
            if (botonActivo.Parent != null)
            {
                foreach (Control c in botonActivo.Parent.Controls)
                    if (c is Panel p && p != botonActivo) { p.BackColor = ColRichBlack; p.Invalidate(); }
            }

            botonActivo.BackColor = ColDarkGreen;
            botonActivo.Invalidate();

            panelCodigo.Visible = false;
            panelTokens.Visible = false;
            panelDocumentacion.Visible = false;

            panelActivo.Visible = true;
            panelActivo.BringToFront();
        }

        private TabPage CrearPaginaRtfInterna(string titulo, string rtfContent)
        {
            TabPage page = new TabPage(titulo) { BackColor = ColDarkGreen };
            RichTextBox rtb = new RichTextBox { Dock = DockStyle.Fill, Rtf = rtfContent, ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = ColDarkGreen, ForeColor = ColAntiFlash, Padding = new Padding(20) };
            page.Controls.Add(rtb);
            return page;
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

        // ── Botón ▶ Simular Planeta ──────────────────────────────────────────

        /// <summary>
        /// Extrae los parámetros del planeta del último AST analizado y los
        /// convierte a <see cref="PlanetaParametros"/> que se guarda en JSON.
        /// </summary>
        private PlanetaParametros ExtraerParametros()
        {
            var p = new PlanetaParametros(); // valores por defecto del DTO

            if (ultimoAST?.BloqueSimulacion == null)
                return p;

            foreach (var sent in ultimoAST.BloqueSimulacion.Sentencias)
            {
                if (sent is NodoOrbitaYEscala orbita)
                {
                    foreach (var instruccion in orbita.Instrucciones)
                    {
                        if (instruccion is NodoAsignacion asig)
                        {
                            float v = ExtraerFloat(asig.Valor);
                            if (float.IsNaN(v)) continue;

                            switch (asig.Identificador)
                            {
                                case "radius":                p.radius                = v; break;
                                case "terrain_height":        p.terrain_height        = v; break;
                                case "atmosphere_height":     p.atmosphere_height     = v; break;
                                case "planet_mass":           p.planet_mass           = v; break;
                                case "star_distance_au":      p.star_distance_au      = v; break;
                                case "rotation_period_hours": p.rotation_period_hours = v; break;
                                case "planet_temp":           p.planet_temp           = v; break;
                                case "atm_pressure":          p.atm_pressure          = v; break;
                                case "atm_co2":               p.atm_co2               = v; break;
                                case "atm_methane":           p.atm_methane           = v; break;
                                case "atm_o2_n2":             p.atm_o2_n2             = v; break;
                                case "planet_water":          p.planet_water          = v; break;
                                case "tectonic_activity":     p.tectonic_activity     = v; break;
                                case "composition_iron":      p.composition_iron      = v; break;
                                case "planet_vegetation":     p.planet_vegetation     = v; break;
                            }
                        }
                    }
                }
            }
            return p;
        }

        /// <summary>
        /// Convierte un nodo de expresión a float.
        /// Maneja: NodoExprNumero (literal), NodoCantidad (con unidad), NodoExprUnaria (negativo).
        /// Devuelve NaN si el nodo no representa un valor numérico.
        /// </summary>
        private static float ExtraerFloat(NodoAST? nodo)
        {
            // Desenvolver cantidad con unidad física (ej. "5.97e24 kg")
            if (nodo is NodoCantidad cant)
                nodo = cant.Expr;

            // Número negativo: -(expresion)
            if (nodo is NodoExprUnaria unaria && unaria.Operador == "-")
            {
                float inner = ExtraerFloat(unaria.Operando);
                return float.IsNaN(inner) ? float.NaN : -inner;
            }

            // Literal numérico
            if (nodo is NodoExprNumero num &&
                float.TryParse(num.Valor,
                               System.Globalization.NumberStyles.Float,
                               System.Globalization.CultureInfo.InvariantCulture,
                               out float resultado))
                return resultado;

            return float.NaN;
        }

        /// <summary>
        /// Handler del botón ▶ — compila, genera JSON, incrusta Godot o actualiza
        /// el planeta si ya está corriendo.
        /// </summary>
        private async void BtnSimular_ClickAsync(object? sender, EventArgs e)
        {
            // 1. Verificar que no haya errores de compilación
            if (currentErrors != null && currentErrors.Count > 0)
            {
                MessageBox.Show(
                    "No se puede iniciar la simulación porque existen errores en el código ASTRA.\n" +
                    "Corrige los errores indicados en la consola y vuelve a intentarlo.",
                    "Error de Compilación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (ultimoAST == null)
            {
                MessageBox.Show(
                    "El código no ha sido analizado aún. Escribe o modifica el código para compilarlo.",
                    "Sin compilación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // 2. Extraer parámetros del AST y escribir el JSON en %TEMP%
            var parametros = ExtraerParametros();
            try
            {
                GodotEmbedder.EscribirJson(parametros);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo escribir el archivo de parámetros:\n{ex.Message}",
                                "Error de E/S", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3a. Si Godot ya está incrustado, el FileSystemWatcher de Godot
            //     detectará el cambio del JSON automáticamente → no hay que relanzar.
            if (_godotEmbedder != null)
            {
                // Mostrar feedback en consola
                txtConsola.SelectionColor = ColCaribbean;
                txtConsola.AppendText("\n▶  Parámetros actualizados → Godot recargará el planeta.\n");
                return;
            }

            // 3b. Primera vez: limpiar labels provisionales e incrustar Godot
            // Eliminar los labels de placeholder
            var ctrlsAEliminar = simRightInner.Controls
                .OfType<Label>()
                .Where(l => l.Name == "lblSimPlaceholder" || l.Name == "lblSimHeader")
                .ToList();
            foreach (var lbl in ctrlsAEliminar)
                simRightInner.Controls.Remove(lbl);

            // Construir argumentos: ruta al proyecto Godot + ruta al JSON
            string argsGodot = $"--path \"{RutaProyectoGodot}\" -- "
                             + $"--astra_json=\"{GodotEmbedder.RutaJsonTemp}\"";

            txtConsola.SelectionColor = ColMeadow;
            txtConsola.AppendText("\n▶  Iniciando simulación en Godot...\n");

            try
            {
                _godotEmbedder = new GodotEmbedder(simRightInner);
                await _godotEmbedder.LanzarYIncrustarAsync(RutaEjeGodot, argsGodot);

                txtConsola.SelectionColor = ColCaribbean;
                txtConsola.AppendText("  ✅ Godot incrustado correctamente.\n");
                txtConsola.SelectionColor = ColPistachio;
                txtConsola.AppendText($"  JSON → {GodotEmbedder.RutaJsonTemp}\n");
            }
            catch (TimeoutException)
            {
                _godotEmbedder?.Dispose();
                _godotEmbedder = null;
                // Restaurar los labels
                simRightInner.Controls.Add(new Label { Text = "Presiona  ▶  para iniciar la simulación en Godot", ForeColor = ColStone, Font = new Font("Segoe UI", 12, FontStyle.Italic), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Name = "lblSimPlaceholder" });
                simRightInner.Controls.Add(new Label { Text = "  ◈  simulación de biósfera", ForeColor = ColMeadow, Font = new Font("Consolas", 10, FontStyle.Bold), Dock = DockStyle.Top, Height = 35, TextAlign = ContentAlignment.MiddleLeft, Name = "lblSimHeader" });

                MessageBox.Show("Godot tardó demasiado en responder. Verifica la ruta del ejecutable.",
                                "Tiempo de espera agotado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                _godotEmbedder?.Dispose();
                _godotEmbedder = null;
                MessageBox.Show($"Error al lanzar Godot:\n{ex.Message}",
                                "Error de Ejecución", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

                gridTokens.Rows.Clear();
                foreach (var token in tokens)
                {
                    string tokenName = token.Type.ToString().Replace("_", " ");
                    int rowIndex = gridTokens.Rows.Add(tokenName, token.Lexeme, token.Line, token.Column);
                    if (token.Type == TokenType.ERROR_LEXICO)
                        gridTokens.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(60, 10, 10);
                }

                txtConsola.Clear();

                if (erroresLexicos.Count > 0)
                {
                    currentErrors = erroresLexicos;
                    errorRenderer.UpdateErrors(currentErrors);
                    txtCodigo.TextArea.TextView.Redraw();
                    ultimoAST = null;
                    MostrarErrores("LÉXICO", erroresLexicos);
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
                    MostrarErrores("SINTÁCTICO", erroresSintacticos);
                    return;
                }

                ultimoAST = programa;

                var semantic = new SemanticAnalyzer();
                var (semSuccess, semanticErrors) = semantic.Analyze(programa!);

                var errores = semanticErrors.Where(e => e.Severity == BioSphereIDE.Core.Severity.Error).ToList();
                var advertencias = semanticErrors.Where(e => e.Severity == BioSphereIDE.Core.Severity.Warning).ToList();

                if (errores.Count > 0)
                {
                    currentErrors = errores;
                    errorRenderer.UpdateErrors(currentErrors);
                    txtCodigo.TextArea.TextView.Redraw();
                    MostrarErrores("SEMÁNTICO", errores);
                    if (advertencias.Count > 0) MostrarAdvertencias(advertencias);
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

                if (advertencias.Count > 0) MostrarAdvertencias(advertencias);

                txtConsola.SelectionColor = ColMeadow;
                txtConsola.AppendText("📖 ÁRBOL SINTÁCTICO:\n\n");
                txtConsola.SelectionColor = ColAntiFlash;
                txtConsola.AppendText(programa!.ToTreeString("", true));
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error parseando: {ex.Message}"); }
        }

        private void MostrarErrores(string fase, List<ErrorInfo> errores)
        {
            txtConsola.SelectionColor = Color.FromArgb(255, 80, 60);
            txtConsola.AppendText($"❌ ERROR {fase} — {errores.Count} error(es)\n");
            txtConsola.SelectionColor = ColMeadow;
            txtConsola.AppendText("──────────────────────────────────────\n");
            foreach (var err in errores)
            {
                string code = string.IsNullOrEmpty(err.Code) ? "" : $"[{err.Code}] ";
                txtConsola.SelectionColor = Color.FromArgb(255, 130, 100);
                txtConsola.AppendText($"  • Línea {err.Line}, Col {err.Column}: {code}{err.Message}\n");
                if (!string.IsNullOrEmpty(err.Suggestion))
                {
                    txtConsola.SelectionColor = ColPistachio;
                    txtConsola.AppendText($"    → {err.Suggestion}\n");
                }
            }
            txtConsola.AppendText("\n");
        }

        private void MostrarAdvertencias(List<ErrorInfo> warns)
        {
            txtConsola.SelectionColor = Color.FromArgb(255, 200, 80);
            txtConsola.AppendText($"⚠️ ADVERTENCIAS — {warns.Count} advertencia(s)\n");
            foreach (var w in warns)
            {
                string code = string.IsNullOrEmpty(w.Code) ? "" : $"[{w.Code}] ";
                txtConsola.SelectionColor = Color.FromArgb(255, 220, 120);
                txtConsola.AppendText($"  • Línea {w.Line}, Col {w.Column}: {code}{w.Message}\n");
                if (!string.IsNullOrEmpty(w.Suggestion))
                {
                    txtConsola.SelectionColor = ColPistachio;
                    txtConsola.AppendText($"    → {w.Suggestion}\n");
                }
            }
            txtConsola.AppendText("\n");
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
                    string code = string.IsNullOrEmpty(found.Code) ? "" : $"[{found.Code}]  ";
                    string sugg = string.IsNullOrEmpty(found.Suggestion) ? "" : $"\n→ {found.Suggestion}";
                    string msg = $"L{found.Line}:{found.Column}  {code}{found.Message}{sugg}";
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

            txtCodigo.Text =
@"inicio
simulacion {

    // ── Bloque de integración con Godot ────────────────
    // Parámetros para un planeta tipo Tierra habitable
    orbita_y_escala {
        radius = 6371.0;
        planet_mass = 1.0;
        star_distance_au = 1.0;
        rotation_period_hours = 24.0;
        planet_temp = 15.0;
        atm_pressure = 1.0;
        atm_co2 = 0.0004;
        atm_methane = 0.000001;
        atm_o2_n2 = 0.99;
        planet_water = 0.71;
        tectonic_activity = 0.4;
        composition_iron = 0.35;
        planet_vegetation = 0.8;
    }

    // ── Variables globales y de evaluación ─────────────
    ciclos = 0;
    habitables = 0;

    // ── Bloque planeta ────────────────────────────────
    planeta {
        masa = 5.97e24 kg;
        radio = 6371 km;
    }

    // ── Bloque atmósfera ──────────────────────────────
    atmosfera {
        presion = 1.0 atm;
        co2 = 415 ppm;
    }

    // ── Bloque agua ───────────────────────────────────
    agua {
        estado_liquido = verdadero;
    }

    // ── Funciones auxiliares ──────────────────────────
    funcion evaluarHabitabilidad(g, temp, tiene_agua) {
        si (g > 8 y g < 12) {
            si (temp > 0 y temp < 40) {
                si (tiene_agua == verdadero) {
                    mostrar(""El planeta reúne todas las condiciones para albergar vida."");
                }
            }
        } sino {
            mostrar(""El planeta presenta condiciones extremas que dificultan la vida."");
        }
    }

    // ── Bloque vida ───────────────────────────────────
    vida {
        temperatura_actual = 15;
        
        mientras (ciclos < 5) {
            ciclos = ciclos + 1;
            
            si (temperatura_actual >= 10 y temperatura_actual <= 30) {
                habitables = habitables + 1;
                reporte(""Clima estable"");
            } sino {
                reporte(""Fluctuación climática severa"");
            }
            
            // Incrementamos un poco la temperatura
            temperatura_actual = temperatura_actual + 2;
        }

        reporte(""Total de ciclos en rango habitable:"");
        reporte(habitables);


        // Llamamos a la función auxiliar simulando una gravedad terrestre 9.8m/s2
        evaluarHabitabilidad(9.8, 15, verdadero);
        
        // Operación con variables
        indice_biologico = (habitables * 100) / ciclos;
        mostrar(""Índice biológico estimado (%):"");
        mostrar(indice_biologico);
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

        private void BtnDocumentacion_Click(object? sender, EventArgs e)
        {
            using (FrmDocumentacion doc = new FrmDocumentacion()) doc.ShowDialog(this);
        }

        private void BtnArboles_Click(object? sender, EventArgs e)
        {
            if (ultimoAST == null)
            {
                MessageBox.Show("No hay un árbol sintáctico válido.\nPrimero analiza un código sin errores.",
                                "Visor de AST", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var arbolesVisual = new List<NodoASTVisual>();
            var arbolGlobal = ConvertirAST(ultimoAST);
            if (arbolGlobal != null) arbolesVisual.Add(arbolGlobal);
            var subArboles = ExtraerSubArboles(ultimoAST);
            arbolesVisual.AddRange(subArboles);

            using (var visor = new FrmVisualizadorArbol(arbolesVisual))
            {
                visor.ShowDialog(this);
            }
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
                case NodoOrbitaYEscala orbita: etiqueta = "Órbita y Escala"; tipo = "OrbitaYEscala"; break;
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
                case NodoExprUnaria una: etiqueta = una.Operador; tipo = "Operador"; break;
                case NodoCantidad cant: etiqueta = cant.ToString()!; tipo = "Cantidad"; break;
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
                case NodoOrbitaYEscala orbita:
                    foreach (var s in orbita.Instrucciones) visual.Hijos.Add(ConvertirAST(s)!);
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
                case NodoExprUnaria una:
                    if (una.Operando != null)
                        visual.Hijos.Add(ConvertirAST(una.Operando)!);
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