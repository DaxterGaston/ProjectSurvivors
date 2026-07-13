# Project Survivors

## Caracteristicas del Sistema de Historia del juego

    Este documento va a definir como se compone la historia del juego.

- ### Se carga al iniciar la partida:
    La historia del juego va a consumir de un archivo Json para determinar los hilos. El json tendra las siguientes caracteristicas:
        - Listado de Misiones
        - Dialogos y id's de Npcs con los que vamos a tener los dialogos
        - Orden y dependencia de las misiones _Ejemplo_: Mision 1 siempre va antes que mision 2, por el orden hasta que no se termina la 1 no desaparece la 2. El id no necesariamente es el orden, ya que las misiones con mismo orden son de decisión, como esta detallado en [Sistema de Misiones](/docs/SistemaDeMisiones.md#misiones-primarias)

- ### No es obligatoria:
    La historia está. Las mecanicas del juego permiten abarcarlo de distintas maneras, no avanzar en la historia no va a generar penalizacion algúna.