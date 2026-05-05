using System;
using System.Collections.Generic;
using System.Linq;
using BioSphereIDE.Core;

namespace BioSphereIDE.Analizadores
{
          public class Symbol
          {
                    public string Nombre { get; set; }
                    public string Tipo { get; set; }      // "numero", "texto", "booleano", "lista", "nulo", "funcion"
                    public object Valor { get; set; }
                    public string Unidad { get; set; }     // opcional: "km", "kg", etc.
                    public int Linea { get; set; }         // línea donde se creó/declaró
                    public string Ambito { get; set; }     // nombre del ámbito

                    public override string ToString() => $"{Nombre} : {Tipo} = {Valor} (unidad={Unidad}) línea={Linea}";
          }

          // Gestor de ámbitos anidados (pila)
          public class SymbolTable
          {
                    private Stack<Dictionary<string, Symbol>> scopes = new Stack<Dictionary<string, Symbol>>();
                    private Stack<string> scopeNames = new Stack<string>();

                    public void PushScope(string name)
                    {
                              scopes.Push(new Dictionary<string, Symbol>());
                              scopeNames.Push(name);
                    }

                    public void PopScope()
                    {
                              scopes.Pop();
                              scopeNames.Pop();
                    }

                    public string CurrentScopeName => scopeNames.Count > 0 ? scopeNames.Peek() : "global";

                    // Buscar símbolo en el ámbito actual y ancestros. Devuelve (symbol, scopeLevel)
                    public (Symbol, int) LookUp(string name)
                    {
                              int level = 0;
                              foreach (var scope in scopes)
                              {
                                        if (scope.TryGetValue(name, out var sym))
                                                  return (sym, level);
                                        level++;
                              }
                              return (null, -1);
                    }

                    // Agregar símbolo al ámbito actual
                    public void AddSymbol(Symbol sym)
                    {
                              if (scopes.Count == 0)
                                        throw new InvalidOperationException("No hay ámbito activo");
                              var current = scopes.Peek();
                              if (current.ContainsKey(sym.Nombre))
                                        throw new Exception($"Símbolo '{sym.Nombre}' ya existe en el ámbito actual");
                              current[sym.Nombre] = sym;
                    }

                    // Actualizar un símbolo existente en el ámbito donde se encontró
                    public void UpdateSymbol(string name, Symbol newSym, int level)
                    {
                              if (level < 0 || level >= scopes.Count) return;
                              var scopeArray = scopes.ToArray();
                              var targetScope = scopeArray[level];
                              targetScope[name] = newSym;
                    }

                    // Para depuración: imprimir tabla
                    public void Print()
                    {
                              Console.WriteLine("\n--- TABLA DE SÍMBOLOS ---");
                              foreach (var scope in scopes.Reverse())
                              {
                                        foreach (var kv in scope)
                                        {
                                                  Console.WriteLine(kv.Value);
                                        }
                              }
                    }
          }

          // Analizador semántico (Visitor)
          public class SemanticAnalyzer
          {
                    private SymbolTable symbolTable;
                    private List<ErrorInfo> errors;
                    private bool stopOnFirstError = true;

                    public SemanticAnalyzer()
                    {
                              symbolTable = new SymbolTable();
                              errors = new List<ErrorInfo>();
                    }

                    public (bool success, List<ErrorInfo> errors) Analyze(NodoPrograma programa)
                    {
                              errors.Clear();
                              try
                              {
                                        symbolTable.PushScope("global");
                                        VisitPrograma(programa);
                                        symbolTable.PopScope();
                                        return (errors.Count == 0, errors);
                              }
                              catch (SemanticException ex)
                              {
                                        errors.Add(ex.Error);
                                        return (false, errors);
                              }
                    }

                    private void AddError(string code, string message, Token? token, int lineOverride = -1)
                    {
                              int line = (token?.Line ?? lineOverride) > 0 ? (token?.Line ?? lineOverride) : 1;
                              int col = token?.Column ?? 1;
                              var err = new ErrorInfo
                              {
                                        Line = line,
                                        Column = col,
                                        Length = token?.Length ?? 1,
                                        Message = $"[{code}] {message}",
                                        Type = ErrorType.Semantico
                              };
                              errors.Add(err);
                              if (stopOnFirstError)
                                        throw new SemanticException(err);
                    }

