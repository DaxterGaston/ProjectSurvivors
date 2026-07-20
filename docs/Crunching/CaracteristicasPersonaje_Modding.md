# Crunching de CaracteristicasPersonaje — Modding

## Alcance de este documento

Este documento resuelve las decisiones específicas de modding del sistema de características del personaje (Atributos, Capacidades, Rasgos, Habilidades), apoyándose en el estándar general ya cerrado en [Modding.md](./Modding.md). No repite reglas ya definidas ahí (manifiestos, hashing, patch/override genérico, sandboxing de scripts, etc.) — solo agrega lo propio de este sistema.

Ver [CaracteristicasPersonaje.md](./CaracteristicasPersonaje.md) para las decisiones de diseño general que este documento asume como base.

## Resource types de este sistema

Se reutilizan los tipos ya listados en `Modding.md`:

- `characterAttributeDefinition` (Atributos)
- `characterTraitDefinition` (Rasgos)
- `skillDefinition` (Habilidades)

Se agrega un tipo nuevo, resuelto en este crunching:

- `characterCapacityDefinition` (Capacidades)

Este tipo nuevo debe incorporarse a la lista de "Tipos iniciales de recursos" de `Modding.md`.

## Qué es abierto a mods y qué es cerrado

| Elemento | Abierto a mods | Notas |
|---|---|---|
| Lista de Atributos | Sí | Un mod puede declarar un Atributo nuevo con su propio `characterAttributeDefinition` (ej. "Radiación"). Namespace `core` reservado para los vanilla (Vida, Resistencia, UMS, Fuerza, Defensa, Hambre, Sed). |
| Lista de Capacidades | Sí | Un mod puede declarar una Capacidad nueva (ej. "Pesca") vía `characterCapacityDefinition`. |
| Contenido de Rasgos | Sí | Cualquier mod puede declarar `characterTraitDefinition` nuevos, disponibles como opción de elección en creación de personaje. |
| Contenido de Habilidades (dentro de las 3 categorías) | Sí | Cualquier mod puede declarar `skillDefinition` nuevos, siempre asignados a una de las tres categorías existentes. |
| Categorías de Habilidad (Humanas/Mutaciones/Pasivas) | **No** | Taxonomía cerrada por el core. Cada categoría tiene lógica propia entrelazada (atributo que consume, presencia/ausencia de cooldown) que un mod no puede redefinir declarativamente sin convertirse en un cambio de mecanismo, no de contenido. Un mod que quiera un tema distinto (ej. "habilidades robóticas") debe expresarlo como Habilidad Humana o de Mutación reskineada. |

## Estructura de datos moddeable por tipo

### `characterAttributeDefinition`

Campos relevantes expuestos como patchable (`x-modding: { patchable: true }`) salvo indicación contraria:

- `hasMaxGrowth` (bool): si el atributo participa del mecanismo de acumulador+umbrales (true para Vida/Resistencia/UMS; false para Hambre/Sed, que tienen rango fijo). **No patchable** — cambia la forma del recurso, no solo un valor.
- `range` (para atributos sin `hasMaxGrowth`, ej. Hambre/Sed): `{ min, max }`. Patchable.
- `growthCurve` (para atributos con `hasMaxGrowth`): tabla de umbrales crecientes. Patchable.
- `baseGainPerEvent`: ganancia fija de experiencia por evento disparador. Patchable.
- `levelCap`: tope de niveles de crecimiento. Patchable.
- `baseRegenRate`: tasa de regeneración pasiva. Patchable. (No aplica a Vida, cuya regeneración depende del futuro sistema de Salud.)
- `thresholds` (solo Hambre/Sed): ver sección de umbrales abajo.

### Umbrales de Hambre/Sed como colección por ID

Siguiendo la regla general de `Modding.md` de modelar colecciones moddeables como **maps por ID, no arrays** (para que JSON Patch estándar alcance sin extensiones):

```json
{
  "thresholds": {
    "core.characterAttribute.hunger.thresholds.low_debuff": {
      "value": 60,
      "effect": { "type": "attributeDebuff", "target": "core.characterAttribute.strength", "modifier": -0.1 }
    },
    "core.characterAttribute.hunger.thresholds.critical_damage": {
      "value": 90,
      "effect": { "type": "periodicHealthDamage", "percent": 0.02, "intervalSeconds": 30 }
    }
  }
}
```

- Cada umbral tiene un ID namespaced propio, permitiendo que un mod agregue un umbral nuevo vía `add` de JSON Patch sin tocar los existentes.
- Todo el bloque `thresholds` es `public + patchable`.
- Orden de evaluación: por `value` ascendente, no por orden de declaración en el JSON (evita depender de orden de carga para algo que es puramente numérico).

