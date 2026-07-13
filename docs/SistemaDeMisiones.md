# Project Survivors

## Caracteristicas del Sistema de Misiones

Se explican las caracteristicas del Sistema de Misiones, este sistema forma parte del [sistema de historia](/docs/SistemaHistoria.md). Si bien va a tener su propia logica, ambos sistemas funcionan en conjunto, dependiente uno del otro.

- ### Misiones Primarias:
    Son las misiones que dan curso a la historia. Las misiones tienen un numero de orden y cada una depende de su predecesora, en el caso de las misiones que comparten un mismo numero de orden son misiones bloqueantes entre sí. Es decir, cuando se desbloquea ese bloque de misiones el jugador debera decidir cual es la que va a hacer. Estas misiones siempre van a tener un impacto en la historia principal, así como también pueden hacerle un aporte al jugador. _Ejemplo_: desbloquear una habilidad o darle algun item.

- ### Misiones Secundarias: 
    Son misiones opcionales. El jugador puede o no hacerlas. Siempre va a complementar la historia o a hacerle un aporte al jugador, igual que las misiones primarias.
    Estas pueden o no estar bloqueadas por otras misiones (pueden ser primarias o secundarias), a diferencia de las primarias pueden depender de otros factores para que la mision se habiliten. _Ejemplo_: Tener cierto nivel en algun atributo/Llevar + de X horas de partida/Encontrar cierto item especial.

- ### Misiones de Alianza:
    Son solicitadas por otras comunidades con las que tengamos un índice de relacion de -1 o superior. 