                    private void VisitPrograma(NodoPrograma node)
                    {
                              VisitBloqueSimulacion(node.BloqueSimulacion);
                    }

                    private void VisitBloqueSimulacion(NodoBloqueSimulacion node)
                    {
                              symbolTable.PushScope("simulacion");
                              bool hasPlaneta = false, hasAtmosfera = false, hasAgua = false, hasVida = false;
                              foreach (var stmt in node.Sentencias)
                              {
                                        if (stmt is NodoBloquePlaneta) hasPlaneta = true;
                                        else if (stmt is NodoBloqueAtmosfera) hasAtmosfera = true;
                                        else if (stmt is NodoBloqueAgua) hasAgua = true;
                                        else if (stmt is NodoBloqueVida) hasVida = true;
                                        VisitSentencia(stmt);
                              }
                              symbolTable.PopScope();

                              if (!hasPlaneta) AddError("SEM-040", "Bloque 'planeta' obligatorio faltante.", null);
                              if (!hasAtmosfera) AddError("SEM-040", "Bloque 'atmosfera' obligatorio faltante.", null);
                              if (!hasAgua) AddError("SEM-040", "Bloque 'agua' obligatorio faltante.", null);
                    }

                    private void VisitBloquePlaneta(NodoBloquePlaneta node)
                    {
                              foreach (var stmt in node.Sentencias)
                                        VisitSentencia(stmt);

                              var (masaSym, _) = symbolTable.LookUp("masa");
                              var (radioSym, _) = symbolTable.LookUp("radio");

                              if (masaSym == null)
                                        AddError("SEM-041", "Variable 'masa' requerida en bloque 'planeta'.", null);
                              else if (masaSym.Tipo != "numero")
                                        AddError("SEM-014", $"Variable 'masa' debe ser numérica, pero se le asignó tipo '{masaSym.Tipo}'.", null, masaSym.Linea);

                              if (radioSym == null)
                                        AddError("SEM-041", "Variable 'radio' requerida en bloque 'planeta'.", null);
                              else if (radioSym.Tipo != "numero")
                                        AddError("SEM-014", $"Variable 'radio' debe ser numérica, pero se le asignó tipo '{radioSym.Tipo}'.", null, radioSym.Linea);
                    }

                    private void VisitBloqueAtmosfera(NodoBloqueAtmosfera node)
                    {
                              foreach (var stmt in node.Sentencias)
                                        VisitSentencia(stmt);

                              var (presionSym, _) = symbolTable.LookUp("presion");
                              var (co2Sym, _) = symbolTable.LookUp("co2");

                              if (presionSym == null)
                                        AddError("SEM-041", "Variable 'presion' requerida en bloque 'atmosfera'.", null);
                              else if (presionSym.Tipo != "numero")
                                        AddError("SEM-014", $"Variable 'presion' debe ser numérica, pero se le asignó tipo '{presionSym.Tipo}'.", null, presionSym.Linea);

                              if (co2Sym == null)
                                        AddError("SEM-041", "Variable 'co2' requerida en bloque 'atmosfera'.", null);
                              else if (co2Sym.Tipo != "numero")
                                        AddError("SEM-014", $"Variable 'co2' debe ser numérica, pero se le asignó tipo '{co2Sym.Tipo}'.", null, co2Sym.Linea);
                    }

                    private void VisitBloqueAgua(NodoBloqueAgua node)
                    {
                              foreach (var stmt in node.Sentencias)
                                        VisitSentencia(stmt);

                              var (estadoSym, _) = symbolTable.LookUp("estado_liquido");
                              if (estadoSym == null)
                                        AddError("SEM-041", "Variable 'estado_liquido' requerida en bloque 'agua'.", null);
                              else if (estadoSym.Tipo != "booleano")
                                        AddError("SEM-014", $"Variable 'estado_liquido' debe ser booleana, pero se le asignó tipo '{estadoSym.Tipo}'.", null, estadoSym.Linea);
                    }

