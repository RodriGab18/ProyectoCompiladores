using System;
using System.Windows.Forms;
using BioSphereIDE.UI; // Importamos el espacio de nombres de tu editor visual

namespace BioSphereIDE
{
          internal static class Program
          {
                    /// <summary>
                    /// Punto de entrada principal para el compilador ASTRA y su IDE.
                    /// </summary>
                    [STAThread]
                    static void Main()
                    {
                              Application.EnableVisualStyles();
                              Application.SetCompatibleTextRenderingDefault(false);

                              // Aquí instanciamos y arrancamos tu interfaz gráfica
                              Application.Run(new BioSphereEditor());
                    }
          }
}