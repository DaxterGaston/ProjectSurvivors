# Crunching de Modding BETA

> **Nota:** este documento fue revisado y consolidado en [Modding.md](./Modding.md), que es la referencia activa. Se conserva aca por trazabilidad historica.

## Alcance de este documento

Este documento resume las decisiones iniciales sobre la arquitectura de modding de `Project Survivors`.

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

## Historias y mods

Las historias y los mods son sistemas separados, pero comparten infraestructura de validacion, instalacion, catalogo y hashing.

- Una partida requiere exactamente una historia activa.
- Una historia puede depender de mods.
- Una historia no es simplemente otro mod.
- `StoryPackage` y `ModPackage` son tipos distintos de `ContentPackage`.
- Ambos usan manifiestos, version, hash, dependencias y validacion.

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

El manifiesto identifica el paquete y declara compatibilidad, dependencias, metadata y entrypoints.

Campos conceptuales:

```json
{
  "id": "author.mod_name",
  "name": "Mod Name",
  "version": "1.0.0",
  "gameVersion": "0.1.x",
  "moddingApi": "1.0",
  "dependencies": []
}
```

## Formato de archivos declarativos

El formato runtime/canonico para definiciones de mods e historias es `JSON`.

- `mod.json`, `story.json`, indices y recursos declarativos son JSON.
- Se deben definir JSON Schemas por tipo de recurso y version de API.
- El runtime consume JSON validado.
- Otros formatos pueden existir como herramientas de autoria externas, pero deben exportar JSON para el runtime.

## Indice central de recursos

Cada paquete debe declarar explicitamente sus recursos en un indice central.

Ejemplo:

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

Reglas:

- El runtime solo carga recursos listados.
- Un archivo no listado no existe para el juego.
- El hash del paquete se calcula desde el manifiesto, indice y recursos listados.
- El orden debe normalizarse o validarse para hashes estables.
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
- El nombre visible al jugador no es el ID.
- Las referencias entre paquetes usan IDs completos.

## Modding API

La API de modding se versiona de forma independiente a la version del juego.

- `gameVersion` expresa compatibilidad general con builds del juego.
- `moddingApi` expresa compatibilidad con schemas, contextos de scripting, lifecycle y extension points.
- Cambios breaking suben major.
- Cambios aditivos suben minor.
- Un mod declara contra que API fue escrito.
- La validacion falla si la API requerida no esta soportada.

## ResolvedModpack

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

El `ResolvedModpack` tiene un hash final y queda congelado para la partida.

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

Los manifiestos pueden usar rangos de version para resolver compatibilidad antes de crear partida.

Ejemplo:

```json
{
  "dependencies": [
    { "id": "author.weapon_assets", "version": ">=1.2.0 <2.0.0" }
  ]
}
```

Una vez resuelto, el `ResolvedModpack`, los saves y multiplayer usan version exacta y hash exacto.

## Saves, snapshots y almacenamiento

Los saves usan exact match siempre.

- Un save solo abre con la misma historia, versiones y hashes exactos.
- No hay migraciones de saves en el estandar inicial.
- Las migraciones explicitas quedan documentadas como posibilidad muy futura, fuera de scope actual.

Cada save debe quedar atado a un snapshot local del contenido usado.

El diseno logico debe ser direccionable por contenido y deduplicable por hash, aunque una implementacion MVP pueda copiar carpetas completas.

Reglas de almacenamiento:

- El juego debe mostrar espacio ocupado por saves, snapshots, mods instalados, cache de descarga y versiones antiguas.
- El jugador puede borrar saves completos.
- El jugador puede borrar mods instalados que no esten siendo usados por saves.
- El jugador puede borrar versiones antiguas no referenciadas.
- El jugador puede borrar cache de descarga.
- El juego no debe borrar automaticamente contenido requerido por un save sin confirmacion explicita.
- Si se usa deduplicacion, borrar un save solo libera blobs no referenciados por otros saves.
- En Android la gestion debe ser mas visible por limitaciones de almacenamiento, sin romper saves sin confirmacion.

## Orden de carga y prioridad

