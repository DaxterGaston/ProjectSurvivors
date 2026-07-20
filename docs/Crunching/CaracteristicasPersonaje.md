# Crunching de CaracteristicasPersonaje

## Alcance de este documento

Este documento resume y aclara las decisiones de diseño derivadas exclusivamente de [../Definiciones/CaracteristicasPersonaje.md](../Definiciones/CaracteristicasPersonaje.md): Atributos, Capacidades, Rasgos y Habilidades del personaje jugable.

No profundiza en el diseño interno de otros sistemas mencionados desde acá (Equipamiento, Combate, Elaboración, Construcción, NPCs, virus/mutaciones, Salud). Cuando el documento depende de uno de esos sistemas, se deja asentado como frontera explícita, no como decisión resuelta.

Este documento se apoya en las decisiones generales ya cerradas en [Idea.md](./Idea.md) y en el estándar de modding cerrado en [Modding.md](./Modding.md). La parte específicamente moddeable de este sistema se resuelve en el documento complementario [CaracteristicasPersonaje_Modding.md](./CaracteristicasPersonaje_Modding.md), siguiendo la regla de división de [CLAUDE.md](./CLAUDE.md).

## Principio de valores tentativos

Todo valor numérico mencionado en este documento (caps, umbrales, tasas, incrementos) es **tentativo**, sujeto a balanceo mediante playtesting una vez el juego sea jugable. Ningún número se considera cerrado. Todos estos valores deben vivir en `ScriptableObjects`, nunca hardcodeados en código.

## Atributos

### Modelo de datos: dos grupos distintos

- **Grupo current/max**: `Vida`, `Resistencia`, `UMS`, `Hambre`, `Sed`. Todos comparten la forma current/max, pero con reglas distintas por atributo (ver abajo).
- **Fuerza y Defensa**: quedan **a completar**. Tienen valores aumentables y escalables, pero su diseño depende de los sistemas de Equipamiento y Combate, que no existen todavía como crunching. No se les asigna modelo de datos en esta sesión.

### Vida, Resistencia, UMS

- Tienen `current` y `max`.
- El `max` puede aumentar de dos formas independientes:
  1. **Progresión pasiva por uso**, mediante el mecanismo de acumulador + umbrales descrito abajo. Evento que dispara ganancia: Vida al recibir daño, Resistencia al caminar/correr/atacar, UMS al usar habilidades de mutación.
  2. **Modificadores externos** (ej. un item tipo jeringa que aumenta el UMS máximo). El diseño concreto de esos items pertenece al futuro sistema de Items — acá solo se deja el requisito: el modelo de datos del personaje debe exponer una función pública para aumentar el `max` de un atributo desde un sistema externo.
- Cuando el `max` sube (por cualquiera de las dos vías), el `current` sube en la misma cantidad, para no dejar al jugador con menos margen relativo.
- Existe un **cap de nivel** para el crecimiento del `max` de cada uno de estos tres atributos (valor pendiente de balanceo). Motivo: sin cap, farmeo de la acción disparadora (recibir daño, caminar en loop, spamear mutaciones) generaría valores arbitrarios sin techo, riesgoso en un juego con multiplayer/PvP y con mods que pueden alterar tasas de ganancia.
- **Regeneración**: los tres regeneran `current` pasivamente con el tiempo. La tasa de regeneración es modificable por factores externos (items, Rasgos), mismo patrón que el resto de modificadores de este documento.
  - **Excepción: Vida.** La lógica concreta de si/cuánto/cuándo regenera Vida (incluyendo heridas, tratamiento, interacción con Hambre/Sed) pertenece a un futuro **sistema de Salud**, fuera del alcance de este crunching. Acá solo se deja establecido que Vida es un atributo current/max con una tasa de regeneración potencialmente modificable; su fórmula real queda pendiente.

### Mecanismo de progresión por uso (acumulador + umbrales)

Aplica a Vida, Resistencia y UMS (y, con su propia instancia de datos, también a Capacidades y Habilidades — ver sección de Capacidades y de Habilidades).

