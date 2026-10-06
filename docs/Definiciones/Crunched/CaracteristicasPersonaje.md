# Project Survivors

## Caracteristicas del personaje jugable

### Todas las caracteristicas de los personajes son [_**Modeable**_].

- #### Atributos: 
    - #####  Vida: 
        La salud del jugador.
    - #####  Resistencia: 
        La resistencia del jugador, la mayoria de las habilidades/acciones que impliquen esfuerzo fisico consumen de este atributo.
    - #####  UMS (Unique Mutation Skill): 
        Equivalente de resistencia para habilidades de tipo mutacion.
    - #####  Fuerza: 
        Modifica el daño que se hace con armas cuerpo a cuerpo, tambien requerido para ciertas acciones.
    - #####  Defensa: 
        Modifica el daño recibido, principalmente interactua con los equipamientos, dependiendo lo que vista el daño que recibe.
    - #####  Hambre: 
        Va a aumentar con el tiempo, las comidas lo reducen, tener un nivel de hambre alto va a reducir la fuerza y la defensa. Eventualmente, si se encuentra en valores demasiado elevados va a reducir un % de vida del personaje cada cierto tiempo.
    - #####  Sed: 
        Va a aumentar con el tiempo, las bebidas lo reducen, tener un nivel de sed alto va a reducir la resistencia. Eventualmente, al igual que el hambre, mantener un valor elevado va a reducir la vida.
    
    Estos son los definidos iniciales, pero es muy problable que se agreguen mas.
    Todos los atributos pueden variar su valor durante el juego, por cambios de equipamientos, uso de items consumibles, etc.

- #### Capacidades
    Son las capacidades que tiene el personaje en distintas áreas. Se entrenan para subir de nivel, y los niveles desbloquean distintos aspectos del personaje, tales como habilidades, creacion de algun item, construcciones, etc. 
    -  ##### Combate: 
        Capacidad de combate cuerpo a cuerpo del personaje. A mayor el nivel, aumenta el daño y la velocidad de ataque del personaje, asi como reduce su consumo de resistencia por golpe.
    - #####  Punteria: 
        Capacidad de combate a distancia. Afecta el daño y la posibilidad de inflingir daño crítico con cualquier forma de ataque a distancia, armas arrojadizas, armas de fuego, arcos, ballestas.
    - ##### Elaboracion: 
        Capacidad para construir [items](/docs/Definiciones/SistemaDeConstruccion.md#items).
    - ##### Construccion: 
        Capacidad para realizar [construcciones](/docs/Definiciones/SistemaDeConstruccion.md#construcciones) en el mapa.
    

- #### Rasgos: 
    Afectan atributos e interacciones del personaje con el juego. Por ejemplo, rasgo de conocimientos de agricultura podria hacer que al cosechar un cultivo el jugador recibe mas alimentos/semillas.

- #### Habilidades: 
    Las habilidades tienen distintos niveles. Dependiendo el nivel de la habilidad su consumo de atributos(resistencia/ums), cooldown y efectos varian. 
    Se dividen en 2 grupos:
    - #####  Humanas: 
        Habilidades comunes que puede tener un ser humano. Por ejemplo, tener buenos reflejos y permitirle al usuario realizar un esquive a coste de un valor del atributo de resistencia.
    - #####  Mutaciones: 
        Se consiguen al interactuar con mutaciones del virus. Por ejemplo, a costa de ums el jugador puede obtener una mejora en la regeneracion de resistencia/salud por X tiempo, o alguna habilidad de ataque como lanzar algun tipo de proyectil biologico.