El `ResolvedModpack` tiene orden final explicito.

El orden de carga es editable por el jugador con guardrails.

Reglas:

- Top = mayor prioridad.
- Lo que carga primero gana conflictos.
- Las dependencias siempre cargan antes que los dependientes.
- La UI bloquea ordenes que violen dependencias.
- El usuario puede reordenar mods independientes.
- Al crear partida, el orden final queda congelado en el save.

## Overrides y patches

Por defecto, un mod agrega contenido.

Para modificar contenido existente debe declarar una accion explicita:

- `override`: reemplazo total de un recurso.
- `patch`: modificacion parcial declarativa.

El comportamiento vanilla tambien puede ser modificado mediante estos mecanismos.

Los patches usan JSON Patch RFC 6902 restringido.

Operaciones iniciales:

- `add`
- `remove`
- `replace`
- `test`

No se incluyen inicialmente:

- `move`
- `copy`

Reglas de conflicto:

- Primero que llega gana segun orden de carga.
- El primer cambio sobre un campo gana.
- Un `override` total equivale a decidir todos los campos patchables del recurso.
- Un `patch` decide solo los campos que toca.
- Si un patch/override posterior pierde, se reporta warning.
- Si la accion estaba marcada como `required`, perder genera error de validacion.

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

Colecciones extensibles:

- Deben preferir maps por ID.
- Los patches sobre arrays por indice solo se permiten donde el schema declare orden posicional real.
- Las colecciones logicas modeables no deben depender de indices fragiles.

## Visibilidad y modificabilidad de recursos

Todo recurso puede declarar visibilidad:

- `private`
- `public`

Default recomendado: `private`.

Reglas:

- `public`: otros mods pueden referenciarlo si declaran dependencia directa.
- `private`: solo recursos del mismo paquete pueden referenciarlo.
- Si un mod referencia un recurso privado de otro paquete, falla validacion.

Ademas, un recurso publico debe poder declarar modificabilidad:

```json
{
  "modification": {
    "patchable": true,
    "overridable": false
  }
}
```

Reglas:

- `public + patchable`: otros mods pueden parchear campos permitidos.
- `public + overridable`: otros mods pueden reemplazar la definicion completa.
- `public` sin patch/override: otros mods solo pueden referenciar.
- El schema puede marcar campos no patchables aunque el recurso sea patchable.

## Taxonomia de puntos de extension

El estandar usa categorias comunes para evitar que cada sistema redefina que significa ser modeable.

Categorias:

- `Definitions`: recursos declarativos.
- `Tables`: listas ponderadas o configurables.
- `Rules`: formulas o reglas declarativas.
- `Behaviors`: logica ejecutable registrada en puntos seguros.
- `Assets`: sprites, audio, musica, localizacion y otros recursos permitidos.

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

Los schemas finos de cada tipo se definen en los crunchings especificos de cada sistema.

## Metadata comun de recursos

Todo recurso declarativo debe tener metadata comun minima.

Obligatorios:

- `id`
- `type`
- `apiVersion`
- `payload`

Opcionales:

- `display`
- `tags`
- `visibility`
- metadata de tooling

Ejemplo:

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

## Assets

Los assets se referencian solo por ID declarado.

No se permiten referencias directas por path desde recursos de gameplay.

Cada asset usable por runtime debe tener metadata declarativa.

Ejemplo:

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
- El hash incluye metadata y binario.
- Assets no declarados no forman parte del contrato publico.
- Otros mods pueden referenciar assets publicos de otro mod solo con dependencia directa.

### Assets fuera de scope inicial

Quedan fuera del estandar inicial:

- prefabs Unity
- escenas Unity
- shaders propios
- assets que requieran compilacion/importacion Unity compleja
- AssetBundles o packages generados desde una extension de editor

Esto no es una prohibicion permanente.

A futuro se preve un `Unity Modding Package` o extension de editor para permitir ese tipo de contenido de forma controlada. Los tipos/capabilities relacionados quedan reservados, pero son invalidos en la Modding API inicial.

## Scripting

