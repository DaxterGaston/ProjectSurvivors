# Project Survivors

## Definiciones sobre la generacion aleatoria del mapa.

El mapamundi va a ser una isla. La idea principal para generar re-jugabilidad es que el mapa se genere aleatoriamente, dentro de un entorno no tan aleatorio.

### Caracteristicas principales sobre la generacion del mapa

- El mapa se va a componer dependiendo lo que se genera aleatoriamente con un script, este script genera un Grafo aleatorio, con nodos que determinan en que posicion del mapa va cada cosa. Esto lo podemos ver en Assets/Code/Scripts/Map. El [script](../Assets/Code/Scripts/Map/RandomMapGenerator.cs) tiene adentro la logica de generacion aleatoria. El [script base](../Assets/Code/Scripts/Map/BaseNode.cs) contiene los datos necesarios para cualquier nodo.
- Nodos: 
    Representan secciones del mapa. Un nodo podria referenciar cualquier parte de un mapa, y definir cierto comportamiento dentro de esa seccion del mapa, se detalla en [nodos](docs/Nodos.md). Se separan en 3 grupos: 
        - Naturaleza: Bosque - Lago - Pantano
        - Urbano: Una manzana (la definicion de manzana dentro del entorno de Urbanismo, no la fruta) - La propia ciudad - Edificios, estos se separan en 3 grupos:
            - Generales del mapa: Por ejemplo, una parte de la ruta que lleva de una ciudad a otra - Un bloqueo de la policia en la ruta - Un campamento en un bosque.
            - Normales: Son los edificios mas comunes, casas, un restaurante, un taller.
            - Importantes: Son edificios con mayor relevancia en cuanto a el loot que se puede obtener: Estaciones de policia (armas), Tiendas de camping (equipamiento y herramientas) y otros edificios similares.
            - Clave: Son edificios donde podemos encontrar objetos claves para avanzar en la mision principal o encontrar algun superviviente (no supervivientes comunes, de los de la historia principal, esta detallado en la [documentacion](docs/NPCs.md))
- Inicialmente el mapa va a contar con 3 ciudades.