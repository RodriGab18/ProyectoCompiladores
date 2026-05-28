// ============================================================================
//  Interpreter.cs
//  Intérprete del Árbol Sintáctico Abstracto (AST) para el lenguaje ASTRA.
//
//  Recorre y evalúa el AST producido por el Parser, capturando las salidas
//  textuales de los comandos mostrar() y reporte() para el generador de
//  informes astrobiológicos.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace BioSphereIDE.Analizadores
{
    // ── Excepciones de control de flujo ─────────────────────────────────────
    internal sealed class BreakException    : Exception { }
    internal sealed class ContinueException : Exception { }

    // ── Resultado de ejecución de la simulación ──────────────────────────────
    public sealed class InterpreterResult
    {
        /// <summary>Salidas producidas por mostrar(...).</summary>
        public List<string> SalidasMostrar { get; } = new();

        /// <summary>Salidas producidas por reporte(...).</summary>
        public List<string> SalidasReporte { get; } = new();

        /// <summary>Mensaje de error si la ejecución falló, vacío si fue exitosa.</summary>
        public string ErrorMensaje { get; set; } = string.Empty;

        /// <summary>True si la simulación terminó sin excepciones no controladas.</summary>
        public bool Exitosa => string.IsNullOrEmpty(ErrorMensaje);
    }

    // ── Entorno de ejecución (tabla de símbolos de tiempo de ejecución) ──────
    internal sealed class RuntimeEnvironment
    {
        private readonly Stack<Dictionary<string, object?>> _scopes = new();

        public RuntimeEnvironment() => _scopes.Push(new Dictionary<string, object?>(StringComparer.Ordinal));

        public void PushScope() => _scopes.Push(new Dictionary<string, object?>(StringComparer.Ordinal));

        public void PopScope()
        {
            if (_scopes.Count > 1) _scopes.Pop();
        }

        /// <summary>Busca la variable desde el ámbito más interno hacia afuera.</summary>
        public object? Get(string name)
        {
            foreach (var scope in _scopes)
                if (scope.TryGetValue(name, out var val)) return val;
            return null;
        }

        /// <summary>Asigna en el ámbito más interno donde ya exista; si no, en el más interno.</summary>
        public void Set(string name, object? value)
        {
            foreach (var scope in _scopes)
            {
                if (scope.ContainsKey(name)) { scope[name] = value; return; }
            }
            // Variable nueva: declara en el ámbito actual
            _scopes.Peek()[name] = value;
        }

        /// <summary>Declara en el ámbito local más interno (para parámetros de función).</summary>
        public void SetLocal(string name, object? value) => _scopes.Peek()[name] = value;

        public bool Contains(string name)
        {
            foreach (var scope in _scopes)
                if (scope.ContainsKey(name)) return true;
            return false;
        }
    }

    // ── Representación de una función definida por el usuario ────────────────
    internal sealed class FunctionDefinition
    {
        public List<string>        Parameters { get; }
        public List<NodoSentencia> Body       { get; }

        public FunctionDefinition(List<string> parameters, List<NodoSentencia> body)
        {
            Parameters = parameters;
            Body       = body;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  INTÉRPRETE PRINCIPAL
    // ════════════════════════════════════════════════════════════════════════
    public sealed class ASTInterpreter
    {
        private readonly RuntimeEnvironment                      _env   = new();
        private readonly Dictionary<string, FunctionDefinition> _funcs = new(StringComparer.Ordinal);
        private          InterpreterResult                       _result = null!;

        private const int MaxIterations = 10_000; // protección anti-bucle infinito

        // ── Punto de entrada ─────────────────────────────────────────────────
        public InterpreterResult Execute(NodoPrograma programa)
        {
            _result = new InterpreterResult();
            try
            {
                ExecutePrograma(programa);
            }
            catch (BreakException)
            {
                _result.ErrorMensaje = "'romper' usado fuera de un bucle.";
            }
            catch (ContinueException)
            {
                _result.ErrorMensaje = "'continuar' usado fuera de un bucle.";
            }
            catch (Exception ex)
            {
                _result.ErrorMensaje = ex.Message;
            }
            return _result;
        }

        // ── Nodo raíz ────────────────────────────────────────────────────────
        private void ExecutePrograma(NodoPrograma nodo) =>
            ExecuteBloqueSim(nodo.BloqueSimulacion);

        private void ExecuteBloqueSim(NodoBloqueSimulacion nodo)
        {
            // Primera pasada: registrar todas las funciones definidas en el ámbito
            foreach (var sent in nodo.Sentencias)
                if (sent is NodoDefinicionFuncion fDef)
                    RegisterFunction(fDef);

            // Segunda pasada: ejecutar sentencias (saltando redefiniciones de funciones ya registradas)
            foreach (var sent in nodo.Sentencias)
                if (sent is not NodoDefinicionFuncion)
                    ExecuteSentencia(sent);
        }

        // ── Registro de funciones ────────────────────────────────────────────
        private void RegisterFunction(NodoDefinicionFuncion nodo) =>
            _funcs[nodo.Nombre] = new FunctionDefinition(nodo.Parametros, nodo.Sentencias);

        // ── Dispatch de sentencias ───────────────────────────────────────────
        private void ExecuteSentencia(NodoSentencia sent)
        {
            switch (sent)
            {
                case NodoAsignacion         a:  ExecuteAsignacion(a);     break;
                case NodoMostrar            m:  ExecuteMostrar(m);        break;
                case NodoReporte            r:  ExecuteReporte(r);        break;
                case NodoIf                 i:  ExecuteIf(i);             break;
                case NodoWhile              w:  ExecuteWhile(w);          break;
                case NodoDefinicionFuncion  f:  RegisterFunction(f);      break;
                case NodoBloquePlaneta      p:  ExecuteBloque(p.Sentencias); break;
                case NodoBloqueAtmosfera    at: ExecuteBloque(at.Sentencias); break;
                case NodoBloqueAgua         ag: ExecuteBloque(ag.Sentencias); break;
                case NodoBloqueVida         v:  ExecuteBloqueVida(v);     break;
                case NodoOrbitaYEscala      o:  ExecuteBloque(o.Instrucciones); break;
                case NodoContinuar          _:  throw new ContinueException();
                case NodoRomper             _:  throw new BreakException();
                case NodoExpresionSentencia es: EvalExpr(es.Expresion);   break;
            }
        }

        private void ExecuteBloque(List<NodoSentencia> sentencias)
        {
            // Primera pasada: registrar funciones definidas dentro del bloque
            foreach (var s in sentencias)
                if (s is NodoDefinicionFuncion fDef)
                    RegisterFunction(fDef);

            foreach (var s in sentencias)
                if (s is not NodoDefinicionFuncion)
                    ExecuteSentencia(s);
        }

        private void ExecuteBloqueVida(NodoBloqueVida nodo)
        {
            // Primera pasada: registrar funciones
            foreach (var s in nodo.Sentencias)
                if (s is NodoDefinicionFuncion fDef)
                    RegisterFunction(fDef);

            foreach (var s in nodo.Sentencias)
                if (s is not NodoDefinicionFuncion)
                    ExecuteSentencia(s);
        }

        // ── Asignación ───────────────────────────────────────────────────────
        private void ExecuteAsignacion(NodoAsignacion nodo)
        {
            var val = EvalValor(nodo.Valor);
            _env.Set(nodo.Identificador, val);
        }

        // ── mostrar / reporte ────────────────────────────────────────────────
        private void ExecuteMostrar(NodoMostrar nodo)
        {
            var val = EvalValor(nodo.Valor);
            _result.SalidasMostrar.Add(FormatValue(val));
        }

        private void ExecuteReporte(NodoReporte nodo)
        {
            var val = EvalValor(nodo.Valor);
            _result.SalidasReporte.Add(FormatValue(val));
        }

        // ── Condicional ──────────────────────────────────────────────────────
        private void ExecuteIf(NodoIf nodo)
        {
            var cond = EvalValor(nodo.Condicion);
            bool truthy = IsTruthy(cond);

            _env.PushScope();
            try
            {
                if (truthy)
                    ExecuteBloque(nodo.ThenSentencias);
                else if (nodo.ElseSentencias.Count > 0)
                    ExecuteBloque(nodo.ElseSentencias);
            }
            finally { _env.PopScope(); }
        }

        // ── Bucle mientras ───────────────────────────────────────────────────
        private void ExecuteWhile(NodoWhile nodo)
        {
            int iterations = 0;
            while (true)
            {
                if (++iterations > MaxIterations)
                    throw new InvalidOperationException(
                        $"Límite de {MaxIterations} iteraciones alcanzado. Posible bucle infinito.");

                var cond = EvalValor(nodo.Condicion);
                if (!IsTruthy(cond)) break;

                _env.PushScope();
                bool shouldBreak = false;
                try { ExecuteBloque(nodo.Sentencias); }
                catch (BreakException)    { shouldBreak = true; }
                catch (ContinueException) { /* continuar al inicio del bucle */ }
                finally { _env.PopScope(); }

                if (shouldBreak) break;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  EVALUACIÓN DE EXPRESIONES / VALORES
        // ════════════════════════════════════════════════════════════════════
        private object? EvalValor(NodoValor nodo) =>
            nodo is NodoExpr expr ? EvalExpr(expr) : null;

        private object? EvalExpr(NodoExpr nodo)
        {
            switch (nodo)
            {
                case NodoExprNumero num:
                    if (double.TryParse(num.Valor,
                            NumberStyles.Float | NumberStyles.AllowExponent,
                            CultureInfo.InvariantCulture, out double d))
                        return d;
                    return 0.0;

                case NodoBooleano boo:
                    return boo.Valor;

                case NodoTexto txt:
                    return txt.Texto;

                case NodoNulo _:
                    return null;

                case NodoCantidad cant:
                    return EvalExpr(cant.Expr); // ignoramos la unidad en tiempo de ejecución

                case NodoExprIdentificador id:
                    return _env.Get(id.Nombre);

                case NodoExprParentesis par:
                    return EvalExpr(par.Expr);

                case NodoExprUnaria una:
                    var operand = EvalExpr(una.Operando);
                    if (una.Operador == "-" && operand is double dOp)
                        return -dOp;
                    return operand;

                case NodoExprBinaria bin:
                    return EvalBinaria(bin);

                case NodoExprPotencia pot:
                    var baseVal = EvalExpr(pot.Base);
                    var expVal  = EvalExpr(pot.Exponente);
                    if (baseVal is double dBase && expVal is double dExp)
                        return Math.Pow(dBase, dExp);
                    return 0.0;

                case NodoLlamadaFuncion call:
                    return EvalLlamada(call);

                case NodoLista lst:
                    var items = new List<object?>();
                    foreach (var v in lst.Valores) items.Add(EvalValor(v));
                    return items;

                default:
                    return null;
            }
        }

        // ── Expresión binaria ────────────────────────────────────────────────
        private object? EvalBinaria(NodoExprBinaria bin)
        {
            string op = bin.Operador;

            // Evaluación corta para operadores lógicos
            if (op == "y")
            {
                var left = EvalExpr(bin.Izquierda);
                if (!IsTruthy(left)) return false;
                return IsTruthy(EvalExpr(bin.Derecha));
            }
            if (op == "o")
            {
                var left = EvalExpr(bin.Izquierda);
                if (IsTruthy(left)) return true;
                return IsTruthy(EvalExpr(bin.Derecha));
            }

            var l = EvalExpr(bin.Izquierda);
            var r = EvalExpr(bin.Derecha);

            // Aritméticos
            if (l is double dl && r is double dr)
            {
                return op switch
                {
                    "+"  => dl + dr,
                    "-"  => dl - dr,
                    "*"  => dl * dr,
                    "/"  => dr != 0 ? dl / dr : throw new DivideByZeroException("División por cero en la simulación."),
                    "<"  => (object)(dl <  dr),
                    ">"  => (object)(dl >  dr),
                    "<=" => (object)(dl <= dr),
                    ">=" => (object)(dl >= dr),
                    "==" => (object)(dl == dr),
                    "!=" => (object)(dl != dr),
                    _    => null
                };
            }

            // Comparaciones generales (incluyendo booleano == booleano)
            return op switch
            {
                "==" => Equals(l, r),
                "!=" => !Equals(l, r),
                _    => null
            };
        }

        // ── Llamada a función ────────────────────────────────────────────────
        private object? EvalLlamada(NodoLlamadaFuncion call)
        {
            if (!_funcs.TryGetValue(call.Nombre, out var funcDef))
                return null; // función no definida (el semántico ya lo detectó)

            // Evaluar argumentos
            var argValues = new List<object?>();
            foreach (var arg in call.Argumentos)
                argValues.Add(EvalValor(arg));

            // Crear ámbito local con los parámetros enlazados
            _env.PushScope();
            try
            {
                for (int i = 0; i < funcDef.Parameters.Count && i < argValues.Count; i++)
                    _env.SetLocal(funcDef.Parameters[i], argValues[i]);

                // Primera pasada: registrar subfunciones
                foreach (var s in funcDef.Body)
                    if (s is NodoDefinicionFuncion fDef) RegisterFunction(fDef);

                foreach (var s in funcDef.Body)
                    if (s is not NodoDefinicionFuncion)
                        ExecuteSentencia(s);

                return null; // ASTRA no tiene return explícito todavía
            }
            finally { _env.PopScope(); }
        }

        // ── Utilidades ───────────────────────────────────────────────────────
        private static bool IsTruthy(object? val) => val switch
        {
            bool   b => b,
            double d => d != 0,
            string s => !string.IsNullOrEmpty(s),
            null     => false,
            _        => true
        };

        private static string FormatValue(object? val) => val switch
        {
            double d => d.ToString("G", CultureInfo.InvariantCulture),
            bool   b => b ? "verdadero" : "falso",
            null     => "nulo",
            _        => val.ToString() ?? "nulo"
        };
    }
}
