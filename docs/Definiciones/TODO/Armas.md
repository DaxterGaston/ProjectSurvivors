# Project Survivors

## Caracteristicas del sistema de Armas
Exceptuando las propias categorias de armas, que los tipos y cantidad de categorias que existen dentro del juego son conceptos inmutables, las armas que existen dentro del juego son [_*Moddeables*_], tanto para agregar nuevas armas como modificar las ya existentes.

### Tipos de armas
Las armas del juego se dividen en 3 categorias, que a su vez cada una tiene dentro sub-categorias que separan las armas por tipo - utilidad - funcion. Los atributos comunes de un arma son: Daño - Retroceso en daño - Posibilidad de Stun - Posibilidad de crítico - Velocidad de ataque - Durabilidad.
Este ultimo atributo es el fundamental, a medida que su durabilidad baja, su daño cae. Pueden ser reparadas indefinidamente para recuperarlo.

Las categorias de las armas son las siguientes:

- #### Cuerpo a cuerpo:
    Son armas de combate cuerpo a cuerpo. Dependiendo su tipo varian sus caracteristicas:


- ##### Filo:
    Armas con filo como espadas, machetes, o cuchillos. Se caracterizan por velocidad de ataque y el daño al enemigos, asi como mayor probabilidad de critico.
    Su daño, velocidad y probabilidad de critico se ve afectado por la [capacidad de combate](../Crunched/CaracteristicasPersonaje.md#combate) del personaje.

- ##### Contusión:
    Armas sin filo, poseen menor daño y menor velocidad de combate, a cambio de que generan mas retroceso de los enemigos, con la posibilidad de stunearlos, con una capacidad de critico moderada.
    Su daño y probabilidad de stunear se ven afectados por la [fuerza](../Crunched/CaracteristicasPersonaje.md#fuerza) del personaje, y su velocidad y capacidad de criticos se ven afectados por la [capacidad de combate](../Crunched/CaracteristicasPersonaje.md#combate).


    Las armas pertenecientes a las sub-categorias anteriores tambien pueden agruparse en otras 2 sub-categorias: 

- ###### Cortas:
    Poseen menos daño que las armas largas, pero mayor durabilidad. El resto de los atributos varian de arma a arma.

- ###### Largas:
    La descripcion larga justamente detalla las armas largas.

- #### Armas a distancia:
    Como dice su nombre, sirven para atacar a distancia sin acercarse. Dependiendo la sub-categoria a la que pertenecen pueden tener distintos atributos, los comunes son: Daño