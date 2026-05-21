# Godot Planetary Simulation - API Integration Guide

Este documento detalla la interfaz de parámetros del simulador planetario en Godot. Su propósito es servir como una guía técnica para que cualquier compilador, motor externo o IA pueda generar un archivo `.json` compatible con las propiedades expuestas en la clase `Planet` (`planet.gd`) y garantizar que la simulación se renderice sin inconsistencias.

## Estructura del Nodo `Planet`

Todos los parámetros que controlan la estética y la física del mundo están expuestos como variables de exportación (`@export_range`) en el script principal del planeta (`planet.gd`). El compilador externo debe generar un JSON con claves que coincidan **exactamente** con los nombres de estas variables.

Al leer el JSON dentro de Godot, solo debes iterar sobre las llaves del diccionario y asignar el valor al nodo `Planet` directamente, seguido de una llamada a `planet.rebuild_planet()` o depender del bucle `_process()` (que ya transmite todas estas variables a los shaders `terrain.gdshader`, `atmosphere.gdshader` y `clouds.gdshader`).

---

## Esquema de Parámetros (JSON Schema)

A continuación se listan todos los parámetros simulables, sus rangos permitidos (min - max) y cómo interactúan con el motor interno:

### Parámetros Estructurales Base
- `radius` **(Float) [100.0, 10000.0]**
  - **Uso:** Determina el tamaño físico (escala) de la malla esférica. Controla el radio base antes de aplicar ruido de relieve.
- `planet_mass` **(Float) [0.1, 20.0]**
  - **Uso:** (1.0 = 1 Masa Terrestre). 
  - **Efectos:** 
    - **Gigante Gaseoso:** Si `planet_mass > 5.0`, el planeta oculta su superficie sólida, activa las texturas de nubes densas y las somete a patrones de bandas horizontales.
    - **Achatamiento:** En conjunto con una rotación rápida, achata los polos de la malla.
    - **Gravedad/Relieve:** En conjunto con alta composición de hierro, aplana y reduce la altura de las montañas generadas por ruido procedural.

### Entorno y Sistema Estelar
- `star_distance_au` **(Float) [0.1, 5.0]**
  - **Uso:** Distancia a la estrella en Unidades Astronómicas.
  - **Efectos:**
    - Modifica dinámicamente la intensidad de luz recibida en los shaders (desde cegadora si `< 0.4` hasta tenue/fría si `> 3.0`).
    - Es un requisito para habilitar el Acoplamiento de Marea (*Tidal Locking*) si el valor es `< 0.2`.
- `rotation_period_hours` **(Float) [1.0, 1000.0]**
  - **Uso:** Período que tarda el planeta en dar una vuelta sobre su eje.
  - **Efectos:**
    - **Oblatidad:** Si `< 12.0` y `planet_mass > 5.0`, deforma la escala del nodo raíz del planeta (`scale.y = 0.85`).
    - **Nubes:** Estira las nubes horizontalmente si `< 12.0` y hay presión suficiente.
    - **Tidal Locking:** Si `> 500.0` y la distancia es muy corta, bloquea el ciclo día-noche y quema el hemisferio diurno mientras congela el nocturno.

### Clima y Atmósfera
- `planet_temp` **(Float) [-100.0, 500.0]**
  - **Uso:** Temperatura media en superficie (Celsius).
  - **Efectos:**
    - Si `< 0.0`: Convierte progresivamente la superficie y océanos en hielo azul/blanco.
    - Si `> 100.0`: Evapora el nivel de agua `planet_water` hasta secarlo y activa emisión de luz térmica/magma en las zonas bajas si la temperatura se acerca a `400.0`.
- `atm_pressure` **(Float) [0.0, 2.0]**
  - **Uso:** Multiplicador de densidad (1.0 = Tierra).
  - **Efectos:** En `0.0` desactiva completamente los shaders de atmósfera y nubes. En valores `> 1.5`, la atmósfera se vuelve 100% opaca, ocultando la superficie bajo una capa espesa de gas.
