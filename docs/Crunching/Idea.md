# Crunching de Idea.md

## Alcance de este documento

Este documento resume y aclara las decisiones de producto derivadas exclusivamente de [../Definiciones/Idea.md](../Definiciones/Idea.md). No profundiza en el diseño interno de los módulos específicos definidos en otros documentos como personajes, misiones, mapa, NPCs o comunidades.

Su objetivo es dejar cerradas las definiciones de alto nivel que condicionan la arquitectura, el flujo de juego y el alcance inicial del proyecto.

## Identidad general del proyecto

- `Project Survivors` es un videojuego de supervivencia en una isla afectada por un virus de origen vegetal que transforma a las personas en zombies.
- El jugador controla a un superviviente inmune.
- El juego combina supervivencia, recolección de recursos, construcción, comunidad, exploración, historia y progresión.
- La finalidad narrativa principal vanilla es reparar un barco para escapar de la isla.
- Ese objetivo narrativo no obliga al jugador a abandonar la partida ni invalida el juego emergente o sandbox.

## Stack y dirección técnica general

- Motor: `Unity 6 LTS`
- Input: `Unity New Input System`
- Runtime .NET: versión compatible con Unity
- Networking: no se define todavía una librería o framework específico

### Criterio de arquitectura de red

- El juego debe diseñarse con multiplayer en mente desde el inicio.
- El modelo de red definido es `host-authoritative` con `arquitectura híbrida`.
- No se adopta un modelo `server-authoritative` puro como base.
- La arquitectura debe permitir evolucionar hacia `servidor dedicado` sin rehacer los sistemas centrales.

### Implicancias de la arquitectura híbrida

- El cliente puede mantener respuesta inmediata para input y presentación.
- El `host/server` valida la lógica con impacto compartido o competitivo.
- La lógica crítica no debe depender del cliente como fuente final de verdad.

## Modalidad de juego y plataformas

### Single-player

- El juego debe soportar `single-player` en `PC` y `Android` desde la etapa inicial del producto.

### Multiplayer

- El juego debe soportar `multiplayer` cooperativo y con posibilidad de `PvP de supervivencia`.
- El `multiplayer inicial` se limita a `PC`.
- `Android multiplayer` queda previsto a futuro, pero no forma parte del alcance inicial.

### Tamaño de sesión

- El objetivo inicial de validación es `8 jugadores por sesión`.
- Ese valor no debe tratarse como un límite arquitectónico rígido.
- El límite debe ser configurable y ampliable a futuro.
- Los sistemas no deben asumir internamente un máximo fijo de 4 u 8 jugadores si eso puede evitarse.

## Servidor dedicado

- El producto debe contemplar `servidor dedicado` desde el diseño.
- El servidor dedicado no es necesariamente prioridad del primer entregable.
- Debe existir compatibilidad arquitectónica para una build separada de servidor sin cliente jugable.
- Esa build de servidor puede omitir render y otros componentes no necesarios para hostear.

## Relación entre historia y sandbox

- No existen `modos separados` de juego tipo `Sandbox` y `Historia`.
- Existe una sola clase de partida.
- Toda partida comienza con la selección de una `historia`.
- Luego, el jugador puede:
  - seguir la progresión narrativa y sus misiones
  - ignorarla parcial o totalmente
  - alternar entre juego emergente y avance narrativo cuando quiera

### Regla de base

- `Sandbox` y `Historia` son dos formas de jugar una misma partida.
- No deben modelarse como productos paralelos ni como flujos de inicio distintos.

## Historias

- Toda partida requiere una `historia` seleccionada al momento de su creación.
- La historia se define como un recurso declarativo, basado en `JSON`.
- La historia no puede incluir scripts propios como mecanismo principal.
- La historia puede ser autosuficiente o depender de mods.

### Qué representa una historia

La historia actúa como manifiesto y marco narrativo de la partida. Puede definir o referenciar:

- contenido narrativo
- misiones
- diálogos
- condiciones de progresión
- dependencias de mods, si fueran necesarias

