# Crunching de NPCs

## Alcance de este documento

Este documento resume y aclara las decisiones de diseño derivadas exclusivamente de [../Definiciones/NPCs.md](../Definiciones/NPCs.md).

No profundiza en el diseño interno de otros sistemas mencionados desde acá (Comunidades, generación de mapa/spawn, sistema de managers, virus/mutaciones, Salud). Cuando el documento depende de uno de esos sistemas, se deja asentado como frontera explícita, no como decisión resuelta.

Este documento se apoya en las decisiones generales ya cerradas en [Idea.md](./Idea.md), en el estándar de modding cerrado en [Modding.md](./Modding.md), y en el sistema de personaje ya resuelto en [CaracteristicasPersonaje.md](./CaracteristicasPersonaje.md), del cual los NPCs reutilizan parte del modelo de datos. La parte específicamente moddeable de este sistema se resuelve en el documento complementario [NPCs_Modding.md](./NPCs_Modding.md), siguiendo la regla de división de [CLAUDE.md](./CLAUDE.md).

## Principio de valores tentativos

Igual que en `CaracteristicasPersonaje.md`, todo valor numérico mencionado en este documento (mapeos de ánimo, probabilidades, multiplicadores) es **tentativo**, sujeto a balanceo mediante playtesting. Debe vivir en `ScriptableObjects`, nunca hardcodeado.

## Relación con el sistema de Personaje

Los NPCs comparten parte del modelo de datos de Atributos, Capacidades, Rasgos y Habilidades del personaje jugable, con diferencias clave:

- **Atributos**: comparten Vida, Resistencia, Hambre y Sed (modelo `current/max` u rango fijo, según corresponda, igual que el personaje). **No tienen UMS ni acceso a Habilidades de Mutación** — a diferencia del jugador, un NPC de comunidad no es un superviviente inmune conocido. Un futuro sistema de virus/infección para NPCs (ej. NPCs que se convierten en zombies) es un tema aparte, no una variante de este sistema.
- **Capacidades**: comparten el mismo modelo de datos (Combate, Puntería, Elaboración, Construcción), pero **sin progresión activa por uso**. El nivel de cada Capacidad se fija en el momento de spawn según lo que declare su `npcArchetypeDefinition` (valor fijo o sorteado dentro de un rango). Esto evita tener que trackear y sincronizar acumuladores de experiencia por cada NPC activo en cada tick, coherente con la nota de presupuesto de performance de `Modding.md`. Una posible progresión de Capacidades vía interacción con construcciones del jugador (ej. una galería de tiro) o vía tareas específicas (ej. un guardia que acumula experiencia con el tiempo) queda como **frontera hacia el futuro crunching de Comunidades**, ya que depende de ese sistema.
- **Habilidades**: un NPC puede tener Habilidades Humanas y Pasivas (vía Rasgo o arquetipo). **No tiene Habilidades de Mutación**, consistente con no tener UMS.
- **Rasgos**: los NPCs reutilizan el mismo catálogo de `characterTraitDefinition` que el personaje jugable — no existe un catálogo de Rasgos exclusivo para NPCs. A diferencia del jugador (que elige conscientemente 1 Rasgo en creación de personaje), un NPC no elige: al spawnear, se sortea desde un **pool acotado de Rasgos posibles** que declara su `npcArchetypeDefinition`. Un NPC puede terminar con **0 o 1** Rasgo (nunca más de 1, igual que el jugador).

## Pertenencia a una comunidad

- Todo NPC porta una referencia a su comunidad (ej. `communityId`).
- Esa referencia determina quién puede darle órdenes/tareas: el jugador si es su propia comunidad, o un manager de comunidad si no lo es.
- El jugador no puede asignar órdenes/tareas directamente a NPCs que no pertenecen a su comunidad; hacerlo requiere gastar favor, mecanismo que depende enteramente del sistema de Comunidades.
- Cómo se crea/gestiona una comunidad, cómo un NPC se une, se recluta o cambia de comunidad, el sistema de favores y la lógica de manager de comunidad quedan **totalmente fuera de alcance**: son frontera hacia el futuro crunching de Comunidades, incluso a nivel de ganchos.

## Arquetipos (`npcArchetypeDefinition`)

Un arquetipo es la "receta" a partir de la cual se crea una instancia concreta de NPC. Declara:

- rango de valores de Atributos compartidos (Vida, Resistencia, Hambre, Sed).
- nivel fijo (o rango) de cada Capacidad.
- pool acotado de Rasgos sorteables.
- referencia visual/apariencia.
- comunidad por defecto, si aplica.

