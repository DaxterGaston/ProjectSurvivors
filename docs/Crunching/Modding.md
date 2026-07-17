# Crunching de Modding

## Alcance de este documento

Este documento es la version definitiva (sujeta a revision) de la arquitectura de modding de `Project Survivors`, resultado de dos sesiones de crunching. Reemplaza a [Modding_BETA.md](./Modding_BETA.md) como referencia activa; ese documento queda conservado por trazabilidad historica.

No define todavia el schema completo de cada sistema especifico, como items, misiones, NPCs, mapa, comunidades o habilidades. Su objetivo es establecer los estandares comunes que esos sistemas deben respetar cuando expongan puntos de extension.

Este documento se apoya en las decisiones generales ya cerradas en [Idea.md](./Idea.md), especialmente:

- `modpack estricto`
- validacion por `id + version + hash`
- carga de mods al iniciar partida
- sin modificacion efectiva de mods durante una partida iniciada
- save atado a `historia + version + modpack exacto`
- cliente y host/server deben coincidir exactamente en multiplayer
- gameplay compartido ejecutado o validado por host/server
- soporte offline parcial en single-player si todo el contenido esta instalado

## Principio base

El sistema de modding es `data-first`.

Los mods deben declarar contenido, reglas, assets y puntos de extension principalmente mediante archivos declarativos. Los scripts existen solo para casos donde los datos no alcanzan, como comportamientos de NPC, condiciones custom o efectos especificos.

Los mods no modifican clases internas del juego ni dependen del layout interno de implementacion. Interactuan con una API publica de modding.

## Historias, mods y perfiles

Las historias, los mods y los perfiles son sistemas separados, pero comparten infraestructura de validacion, instalacion, catalogo y hashing.

- Una partida requiere exactamente una historia activa.
- Una historia puede depender de mods.
- Una historia no es simplemente otro mod.
- `StoryPackage`, `ModPackage` y `ProfilePackage` son tres tipos distintos de `ContentPackage`.
- Los tres usan manifiestos, version, hash, dependencias y validacion compartidos.

## Paquetes y manifiestos

Todo mod debe ser un paquete autocontenido con manifiesto obligatorio.

Ejemplo conceptual:

```text
MyMod/
  mod.json
  resources.json
  data/
  scripts/
  assets/
```

### Estructura de `mod.json` y `story.json`

El manifiesto se divide en dos bloques:

- Bloque obligatorio tecnico: es lo unico que el runtime necesita para cargar, resolver dependencias y validar. Forma parte del hash del paquete.
- Bloque `meta` opcional: metadata de catalogo (autor, licencia, descripcion, tags, icono, etc.). No afecta si el mod carga o no, y no forma parte del hash tecnico del paquete.

Motivo de la separacion: si la metadata de catalogo formara parte del bloque hasheado, una edicion cosmetica (por ejemplo corregir una descripcion) rompería el hash y invalidaria saves/multiplayer sin motivo real.

`mod.json`:

```json
{
  "id": "author.mod_name",
  "name": "Mod Name",
  "version": "1.0.0",
  "gameVersion": "0.1.x",
  "moddingApi": "1.0",
  "dependencies": [],
  "resourcesIndex": "resources.json",
  "meta": {
    "author": "author_name",
    "license": "MIT",
    "description": "...",
    "tags": ["weapon", "melee"],
    "iconAsset": "author.mod_name.assets.icon"
  }
}
```

`story.json` usa exactamente la misma forma, agregando un unico campo obligatorio adicional: el punto de entrada narrativo.

```json
{
  "id": "author.story_id",
  "name": "Story Name",
  "version": "1.0.0",
  "gameVersion": "0.1.x",
  "moddingApi": "1.0",
  "dependencies": [],
  "resourcesIndex": "resources.json",
  "entryPoint": "author.story_id.storyGraph.main",
  "meta": { "...": "..." }
}
```

### Metadata de catalogo: local vs. publicacion

- Para uso local/dev, todo el bloque `meta` es opcional. Esto minimiza friccion para modders durante desarrollo.
- Para **publicar** en el catalogo oficial, el backend externo exige como minimo `author`, `description` y al menos un tag de la taxonomia global.
- `license` es recomendado pero no bloqueante.
- El Mod Validator marca metadata de catalogo faltante como warning, no como error, salvo que el mod se este empaquetando especificamente para publicacion.

## Formato de archivos declarativos

El formato runtime/canonico para definiciones de mods e historias es `JSON`.

- `mod.json`, `story.json`, indices y recursos declarativos son JSON.
- Se deben definir JSON Schemas por tipo de recurso y version de API.
- El runtime consume JSON validado.
- Otros formatos pueden existir como herramientas de autoria externas, pero deben exportar JSON para el runtime.
- `version` en `mod.json`/`story.json`/`profile.json`, `gameVersion` y `moddingApi` deben cumplir semver (`MAJOR.MINOR.PATCH`) y son validados por el Mod Validator. Un formato de version invalido es error de validacion (`MANIFEST_VERSION_INVALID`).