- Cada atributo tiene un **acumulador de experiencia** propio.
- Cada evento disparador (recibir daño, caminar, usar mutación) suma una **cantidad fija** al acumulador.
- Esa cantidad fija puede verse alterada por **modificadores externos** (items, equipamiento), que multiplican o alteran la ganancia por evento — esto habilita trade-offs de diseño (ej. un item que aumenta el consumo de Resistencia a cambio de dar más experiencia por uso, análogo a entrenar con peso).
- El acumulador se compara contra una **tabla/curva de umbrales crecientes** (ej. 100 para el primer aumento de max, 200 para el segundo, 300 para el tercero...). Al cruzar el umbral vigente, el `max` sube (cantidad de incremento también pendiente de balanceo) y el acumulador avanza al siguiente umbral.
- Los umbrales son crecientes (retornos decrecientes) para evitar que farmear la acción disparadora sea la estrategia dominante y trivialmente explotable.
- Esta estructura vive en `ScriptableObjects` para permitir ajuste por playtesting y exposición a modding.

### Consumo y bloqueo

- Cuando una acción requiere gastar `current` de un atributo (ej. Resistencia o UMS) y el `current` disponible es menor al costo requerido, la acción se **bloquea completamente**: no se ejecuta sin el costo completo disponible. No se permite ejecutar y dejar el atributo en negativo.
- **Nota abierta, no cerrada:** se planteó la posibilidad de permitir "sobreesfuerzo" — ejecutar la acción igual aunque falte `current`, llevando el atributo a 0 y cobrando el excedente como daño a Vida (ej. entrenar exhausto y lastimarse). Queda como idea a pulir o descartar en una sesión futura, no como regla activa.

### Hambre y Sed

- Rango fijo **0-100** para ambos. A diferencia de Vida/Resistencia/UMS, **no** tienen el mecanismo de crecimiento de `max` por uso — su tope es constante.
- Suben con el tiempo; las comidas/bebidas los reducen (mecanismo concreto de consumibles pertenece al futuro sistema de Items).
- La tasa de subida es modificable por factores externos (Rasgos, items) — ej. un Rasgo de "aguanta mucho sin comer" desacelera específicamente el ritmo de Hambre.
- Efectos por nivel: en vez de un único umbral fijo, se modelan mediante una **lista abierta de N umbrales**, cada uno con su propio efecto asociado (debuff a un atributo, daño periódico a Vida, etc.). Esto reemplaza la idea original de "dos umbrales fijos" y está pensado explícitamente para que mods puedan agregar sus propios umbrales/efectos sin tocar el core.

## Capacidades

Combate, Puntería, Elaboración, Construcción. Ver documento base para su descripción funcional.

### Por qué es un sistema de progresión independiente de Atributos

Las Capacidades, a diferencia de los Atributos, **requieren interactuar con otro sistema para siquiera aplicarse** (Combate necesita un arma o cuerpo a cuerpo, Puntería necesita un arma a distancia, Elaboración necesita materiales, Construcción necesita materiales). Los Atributos pueden interactuar con otros sistemas, pero no de forma obligatoria. Por esto, Capacidades y Atributos son dos mecanismos de progresión independientes, aunque comparten la misma forma de datos.

### Estructura de datos

- Reutiliza la misma estructura de acumulador + tabla de umbrales crecientes definida para Atributos, con su propia instancia de datos por Capacidad (no comparten experiencia entre sí).
- A diferencia de Atributos (donde el propio personaje puede disparar el evento de ganancia, ej. caminar), en Capacidades el evento de ganancia depende inherentemente de sistemas externos: el personaje expone una función pública genérica (ej. `OtorgarExperienciaCapacidad`) invocable desde Combate, Puntería/combate a distancia, Elaboración y Construcción cuando ocurra su evento relevante (golpe conectado, disparo, item creado, construcción completada). El detalle exacto de cada evento disparador pertenece al crunching de cada sistema externo, no a este documento.
- Existe un **cap de nivel** por Capacidad (valor pendiente de balanceo), mismo razonamiento de riesgo de balance/exploit que con los Atributos — agravado acá porque el nivel de Capacidad además desbloquea contenido (ver abajo).

