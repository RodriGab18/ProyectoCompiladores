# BioSphere IDE - Analizador Léxico (ASTRA DSL)

Bienvenido a **BioSphere IDE**, un Entorno de Desarrollo Integrado (IDE) personalizado diseñado para el lenguaje de dominio específico **ASTRA** (Astrobiology Simulation and Terrain Analysis).

Este proyecto implementa la primera fase de un compilador: el **Analizador Léxico (Lexer)**. Lee el código fuente y lo divide en componentes básicos ("tokens"), verificando que todo cumpla con las reglas léxicas y estructurales básicas del lenguaje.

## ¿Qué hace el proyecto?
El IDE consta de las siguientes características:
1. **Editor de Código Avanzado**: Una interfaz gráfica con soporte de números de línea, resaltado de sintaxis dinámico por tipología léxica.
2. **Análisis Léxico en Tiempo Real**: Al escribir, el analizador funciona en segundo plano segmentando el texto en tokens correspondientes al lenguaje (`PALABRA_RESERVADA`, `IDENTIFICADOR`, `NUMERO`, `CADENA`, `OPERADOR`, etc).
3. **Detección Visual de Errores**: Subraya en tiempo real errores léxicos como caracteres no permitidos, identificadores mal declarados (ej. que empiezan con número), o cadenas de texto sin cerrar. Además, detecta problemas estructurales (falta de cierre de llaves `{ }`, paréntesis `( )` o corchetes `[ ]`).
4. **Tabla de Símbolos y Consola**: Muestra todo el proceso interno del lexer desglosado en un listado tabular (DataGridView) con el Tipo de Token, el Lexema, la Línea y Columna. Paralelamente, emite un reporte en una terminal simulada.
5. **Documentación Integrada**: Contiene un manual de usuario interactivo (`FrmDocumentacion`) sobre la sintaxis de ASTRA y sus palabras reservadas.

## Requisitos de Instalación (Lo que necesitas instalar)
Para poder compilar, ejecutar o verificar este proyecto en tu entorno local, necesitas lo siguiente:

* **.NET 8.0 SDK**: El entorno principal subyacente (`net8.0-windows`). [Descargar .NET 8 SDK aquí](https://dotnet.microsoft.com/es-es/download/dotnet/8.0).
* **Sistema Operativo Windows**: Requerido puesto que la aplicación gráfica hace uso nativo de las tecnologías `Windows Forms`.
* *(Opcional)* **Visual Studio 2022 o VS Code**: Para editar el código C# del analizador y los componentes de la interfaz.

## Bibliotecas dependientes (NuGet Packages)
El proyecto depende de la siguiente biblioteca externa que se descargará automáticamente durante el proceso de compilación `dotnet restore`:

* **[AvalonEdit](https://github.com/icsharpcode/AvalonEdit) (Versión 6.3.1.120)**
  * *¿Por qué se usa?* Es un potente y premiado control de edición de código WPF desarrollado por ICSharpCode. Se incrusta aquí a través de `ElementHost` para habilitar el motor de resaltado léxico (`SyntaxColorizer`) y renderizar el subrayado rojo ondulado clásico de los IDE (`ErrorSquiggleRenderer`) debajo de los fragmentos de código con errores.

---

## Cómo ejecutar el proyecto

Abre una terminal en la raíz de tu proyecto (donde está tu archivo `.sln`) y ejecuta los siguientes comandos:

```bash
# 1. Restaura los paquetes y bibliotecas necesarias (AvalonEdit)
dotnet restore

# 2. Compila y ejecuta la ventana del IDE
dotnet run --project BioSphereIDE\BioSphereIDE.csproj
```

---

## Texto para prueba del Lexer

Puedes usar este código de simulación estructurado en ASTRA para poner a prueba la tabla de Tokens y los errores léxicos. Al pegarlo presiona en el botón de compilación (▷).

```plaintext
// Simulacion Avanzada: Proyecto Terraformacion Alpha
simulacion {
    planeta {
        radio = 3389;
        masa = 0.107;
        // Calculo dinamico de la gravedad
        gravedad = ( G * masa ) / ( radio ^ 2 );
        temperatura = -60 °;
    }

    atmosfera {
        presion = 0.006;
        co2 = 95;
        oxigeno = 0.1;
        radiacion = 2.5;
    }

    agua {
        estado_liquido = falso;
        volumen = 0;
    }

    // Ciclo de evolucion temporal (Simulando 100 años)
    iterar ( 100 ) {
        temperatura = temperatura + 0.5;
        presion = presion + 0.01;

        si ( temperatura >= 0 y presion > 0.5 ) {
            estado_liquido = verdadero;
            mostrar ( "El hielo se ha derretido con exito" );
        } sino {
            continuar;
        }
    }

    // Evaluacion final de habitabilidad
    si ( temperatura > 15 y oxigeno >= 20 ) {
        vida = verdadero;
        reporte ( "Planeta listo para colonizacion" );
    }

    // ==========================================
    // TRAMPAS PARA EL ANALIZADOR LÉXICO
    // ==========================================
    
    // 1. Error: Identificador que empieza con numero
    5000colonos = verdadero ;
    
    // 2. Error: Simbolos ajenos al alfabeto del lenguaje
    @ $ #
}
```
