using NavigationDJIA.World;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// TODO(alumno):
/// Define el "estado" que usará la Tabla Q para identificar cada situación del agente.
/// 
/// El estado debe contener toda la información necesaria para que el agente pueda
/// tomar decisiones informadas. Tú decides qué características incluir según lo
/// que consideres relevante para resolver el problema.
/// 
/// Ejemplos típicos de información que puede formar un estado:
///   - Posición del agente en la grid.
///   - Posición del otro personaje (enemigo).
///   - Distancia relativa entre agente y enemigo.
///   - Si hay muros en direcciones cercanas.
///   - Cualquier otro dato que consideres útil.
/// 
/// En este ejercicio te damos un ejemplo simple basado únicamente en las posiciones
/// del agente y del oponente. Puedes usarlo tal cual o ampliarlo.
/// 
/// IMPORTANTE: 
///  El estado debe poder convertirse a una clave única (string) mediante ToKey(),
///  ya que esa clave se usará como índice en la TablaQ y en el archivo CSV.
/// </summary>

namespace GrupoT
{
    public sealed class QState
    {
        public int AgentX { get; }
        public int AgentY { get; }
        public int OtherX { get; }
        public int OtherY { get; }

        //Para poder ponerlas en el toKey Sin problema
        public float dist { get; }
        public bool wallUp { get; }
        public bool wallDown { get; }
        public bool wallLeft { get; }
        public bool wallRight { get; }

        //vector relativo
        public int RelX { get; }
        public int RelY { get; }


        public QState(CellInfo agent, CellInfo other, WorldInfo worldInfo)
        {
            AgentX = agent.x;
            AgentY = agent.y;
            OtherX = other.x;
            OtherY = other.y;

            //Distancia a la que se encuentra del jugador
              dist = Mathf.RoundToInt( agent.Distance(other, CellInfo.DistanceType.Euclidean));


            //Obstaculos cercanos al agente
            
            wallUp = worldInfo[agent.x, agent.y + 1].Walkable;
            wallDown = worldInfo[agent.x, agent.y - 1].Walkable; 

            wallLeft = worldInfo[agent.x - 1, agent.y].Walkable;
            wallRight = worldInfo[agent.x + 1, agent.y].Walkable;

            //Hasta el momento el agente solo recibe informacion sobre su distancia con el jugador y si hay obstaculos cercanos a el.
            //Pero no sabe la direccion desde la que viene el jugador, Entonces puede darse el caso de que el agente vaya directo hacia él.

            RelX = Mathf.Clamp(agent.x - other.x, -4, 4);
            RelY = Mathf.Clamp(agent.y - other.y, -4, 4);
        }




        public string ToKey()
        {
            return $"{dist}|{RelX},{RelY}|{wallUp},{wallDown},{wallLeft},{wallRight}"; //Devuelve el estado como una cadena que representa cada atributo
        }
    }
}