### Desbloqueos por nivel de Capacidad

- Subir de nivel una Capacidad desbloquea aspectos del personaje: Habilidades Humanas (resuelto en este documento, ver sección de Habilidades), creación de items (Elaboración — pertenece al futuro sistema de Items, pendiente) y construcciones (Construcción — pertenece al futuro sistema de Construcción, pendiente).
- Este documento solo resuelve el uso de nivel de Capacidad como **gate/condición de desbloqueo**. Qué item o construcción específica se desbloquea en cada nivel es contenido de esos sistemas externos.

## Rasgos

- Se eligen **una única vez, en el momento de creación del personaje**, como un "regalo de bienvenida". No existe ningún mecanismo para obtener Rasgos adicionales durante la partida.
- El jugador elige **exactamente 1 Rasgo** de una lista de Rasgos predefinidos.
- Cada Rasgo es un **paquete de recompensas predefinido y fijo** (no hay sistema de puntos/presupuesto libre para armar combinaciones). Un Rasgo puede otorgar cualquier combinación de:
  - nivel(es) de Atributo,
  - nivel(es) de Capacidad,
  - una Habilidad Pasiva,
  - una Habilidad de Mutación.
- Los Rasgos son un ejemplo central de contenido moddeable de este sistema (ver [CaracteristicasPersonaje_Modding.md](./CaracteristicasPersonaje_Modding.md)).

## Habilidades

Tres categorías, **cerradas por diseño** (un mod no puede declarar una cuarta categoría; sí puede declarar Habilidades nuevas dentro de las tres existentes):

- **Humanas**: consumen Resistencia.
- **Mutaciones**: consumen UMS.
- **Pasivas**: siempre activas, sin costo ni cooldown, siempre benefician al jugador.

Se mantienen como categorías separadas (en vez de una propiedad ortogonal "activa/pasiva" sobre Humanas/Mutaciones) porque cada categoría tiene lógica propia entrelazada (qué atributo consume, si tiene cooldown, cómo se activa) — una categoría nueva o una propiedad ortogonal obligaría a redefinir esa lógica desde cero.

### Nivel de Habilidad

- Toda Habilidad (de cualquiera de las tres categorías) tiene nivel, usando la misma estructura de acumulador + umbrales ya definida, disparada por el evento "se usó esta Habilidad".
  - **Pendiente sin resolver**: para Habilidades Pasivas, al no tener una activación discreta (están siempre activas), no está definido qué cuenta como "uso" para disparar la ganancia de experiencia. Esto queda abierto para una sesión futura; no se inventa una regla sin base.
- En Humanas y Mutaciones, subir de nivel mejora eficiencia: cooldown más corto y consumo (Resistencia/UMS) más bajo.
- En Pasivas, el nivel solo escala la magnitud del efecto (no aplica cooldown ni consumo).
- El cooldown es **independiente por Habilidad**; no existe cooldown global/compartido entre Habilidades.

### Adquisición

- **Mutaciones**: se obtienen al interactuar con mutaciones del virus. Mecanismo completo pertenece a un futuro sistema de virus/mutaciones, fuera de alcance de este documento — pendiente de crunching específico.
- **Humanas**: se desbloquean por al menos dos vías:
  1. **Nivel de Capacidad** (resuelto en este documento — es la única vía 100% propia del personaje).
  2. Otras fuentes externas (ej. aprendizaje de un NPC maestro), cuyo mecanismo concreto pertenece al futuro crunching de NPCs — pendiente.
  - Del lado del personaje, ambas vías (y cualquier otra futura) usan un gancho genérico `OtorgarHabilidad`, para que el personaje no necesite saber de dónde vino la Habilidad.
- **Pasivas**: se obtienen principalmente vía Rasgo en creación de personaje. El gancho genérico `OtorgarHabilidad` también aplica a esta categoría para cualquier fuente externa futura.

## Decisiones cerradas en este crunching