### `characterCapacityDefinition`

- `growthCurve`, `levelCap`: mismo patrón que Atributos, patchable.
- `unlocksHabilidadesHumanas`: referencia a qué Habilidades Humanas desbloquea cada nivel — modelado como map por ID (`nivel` → lista de IDs de `skillDefinition`), patchable/extensible por otros mods vía dependencia directa.
- El evento que otorga experiencia a una Capacidad **no se declara en este resource type**: lo dispara el sistema externo correspondiente (Combate, Puntería, Elaboración, Construcción) llamando al hook `OtorgarExperienciaCapacidad`. Los extension points concretos de esos sistemas se definen en sus propios crunchings, no acá.

### `characterTraitDefinition` (Rasgos)

- `rewards`: lista fija de recompensas que otorga el Rasgo (niveles de Atributo, niveles de Capacidad, una Habilidad Pasiva, una Habilidad de Mutación). No es un pool de puntos — el propio recurso declara la combinación completa.
- Un Rasgo que otorga una Habilidad de Mutación o Pasiva de otro mod debe declarar **dependencia directa** hacia ese mod, según la regla general de referencias cross-mod de `Modding.md`.
- No existe mecanismo de "peso"/costo entre Rasgos: cada uno es una opción independiente en la lista de elección de creación de personaje.

### `skillDefinition` (Habilidades)

- `category`: `humana | mutacion | pasiva`. Enum cerrado — **no** extensible por mods (ver tabla de arriba). Un mod debe elegir una de las tres.
- `resourceCost` y `attributeConsumed`: para `humana` (Resistencia) y `mutacion` (UMS). No aplica a `pasiva`.
- `cooldownCurve`, `costEfficiencyCurve`: por nivel, para `humana`/`mutacion`. Patchable.
- `effectMagnitudeCurve`: por nivel, para `pasiva`. Patchable.
- `unlockConditions`: lista abierta de condiciones de desbloqueo (ej. `{ type: "capacityLevel", capacity: "core.characterCapacity.combate", level: 5 }`). Modelada como lista abierta para que mods de otros sistemas (NPCs, misiones) puedan declarar sus propios tipos de condición sin que este recurso necesite conocerlos de antemano — el motor solo garantiza evaluar `capacityLevel` de forma nativa; cualquier otro `type` es resuelto por el sistema externo que lo registre.
- Efecto real de la Habilidad (qué hace al activarse): se declara vía `scriptBehavior` (autoridad `server` si afecta gameplay compartido, según estándar general de scripting de `Modding.md`). El diseño de esos efectos concretos no es tema de este documento.

## Taxonomía de extension points (categorías de `Modding.md`)

| Categoría estándar | Aplicación en este sistema |
|---|---|
| `Definitions` | `characterAttributeDefinition`, `characterCapacityDefinition`, `characterTraitDefinition`, `skillDefinition` |
| `Tables` | Curvas de crecimiento (`growthCurve`), umbrales de Hambre/Sed, curvas de cooldown/eficiencia/magnitud |
| `Rules` | `unlockConditions` de Habilidades, `rewards` de Rasgos |
| `Behaviors` | Efecto de Habilidad vía `scriptBehavior` |
| `Assets` | Íconos/sprites de Atributos, Capacidades, Rasgos y Habilidades (referenciados por ID, según estándar general) |

## Pendiente sin resolver

- Evento de experiencia para Habilidades Pasivas (ver también `CaracteristicasPersonaje.md`): mientras no se resuelva del lado de diseño, tampoco puede declararse su extension point de modding.
- Extension points concretos de Elaboración/Construcción/Combate/Puntería que otorgan experiencia de Capacidad: pertenecen a los crunchings específicos de esos sistemas, no a este documento.
- Extension point de aprendizaje de Habilidades Humanas vía NPC: [NPCs_Modding.md](./NPCs_Modding.md) ya está resuelto para su parte del sistema, pero deja este flujo concreto también pendiente.
- Extension point de obtención de Habilidades de Mutación vía virus: pertenece al futuro crunching de virus/mutaciones.

## Resultado esperado de este documento

Da a los mods un contrato claro sobre el sistema de personaje: qué pueden extender libremente (Atributos, Capacidades, contenido de Rasgos y Habilidades) y qué permanece fijo por diseño (categorías de Habilidad), evitando que cada mod de personaje deba redescubrir estas reglas o inventar su propia forma de extender el sistema.