## Indice central de recursos

Cada paquete debe declarar explicitamente sus recursos en un indice central.

`resources.json` es una **lista plana sin categorizacion por tipo**:

```json
{
  "resources": [
    "data/items/scrap_spear.json",
    "data/recipes/scrap_spear_recipe.json",
    "assets/sprites/scrap_spear_icon.asset.json",
    "scripts/npc_orders/guard_area.behavior.json"
  ]
}
```

El tipo real de cada recurso vive unicamente en su propio archivo (campo `type` de la metadata comun). El indice no duplica esa informacion para evitar que ambas fuentes se desincronicen.

Reglas:

- El runtime solo carga recursos listados.
- Un archivo no listado no existe para el juego.
- El hash del paquete se calcula desde el manifiesto, indice y recursos listados, ordenados de forma canonica (alfabetico por path/ID), independientemente del orden de aparicion en el indice.
- Los archivos temporales, documentacion o tooling no afectan el mod si no estan declarados.

## Namespaces e IDs

Todos los recursos deben usar IDs globales con namespace obligatorio.

Formato conceptual:

```text
package_id.resource_type.resource_id
```

Ejemplos:

```text
core.items.water_bottle
core.recipes.boiled_water
core.npc_orders.scavenge
author.extra_weapons.items.scrap_spear
```

Reglas:

- `core` queda reservado para contenido vanilla.
- Cada mod usa su `mod.id` como raiz de namespace.
- Cada historia usa su `story.id` como raiz para recursos propios.
- Cada perfil usa su `profile.id` como raiz para recursos propios.
- El nombre visible al jugador no es el ID.
- Las referencias entre paquetes usan IDs completos.

## Modding API

La API de modding se versiona de forma independiente a la version del juego.

- `gameVersion` expresa compatibilidad general con builds del juego.
- `moddingApi` expresa compatibilidad con schemas, contextos de scripting, lifecycle y extension points.
- Cambios breaking suben major. Cambios aditivos suben minor.
- Un mod declara contra que API fue escrito.
- La validacion falla si la API requerida no esta soportada.

## Calculo de hashes

El hash de un paquete usa un **arbol de hashes por archivo**, no un hash monolitico:

- Cada recurso individual (incluyendo binarios de assets) se hashea por separado (SHA-256).
- El indice se ordena de forma canonica (alfabetico por ID de recurso).
- El hash final del paquete se calcula sobre la lista canonica de `(resourceId, resourceHash)` mas el manifiesto.

Motivo: con hash por archivo, el Mod Validator puede reportar exactamente que recurso difiere cuando el hash no matchea, en vez de solo "el paquete cambio". Tambien habilita deduplicacion content-addressed real (los blobs se direccionan por hash de recurso individual) y refuerza la deteccion de ediciones locales maliciosas apuntadas a un archivo puntual.

## `ResolvedModpack`

El juego nunca consume mods crudos directamente durante una partida.

Antes de crear mundo, abrir save o iniciar una sesion multiplayer, se construye un `ResolvedModpack` validado.

Incluye:

- historia seleccionada
- dependencias
- mods activos
- orden final de carga
- versiones exactas
- hashes exactos
- assets indexados
- recursos parseados
- scripts permitidos registrados
- patches y overrides aplicados
- conflictos resueltos o reportados

El `ResolvedModpack` es un artefacto **serializable y persistido**: se calcula una vez al crear la partida, se serializa a JSON con su propio hash final, y ese JSON congelado queda atado al save.

Al abrir un save, el juego **no** vuelve a resolver dependencias contra los mods instalados: carga el `ResolvedModpack` congelado y solo valida que cada mod referenciado siga instalado con el hash exacto. La resolucion completa desde cero solo ocurre al **crear** una partida nueva.

### Reporte exportable de modpack

Distinto del `ResolvedModpack` persistido, el Mod Validator genera bajo demanda un **reporte legible para humanos**: lista de mods con id/version/hash, orden final, warnings, errores con codigos estables, y un resumen apto para pegar en un ticket de soporte o post de foro. No se persiste con el save.

## Dependencias

Solo existen dependencias obligatorias.

No se soportan en el estandar inicial:

- dependencias opcionales
- contenido condicional por mod presente
- bloques `when modLoaded`

Reglas:

- Si un mod referencia algo de otro mod, debe declararlo como dependencia obligatoria.
- Si falta una dependencia, el mod no carga.
- Los mods de compatibilidad deben existir como mods separados con dependencias obligatorias hacia los mods que integran.
- Todo acceso cross-mod requiere dependencia directa, incluso si el paquete ya llega transitivamente.

Los manifiestos usan rangos de version para resolver compatibilidad antes de crear partida:

```json
{
  "dependencies": [
    { "id": "author.weapon_assets", "version": ">=1.2.0 <2.0.0" }
  ]
}
```

Una vez resuelto, el `ResolvedModpack`, los saves y multiplayer usan version exacta y hash exacto.

### Algoritmo de resolucion y orden de carga

- El orden inicial se propone mediante **orden topologico** sobre el grafo de dependencias.
- Desempate entre mods sin relacion de dependencia: alfabetico por `id`, para que el resultado sea reproducible y no dependa del orden de instalacion.
- Las dependencias siempre cargan antes que los dependientes.
- Top = mayor prioridad. Lo que carga primero gana conflictos.
- La UI bloquea ordenes que violen dependencias; el usuario puede reordenar mods independientes.
- Al crear partida, el orden final queda congelado en el save (dentro del `ResolvedModpack`).

Casos sin resolucion automatica (error duro, no se intenta ninguna heuristica de "version cercana" ni instalacion paralela de dos versiones del mismo mod dentro de un mismo modpack):

- Ciclo de dependencias: `MOD_DEPENDENCY_CYCLE`, se lista el ciclo completo.
- Conflicto de rango de version entre dos mods que dependen de un tercero con rangos incompatibles: `MOD_VERSION_CONFLICT`.

## Saves, snapshots y almacenamiento

Los saves usan exact match siempre.

- Un save solo abre con la misma historia, versiones y hashes exactos.
- No hay migraciones de saves en el estandar inicial.
- Las migraciones explicitas quedan documentadas como posibilidad muy futura, fuera de scope actual.

Cada save debe quedar atado a un snapshot local del contenido usado.

El diseno logico debe ser direccionable por contenido y deduplicable por hash (por archivo, ver seccion de hashing), aunque una implementacion MVP pueda copiar carpetas completas.

### Multi-version instalada

El juego permite **multiples versiones del mismo mod instaladas en paralelo**, direccionadas por `id + version + hash`. Esto es consecuencia directa de exact match sin migraciones: distintos saves, distintos servidores o distintos perfiles pueden requerir versiones distintas del mismo mod simultaneamente.

Reglas de almacenamiento:

- El juego debe mostrar espacio ocupado por saves, snapshots, mods instalados, cache de descarga y versiones antiguas.
- El jugador puede borrar saves completos.
- El jugador puede borrar mods instalados que no esten siendo usados por saves.
- El jugador puede borrar versiones antiguas no referenciadas.
- El jugador puede borrar cache de descarga.
- El juego no debe borrar automaticamente contenido requerido por un save sin confirmacion explicita.
- Si se usa deduplicacion, borrar un save solo libera blobs no referenciados por otros saves.
- En Android la gestion debe ser mas visible por limitaciones de almacenamiento, sin romper saves sin confirmacion.

La forma concreta de mostrar el gestor de almacenamiento en UI queda **pendiente**, es una decision de UI/UX fuera del alcance de este crunching de arquitectura.

## Overrides y patches

Por defecto, un mod agrega contenido.

Para modificar contenido existente debe declarar una accion explicita:

- `override`: reemplazo total de un recurso.
- `patch`: modificacion parcial declarativa.

El comportamiento vanilla tambien puede ser modificado mediante estos mecanismos.

Los patches usan **JSON Patch RFC 6902 estandar, sin extensiones propias**.

Operaciones iniciales: `add`, `remove`, `replace`, `test`. No incluidas inicialmente: `move`, `copy`.

Reglas de conflicto:

- Primero que llega gana segun orden de carga.
- El primer cambio sobre un campo gana.
- Un `override` total equivale a decidir todos los campos patchables del recurso.
- Un `patch` decide solo los campos que toca.
- Si un patch/override posterior pierde, se reporta **warning**.
- Si la accion estaba marcada como `required`, perder genera **error** de validacion.

Ejemplo conceptual:

```json
{
  "target": "core.items.canned_food",
  "required": false,
  "operations": [
    { "op": "replace", "path": "/payload/nutrition", "value": 15 },
    { "op": "add", "path": "/tags/-", "value": "rare_food" }
  ]
}
```

### Colecciones por ID, no por indice

Las colecciones logicas modeables se modelan como **maps por ID** en el propio JSON del recurso (no arrays), de forma que JSON Patch estandar alcanza sin extensiones:

```json
{
  "entries": {
    "a": { "weight": 1 },
    "b": { "weight": 2 }
  }
}
```

```json
{ "op": "replace", "path": "/payload/entries/a/weight", "value": 5 }
```

Los patches sobre arrays por indice solo se permiten donde el schema declare orden posicional real (ej. pasos de una receta), aceptando ahi la fragilidad inherente como excepcion consciente.

## Visibilidad y modificabilidad de recursos

Todo recurso puede declarar visibilidad: `private` (default) o `public`.

Reglas:

- `public`: otros mods pueden referenciarlo si declaran dependencia directa.
- `private`: solo recursos del mismo paquete pueden referenciarlo.
- Si un mod referencia un recurso privado de otro paquete, falla validacion.

Un recurso publico declara modificabilidad:

```json
{
  "modification": {
    "patchable": true,
    "overridable": false
  }
}
```

- `public + patchable`: otros mods pueden parchear campos permitidos.
- `public + overridable`: otros mods pueden reemplazar la definicion completa.
- `public` sin patch/override: otros mods solo pueden referenciar.

### Declaracion de patchability en el schema

Que campos son patcheables se declara **dentro del propio JSON Schema del tipo de recurso**, con una keyword custom por propiedad, por ejemplo `"x-modding": { "patchable": true }`. El Mod Validator la lee al validar una operacion de patch/override entrante: si el path apuntado cae en un campo sin esa marca (o marcado `false`), la operacion se rechaza.

Motivo: el mismo schema que valida la forma del payload documenta que es modeable, evitando una segunda fuente de verdad (lista de paths separada) que se desincroniza cuando el schema cambia.

## Taxonomia de puntos de extension

El estandar usa categorias comunes para evitar que cada sistema redefina que significa ser modeable.

Categorias: `Definitions`, `Tables`, `Rules`, `Behaviors`, `Assets`.

Cada sistema especifico debe listar que extension points expone bajo estas categorias.

## Tipos iniciales de recursos

Lista inicial extensible:

- `itemDefinition`
- `recipeDefinition`
- `characterAttributeDefinition`
- `characterTraitDefinition`
- `skillDefinition`
- `missionDefinition`
- `dialogueDefinition`
- `storyGraphDefinition` solo en historias
- `npcArchetypeDefinition`
- `npcOrderDefinition`
- `npcTaskDefinition`
- `communityDefinition`
- `communityPerkDefinition`
- `tradeRuleDefinition`
- `mapNodeDefinition`
- `mapGenerationRuleDefinition`
- `lootTable`
- `spawnTable`
- `assetDefinition`
- `scriptBehavior`
- `profileDefinition` (contenido de `profile.json`: historia referenciada, mods con version exacta, bloque de configuracion)

Que tipos entran realmente al MVP **no se decide en este crunching**: es una decision de alcance de producto por sistema (items, misiones, NPCs, etc.) que se resuelve en el crunching especifico de cada uno, porque el modding de cada sistema tiene logica propia (no es lo mismo modear un vehiculo que rutinas automatizadas de NPC). Lo que este documento cierra es el *mecanismo* de habilitacion (registro versionado en la Modding API con su propio schema), no la lista final.

Los schemas finos de cada tipo se definen en los crunchings especificos de cada sistema.

## Metadata comun de recursos

Todo recurso declarativo debe tener metadata comun minima.

Obligatorios: `id`, `type`, `apiVersion`, `payload`.

Opcionales: `display`, `tags`, `visibility`, metadata de tooling.

```json
{
  "id": "author.weapon_pack.items.scrap_spear",
  "type": "itemDefinition",
  "apiVersion": "1.0",
  "visibility": "public",
  "display": {
    "nameKey": "author.weapon_pack.loc.items.scrap_spear.name",
    "descriptionKey": "author.weapon_pack.loc.items.scrap_spear.description"
  },
  "tags": ["weapon", "melee"],
  "payload": {}
}
```

### Tags: globales vs. namespaced

Dos capas conviven:

- **Tags globales core**: lista cerrada definida por el juego, sin namespace (`weapon`, `melee`, `food`, `rare`, etc.), usada para filtros consistentes de UI/catalogo entre todo el contenido.
- **Tags libres namespaced**: cualquier mod puede crear los suyos (`author.mod_id.tag_name`) para organizacion propia o para que mods de compatibilidad los referencien. No estan garantizados en filtros estandar de catalogo.

## Localizacion

Cada mod trae sus propios archivos de localizacion dentro de su paquete, uno por idioma, declarados en el indice central igual que cualquier otro recurso (ej. `loc/en.json`, `loc/es.json`), con claves namespaced bajo el propio `mod.id`.

El motor arma la tabla de strings en runtime combinando todos los mods activos del `ResolvedModpack`. Si una clave no existe en el idioma activo, cae a ingles como fallback; si tampoco existe ahi, muestra la clave cruda como ultimo recurso, solo util para diagnostico.

## Assets

Los assets se referencian solo por ID declarado. No se permiten referencias directas por path desde recursos de gameplay.

```json
{
  "id": "author.weapon_pack.assets.sprites.scrap_spear_icon",
  "type": "assetDefinition",
  "apiVersion": "1.0",
  "payload": {
    "assetType": "sprite",
    "file": "assets/sprites/scrap_spear_icon.png",
    "import": {
      "pixelsPerUnit": 64,
      "filterMode": "point",
      "compression": "none"
    }
  }
}
```

Reglas:

- El path del archivo binario es detalle interno del asset declarado.
- El hash incluye metadata y binario (como archivo individual dentro del arbol de hashes).
- Assets no declarados no forman parte del contrato publico.
- Otros mods pueden referenciar assets publicos de otro mod solo con dependencia directa.

### Assets fuera de scope inicial

Quedan fuera del estandar inicial: prefabs Unity, escenas Unity, shaders propios, assets que requieran compilacion/importacion Unity compleja, AssetBundles o packages generados desde una extension de editor.

Esto no es una prohibicion permanente. A futuro se preve un `Unity Modding Package` o extension de editor para permitir ese tipo de contenido de forma controlada. Los tipos/capabilities relacionados quedan reservados, pero son invalidos en la Modding API inicial.

## Scripting

Los lenguajes previstos son Lua y JavaScript. La API publica es la misma para ambos (`Mod Scripting API`), con adaptadores por runtime.

### Eleccion tecnica de runtime

Runtimes **100% C# puro**: `MoonSharp` para Lua, `Jint` para JavaScript. Se descartan variantes con binario nativo (`NLua`, `ClearScript`/V8).

Motivo: portabilidad a Android y servidor headless sin compilar binarios nativos por arquitectura, y sandboxing mas simple (sin capa de bindings nativos que auditar). El costo es rendimiento por llamada en extension points de alta frecuencia (ej. `tick(ctx)` de `NPCOrder` llamado por cada NPC en cada tick del servidor, que escala con cantidad de NPCs activos, no es una llamada aislada). Este costo se mitiga con: presupuestos de tiempo por llamada mas conservadores en extension points de tipo tick, presupuestos diferenciados por plataforma (ver mas abajo), y un limite de cantidad de NPCs por comunidad que acota el peor caso.

Reglas generales:

- No se expone C# arbitrario como estandar de modding.
- Los scripts se ejecutan en sandbox.
- No acceden directamente a Unity ni a singletons internos.
- No pueden leer/escribir archivos arbitrarios.
- No pueden abrir red arbitraria.
- No modifican memoria/codigo del juego.
- Solo interactuan mediante contextos y comandos permitidos.

### Modelo de permisos

**Sandbox uniforme fijo por extension point**, sin sistema de permisos declarativo por mod. Cada extension point (`NPCOrder`, efecto de item, regla de generacion de mapa, etc.) define de antemano, a nivel de la Modding API, exactamente que contexto de lectura y que comandos puede emitir. Un mod que implementa un extension point tiene ese contexto y esos comandos, ni mas ni menos; no existe forma de pedir capabilities adicionales ni de que el jugador otorgue permisos extra.

Motivo: el sandbox ya es acotado por diseno en cada extension point (nunca hay red, filesystem ni acceso arbitrario bajo ninguna circunstancia), asi que no hay un menu de capabilities de riesgo variable entre las que elegir. Mismo patron que los tipos/capabilities reservados para el futuro Unity Modding Package: capabilities controladas centralmente, no otorgadas dinamicamente.

### Autoridad de scripts

- `server`: afectan gameplay, se ejecutan/validan en host/server.
- `clientPresentation`: solo afectan presentacion.
- No existe `sharedPrediction` en el estandar inicial.
- El cliente no decide resultados finales de gameplay.

### Determinismo

- Generacion de mapa: determinismo obligatorio con seed/contexto.
- Loot/rewards: deterministas o generados por host con resultado persistido.
- NPC orders/tasks: pueden no ser perfectamente deterministas, pero sus efectos finales deben persistirse/sincronizarse.
- Random debe venir de `ctx.random`.
- No se permite random global, tiempo real, filesystem o estado externo.

### Modelo de interaccion con estado

Los scripts no mutan estado interno directamente. Modelo: lectura acotada desde contexto -> retorno de comandos/eventos validados -> aplicacion por el motor. No se permiten setters directos sobre entidades internas. Los comandos forman parte de la API versionada.

### Lifecycles

Se definen por tipo de extension, no como lifecycle universal.

Base comun conceptual:

```text
validate(ctx)
init(ctx)
dispose(ctx)
```

Ejemplo `NPCOrder`:

```text
canStart(ctx) -> bool
score(ctx) -> number
onStart(ctx) -> commands
tick(ctx) -> commands
canComplete(ctx) -> bool
onComplete(ctx) -> commands
onCancel(ctx) -> commands
```

Cada extension point define funciones permitidas, defaults y presupuestos.

### Limites y performance

Los scripts tienen limites obligatorios definidos por el juego, por extension point/runtime: tiempo maximo por llamada, memoria maxima aproximada, cantidad maxima de comandos devueltos, tamano/profundidad de datos retornados, limite de logs, limite de eventos emitidos.