Los lenguajes previstos son Lua y JavaScript.

La API publica debe ser la misma para ambos lenguajes. El contrato real es `Mod Scripting API`, con adaptadores por runtime.

Reglas:

- No se expone C# arbitrario como estandar de modding.
- Los scripts se ejecutan en sandbox.
- No acceden directamente a Unity ni a singletons internos.
- No pueden leer/escribir archivos arbitrarios.
- No pueden abrir red arbitraria.
- No modifican memoria/codigo del juego.
- Solo interactuan mediante contextos y comandos permitidos.

### Autoridad de scripts

El estandar inicial separa scripts en:

- `server`
- `clientPresentation`

No existe `sharedPrediction` en el estandar inicial.

Reglas:

- Scripts `server` afectan gameplay y se ejecutan/validan en host/server.
- Scripts `clientPresentation` solo afectan presentacion.
- El cliente no decide resultados finales de gameplay.
- La prediccion compartida queda fuera de scope inicial.

### Determinismo

Los scripts autoritativos usan APIs deterministas provistas por el contexto.

Reglas:

- Generacion de mapa: determinismo obligatorio con seed/contexto.
- Loot/rewards: deterministas o generados por host con resultado persistido.
- NPC orders/tasks: pueden no ser perfectamente deterministas, pero sus efectos finales deben persistirse/sincronizarse.
- Random debe venir de `ctx.random`.
- No se permite random global, tiempo real, filesystem o estado externo.

### Modelo de interaccion con estado

Los scripts no mutan estado interno directamente.

Modelo:

- lectura acotada desde contexto
- retorno de comandos/eventos validados
- aplicacion por el motor

No se permiten setters directos sobre entidades internas.

Los comandos forman parte de la API versionada.

### Lifecycles

Los lifecycles se definen por tipo de extension, no como lifecycle universal.

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

### Limites

Los scripts tienen limites obligatorios definidos por el juego por extension point/runtime.

Ejemplos:

- tiempo maximo por llamada
- memoria maxima aproximada
- cantidad maxima de comandos devueltos
- tamano/profundidad de datos retornados
- limite de logs
- limite de eventos emitidos

El mod puede pedir limites menores, pero no elevar los maximos del juego.

### Errores en runtime

Politica general: `fail-contained`.

Reglas:

- Un error de script no debe crashear el juego.
- Scripts de presentacion fallidos se desactivan con fallback.
- NPC orders/tasks fallidas se cancelan y el NPC vuelve a fallback/idle.
- Efectos de habilidad/item/combat fallidos rechazan la accion sin aplicar cambios parciales.
- Errores de generacion de mapa fallan la creacion del mundo.
- Gameplay autoritativo debe ser transaccional: todo o nada.
- Logs deben incluir paquete, recurso, extension point y causa.

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

Los mods locales no publicados pueden usarse en multiplayer si todos los participantes tienen el mismo `id + version + hash`.

El juego debe indicar cuando el contenido no esta verificado por catalogo.

## Catalogo, fuente y distribucion

La identidad de ejecucion siempre es `id + version + hash`.

La metadata de fuente/catalogo es opcional y no forma parte de la identidad.

Ejemplo:

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

- `source` ayuda a encontrar o reinstalar contenido.
- Un paquete instalado desde archivo local es valido si el hash coincide.
- El save puede guardar `source` como ayuda de recuperacion, pero no depender de eso.
- Siempre se valida hash.

### Formatos de distribucion

Se soportan dos formatos con roles distintos:

- carpeta suelta para desarrollo
- paquete comprimido/inmutable para distribucion

La carpeta suelta es para modders durante desarrollo local.

El paquete comprimido es para workshop/catalogo y distribucion estable.

Extensiones conceptuales:

- `.psmod` para mods
- `.psstory` para historias

El runtime de partidas debe usar contenido instalado/snapshot, no carpetas dev editables.

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
- generar reportes de errores
- opcionalmente empaquetar para publicacion

El juego, el catalogo/workshop y los modders deben usar la misma logica de validacion.

## Logs y diagnostico

