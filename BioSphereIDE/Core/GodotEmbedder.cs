// ============================================================================
//  GodotEmbedder.cs
//  Clase auxiliar para incrustar la ventana de Godot 4 dentro de un Panel
//  de Windows Forms mediante Win32 P/Invoke.
//
//  Uso:
//    var embedder = new GodotEmbedder(simRightInner);
//    await embedder.LanzarYIncrustarAsync(rutaExe, argsExtra);
//    embedder.Dispose();  // al cerrar el formulario
// ============================================================================

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BioSphereIDE.Core
{
    /// <summary>
    /// Gestiona el ciclo de vida del proceso de Godot e incrusta su ventana
    /// nativa dentro de un <see cref="Panel"/> de Windows Forms.
    /// </summary>
    public sealed class GodotEmbedder : IDisposable
    {
        // ── Win32 API ────────────────────────────────────────────────────────
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y,
                                              int width, int height, bool repaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Índices y flags de SetWindowLong
        private const int GWL_STYLE   = -16;
        private const int GWL_EXSTYLE = -20;

        // Estilos de ventana que se eliminan para lograr incrustación sin bordes
        private const int WS_CAPTION     = 0x00C00000; // Barra de título
        private const int WS_THICKFRAME  = 0x00040000; // Borde redimensionable
        private const int WS_BORDER      = 0x00800000; // Borde simple
        private const int WS_SYSMENU     = 0x00080000; // Menú del sistema
        private const int WS_MINIMIZE    = 0x00020000;
        private const int WS_MAXIMIZE    = 0x01000000;
        private const int WS_EX_DLGFRAME = 0x00000001;
        private const int WS_EX_WINDOWEDGE = 0x00000100;
        private const int WS_EX_CLIENTEDGE = 0x00000200;

        private const int SW_SHOW = 5;

        // ── Estado interno ───────────────────────────────────────────────────
        private readonly Panel        _targetPanel;
        private Process?              _godotProcess;
        private IntPtr                _godotHwnd = IntPtr.Zero;
        private bool                  _disposed   = false;

        /// <summary>
        /// Ruta al archivo JSON de parámetros en %TEMP%.
        /// Debe coincidir con JsonPath en AstraWatcher.cs / astra_watcher.gd.
        /// </summary>
        public static readonly string RutaJsonTemp =
            Path.Combine(Path.GetTempPath(), "astra_planeta.json");

        // Opciones de serialización reutilizables (creación costosa, una sola vez)
        private static readonly JsonSerializerOptions _jsonOpts =
            new() { WriteIndented = true };

        // ── Constructor ──────────────────────────────────────────────────────

        /// <param name="targetPanel">Panel de WinForms donde se incrustará Godot.</param>
        public GodotEmbedder(Panel targetPanel)
        {
            _targetPanel = targetPanel ?? throw new ArgumentNullException(nameof(targetPanel));
        }

        // ────────────────────────────────────────────────────────────────────
        //  API pública
        // ────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Serializa los parámetros del planeta al archivo JSON temporal que
        /// Godot monitorea con FileSystemWatcher.
        /// </summary>
        public static void EscribirJson(PlanetaParametros p)
        {
            string json = JsonSerializer.Serialize(p, _jsonOpts);
            File.WriteAllText(RutaJsonTemp, json);
        }

        /// <summary>
        /// Lanza el proceso de Godot, espera a que su ventana aparezca,
        /// elimina sus bordes nativos y la incrusta en <c>_targetPanel</c>.
        /// </summary>
        /// <param name="rutaExeGodot">Ruta al ejecutable visualizador_astra.exe.</param>
        /// <param name="argumentosExtra">Argumentos adicionales para el proceso.</param>
        public async Task LanzarYIncrustarAsync(string rutaExeGodot,
                                                string argumentosExtra = "")
        {
            if (_godotProcess != null && !_godotProcess.HasExited)
                return; // Ya está corriendo

            var psi = new ProcessStartInfo
            {
                FileName               = rutaExeGodot,
                Arguments              = argumentosExtra,
                UseShellExecute        = false,
                CreateNoWindow         = false,
                WindowStyle            = ProcessWindowStyle.Minimized
            };

            _godotProcess = Process.Start(psi)
                            ?? throw new InvalidOperationException(
                                   "No se pudo iniciar el proceso de Godot.");

            // Esperar a que Godot tenga una ventana visible (timeout 15s)
            _godotHwnd = await EsperarVentanaAsync(_godotProcess, TimeSpan.FromSeconds(15));

            if (_godotHwnd == IntPtr.Zero)
                throw new TimeoutException("Godot no mostró su ventana en el tiempo esperado.");

            // Quitar bordes y decoraciones de la ventana de Godot
            EliminarBordesTomados(_godotHwnd);

            // Reparentar la ventana al panel de WinForms
            SetParent(_godotHwnd, _targetPanel.Handle);

            // Ajustar al tamaño actual del panel
            AjustarTamano();

            // Suscribir al evento Resize del panel para reposicionar en tiempo real
            _targetPanel.Resize += Panel_Resize;

            ShowWindow(_godotHwnd, SW_SHOW);
        }

        /// <summary>
        /// Termina el proceso de Godot de forma segura.
        /// </summary>
        public void Detener()
        {
            try
            {
                if (_godotProcess != null && !_godotProcess.HasExited)
                    _godotProcess.Kill();
            }
            catch { /* ignorar si ya terminó */ }
        }

        // ────────────────────────────────────────────────────────────────────
        //  Métodos privados de Win32
        // ────────────────────────────────────────────────────────────────────

        private static void EliminarBordesTomados(IntPtr hwnd)
        {
            int estilo = GetWindowLong(hwnd, GWL_STYLE);
            estilo &= ~(WS_CAPTION | WS_THICKFRAME | WS_BORDER | WS_SYSMENU
                        | WS_MINIMIZE | WS_MAXIMIZE);
            SetWindowLong(hwnd, GWL_STYLE, estilo);

            int estiloEx = GetWindowLong(hwnd, GWL_EXSTYLE);
            estiloEx &= ~(WS_EX_DLGFRAME | WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
            SetWindowLong(hwnd, GWL_EXSTYLE, estiloEx);
        }

        private void AjustarTamano()
        {
            if (_godotHwnd == IntPtr.Zero) return;
            var sz = _targetPanel.ClientSize;
            MoveWindow(_godotHwnd, 0, 0, sz.Width, sz.Height, true);
        }

        private void Panel_Resize(object? sender, EventArgs e) => AjustarTamano();

        // ────────────────────────────────────────────────────────────────────
        //  Espera activa para detectar la ventana de Godot
        // ────────────────────────────────────────────────────────────────────

        private static async Task<IntPtr> EsperarVentanaAsync(Process proceso,
                                                               TimeSpan timeout)
        {
            var cts   = new CancellationTokenSource(timeout);
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    proceso.Refresh();
                    IntPtr hwnd = proceso.MainWindowHandle;
                    if (hwnd != IntPtr.Zero)
                        return hwnd;
                }
                catch { /* el proceso aún puede estar iniciando */ }

                await Task.Delay(200, token).ContinueWith(_ => { }); // no lanzar al cancelar
            }

            return IntPtr.Zero;
        }

        // ────────────────────────────────────────────────────────────────────
        //  IDisposable
        // ────────────────────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _targetPanel.Resize -= Panel_Resize;
            Detener();
            _godotProcess?.Dispose();
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    //  DTO: Parámetros que el compilador envía a Godot vía JSON
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Modelo de datos plano que se serializa en <c>astra_planeta.json</c>.
    /// Los nombres de propiedad coinciden exactamente con los @export de planet.gd.
    /// </summary>
    public sealed class PlanetaParametros
    {
        // Geometría
        public float radius                 { get; set; } = 6371.0f;
        public float terrain_height         { get; set; } = 16.0f;
        public float atmosphere_height      { get; set; } = 222.0f;

        // Masa y órbita
        public float planet_mass            { get; set; } = 1.0f;
        public float star_distance_au       { get; set; } = 1.0f;
        public float rotation_period_hours  { get; set; } = 24.0f;

        // Clima
        public float planet_temp            { get; set; } = 15.0f;
        public float planet_water           { get; set; } = 0.71f;
        public float tectonic_activity      { get; set; } = 0.4f;

        // Atmósfera
        public float atm_pressure           { get; set; } = 1.0f;
        public float atm_co2                { get; set; } = 0.0004f;
        public float atm_methane            { get; set; } = 0.000001f;
        public float atm_o2_n2              { get; set; } = 0.99f;

        // Composición
        public float composition_iron       { get; set; } = 0.35f;
        public float planet_vegetation      { get; set; } = 0.8f;
    }
}