**Presupuestos diferenciados por plataforma**: el juego define, por extension point, un valor de presupuesto para PC/host y uno mas conservador para Android. El mod no sabe ni le importa en que plataforma corre; el presupuesto aplica segun donde corre fisicamente el contexto (cliente Android vs. host PC/dedicado). Motivo: scripts `server` corren en el host (inicialmente PC, mas holgado); scripts `clientPresentation` pueden correr en Android (mas limitado). Un unico valor global o penaliza al host sin necesidad, o deja pasar scripts que saturan un Android.

El mod puede pedir limites menores, pero no elevar los maximos del juego.

### Errores en runtime

Politica general: `fail-contained`.

- Un error de script no debe crashear el juego.
- Scripts de presentacion fallidos se desactivan con fallback.
- NPC orders/tasks fallidas se cancelan y el NPC vuelve a fallback/idle.
- Efectos de habilidad/item/combat fallidos rechazan la accion sin aplicar cambios parciales.
- Errores de generacion de mapa fallan la creacion del mundo.
- Gameplay autoritativo debe ser transaccional: todo o nada.
- Logs deben incluir paquete, recurso, extension point y causa.

## Mod-Manager y perfiles

El juego expone un **Mod-Manager** en el menu de inicio, donde el jugador crea **perfiles**: combinaciones reusables de historia + lista de mods con version + configuraciones del juego. Al iniciar una partida, el jugador puede precargar uno de estos perfiles.

### Perfiles y `ProfilePackage`

- Un perfil guarda **versiones exactas pineadas** de historia y mods, no rangos. No se auto-actualiza silenciosamente.
- Usar un perfil para iniciar una partida nueva pre-rellena la seleccion y corre el flujo normal ya definido (resolver dependencias -> validar -> generar mundo). El perfil en si no es un `ResolvedModpack` congelado, es una plantilla de seleccion; la resolucion completa se vuelve a correr cada vez que se usa para crear partida (por si algo cambio, ej. un mod fue desinstalado).
- `ProfilePackage` es el tercer tipo de `ContentPackage` (junto a `ModPackage` y `StoryPackage`), con la misma infraestructura de manifiesto, id namespaced, hash, version y dependencias (sus dependencias obligatorias son la historia y los mods que referencia).
- Formato de distribucion: `.psprofile`, mismo contenedor ZIP que `.psmod`/`.psstory`, tipicamente conteniendo solo `profile.json` (sin `data/`/`assets/`/`scripts/` propios).
- `profileDefinition` es el resource type asociado al contenido de `profile.json`.
- Los perfiles son **publicables en el workshop**, igual que mods e historias, para que jugadores compartan combinaciones de mods/configuraciones que consideren buenas.

### Configuracion y mods

- Los mods **nunca** pueden sobreescribir la configuracion vanilla del juego. Solo pueden **agregar** configuracion adicional.
- El mecanismo de resolucion de override entre configuraciones (por ejemplo cuando dos mods agregan configuracion relacionada) queda pendiente para el crunching especifico del sistema de configuracion.
- Los **perfiles** son el mecanismo para establecer/fijar configuracion al iniciar una partida.

## Multiplayer

El multiplayer usa modpack estricto.

Reglas:

- Host/server declara el `ResolvedModpack` requerido.
- Cliente valida identidad exacta.
- Si falta algo o el hash no coincide, no entra.
- El host nunca distribuye mods automaticamente.
- La UI del cliente debe mostrar que historia/mods/versiones/hashes necesita.
- Si hay fuente de catalogo conocida, la UI puede abrir el catalogo prefiltrado.
- Si el mod es local/no publicado, se muestra que requiere instalacion manual.
- No se ofrecen versiones cercanas como compatibles.

Los mods locales no publicados pueden usarse en multiplayer si todos los participantes tienen el mismo `id + version + hash`. El juego debe indicar cuando el contenido no esta verificado por catalogo.

### Contenido no verificado en servidores publicos

Un servidor dedicado puede activar un flag de configuracion (`requireVerifiedContent`, default `false`) para rechazar modpacks que incluyan contenido no verificado por catalogo, aceptando solo mods publicados oficialmente. Si el flag esta activo, el `ResolvedModpack` del servidor rechaza cualquier mod cuyo `source` no apunte a catalogo oficial verificado, antes de validacion de clientes.

Cuando una conexion es rechazada por este motivo, el servidor debe devolver al cliente un **mensaje explicito indicando cual mod especifico causo el rechazo** (no un rechazo generico). Motivo: el cliente no necesariamente actua de mala fe — puede tener una descarga corrupta o desactualizada — y con esa informacion puede volver a descargar el mod correcto desde el workshop en vez de quedar sin diagnostico.

## Catalogo, fuente y distribucion

La identidad de ejecucion siempre es `id + version + hash`. La metadata de fuente/catalogo es opcional y no forma parte de la identidad.

```json
{
  "source": {
    "provider": "officialWorkshop",
    "catalogId": "abc123",
    "url": "https://..."
  }
}
```

Reglas:

- `source` ayuda a encontrar o reinstalar contenido, y habilita el filtro `requireVerifiedContent` en servidores publicos.
- Un paquete instalado desde archivo local es valido si el hash coincide.
- El save puede guardar `source` como ayuda de recuperacion, pero no depender de eso.
- Siempre se valida hash.

### Sin firma criptografica de autor

No se agrega firma criptografica propia sobre paquetes publicados. La cuenta autenticada del backend externo (requerida para publicar, segun Idea.md) ya resuelve la procedencia; el hash sigue siendo la unica garantia de integridad de contenido. Se considero desproporcionado agregar gestion de claves/PKI sin una amenaza concreta que lo justifique — la superficie de riesgo real del sistema es cliente-vs-host, no autenticidad de autor.

### Formatos de distribucion

- Carpeta suelta: para modders durante desarrollo local.
- Paquete comprimido/inmutable: para workshop/catalogo y distribucion estable. Es un **ZIP estandar renombrado**, con la misma estructura interna que la carpeta de desarrollo, mas un archivo con los hashes precalculados por recurso.

Extensiones: `.psmod` (mods), `.psstory` (historias), `.psprofile` (perfiles).

El runtime de partidas debe usar contenido instalado/snapshot, no carpetas dev editables.

### Multi-version e instalacion desde catalogo

El juego permite tener instaladas varias versiones del mismo mod en paralelo (ver seccion de saves/almacenamiento). La UI muestra por default la version mas reciente, pero puede instalar/conservar versiones especificas segun lo que exijan saves, servidores o perfiles.

## Mod Validator

Debe existir un validador oficial de mods.

Responsabilidades:

- validar manifiestos
- validar JSON Schemas
- resolver dependencias
- calcular hashes
- validar indice central
- validar IDs/namespaces
- validar assets registrados
- validar scripts contra metadata y limites
- detectar y resolver conflictos segun orden de carga
- construir `ResolvedModpack`
- generar reportes de errores (incluyendo el reporte exportable de modpack)
- opcionalmente empaquetar para publicacion

El juego, el catalogo/workshop y los modders deben usar la misma logica de validacion.

### Alcance de la validacion

El Mod Validator inicial se limita a **validacion estatica** (forma, dependencias, hashes, limites declarados) mas construccion del `ResolvedModpack`. No incluye ejecucion simulada de scripts con datos sinteticos para detectar errores de runtime antes de publicar — queda fuera de scope inicial, como posibilidad futura.

Esto es intencional: el sistema no busca impedir que un jugador use mods que alteren el balance o habiliten ventajas en su propia partida single-player u host-authoritative confiable — esa es una libertad explicita del modding. La proteccion de integridad esta reservada para el caso de sesiones compartidas con desconocidos, donde el operador de un servidor dedicado decide que mods acepta y puede rechazar conexiones que no cumplan sus contratos (ver `requireVerifiedContent` y modpack estricto).

### Politica de warnings vs. errores

Criterio general:

- **Error**: todo lo que compromete determinismo, integridad de identidad (hash/version/id) o consistencia cliente-host. Bloquea la resolucion del `ResolvedModpack` / creacion de partida / entrada a multiplayer.
- **Warning**: todo lo demas (patch perdedor no-`required`, tag desconocido, metadata de catalogo faltante, recurso `public` sin declarar modificabilidad). Nunca bloquea; se loguea y opcionalmente se muestra antes de confirmar.

Las validaciones deben producir codigos de error estables, por ejemplo:

- `MOD_DEPENDENCY_MISSING`
- `MOD_DEPENDENCY_CYCLE`
- `MOD_VERSION_CONFLICT`
- `RESOURCE_SCHEMA_INVALID`
- `PACKAGE_HASH_MISMATCH`
- `SCRIPT_LIMIT_EXCEEDED`
- `MANIFEST_VERSION_INVALID`

## Logs y diagnostico

El sistema debe tener diagnostico separado para jugador y modder/dev.

Jugador: mensajes claros de incompatibilidad, dependencia faltante, hash distinto, recurso invalido, save requiere version especifica, script fallido/desactivado, mod causante de rechazo por `requireVerifiedContent`.

Modder/dev: paquete, recurso, archivo, linea/columna si aplica, schema violado, extension point, runtime, stack trace sandbox, comandos rechazados, limites excedidos.

## Documentacion para modders

La documentacion publica de la Modding API se **genera a partir de los JSON Schemas y metadata de extension points** (los mismos artefactos que ya son fuente de verdad para validacion y patchability), mas comentarios manuales agregados donde el autor del sistema lo considere necesario. Cada punto expuesto del juego compone su propia libreria de documentacion; cada sistema (items, NPCs, mapa, etc.) tiene su propio set de "endpoints" documentados, porque cada uno aplica a un modulo distinto del juego.

Motivo: evita que la documentacion se desincronice del contrato real (mismo problema que ya se evito separando patchability del schema), y por diseno un JSON Schema documenta solo el contrato publico, nunca el codigo interno del motor.

