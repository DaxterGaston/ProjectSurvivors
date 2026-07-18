# CLAUDE

## Propósito

Los documentos dentro de `docs/Definiciones` son los documentos base del proyecto: definen las ideas, sistemas y mecánicas del juego en su forma inicial, antes de ser refinados en profundidad.

Estos documentos no son la fuente final de decisiones cerradas. Son el punto de partida sobre el cual se realizan sesiones de crunching que refinan, acotan y resuelven ambigüedades.

## Relación con docs/Crunching

- Las sesiones de crunching que trabajemos sobre estos documentos se guardan como archivos independientes en `docs/Crunching`, no como ediciones directas de los documentos de `docs/Definiciones`.
- `docs/Definiciones` mantiene el material original; `docs/Crunching` mantiene las decisiones refinadas resultantes de discutirlo.
- Ver `docs/Crunching/AGENTS.md` para las reglas de mantenimiento y trazabilidad entre documentos de crunching.

## Antes de cualquier sesión de crunching

Antes de iniciar o continuar una sesión de crunching sobre cualquier tema de este directorio, leer siempre:

1. El documento de `docs/Definiciones` correspondiente al tema a trabajar.
2. `docs/Crunching/Idea.md` — decisiones ya refinadas sobre las bases del juego.
3. `docs/Crunching/Modding.md` — decisiones ya refinadas sobre el sistema de modding.

Estos dos crunchings existentes pueden contener decisiones, dependencias o límites que afectan al tema nuevo, incluso si no es evidente desde el documento de definición. No asumir que un tema es aislado sin haberlos revisado primero.

## Alcance de cada sesión de crunching

- Una sesión de crunching sobre un documento de `docs/Definiciones` abarca solo lo exclusivo de ese documento, no el diseño interno de otros sistemas con los que se relaciona.
- Cuando el documento mencione o dependa de un elemento que pertenece a otro sistema (por ejemplo, una receta de construcción mencionada desde el documento de personaje), ese elemento no se resuelve en la sesión actual: se deja asentado como dependencia o frontera explícita hacia el crunching correspondiente de ese otro sistema.
- Esto es consistente con la regla de alcance ya definida en `docs/Crunching/AGENTS.md`.