El **spawn** (dónde, cuándo, cuántos NPCs de qué arquetipo aparecen en el mundo) no es competencia de esta sesión: pertenece al futuro sistema de generación de mapa y al sistema de managers correspondiente, que consumirá arquetipos como los define `spawnTable` en `Modding.md`. Este documento solo define qué es un arquetipo y que actúa como fábrica de instancias.

## Identidad y persistencia

Todo NPC es una entidad **persistente** desde el momento en que spawnea: no es una instancia genérica/descartable. Una vez creado a partir de un arquetipo, mantiene su propio estado individual (Rasgo sorteado, nivel de Capacidades fijado, Estado Anímico si corresponde, comunidad) de forma continua.

## Órdenes y Tareas

### Diferencia fundamental

Órdenes y Tareas son **catálogos de comportamiento distintos**, no una envoltura de scheduling sobre uno solo:

- **Orden**: acción puntual. Se da, se ejecuta y con eso concluye su ciclo (tiene condición de finalización definida).
- **Tarea**: comportamiento persistente e indefinido. El jugador (o manager) la asigna sin especificar tiempo ni condiciones — el NPC evalúa por sí mismo cuándo actuar dentro de la tarea (ej. revisar si un cultivo está listo para cosechar, si necesita agua). Esa lógica de timing/condiciones vive en la implementación propia de cada tarea, no como parámetro genérico expuesto al jugador. Se ejecuta indefinidamente hasta que el jugador ordene lo contrario.

Ambas son dadas exclusivamente por el jugador (si el NPC es de su comunidad) o por el manager de comunidad correspondiente (si no lo es). Existen casos particulares donde una orden manual y una tarea asignada comparten comportamiento, pero en general son acciones distintas: no toda tarea tiene su equivalente como orden puntual (ej. no se le puede ordenar "ocupate de los cultivos, pero una sola vez").

### Arquitectura de ejecución

Los NPCs se implementan con un **behaviour tree único** con un selector de prioridad basado en `score`, en vez de una máquina de estados con capas separadas gestionadas manualmente:

- **Reacciones de supervivencia** (ej. defenderse o huir de una amenaza directa): prioridad **absoluta**, evaluadas fuera del sistema de scoring — no compiten por `score` con nada. Si hay una amenaza activa, la reacción gana siempre, sin excepción, incluyendo contra mods que intenten inflar el score de una orden o tarea.
- **Orden activa** y **Tarea de fondo**: compiten entre sí por `score` dentro del mismo selector, normalmente con la orden ganando por diseño de sus scores base.
- El behaviour tree reevalúa en cada tick cuál rama gana; cuando una rama de menor prioridad deja de ganar, queda interrumpida sola, y si vuelve a ganar más adelante, se retoma sola — sin necesidad de lógica explícita de pausa/resume por capa.

Reglas de interrupción resultantes:

- Una nueva orden dada por el jugador/manager **reemplaza directamente** la orden en curso (dispara su `onCancel`) — no existe cola de órdenes en el estándar inicial.
- Una orden interrumpe una tarea en curso; al completarse la orden, el NPC retoma automáticamente la tarea asignada.
- Una reacción de supervivencia interrumpe orden o tarea en curso; al resolverse la amenaza, se retoma automáticamente lo que estaba interrumpido, con el mismo mecanismo.

### Autoridad de red

Toda la evaluación del behaviour tree (selección de rama, `score`, `tick`, comandos resultantes) corre exclusivamente en el host/server, coherente con que `NPCOrder`/`npcTaskDefinition` son autoridad `server` según el estándar general de `Modding.md`. El cliente no predice decisiones de IA de NPCs — solo representa/interpola el resultado que el host ya decidió (posición, animación). No hay necesidad de responsividad de input local para NPCs como sí la hay para el jugador.

## Estado Anímico

- Rango fijo de **-5 a 5**.
- **Exclusivo de los NPCs que pertenecen a la comunidad del jugador**: los NPCs de otras comunidades no tienen esta mecánica (ni el dato ni el efecto de comportamiento) — se simulan de forma más simple del lado de su manager.
- Afecta por igual a Órdenes y Tareas (no está restringido a uno de los dos tipos): valores bajos aumentan la probabilidad de rechazo del comando nuevo, y reducen la eficiencia de ejecución (ej. traer menos recursos).
- **Rechazo**: cuando el ánimo hace que el NPC rechace un comando nuevo, ese comando simplemente nunca gana el selector de prioridad del behaviour tree. El NPC retoma su comportamiento de fondo normal (tarea existente, o idle si no tenía ninguna). No existe un estado explícito de "negarse" — la comunicación de que fue rechazado es responsabilidad de la UI, no del comportamiento del NPC.
- **Eficiencia**: el NPC expone un modificador de eficiencia derivado del ánimo (multiplicador) como parte de su contexto. Cada sistema externo que calcule el resultado de una orden/tarea (pesca, construcción, etc.) es responsable de leerlo y aplicarlo si corresponde — el sistema de NPCs no conoce ni fuerza el resultado concreto.
- **Disparadores** (qué sube o baja el ánimo: rechazo repetido, condiciones de vida en la comunidad, eventos sociales, etc.) quedan como **frontera hacia el futuro crunching de Comunidades**. Esta sesión solo deja establecido el gancho genérico `ModificarAnimo(delta, razon)` que un sistema externo puede invocar.
- **Visibilidad**: el jugador solo ve el ánimo de los NPCs de su propia comunidad (coherente con que es la única comunidad donde la mecánica existe).