## Decisiones cerradas

- Modding `data-first`. Scripts solo para comportamientos concretos.
- Historias, mods y perfiles son sistemas separados con validacion compartida (`ContentPackage`).
- Manifiesto obligatorio, dividido en bloque tecnico (hasheado) y `meta` opcional (no hasheado).
- `story.json` = `mod.json` + `entryPoint` obligatorio.
- JSON-only como formato runtime/canonico.
- Semver obligatorio y validado en `version`, `gameVersion`, `moddingApi`.
- Indice central obligatorio, lista plana sin categorizar por tipo.
- Hash por arbol de archivos individuales, no monolitico.
- Namespace obligatorio. `core` reservado para vanilla.
- API de modding versionada aparte del juego.
- El juego consume `ResolvedModpack` (serializado y persistido con el save), no mods crudos.
- Reporte de modpack como artefacto separado, legible para humanos.
- Solo dependencias obligatorias. Resolucion por orden topologico + desempate alfabetico.
- Ciclos y conflictos de version son error duro, sin resolucion automatica.
- Rangos de version en manifiesto, exact match en partida/save/multiplayer.
- Sin migraciones de saves en el estandar inicial. Snapshots locales por save.
- Multi-version instalada en paralelo, direccionable por id+version+hash.
- Diseno de almacenamiento content-addressed/deduplicable por archivo.
- Orden de carga editable con guardrails. Top = mayor prioridad. Primero que llega gana.
- Patches con JSON Patch RFC 6902 estandar, sin extensiones propias.
- Colecciones modeables como maps por ID, no por indice.
- Recursos con `public/private`. Recursos publicos con `patchable/overridable` declarado en el propio JSON Schema (`x-modding`).
- Tags: vocabulario global core + tags libres namespaced.
- Localizacion por mod, archivos propios namespaced, fallback a ingles.
- Assets solo por ID declarado. Metadata obligatoria por asset.
- Prefabs/escenas/shaders Unity fuera de scope inicial.
- Lua y JavaScript con la misma API, runtimes 100% C# puro (MoonSharp, Jint) por portabilidad Android/servidor y sandboxing simple.
- Sin C# arbitrario para mods.
- Sandbox uniforme fijo por extension point, sin permisos declarativos por mod.
- Scripts `server` y `clientPresentation`. Sin `sharedPrediction` inicial.
- Scripts autoritativos con contexto determinista y comandos validados.
- Lifecycles por extension point. Limites obligatorios, diferenciados por plataforma (PC/host vs. Android).
- Errores runtime con politica `fail-contained`.
- Mod Validator: solo validacion estatica, sin ejecucion simulada. Libertad del jugador en su propia partida es una decision de producto.
- Warning vs. error segun si compromete determinismo/identidad/consistencia.
- Host nunca distribuye mods automaticamente. Mods locales permitidos en multiplayer con exact match.
- Servidores dedicados pueden exigir `requireVerifiedContent`, con mensaje explicito de que mod causo el rechazo.
- Metadata de fuente/catalogo opcional. Sin firma criptografica de autor.
- Carpeta suelta para desarrollo, paquete comprimido (ZIP renombrado) para distribucion: `.psmod`, `.psstory`, `.psprofile`.
- Mod-Manager con perfiles: historia + mods pineados + configuracion, publicables en workshop como `ProfilePackage`.
- Mods nunca sobreescriben configuracion vanilla, solo agregan. Los perfiles fijan configuracion.
- Documentacion generada desde JSON Schema + metadata + comentarios manuales.
- Que resource types entran al MVP y el primer set de extension points por sistema **no se deciden aca**: son decision de cada crunching especifico.

## Pendientes para continuar

- Como se muestra el gestor de almacenamiento en UI (decision de UI/UX, no de arquitectura).
- Mecanismo de resolucion de override entre configuraciones agregadas por distintos mods (pertenece al crunching del sistema de configuracion).
- Que resource types entran realmente en el primer MVP de cada sistema (items, misiones, NPCs, mapa, comunidades, habilidades, vehiculos, etc.) — a resolver en el crunching especifico de cada uno.
- Primer set de extension points concreto por sistema.
- Modelo de permisos/capabilities especifico si en el futuro se habilita el `Unity Modding Package` (prefabs/escenas/shaders).

## Resultado esperado de este documento

Este documento debe funcionar como referencia viva de la arquitectura de modding para:

- evitar que cada sistema especifico (items, NPCs, mapa, misiones, comunidades, habilidades) redefina desde cero conceptos como manifiesto, hash, dependencias, patch/override, visibilidad o sandboxing de scripts.
- dar a cada crunching de sistema especifico un contrato base sobre el cual declarar sus propios extension points.
- mantener la coherencia entre modding, historias, perfiles, saves y multiplayer definida en [Idea.md](./Idea.md).