### Dependencias de mods en historias

- Una historia puede no depender de ningún mod.
- Si una historia declara dependencias, esas dependencias son obligatorias.
- Si falta un mod requerido por la historia, la partida no puede iniciarse.
- Cada partida tiene una sola historia activa desde su creación.
- No se contempla una misma partida con múltiples historias simultáneas activas.

## Flujo de creación de partida

El flujo base definido para crear una partida es:

1. El jugador selecciona una historia.
2. El juego valida si la historia requiere mods.
3. Si hay dependencias faltantes, se informa al jugador antes de iniciar.
4. Una vez resueltas las dependencias, se genera el mundo.
5. Se inicia la partida.

### Regla de precedencia

- Primero se resuelve `historia + dependencias`.
- Después se ejecuta la `generación del mundo`.
- Esto es obligatorio porque algunos mods pueden afectar la generación del mapa.

## Generación del mapa

- El mapa es procedural/aleatorio.
- La generación del mapa puede verse afectada por mods activos.
- La generación efectiva del mundo depende del conjunto final de contenido cargado, no solo de una semilla.

## Modding

El modding es una característica central del producto y debe estar disponible tanto en single-player como en multiplayer.

### Principios base del sistema de mods

- Los mods pueden alterar comportamiento, datos y contenido.
- Los mods pueden incluir assets como sprites.
- Los mods deben poder participar en partidas multiplayer.
- Los mods relevantes deben existir tanto en host/server como en cliente cuando la partida los requiera.
- La lógica modeada se carga al iniciar la partida.
- No se contempla modificación efectiva de archivos de mod durante una partida ya iniciada.

### Política de integridad

- El sistema usa `modpack estricto`.
- Cliente y host/server deben coincidir exactamente en el conjunto requerido de mods.
- La validación debe contemplar identidad exacta del modpack.
- Si el contenido no coincide, el ingreso a la sesión debe rechazarse.

### Validación de mods

La recomendación aceptada para la validación es:

- `id`
- `version`
- `hash`

El `hash` debe representar el paquete de contenido relevante del mod, incluyendo scripts, datos y assets que formen parte de su identidad.

### Implicancia práctica

- Si un cliente modifica un archivo de un mod localmente y ese cambio altera el hash, no puede entrar a la partida.
- Esto protege la consistencia entre cliente y host/server.
- También evita que un cliente altere localmente lógica modeada para obtener ventaja sin ser detectado por la validación de ingreso.

### Límite explícito de esa protección

- Esta política no busca impedir que el `host` manipule deliberadamente su propia sesión.
- En un modelo `host-authoritative`, el host/server es la autoridad confiable de la partida.
- La protección está orientada a integridad de sesión y consistencia entre participantes, especialmente frente a clientes no confiables.

### Ejecución de lógica modeada

- La lógica que afecta gameplay compartido o competitivo debe ejecutarse y/o validarse del lado `host/server`.
- Esto aplica especialmente a partidas multiplayer y a sesiones con PvP.
- El cliente no debe decidir por sí solo el resultado final de lógica relevante para el estado compartido.

## Reglas de autoridad en multiplayer y PvP

Aunque la arquitectura general sea híbrida, el `host/server` debe tener autoridad final sobre:

- combate
- inventario
- uso de items
- uso de habilidades
- construcción
- estado y uso de vehículos

### Regla de diseño

- El cliente puede predecir o representar.
- El host/server valida y confirma el resultado efectivo.

## Guardado y persistencia

- Una partida guardada debe quedar atada a la `historia + versión + modpack exacto` con el que fue creada.
- El save no debe tratar la historia o el modpack como contenido implícito mutable.
- Si el conjunto requerido ya no existe o no coincide, la partida no debe abrirse.

### Objetivo de esta regla

- evitar corrupción silenciosa
- evitar cargar partidas en contextos incompatibles
- preservar reproducibilidad del estado jugable

## Catálogo, distribución y cuentas

