# Project Survivors

## Caracteristicas del sistema de Salud

### Barra de Salud:
    Es la caracteristica escencial del sistema. Si llega a 0, el personaje muere y vuelve a cargar la partida desde su ultimo punto de guardado.

### Regeneracion [_*Moddeable*_]: 
    La barra de salud regenera su valor actual de forma pasiva, cada X tiempo se regenera X porcentaje de la salud máxima. Esto varia dependiendo de ciertos factores:

- #### Heridas: 
    Reducen porcentualmente la regeneración de salud.

- #### Sed y Hambre:
    Por separado, poseen umbrales que van a determinar la capacidad de regeneracion de salud. Existe un umbral en el que anulan completamente la capacidad de un personaje para regenerar su salud de forma pasíva. Tambien tienen umbrales en los que la regeneracion excede el estandar y mejora la tasa de tiempo y % del maximo.

- #### Consumibles:
    Items que mejoran los tiempos y % de regeneracion.

### Heridas [_*Moddeable*_]:
    Existe un listado de posibles heridas que puede sufrir un personaje, estas poseen distintas caracteristicas, pero siempre abarcadas dentro de los siguientes grupos:
    
- #### Efecto negativo principal: 
    Existe un unico efecto negativo que se comparte entre todas las heridas indefectiblemente: Reduce un % de la capacidad de salud maxima y la capacidad de regeneracion. El valor maximo ya obtenido se mantiene, pero el maximo regenerable se reduce hasta que la herida se cure/sea tratada. 
- #### Efectos negativos secundarios: 
    Depende de la herida, pueden ser otros efectos negativos sumados a ese. _Ejemplos_: Reduccion de daño/resistencia/UMS/velocidad de movimiento.
- #### Curacion:
    Tienen un tiempo estimado de curacion. Por tiempo se agrega valor a la barra de curacion de la herida, una vez que se llega al valor necesario para la curación la herida desaparece. Este índice puede variar con los siguientes factores:

    - ##### Sed y Hambre:
        Tiene umbrales igual que en el caso de la [regeneración](#regeneracion-moddeable).
    - ##### Equipables:
        Existen items que, al equiparse sobre la herida, mejoran su tasa de curacion. _Ejemplo_: Vendas.
    - ##### Consumibles:
        Items que aumentan la tasa de curación por X tiempo. _Ejemplo_: Crema cicatrizante.
