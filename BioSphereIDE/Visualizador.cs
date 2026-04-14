using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace BioSphereIDE
{
          public class NodoASTVisual
          {
                    public string Etiqueta { get; set; }
                    public string TipoElemento { get; set; }
                    public List<NodoASTVisual> Hijos { get; set; } = new List<NodoASTVisual>();

                    public NodoASTVisual(string etiqueta, string tipo)
                    {
                              Etiqueta = etiqueta;
                              TipoElemento = tipo;
                    }
          }

          public class FrmVisualizadorArbol : Form
          {
                    private List<NodoASTVisual> arboles;
                    private int indiceActual = 0;
                    private Panel canvas;
                    private float zoom = 1.0f;
                    private Label lblEstado;

                    public FrmVisualizadorArbol(List<NodoASTVisual> arbolesAnalizados)
                    {
                              this.arboles = arbolesAnalizados;
                              this.Text = "Visor de Árboles Sintácticos (AST) - Fase 2";
                              this.Size = new Size(1000, 700);
                              this.StartPosition = FormStartPosition.CenterParent;
                              this.BackColor = Color.FromArgb(30, 30, 30);
                              this.ShowIcon = false;
                              ConfigurarInterfaz();
                    }

                    private void ConfigurarInterfaz()
                    {
                              canvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 245, 245) };
                              canvas.Paint += (s, e) => DibujarArbol(e.Graphics);
                              this.MouseWheel += (s, e) =>
                              {
                                        zoom += e.Delta > 0 ? 0.1f : -0.1f;
                                        zoom = Math.Max(0.5f, Math.Min(3.0f, zoom));
                                        canvas.Invalidate();
                              };

                              Panel nav = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.FromArgb(45, 45, 48) };
                              Button btnAnt = CrearBoton("◀ Anterior", 20);
                              btnAnt.Click += (s, e) => { if (indiceActual > 0) { indiceActual--; canvas.Invalidate(); } };
                              Button btnSig = CrearBoton("Siguiente ▶", 140);
                              btnSig.Click += (s, e) => { if (indiceActual < arboles.Count - 1) { indiceActual++; canvas.Invalidate(); } };
                              lblEstado = new Label
                              {
                                        ForeColor = Color.White,
                                        Location = new Point(280, 20),
                                        AutoSize = true,
                                        Font = new Font("Segoe UI", 11, FontStyle.Bold)
                              };
                              nav.Controls.Add(btnAnt);
                              nav.Controls.Add(btnSig);
                              nav.Controls.Add(lblEstado);
                              this.Controls.Add(canvas);
                              this.Controls.Add(nav);
                              ActualizarEstado();
                    }

                    private Button CrearBoton(string texto, int x)
                    {
                              return new Button
                              {
                                        Text = texto,
                                        Location = new Point(x, 15),
                                        Size = new Size(110, 30),
                                        BackColor = Color.FromArgb(60, 60, 60),
                                        ForeColor = Color.White,
                                        FlatStyle = FlatStyle.Flat,
                                        Cursor = Cursors.Hand
                              };
                    }

                    private void ActualizarEstado()
                    {
                              if (arboles.Count > 0)
                                        lblEstado.Text = $"Árbol {indiceActual + 1} de {arboles.Count}   |   Raíz: [{arboles[indiceActual].Etiqueta}]";
                              else
                                        lblEstado.Text = "No hay árboles para mostrar.";
                    }

                    private void DibujarArbol(Graphics g)
                    {
                              g.SmoothingMode = SmoothingMode.AntiAlias;
                              g.ScaleTransform(zoom, zoom);
                              if (arboles.Count == 0) return;
                              ActualizarEstado();

                              // Calculamos el ancho y alto total del árbol (layout)
                              var tamanios = new Dictionary<NodoASTVisual, SizeF>();
                              CalcularTamanios(arboles[indiceActual], g, tamanios);
                              var posiciones = new Dictionary<NodoASTVisual, PointF>();
                              int anchoTotal = (int)ColocarNodos(arboles[indiceActual], 0, 0, tamanios, posiciones, g);
                              int altoTotal = (int)(posiciones[arboles[indiceActual]].Y + tamanios[arboles[indiceActual]].Height + 50);

                              // Centrar el árbol si es más pequeño que el canvas (con zoom)
                              float offsetX = (canvas.Width / zoom - anchoTotal) / 2;
                              float offsetY = 20;

                              // Dibujar líneas primero
                              foreach (var nodo in posiciones.Keys)
                              {
                                        var pos = posiciones[nodo];
                                        var tam = tamanios[nodo];
                                        PointF centroPadre = new PointF(pos.X + tam.Width / 2, pos.Y + tam.Height);
                                        foreach (var hijo in nodo.Hijos)
                                        {
                                                  var posHijo = posiciones[hijo];
                                                  var tamHijo = tamanios[hijo];
                                                  PointF centroHijo = new PointF(posHijo.X + tamHijo.Width / 2, posHijo.Y);
                                                  g.DrawLine(new Pen(Color.FromArgb(120, 120, 120), 2),
                                                      centroPadre.X + offsetX, centroPadre.Y + offsetY,
                                                      centroHijo.X + offsetX, centroHijo.Y + offsetY);
                                        }
                              }

                              // Dibujar nodos (rectángulos) encima
                              foreach (var nodo in posiciones.Keys)
                              {
                                        var pos = posiciones[nodo];
                                        var tam = tamanios[nodo];
                                        RectangleF rect = new RectangleF(pos.X + offsetX, pos.Y + offsetY, tam.Width, tam.Height);
                                        Brush colorFondo = ObtenerColorFondo(nodo.TipoElemento);
                                        g.FillRoundRectangle(colorFondo, rect, 10);
                                        g.DrawRoundRectangle(new Pen(Color.FromArgb(80, 80, 80), 1.5f), rect, 10);
                                        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                                        {
                                                  g.DrawString(nodo.Etiqueta, new Font("Consolas", 9, FontStyle.Bold), Brushes.Black, rect, sf);
                                        }
                              }
                    }

                    private void CalcularTamanios(NodoASTVisual nodo, Graphics g, Dictionary<NodoASTVisual, SizeF> dict)
                    {
                              var formato = new StringFormat(StringFormat.GenericTypographic);
                              SizeF tamTexto = g.MeasureString(nodo.Etiqueta, new Font("Consolas", 9, FontStyle.Bold), int.MaxValue, formato);
                              float ancho = Math.Max(tamTexto.Width + 20, 40);
                              float alto = Math.Max(tamTexto.Height + 12, 30);
                              dict[nodo] = new SizeF(ancho, alto);
                              foreach (var hijo in nodo.Hijos)
                                        CalcularTamanios(hijo, g, dict);
                    }

                    private float ColocarNodos(NodoASTVisual nodo, float x, float y, Dictionary<NodoASTVisual, SizeF> tamanios,
                                               Dictionary<NodoASTVisual, PointF> posiciones, Graphics g)
                    {
                              var tam = tamanios[nodo];
                              float anchoTotal = tam.Width;
                              float yNodo = y;
                              posiciones[nodo] = new PointF(x, yNodo);
                              if (nodo.Hijos.Count == 0) return anchoTotal;

                              float yHijos = yNodo + tam.Height + 40;
                              float anchoHijos = 0;
                              float xHijo = x;
                              float separacion = 20;
                              for (int i = 0; i < nodo.Hijos.Count; i++)
                              {
                                        float anchoHijo = ColocarNodos(nodo.Hijos[i], xHijo, yHijos, tamanios, posiciones, g);
                                        anchoHijos += anchoHijo;
                                        if (i < nodo.Hijos.Count - 1) anchoHijos += separacion;
                                        xHijo += anchoHijo + separacion;
                              }
                              // Centrar el padre sobre los hijos
                              float anchoPadre = anchoTotal;
                              float inicioHijos = posiciones[nodo.Hijos[0]].X;
                              float finHijos = posiciones[nodo.Hijos[nodo.Hijos.Count - 1]].X + tamanios[nodo.Hijos[nodo.Hijos.Count - 1]].Width;
                              float centroHijos = (inicioHijos + finHijos) / 2;
                              float nuevaX = centroHijos - anchoPadre / 2;
                              posiciones[nodo] = new PointF(nuevaX, yNodo);
                              return Math.Max(anchoPadre, anchoHijos);
                    }

                    private Brush ObtenerColorFondo(string tipo)
                    {
                              switch (tipo)
                              {
                                        case "Operador": return Brushes.LightSkyBlue;
                                        case "Numero": return Brushes.LightGreen;
                                        case "Identificador": return Brushes.LightYellow;
                                        case "PalabraReservada": return Brushes.LightCoral;
                                        case "Programa": return Brushes.Lavender;
                                        case "Bloque": return Brushes.LightGray;
                                        case "Condicion": return Brushes.Moccasin;
                                        case "Funcion": return Brushes.Plum;
                                        case "Llamada": return Brushes.PeachPuff;
                                        default: return Brushes.White;
                              }
                    }
          }

          // Extensiones para dibujar rectángulos redondeados (no están en System.Drawing)
          public static class GraphicsExtensions
          {
                    public static void FillRoundRectangle(this Graphics g, Brush brush, RectangleF rect, float radius)
                    {
                              using (var path = GetRoundRectanglePath(rect, radius))
                                        g.FillPath(brush, path);
                    }

                    public static void DrawRoundRectangle(this Graphics g, Pen pen, RectangleF rect, float radius)
                    {
                              using (var path = GetRoundRectanglePath(rect, radius))
                                        g.DrawPath(pen, path);
                    }

                    private static GraphicsPath GetRoundRectanglePath(RectangleF rect, float radius)
                    {
                              float r = radius;
                              var path = new GraphicsPath();
                              path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
                              path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
                              path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
                              path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
                              path.CloseFigure();
                              return path;
                    }
          }
}