                    private void VisitBloqueVida(NodoBloqueVida node)
                    {
                              foreach (var stmt in node.Sentencias)
                                        VisitSentencia(stmt);
                    }

                    private void VisitDefinicionFuncion(NodoDefinicionFuncion node)
                    {
                              // NUEVO: 1. Registrar la firma de la función en el Scope Padre
                              var funcSym = new Symbol
                              {
                                        Nombre = $"func_{node.Nombre}",
                                        Tipo = "funcion",
                                        Valor = node.Parametros.Count, // Guardar la cantidad de parámetros previstos
                                        Unidad = null,
                                        Linea = 1, // En un rediseño, extraer esto del token del Parser
                                        Ambito = symbolTable.CurrentScopeName
                              };
                              try { symbolTable.AddSymbol(funcSym); } catch { }

                              // 2. Entrar al Scope hijo y cargar variables
                              symbolTable.PushScope($"funcion {node.Nombre}");
                              foreach (var param in node.Parametros)
                              {
                                        var paramSym = new Symbol
                                        {
                                                  Nombre = param,
                                                  Tipo = "numero", // En ASTRA por defecto asumimos números en funciones algebraicas
                                                  Valor = null,
                                                  Linea = 1,
                                                  Ambito = symbolTable.CurrentScopeName
                                        };
                                        try { symbolTable.AddSymbol(paramSym); } catch { }
                              }
                              foreach (var stmt in node.Sentencias)
                                        VisitSentencia(stmt);

                              symbolTable.PopScope();
                    }

                    private void VisitSentencia(NodoSentencia stmt)
                    {
                              switch (stmt)
                              {
                                        case NodoAsignacion asig: VisitAsignacion(asig); break;
                                        case NodoMostrar mostrar: VisitMostrar(mostrar); break;
                                        case NodoReporte reporte: VisitReporte(reporte); break;
                                        case NodoIf nif: VisitIf(nif); break;
                                        case NodoWhile nwhile: VisitWhile(nwhile); break;
                                        case NodoDefinicionFuncion func: VisitDefinicionFuncion(func); break;
                                        case NodoBloquePlaneta p: VisitBloquePlaneta(p); break;
                                        case NodoBloqueAtmosfera a: VisitBloqueAtmosfera(a); break;
                                        case NodoBloqueAgua a: VisitBloqueAgua(a); break;
                                        case NodoBloqueVida v: VisitBloqueVida(v); break;
                                        case NodoExpresionSentencia expSent: VisitExpresion(expSent.Expresion); break;
                                        default: break;
                              }
                    }

                    private void VisitAsignacion(NodoAsignacion node)
                    {
                              var (tipo, valor, unidad) = VisitValor(node.Valor, null);
                              var (existingSym, scopeLevel) = symbolTable.LookUp(node.Identificador);

                              if (existingSym == null)
                              {
                                        var newSym = new Symbol
                                        {
                                                  Nombre = node.Identificador,
                                                  Tipo = tipo,
                                                  Valor = valor,
                                                  Unidad = unidad,
                                                  Linea = 1, // NUEVO: Evitamos usar "node.Identificador?.Length" como línea
                                                  Ambito = symbolTable.CurrentScopeName
                                        };
                                        symbolTable.AddSymbol(newSym);
                              }
                              else
                              {
                                        if (existingSym.Tipo != tipo && !(existingSym.Tipo == "numero" && tipo == "numero"))
                                        {
                                                  AddError("SEM-014", $"Incompatibilidad de tipos en asignación. '{node.Identificador}' es {existingSym.Tipo} pero se asigna {tipo}.", null, existingSym.Linea);
                                        }
                                        existingSym.Valor = valor;
                                        existingSym.Unidad = unidad;
                                        symbolTable.UpdateSymbol(node.Identificador, existingSym, scopeLevel);
                              }
                    }