## Decisiones cerradas en este crunching

- NPCs comparten Vida/Resistencia/Hambre/Sed, Capacidades y Habilidades Humanas/Pasivas con el jugable. Sin UMS ni Habilidades de Mutación.
- Capacidades de NPC: nivel fijo por arquetipo en spawn, sin progresión activa en tiempo real. Progresión vía interacción con comunidad (construcciones, tareas de largo plazo) es frontera hacia Comunidades.
- Rasgos: mismo catálogo `characterTraitDefinition` que el jugador, sorteado (0 o 1) de un pool acotado por arquetipo en spawn, sin elección consciente.
- Pertenencia a comunidad: solo referencia (`communityId`). Reclutamiento, favores, managers y gestión de comunidad son frontera hacia Comunidades.
- `npcArchetypeDefinition` como fábrica de instancias: rango de Atributos, nivel de Capacidades, pool de Rasgos, visual, comunidad por defecto. Spawn concreto (dónde/cuándo/cuántos) es frontera hacia mapa/managers.
- Todo NPC es persistente desde que spawnea, sin distinción por comunidad.
- Órdenes (una vez, terminan) y Tareas (persistentes, indefinidas, autoevaluadas) son catálogos distintos, ambos dados solo por jugador (comunidad propia) o manager (otras comunidades); el jugador no puede asignar directo a NPCs ajenos sin gastar favor (Comunidades).
- Ejecución vía behaviour tree único con selector por `score`. Reacciones de supervivencia con prioridad absoluta fuera de scoring; Orden y Tarea compiten por score entre sí.
- Nueva orden reemplaza la orden en curso, sin cola. Orden interrumpe tarea (retoma al completar); reacción interrumpe orden/tarea (retoma al resolverse), mismo mecanismo de interrupción/retoma automática.
- Behaviour tree corre 100% en host/server; sin predicción de IA en cliente.
- Estado Anímico (-5 a 5): exclusivo de NPCs de la comunidad del jugador. Afecta rechazo y eficiencia de órdenes y tareas por igual. Rechazo sin estado explícito, solo no-inicio del comando. Eficiencia expuesta como gancho genérico, aplicado por cada sistema externo. Disparadores concretos son frontera hacia Comunidades.
- Todos los valores numéricos son tentativos, pendientes de playtesting, y deben vivir en `ScriptableObjects`.

## Riesgos y límites explícitos

- Este documento no resuelve el diseño de Comunidades (reclutamiento, favores, managers, disparadores de ánimo, progresión de Capacidad vía interacción social/construcciones), generación de mapa/spawn, virus/infección de NPCs, ni Salud. Solo deja asentados los ganchos que el NPC debe exponer hacia esos sistemas.
- El mapeo concreto ánimo → probabilidad de rechazo y ánimo → multiplicador de eficiencia no está definido numéricamente; requiere balanceo por playtesting.

## Pendientes para continuar

- Mecanismo de reclutamiento, favores y managers de comunidad (depende de futuro crunching de Comunidades).
- Disparadores concretos que suben/bajan el Estado Anímico (depende de futuro crunching de Comunidades).
- Progresión de Capacidades vía interacción con construcciones del jugador o tareas de largo plazo (depende de futuro crunching de Comunidades).
- Algoritmo y reglas concretas de spawn de NPCs en el mundo (depende de futuro crunching de mapa/generación).
- Mecanismo de infección/conversión de NPCs a zombies, si existiera (depende de futuro crunching de virus/mutaciones).
- Valores numéricos concretos (mapeos de ánimo, rangos de Atributos/Capacidades por arquetipo).

## Documento de modding relacionado

Ver [NPCs_Modding.md](./NPCs_Modding.md) para las decisiones específicas de modding de este sistema (resource types, lifecycle de extension points, qué es abierto/cerrado a mods), apoyadas en el estándar general de [Modding.md](./Modding.md).
