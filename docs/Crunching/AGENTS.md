# AGENTS

## Propósito

Los documentos dentro de `docs/Crunching` representan decisiones refinadas obtenidas a partir de sesiones de crunching sobre los documentos base de `docs/Definiciones`.

Cada documento de crunching debe funcionar como referencia viva de decisiones de producto, alcance, límites, dependencias y relaciones entre módulos.

## Reglas de mantenimiento

- Cuando se cree un nuevo documento en `docs/Crunching`, revisar los crunchings existentes para detectar si alguno mencionaba ese módulo como tema pendiente, dependencia o límite no resuelto.
- Si un crunching existente depende de un nuevo crunching ya resuelto, actualizar el documento anterior para agregar una referencia explícita al nuevo documento.
- No reemplazar decisiones ya cerradas en un crunching anterior salvo que una nueva sesión lo redefina explícitamente.
- Si una nueva sesión redefine una decisión anterior, actualizar ambos documentos:
  - el documento nuevo debe dejar asentada la redefinición
  - el documento anterior debe marcar que ese punto fue revisado y referenciar el documento más reciente
- Mantener trazabilidad entre documentos usando enlaces relativos dentro de `docs/Crunching`.

## Referencias cruzadas

- Si un documento menciona que cierto tema queda fuera de alcance porque pertenece a otro módulo, debe enlazar el crunching de ese módulo cuando exista.
- Si ese crunching todavía no existe, dejar el tema señalado como pendiente de resolución.
- Cuando el crunching faltante sea creado, volver al documento anterior e incorporar la referencia correspondiente.

## Alcance de cada crunching

- Cada sesión de crunching debe mantenerse enfocada en el documento o módulo solicitado por el usuario.
- No mezclar en profundidad decisiones de otros módulos salvo para dejar dependencias, supuestos o límites explícitos.
- Cuando aparezcan temas que pertenecen a otro documento, registrarlos como dependencia o frontera, no resolverlos por completo fuera de su sesión correspondiente.

## División en documento general y documento de modding

- Si el sistema trabajado en la sesión tiene partes moddeables (extension points, resource types, reglas de patch/override, etc.), la sesión debe producir **dos documentos** en `docs/Crunching`, no uno solo:
  - `<Sistema>.md`: decisiones generales de diseño/producto del sistema, sin profundizar en modding.
  - `<Sistema>_Modding.md`: decisiones específicas de modding del sistema (resource types nuevos, extension points, reglas de patch/override/visibilidad propias del sistema), apoyándose en el estándar ya cerrado en [Modding.md](./Modding.md).
- Ejemplo: la sesión sobre `CaracteristicasPersonaje` produce `CaracteristicasPersonaje.md` y `CaracteristicasPersonaje_Modding.md`.
- Si el sistema no tiene partes moddeables, alcanza con un único documento `<Sistema>.md`.
- Ambos documentos deben enlazarse entre sí (el general referencia al de modding y viceversa), y el de modding debe enlazar a `Modding.md` como estándar base.

## Objetivo editorial

- Los documentos de `docs/Crunching` deben leerse como una red coherente de definiciones refinadas.
- Deben poder recorrerse por enlaces entre módulos sin perder contexto sobre qué quedó resuelto en cada uno.
- El estado ideal es que un lector pueda entrar por cualquier crunching y encontrar referencias claras hacia los demás documentos relevantes.