- Vida, Resistencia, UMS, Hambre, Sed usan modelo `current/max`. Fuerza y Defensa quedan a completar (dependen de Equipamiento/Combate).
- Vida, Resistencia y UMS: `max` crece por progresión de uso (acumulador + umbrales crecientes) y por modificadores externos (items). `current` sube junto con `max`. Cap de nivel obligatorio para ambos.
- Regeneración pasiva para Vida, Resistencia y UMS, con tasa modificable externamente. La fórmula real de regeneración de Vida es responsabilidad de un futuro sistema de Salud.
- Bloqueo estricto de acciones si falta `current` suficiente (sin negativos). Idea de "sobreesfuerzo" queda como nota abierta, no como regla.
- Hambre y Sed: rango fijo 0-100 (sin crecimiento de max), tasa de subida modificable externamente, efectos modelados como lista abierta de N umbrales (no fijo en 2).
- Capacidades: sistema de progresión independiente de Atributos porque requieren interacción obligatoria con otro sistema. Misma estructura de datos (acumulador + umbrales), gancho externo `OtorgarExperienciaCapacidad`, cap de nivel obligatorio. Su nivel desbloquea Habilidades Humanas (resuelto acá) y contenido de otros sistemas (Elaboración/Construcción, pendiente).
- Rasgos: elección única en creación de personaje, exactamente 1, paquete de recompensas predefinido y fijo (sin sistema de puntos).
- Habilidades: 3 categorías cerradas (Humanas, Mutaciones, Pasivas), contenido abierto dentro de cada una. Mismo mecanismo de acumulador+umbrales para nivel de Habilidad. Cooldown independiente por Habilidad. Nivel mejora eficiencia en Humanas/Mutaciones y magnitud en Pasivas. Adquisición de Humanas vía nivel de Capacidad (resuelto) u otras fuentes externas (pendiente); Mutaciones vía virus (pendiente); Pasivas vía Rasgo, con gancho genérico `OtorgarHabilidad` para cualquier categoría.
- Todos los valores numéricos son tentativos, pendientes de playtesting, y deben vivir en `ScriptableObjects`.

## Riesgos y límites explícitos

- Este documento no resuelve el diseño de Fuerza, Defensa, Equipamiento, Combate, Elaboración, Construcción, NPCs, virus/mutaciones ni Salud. Solo deja asentados los ganchos que el personaje debe exponer hacia esos sistemas.
- El evento de progreso de experiencia para Habilidades Pasivas queda explícitamente sin resolver — no inventar una regla en implementación sin volver a esta discusión.
- Los valores concretos de caps, umbrales, tasas e incrementos no deben tratarse como definitivos en ninguna implementación; todos requieren balanceo por playtesting.

## Pendientes para continuar

- Diseño completo de Fuerza y Defensa (depende de crunching de Equipamiento y de Combate).
- Fórmula de regeneración de Vida, incluyendo heridas y tratamiento (depende de futuro crunching de Sistema de Salud).
- Mecanismo concreto de desbloqueo de Habilidades Humanas vía NPCs (depende de [NPCs.md](./NPCs.md) / [NPCs_Modding.md](./NPCs_Modding.md) — el crunching de NPCs ya está resuelto en lo que le compete a ese sistema, pero el flujo concreto de aprendizaje jugador-NPC todavía no).
- Mecanismo de obtención de Habilidades de Mutación vía virus (depende de futuro crunching de virus/mutaciones).
- Qué desbloquea exactamente cada nivel de Elaboración/Construcción (depende de futuros crunchings de Items y Construcción).
- Evento disparador de experiencia para Habilidades Pasivas.
- Idea de "sobreesfuerzo" (ejecutar acción sin `current` suficiente a cambio de daño a Vida): pulir o descartar.
- Todos los valores numéricos (caps, umbrales, tasas, incrementos).

## Documento de modding relacionado

Ver [CaracteristicasPersonaje_Modding.md](./CaracteristicasPersonaje_Modding.md) para las decisiones específicas de modding de este sistema (resource types, qué es abierto/cerrado a mods, patchability), apoyadas en el estándar general de [Modding.md](./Modding.md).