                    private (string tipo, object valor, string unidad) VisitValor(NodoValor valor, Token? tokenContext)
                    {
                              switch (valor)
                              {
                                        case NodoExprNumero num: return ("numero", num.Valor, null);
                                        case NodoExprIdentificador id:
                                                  var (sym, _) = symbolTable.LookUp(id.Nombre);
                                                  if (sym == null) AddError("SEM-001", $"Variable '{id.Nombre}' no inicializada.", tokenContext);
                                                  return (sym?.Tipo ?? "nulo", sym?.Valor, sym?.Unidad);
                                        case NodoTexto txt: return ("texto", txt.Texto, null);
                                        case NodoBooleano boo: return ("booleano", boo.Valor, null);
                                        case NodoNulo _: return ("nulo", null, null);
                                        case NodoLista list: return ("lista", list.Valores, null);
                                        case NodoExprBinaria bin: return VisitExprBinaria(bin, tokenContext);
                                        case NodoExprPotencia pot: return VisitExprPotencia(pot, tokenContext);
                                        case NodoExprParentesis par: return VisitValor(par.Expr, tokenContext);
                                        case NodoLlamadaFuncion call: return VisitLlamadaFuncion(call, tokenContext);
                                        case NodoCantidad cant:
                                                  var (tExpr, vExpr, uExpr) = VisitValor(cant.Expr, tokenContext);
                                                  string unidadFinal = cant.Unidad ?? uExpr;
                                                  return (tExpr, vExpr, unidadFinal);
                                        default: return ("nulo", null, null);
                              }
                    }

                    private (string tipo, object valor, string unidad) VisitExprBinaria(NodoExprBinaria bin, Token? tokenContext)
                    {
                              var (tipoIzq, valIzq, unidIzq) = VisitValor(bin.Izquierda, tokenContext);
                              var (tipoDer, valDer, unidDer) = VisitValor(bin.Derecha, tokenContext);

                              if (tipoIzq == "nulo" || tipoDer == "nulo")
                                        AddError("SEM-013", $"Operación no permitida con valor 'nulo'.", tokenContext);

                              string op = bin.Operador;
                              if (op == "+" || op == "-" || op == "*" || op == "/" || op == "^")
                              {
                                        if (tipoIzq != "numero" || tipoDer != "numero")
                                                  AddError("SEM-010", $"Operador '{op}' requiere valores de tipo Numero.", tokenContext);
                                        if ((op == "+" || op == "-") && unidIzq != null && unidDer != null && unidIzq != unidDer)
                                                  AddError("SEM-030", $"Unidades incompatibles: '{unidIzq}' y '{unidDer}'.", tokenContext);
                                        if (op == "^" && unidDer != null)
                                                  AddError("SEM-031", $"Exponente no puede tener unidad física.", tokenContext);
                                        string unidadResult = (op == "+" || op == "-") ? unidIzq : null;
                                        return ("numero", null, unidadResult);
                              }
                              else if (op == "<" || op == ">" || op == "<=" || op == ">=")
                              {
                                        if (tipoIzq != "numero" || tipoDer != "numero")
                                                  AddError("SEM-011", $"Operador '{op}' requiere tipos numéricos.", tokenContext);
                                        return ("booleano", null, null);
                              }
                              else if (op == "==" || op == "!=")
                              {
                                        return ("booleano", null, null);
                              }
                              else
                              {
                                        AddError("SEM-010", $"Operador desconocido '{op}'.", tokenContext);
                                        return ("nulo", null, null);
                              }
                    }

                    private (string tipo, object valor, string unidad) VisitExprPotencia(NodoExprPotencia pot, Token? tokenContext)
                    {
                              var (tipoBase, _, unidBase) = VisitValor(pot.Base, tokenContext);
                              var (tipoExp, _, unidExp) = VisitValor(pot.Exponente, tokenContext);
                              if (tipoBase != "numero" || tipoExp != "numero")
                                        AddError("SEM-010", $"Operador '^' requiere valores de tipo Numero.", tokenContext);
                              if (unidExp != null)
                                        AddError("SEM-031", $"Exponente no puede tener unidad física.", tokenContext);
                              return ("numero", null, unidBase);
                    }

