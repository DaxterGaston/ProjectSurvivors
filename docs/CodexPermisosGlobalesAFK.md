# Configuracion Global De Codex Para AFK

Este documento deja registrado lo que se cambio en esta maquina para que la misma configuracion se pueda repetir en otra PC que tambien use Codex con pipeline AFK.

## Objetivo

Dejar a Codex con estos comportamientos por defecto:

- No pedir aprobacion interactiva para ejecutar comandos normales.
- Permitir operaciones de `git` dentro del workspace.
- Permitir uso de `gh` contra GitHub sin tener que aprobar cada comando.
- Evitar `danger-full-access` como configuracion global por defecto.

## Archivos Tocados

Se tocaron estos archivos fuera del repositorio:

- `C:\Users\Gastón\.codex\config.toml`
- `C:\Users\Gastón\.local\bin\codex.bat`

Este documento se agrega dentro del repo solo como referencia:

- `docs/CodexPermisosGlobalesAFK.md`

## Cambio Principal

El cambio importante y suficiente para nuevas sesiones de Codex esta en:

- `C:\Users\Gastón\.codex\config.toml`

Se agregaron estas claves y tablas.

Importante:

- `approval_policy` y `default_permissions` son claves top-level.
- En TOML deben ir antes del primer bloque de tablas, no al final del archivo.
- Si se agregan despues de una tabla existente, Codex puede interpretarlas como parte de esa tabla y fallar al cargar la config.

La forma correcta queda asi:

```toml
model = "gpt-5.4"
model_reasoning_effort = "medium"
approval_policy = "never"
default_permissions = "workspace"

[permissions.workspace.filesystem]
":minimal" = "read"

[permissions.workspace.filesystem.":workspace_roots"]
"." = "write"

[permissions.workspace.network]
enabled = true

[permissions.workspace.network.domains]
"github.com" = "allow"
"api.github.com" = "allow"
"uploads.github.com" = "allow"
"objects.githubusercontent.com" = "allow"
"raw.githubusercontent.com" = "allow"
```

## Que Hace Esa Configuracion

- `approval_policy = "never"`
  Hace que Codex no pida aprobacion interactiva antes de ejecutar comandos.

- `default_permissions = "workspace"`
  Hace que todas las sesiones nuevas usen este perfil por defecto.

- `":minimal" = "read"`
  Mantiene un baseline conservador de lectura.

- `":workspace_roots"."." = "write"`
  Permite escribir dentro del workspace activo.

- `network.enabled = true`
  Habilita red para el perfil.

- `network.domains`
  Restringe esa red a hosts de GitHub necesarios para `gh` y operaciones relacionadas.

## Impacto Practico

Con esta configuracion, en sesiones nuevas de Codex:

- `git` no deberia pedir aprobacion mientras opere dentro del workspace permitido.
- `gh issue list`, `gh issue view` y operaciones similares contra GitHub no deberian pedir aprobacion.
- Codex no queda en modo `danger-full-access`.

## Wrapper Global

Tambien se dejo este wrapper global:

- `C:\Users\Gastón\.local\bin\codex.bat`

Contenido actual:

```bat
@echo off
call "%APPDATA%\npm\codex.cmd" --full-auto %*
```

## Nota Sobre El Wrapper

Ese wrapper ya no es necesario para la politica de permisos global, porque la configuracion real vive en `C:\Users\Gastón\.codex\config.toml`.

Ademas:

- `--full-auto` no es la forma recomendada actual para `codex` interactivo.
- La documentacion actual lo trata como compatibilidad vieja para `codex exec`.
- Para replicar esta configuracion en otra maquina, lo importante es copiar la configuracion de `config.toml`, no el wrapper.

## Recomendacion Para La Otra PC

1. Instalar Codex normalmente.
2. Abrir o crear `C:\Users\<usuario>\.codex\config.toml`.
3. Agregar las claves top-level `approval_policy` y `default_permissions` antes del primer bloque de tablas del archivo.
4. Agregar el perfil `[permissions.workspace]` con filesystem y network.
5. Reiniciar cualquier sesion abierta de Codex.
6. Probar desde un repo con:

```powershell
gh issue list --limit 5
git status
```

## Si Algo Falla

Si `gh` sigue sin funcionar sin prompts o sin red:

- Verificar login de GitHub CLI en esa maquina.
- Verificar que la sesion de Codex sea nueva.
- Verificar que `~/.codex/config.toml` no tenga esas claves metidas accidentalmente dentro de una tabla como `[tui.model_availability_nux]` o `[notice.model_migrations]`.
- Si el flujo AFK necesita otros hosts ademas de GitHub, agregarlos a `[permissions.workspace.network.domains]`.

## Error Real Encontrado

Durante la configuracion aparecio este error:

```text
config defines `[permissions]` profiles but does not set `default_permissions`
```

La causa no era que faltara la clave, sino que `default_permissions` habia quedado adentro de otra tabla TOML y por eso Codex no la veia como top-level.

Despues aparecio otro error:

```text
invalid type: string "never", expected u32
in `tui.model_availability_nux`
```

Eso confirmo el mismo problema: `approval_policy` habia quedado anidado dentro de `[tui.model_availability_nux]`.

La solucion correcta fue mover ambas claves al encabezado del archivo.
