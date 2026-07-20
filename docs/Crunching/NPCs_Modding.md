# Crunching de NPCs — Modding

## Alcance de este documento

Este documento resuelve las decisiones específicas de modding del sistema de NPCs, apoyándose en el estándar general ya cerrado en [Modding.md](./Modding.md). No repite reglas ya definidas ahí (manifiestos, hashing, patch/override genérico, sandboxing de scripts, lifecycles base, etc.) — solo agrega lo propio de este sistema.

Ver [NPCs.md](./NPCs.md) para las decisiones de diseño general que este documento asume como base.

## Resource types de este sistema

MVP de este crunching, los tres ya listados en `Modding.md`:

- `npcArchetypeDefinition`
- `npcOrderDefinition`
- `npcTaskDefinition`

`communityDefinition`, `communityPerkDefinition` y `tradeRuleDefinition` **no** se resuelven en este documento: pertenecen de lleno al futuro crunching de Comunidades, incluso a nivel de forma del resource type.

Los NPCs también reutilizan `characterTraitDefinition` (Rasgos) y `skillDefinition` (Habilidades Humanas/Pasivas), ya resueltos en [CaracteristicasPersonaje_Modding.md](./CaracteristicasPersonaje_Modding.md) — no se redefinen acá.

## `npcArchetypeDefinition`

Campos relevantes:

- `attributeRanges`: rango de valores iniciales para Vida, Resistencia, Hambre, Sed. Patchable.
- `capacityLevels`: nivel fijo (o rango a sortear) por Capacidad, asignado en spawn. Patchable.
- `traitPool`: lista de IDs de `characterTraitDefinition` sorteables para este arquetipo (0 o 1 resultado por instancia). Modelada como lista abierta para que otros mods puedan agregar entradas vía `add` de JSON Patch. Un Rasgo referenciado que pertenece a otro mod requiere dependencia directa, según la regla general de referencias cross-mod.
- `visual`: referencia a asset de apariencia (por ID, según estándar general de Assets).
- `defaultCommunity`: referencia opcional a comunidad por defecto, si aplica (el mecanismo real de asignación de comunidad es responsabilidad del futuro sistema de Comunidades; este campo solo declara el dato).

`npcArchetypeDefinition` no define nada sobre spawn (dónde/cuándo/cuántos): ese uso pertenece a `spawnTable`, ya listado en `Modding.md` como resource type genérico, consumido por el futuro sistema de generación de mapa.

## `npcOrderDefinition` y `npcTaskDefinition`

Ambos comparten la misma base de lifecycle definida como ejemplo en `Modding.md`, con una diferencia deliberada entre sí:

### `npcOrderDefinition`

Lifecycle completo, igual al ejemplo de `Modding.md`:

```text
canStart(ctx) -> bool
score(ctx) -> number
onStart(ctx) -> commands
tick(ctx) -> commands
canComplete(ctx) -> bool
onComplete(ctx) -> commands
onCancel(ctx) -> commands
```

Una orden tiene condición de finalización explícita (`canComplete`) porque su naturaleza es ejecutarse y terminar.

### `npcTaskDefinition`

Lifecycle recortado, sin `canComplete`/`onComplete`:

```text
canStart(ctx) -> bool
score(ctx) -> number
onStart(ctx) -> commands
tick(ctx) -> commands
onCancel(ctx) -> commands
```

Una tarea no "termina" en el sentido de una orden — es indefinida, y solo deja de ejecutarse por interrupción (`onCancel`, disparado por una orden nueva o una reacción de supervivencia con mayor prioridad). La lógica de "¿corresponde actuar ahora?" (ej. revisar si un cultivo está listo) vive dentro de `tick(ctx)`, que decide en cada llamada si emitir comandos ese tick o no hacer nada.

### Autoridad y scoring

- Ambos son extension points de autoridad `server`: afectan gameplay compartido (comportamiento de NPCs de comunidad), se ejecutan y validan exclusivamente en host/server, según el estándar general de scripting de `Modding.md`.
- `score(ctx)` de ambos participa del mismo selector de prioridad del behaviour tree del NPC (ver `NPCs.md`). Las reacciones de supervivencia **no** son un extension point moddeable de tipo `score` — son una capa fija del core, evaluada antes de consultar cualquier `score` de orden/tarea, precisamente para que ningún mod pueda hacer que un NPC ignore una amenaza inflando artificialmente un `score`.
- El contexto (`ctx`) que reciben estos extension points incluye, entre otros datos de lectura acotada: el Estado Anímico actual del NPC (si aplica) y su modificador de eficiencia derivado, para que la implementación de la orden/tarea pueda aplicarlo a su resultado si corresponde (ver sección de Estado Anímico en `NPCs.md`). El sistema de NPCs no aplica ese modificador de forma automática — cada `npcOrderDefinition`/`npcTaskDefinition` decide si y cómo lo usa.

## Taxonomía de extension points (categorías de `Modding.md`)

| Categoría estándar | Aplicación en este sistema |
|---|---|
| `Definitions` | `npcArchetypeDefinition`, `npcOrderDefinition`, `npcTaskDefinition` |
| `Tables` | `attributeRanges`, `capacityLevels`, `traitPool` de `npcArchetypeDefinition` |
| `Rules` | `canStart`/`canComplete` de órdenes, condiciones internas de `tick` en tareas |
| `Behaviors` | Lógica de `score`/`onStart`/`tick`/`onComplete`/`onCancel` de órdenes y tareas vía `scriptBehavior`, autoridad `server` |
| `Assets` | `visual` de `npcArchetypeDefinition` |

## Pendiente sin resolver

- Extension points de Comunidades (reclutamiento, favores, manager, disparadores de ánimo, `communityDefinition`/`communityPerkDefinition`/`tradeRuleDefinition`): pertenecen al futuro crunching de Comunidades, no a este documento.
- Extension point de progresión de Capacidad de NPC vía interacción con construcciones/tareas de largo plazo: depende del futuro crunching de Comunidades.
- Extension point de spawn concreto (`spawnTable` aplicado a NPCs): depende del futuro crunching de generación de mapa.
- Extension point de aprendizaje de Habilidades Humanas por NPC hacia jugador (mencionado como pendiente en `CaracteristicasPersonaje_Modding.md`): depende de qué tan a fondo se resuelva ese flujo, todavía sin definir.

## Resultado esperado de este documento

Da a los mods un contrato claro sobre el sistema de NPCs: qué pueden extender libremente (arquetipos, órdenes, tareas, dentro del set MVP ya cerrado) y qué permanece fijo por diseño (prioridad absoluta de reacciones de supervivencia frente a cualquier score de orden/tarea moddeada), evitando que cada mod de NPCs deba redescubrir estas reglas.