                    private (string tipo, object valor, string unidad) VisitLlamadaFuncion(NodoLlamadaFuncion call, Token? tokenContext)
                    {
                              var (funcSym, _) = symbolTable.LookUp($"func_{call.Nombre}");
                              if (funcSym == null)
                                        AddError("SEM-002", $"Función '{call.Nombre}' no definida.", tokenContext);
                              foreach (var arg in call.Argumentos)
                                        VisitValor(arg, tokenContext);
                              return ("numero", null, null);
                    }

                    private (string tipo, object valor, string unidad) VisitCondicion(NodoCondicion cond, Token? tokenContext)
                    {
                              var (tipoIzq, _, _) = VisitValor(cond.Izquierda, tokenContext);

                              if (!string.IsNullOrEmpty(cond.Operador))
                              {
                                        var (tipoDer, _, _) = VisitValor(cond.Derecha, tokenContext);
                                        if (cond.Operador == "<" || cond.Operador == ">" || cond.Operador == "<=" || cond.Operador == ">=")
                                        {
                                                  if (tipoIzq != "numero" || tipoDer != "numero")
                                                            AddError("SEM-011", $"Operador '{cond.Operador}' requiere tipos numéricos.", tokenContext);
                                        }
                              }

                              // NUEVO: Validar anidaciones lógicas (ej: "X > 2 Y Z < 4")
                              foreach (var (opLogico, subCond) in cond.OperadoresLogicos)
                              {
                                        var (tipoSub, _, _) = VisitCondicion(subCond, tokenContext);
                                        if (tipoSub != "booleano" && tipoSub != "numero") // En ASTRA numérico se usa en condicionales
                                                  AddError("SEM-021", $"Operador lógico '{opLogico}' requiere evaluaciones válidas.", tokenContext);
                              }

                              // Si es un valor único (ej. "si (verdadero)") retorna su tipo natural, sino, retorna booleano
                              if (string.IsNullOrEmpty(cond.Operador) && cond.OperadoresLogicos.Count == 0)
                                        return (tipoIzq, null, null);

                              return ("booleano", null, null);
                    }

                    private void VisitMostrar(NodoMostrar node)
                    {
                              VisitValor(node.Valor, null);
                    }

                    private void VisitReporte(NodoReporte node)
                    {
                              VisitValor(node.Valor, null);
                    }

                    private void VisitIf(NodoIf node)
                    {
                              var (tipoCond, _, _) = VisitCondicion(node.Condicion, null);
                              if (tipoCond != "booleano")
                                        AddError("SEM-020", $"Condición debe resultar Booleana, no '{tipoCond}'.", null);

                              symbolTable.PushScope("if");
                              foreach (var stmt in node.ThenSentencias)
                                        VisitSentencia(stmt);
                              symbolTable.PopScope();

                              if (node.ElseSentencias.Count > 0)
                              {
                                        symbolTable.PushScope("else");
                                        foreach (var stmt in node.ElseSentencias)
                                                  VisitSentencia(stmt);
                                        symbolTable.PopScope();
                              }
                    }

                    private void VisitWhile(NodoWhile node)
                    {
                              var (tipoCond, _, _) = VisitCondicion(node.Condicion, null);
                              if (tipoCond != "booleano")
                                        AddError("SEM-020", $"Condición debe resultar Booleana, no '{tipoCond}'.", null);

                              symbolTable.PushScope("while");
                              foreach (var stmt in node.Sentencias)
                                        VisitSentencia(stmt);
                              symbolTable.PopScope();
                    }

                    private void VisitExpresion(NodoExpr expr)
                    {
                              VisitValor(expr, null);
                    }

                    private class SemanticException : Exception
                    {
                              public ErrorInfo Error { get; }
                              public SemanticException(ErrorInfo error) : base(error.Message) { Error = error; }
                    }
          }
}