# Project Survivors

## Definicion del proyecto y Stack tecnologico: 

Videojuego.
- Unity [Version 6 LTS]
    - Nuevo input system
    - Unity Multiplayer Engine/Photon Engine/Mirror

- .NET [Version compatible con Unity: Framework 4.8/ Core 3.0]


## Premisa

La trama del juego es la siguiente: El jugador es un superviviente en una isla, donde brotó un virus que convierte a la gente en zombies, donde el superviviente va a tener inmunidad. El virus va a ser de origen vegetal, a medida que avanza la historia el superviviente va a obtener habilidades mediante la exposicion a mutaciones del virus. Se va a centrar en la supervivencia mediante la obtencion de recursos y crear una comunidad con aliados que van a ir encontrando.

La finalidad de las misiones principales "vanilla" del juego es reparar un barco para escapar. Para eso, el jugador va a necesitar aliarse con otros supervivientes, conseguir materiales/items "clave", y los conocimientos para la reparacion del barco, o reclutar a los supervivientes que tengan esos conocimientos.

### Caracteristicas principales del juego

- El juego va a ser single player y multiplayer.
- Las plataformas van a ser PC y Android.
- El juego va a ser modeable, va a consumir scripts de un directorio para poder modificar ciertos comportamientos del juego. Todas las caracteristicas modeables estan marcadas como _**Modeable**_ al inicio del item de la lista, o directamende puede ser declarado a nivel de un documento de definiciones, abarcando todas las caracteristicas detalladas en el mismo.
- El mapa se va a generar [aleatoriamente](docs/GeneracionMapa.md)
- Va a tener elementos de [RPG](docs/CaracteristicasPersonaje.md)
- Va a tener NPCs, Aliados - Neutrales - Hostiles, que van a compartir las caracteristicas de los personajes. Además, los NPCs aliados y pertenecientes a la comunidad del jugador van a poder ser [controlados](docs/GestionComunidades.md) por el jugador.
- El juego tiene 2 posibles enfoques para el jugador:

    - Sandbox: El jugador se va a encargar de sobrevivir, crear su comunidad, gestionarla, obtener recursos, etc.
    - Historia: El juego va a tener una historia, la cual se va a ser creada desde un archivo .JSON, para permitir que los usuarios creen sus propias historias, definiendo dialogos, personajes y misiones creando un [grafo](docs/SistemaHistoria.md)

- La historia del juego se va a componer de un sistema de [misiones](docs/SistemaDeMisiones.md) tanto primarias como secundarias/optativas.
- Va a haber vehiculos.
- Sistema de construccion que va a permitirle al jugador construir estructuras, asi como tambien van a poder reforzarse, tanto las construidas por el jugador como las ya existentes.