El sistema debe tener diagnostico separado para jugador y modder/dev.

Jugador:

- mensajes claros de incompatibilidad
- dependencia faltante
- hash distinto
- recurso invalido
- save requiere version especifica
- script fallido/desactivado

Modder/dev:

- paquete
- recurso
- archivo
- linea/columna si aplica
- schema violado
- extension point
- runtime
- stack trace sandbox
- comandos rechazados
- limites excedidos

Las validaciones deben producir codigos de error estables, por ejemplo:

- `MOD_DEPENDENCY_MISSING`
- `RESOURCE_SCHEMA_INVALID`
- `PACKAGE_HASH_MISMATCH`
- `SCRIPT_LIMIT_EXCEEDED`

## Decisiones cerradas

- Modding `data-first`.
- Scripts solo para comportamientos concretos.
- Historias y mods son sistemas separados con validacion compartida.
- Manifiesto obligatorio.
- JSON-only como formato runtime/canonico.
- Indice central obligatorio.
- Namespace obligatorio.
- `core` reservado para vanilla.
- API de modding versionada aparte del juego.
- El juego consume `ResolvedModpack`, no mods crudos.
- Solo dependencias obligatorias.
- Rangos de version en manifiesto, exact match en partida/save/multiplayer.
- Sin migraciones de saves en el estandar inicial.
- Snapshots locales por save.
- Diseno de almacenamiento content-addressed/deduplicable.
- Orden de carga editable con guardrails.
- Top = mayor prioridad.
- Primero que llega gana.
- Patches con JSON Patch restringido.
- Patches fragiles por indice evitados salvo colecciones posicionales reales.
- Recursos con `public/private`.
- Recursos publicos con `patchable/overridable`.
- Assets solo por ID declarado.
- Metadata obligatoria por asset.
- Prefabs/escenas/shaders Unity fuera de scope inicial.
- Lua y JavaScript con la misma API.
- Sin C# arbitrario para mods.
- Scripts `server` y `clientPresentation`.
- Sin `sharedPrediction` inicial.
- Scripts autoritativos con contexto determinista y comandos validados.
- Lifecycles por extension point.
- Limites obligatorios de scripts.
- Errores runtime con politica `fail-contained`.
- Host nunca distribuye mods automaticamente.
- Mods locales permitidos en multiplayer con exact match.
- Metadata de fuente/catalogo opcional.
- Carpeta suelta para desarrollo, paquete comprimido para distribucion.
- Mod Validator oficial.
- Logs/diagnostico con codigos de error estables.

## Pendientes para continuar

Temas que conviene seguir grillando:

- Estructura exacta de `mod.json`, `story.json` y `resources.json`.
- Forma canonica de calcular hashes.
- Algoritmo exacto de resolucion de dependencias y orden de carga.
- Representacion concreta de `ResolvedModpack`.
- Politica de versionado semantico obligatoria o recomendada.
- Campos minimos de metadata de autor, licencia, tags y descripcion de catalogo.
- Modelo de localizacion para textos de mods.
- Vocabulario de tags globales y tags namespaced.
- Definicion de `visibility`, `patchable`, `overridable` en schemas.
- Sintaxis final para patches sobre maps/colecciones por ID.
- Politica de warnings vs errores del validador.
- Formato del reporte exportable de modpack.
- Packaging exacto de `.psmod` y `.psstory`.
- Firma o verificacion adicional de paquetes publicados, si aplica.
- Como se instala una version especifica desde catalogo/workshop.
- Como se muestra el gestor de almacenamiento en UI.
- Reglas para contenido local no verificado en servidores publicos.
- Que resource types entran realmente en el primer MVP.
- Primer set de extension points por sistema: items, NPCs, misiones, mapa, comunidades, habilidades.
- Diseno concreto de `Mod Scripting API`.
- Eleccion tecnica de runtimes Lua y JS en Unity.
- Modelo de permisos/capabilities para scripts.
- Politica de performance para scripts en Android.
- Como testear mods de forma automatizada con el Mod Validator.
- Como documentar contratos para modders sin exponer internals del juego.