El juego debe integrar un ecosistema de distribución de historias y mods, pero sin asumir que el backend de esa plataforma forma parte del proyecto a implementar.

### Alcance del juego

El juego debe incluir interfaz para:

- explorar contenido
- descargar contenido
- publicar contenido
- actualizar contenido

### Backend externo

- El sistema de cuentas, publicación y descarga se apoya en un `servicio externo`.
- El juego solo consume ese servicio mediante interfaz y requests HTTP hacia una API.
- La implementación de ese backend no forma parte del alcance de este proyecto.

### Cuentas

- Las cuentas de usuario existen como concepto del producto.
- Publicar y actualizar contenido requiere cuenta.
- Descargar y jugar no necesariamente requiere autenticación, siempre que el caso de uso lo permita.

## Conectividad y modo offline

- El juego debe ser `offline-capable` en `single-player`.
- Una partida offline puede iniciarse si la historia y todos los mods requeridos ya están instalados localmente.
- `multiplayer`, `catálogo`, `publicación`, `descarga` y `cuentas` requieren conexión.

## Objetivo narrativo principal

- La campaña vanilla tiene como objetivo principal reparar un barco para escapar.
- Ese evento representa un `final narrativo opcional`.
- Completar ese final no obliga a terminar o cerrar el mundo.
- El jugador puede seguir jugando la misma partida después de alcanzarlo.

## Decisiones cerradas en este crunching

- El juego se desarrolla en `Unity`.
- La red se diseña desde el inicio, aunque el entregable inicial no abarque todas las variantes de multiplayer.
- El modelo base es `host-authoritative` con enfoque `híbrido`.
- `Servidor dedicado` está previsto desde el diseño.
- `Single-player` inicial: `PC + Android`.
- `Multiplayer` inicial: `PC`.
- `Android multiplayer`: futuro.
- Las partidas siempre nacen desde una `historia`.
- No hay modo sandbox separado.
- La historia puede depender de mods.
- La creación de mundo ocurre solo después de resolver dependencias.
- Los mods forman parte del contrato de partida y del contrato de red.
- El save depende de la identidad exacta de historia y modpack.
- El catálogo y cuentas existen en el producto, pero su backend es externo al alcance del proyecto.
- El juego debe soportar operación offline parcial en single-player.

## Riesgos y límites explícitos

### Riesgos de alcance

- `PC + Android + single-player + multiplayer + servidor dedicado + modding + catálogo + publicación integrada` es un alcance amplio incluso con decisiones correctas de arquitectura.
- El hecho de prever ciertas capacidades desde el diseño no implica que deban entrar todas en el primer entregable.

### Límite de autoridad

- En `host-authoritative`, el host es confiable para la sesión.
- El sistema no apunta a anti-cheat fuerte contra el host.
- Sí apunta a bloquear divergencia o manipulación del lado cliente en sesiones multiplayer.

### Riesgo de suposiciones fijas

- El objetivo inicial de `8 jugadores` no debe filtrarse como constante dura en los sistemas.
- Las dependencias entre historia, mods y generación de mapa deben mantenerse explícitas para no romper saves ni consistencia de red.

### Límite de este documento

- Este crunching no define en detalle:
  - progresión del personaje ni sistema de habilidades — ver [CaracteristicasPersonaje.md](./CaracteristicasPersonaje.md)
  - estructura de misiones
  - algoritmo de generación de mapa
  - comportamiento de NPCs — ver [NPCs.md](./NPCs.md)
  - gestión de comunidades
  - diseño de construcción
  - reglas finas de PvP

Esos temas corresponden a crunchings separados sobre sus documentos específicos.

## Resultado esperado de este documento

Este documento debe funcionar como referencia de producto y alcance para:

- evitar contradicciones al diseñar módulos específicos
- alinear arquitectura base con necesidades futuras reales
- dejar claros los límites del MVP sin perder visión de largo plazo
- separar con claridad qué pertenece al juego y qué depende de servicios externos