- `atm_co2` **(Float) [0.0, 1.0]**
  - **Uso:** Porcentaje de Dióxido de Carbono. Interpola el color del Rayleigh/Mie scattering hacia colores cálidos (amarillo, naranja, marrón ocre).
- `atm_methane` **(Float) [0.0, 1.0]**
  - **Uso:** Porcentaje de Metano. Tiene prioridad sobre otros gases. Tiñe los shaders atmosféricos a un color cian profundo / esmeralda brillante.
- `atm_o2_n2` **(Float) [0.0, 1.0]**
  - **Uso:** Componentes terrestres (Nitrógeno y Oxígeno). Si este valor es dominante (`> 0.7`) y el CO2 es bajo (`< 0.1`), la atmósfera mantendrá tonos celestes translúcidos.
- `planet_water` **(Float) [0.0, 1.0]**
  - **Uso:** Porcentaje de agua líquida en la superficie. Controla visualmente el "nivel del mar" en la altura del shader de terreno. Interacciona con la temperatura para congelarse o evaporarse.

### Geología y Biología
- `tectonic_activity` **(Float) [0.0, 1.0]**
  - **Uso:** Si es `> 0.7` y la temperatura es elevada (`> 100.0`), se calculan líneas de ruido inverso que fungen como fallas geológicas; estas franjas emiten un canal de luz incandescente simulando magma activo.
- `composition_iron` **(Float) [0.0, 1.0]**
  - **Uso:** Modifica la naturaleza de la corteza. Si domina (`> 0.5`) oscurece el planeta a colores gris basalto y rojo óxido y añade valores pseudo-especulares. También aplana el relieve combinándose con masas densas.
- `planet_vegetation` **(Float) [0.0, 1.0]**
  - **Uso:** Mezcla un tono verde `#228B22` sobre las texturas terrestres. El shader comprueba internamente las condiciones de viabilidad: requiere Temperatura entre `10° y 40°` y un porcentaje de Agua mínimo de `0.1` para renderizarse.

---

## Ejemplo de Generación JSON

Para inyectar esto en el compilador, el archivo de salida debe verse así:

```json
{
  "planet_config": {
    "radius": 4500.0,
    "planet_mass": 0.8,
    "star_distance_au": 0.15,
    "rotation_period_hours": 700.0,
    "planet_temp": 120.0,
    "atm_pressure": 0.9,
    "atm_co2": 0.05,
    "atm_methane": 0.0,
    "atm_o2_n2": 0.85,
    "planet_water": 0.4,
    "tectonic_activity": 0.8,
    "composition_iron": 0.7,
    "planet_vegetation": 0.0
  }
}
```
*(Nota: El ejemplo de arriba resultará automáticamente en un planeta con "Tidal Locking", fallas de magma debido a alta actividad tectónica, y sin vegetación dada la temperatura extrema).*

## Cómo aplicarlo en Godot (Para tu script Parser)

Si creas un script para cargar este `.json` dentro del motor, simplemente debes pasarle los valores al objeto `Planet`:

```gdscript
func load_planet_from_json(json_path: String) -> void:
    var file = FileAccess.open(json_path, FileAccess.READ)
    var data = JSON.parse_string(file.get_as_text())
    
    if data and data.has("planet_config"):
        var config = data["planet_config"]
        # Asumiendo que `planet` es la referencia al nodo Planet
        for key in config.keys():
            if key in planet:
                planet.set(key, config[key])
        
        # Una vez asignados todos, le decimos al nodo que propague los cambios a la UI y Shaders
        planet.rebuild_planet()
        
        # Si tienes conectada la UI, no olvides forzar la actualización de los sliders
        update_hud_sliders_from_planet()
```

Siguiendo esta convención exacta de claves y rangos, el sistema compilará las condiciones planetarias de manera eficiente y delegará toda la complejidad visual, matemática y de materiales puramente a la GPU usando los Shaders implementados.
