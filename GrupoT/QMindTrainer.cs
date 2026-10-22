using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using NUnit.Framework;
using QMind;
using QMind.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GrupoT
{
    public class QMindTrainer : IQMindTrainer
    {

        //Atributo creado para la parte de reward, para penalizar al agente si pasa por las mismas celdas varias veces
        private Queue<CellInfo> ultimasCeldas = new Queue<CellInfo>(); //Cola de las ultimas 5 celdas por las que ha pasado el agente.
       // private int count = 0; //Contador de celdas por las que ha pasado el agente,
        //para saber cuando ha pasado por 5 celdas y empezar a penalizar si vuelve a pasar por alguna de ellas.


        private QMindTrainerParams _params;
        private WorldInfo _worldInfo;
        INavigationAlgorithm _navigationAlgorithm;

        private QTableStorage _qStorage;
        private QTable _qTable;

        private CellInfo _agentPosition;
        private CellInfo _otherPosition;

        private float _return;
        private float _returnAveraged;
        private System.Random _random = new System.Random();

        #region IQMindTrainer implementation

        public CellInfo AgentPosition => _agentPosition;
        public CellInfo OtherPosition => _otherPosition;

        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }

        public float Return => _return;
        public float ReturnAveraged => _returnAveraged;

        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        #endregion

        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            _params = qMindTrainerParams;
            _worldInfo = worldInfo;
            _navigationAlgorithm = navigationAlgorithm;
            _navigationAlgorithm.Initialize(worldInfo);

            _qStorage = new QTableStorage("TablaQ.csv");
            _qTable = new QTable(_qStorage);

            CurrentEpisode = 0;
            StartNewEpisode();
        }

        private void StartNewEpisode()
        {
            ultimasCeldas.Clear(); //Limpiamos la cola de las últimas celdas por las que ha pasado el agente al iniciar un nuevo episodio

            CurrentEpisode++;
            CurrentStep = 0;
            _return = 0f;
            _returnAveraged = 0f;

            _agentPosition = _worldInfo.RandomCell();
            _otherPosition = _worldInfo.RandomCell();

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        private void EndEpisode()
        {
            _qTable.SaveToCsv();

            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);

            if (_params.episodes > 0 && CurrentEpisode >= _params.episodes)
            {
                return;
            }

            StartNewEpisode();
        }

        public void DoStep(bool train)
        {
            // Estado actual del agente
            string stateKey = BuildStateKey(_agentPosition, _otherPosition, _worldInfo);

            // Seleciona la acción a realizar
            QAction action = ChooseAction(stateKey, train);

            // Nuevos estados del agente y del oponente
            CellInfo newAgentPos = ApplyAction(_agentPosition, action);
            CellInfo newOtherPos = MoveOpponent(_otherPosition, newAgentPos.Walkable ? newAgentPos : _agentPosition);
            
            // Nuevo estado del agente
            string nextStateKey = BuildStateKey(newAgentPos, newOtherPos, _worldInfo);
            
            // Calcula la recompensa
            float reward = ComputeReward(newAgentPos, newOtherPos);

            if (train)
            {
                UpdateQ(stateKey, action, reward, nextStateKey);
            }

            // actualiza las posiciones
            _agentPosition = newAgentPos;
            _otherPosition = newOtherPos;

            // Actualizamos estadísticas de recompensas
            CurrentStep++;
            _return += reward;
            _returnAveraged = (_returnAveraged * (CurrentStep - 1) + reward) / CurrentStep;

            // Comprobación de si estamos en el fin de episodio
            if (IsTerminalState(_agentPosition, _otherPosition))
            {
                EndEpisode();
            }
        }

        #region Parte a implementar por el alumno

        private string BuildStateKey(CellInfo agent, CellInfo other, WorldInfo _worldInfo)
        {
            var state = new QState(agent, other, _worldInfo);
            return state.ToKey();
        }

        /// <summary>
        /// Ejemplo orientativo:
        ///    - Si train == false, puedes usar la mejor acción.
        ///    - Si train == true, con probabilidad epsilon elegir acción aleatoria,
        ///      y con probabilidad 1-epsilon la mejor según _qTable.GetBestAction(stateKey).
        /// </summary>
        private QAction ChooseAction(string stateKey, bool train)
        {
            // TODO (alumno):
            // 1. Si !train -> return _qTable.GetBestAction(stateKey);
            // 2. Si train:
            //    - double r = _random.NextDouble();
            //    - si r < _params.epsilon -> acción aleatoria
            //    - si no -> _qTable.GetBestAction(stateKey)

            if (!train)
            {
                return _qTable.GetBestAction(stateKey);
            }
            else
            {
                double r = _random.NextDouble(); //Coge un valor al azar entre 0 y 1
                if (r < _params.epsilon) //Estamos modulando  la estrategia de exploracion con umbral epsilon
                {
                    // Acción aleatoria
                    

//Acciones disponibles: 0,1,2,3,4  --> Es un enum
        //Up,
        //Down,
        //Left,
        //Right,
        //Stay

                    return (QAction)_random.Next(0, 5); //Genera un número aleatorio entre 0 y 4 y lo convierte a QAction
                }
                else
                {
                    return _qTable.GetBestAction(stateKey);
                }
            }
                //throw new NotImplementedException();
        }

        /// <summary>
        /// Actualización de Q-Learning:
        /// Q(s,a) = (1 - alpha) * Q(s,a) + alpha * (reward + gamma * max_a' Q(s',a')).
        /// Usa _qTable.GetQ, _qTable.SetQ y _qTable.GetMaxQ.
        /// </summary>
        private void UpdateQ(string stateKey, QAction action, float reward, string nextStateKey)
        {
            // TODO (alumno):
            // float oldQ = _qTable.GetQ(stateKey, action);
            // float maxQNext = _qTable.GetMaxQ(nextStateKey);
            // float target = reward + _params.gamma * maxQNext;
            // float newQ = (1 - _params.alpha) * oldQ + _params.alpha * target;
            // _qTable.SetQ(stateKey, action, newQ);

            //FASE DE APRENDIZAJE
            float oldQ = _qTable.GetQ(stateKey, action); //Valor de la celda actual
            float maxQNext = _qTable.GetMaxQ(nextStateKey); //Calidad maxima de una accion en el siguiente estado
            float target = reward + _params.gamma * maxQNext;//Fracción de lo que espera conseguir en el futuro por realizar esa accion en ese estado (reward + gamma * maxQNext)
            float newQ = (1 - _params.alpha) * oldQ + _params.alpha * target; //Actualiza la celda Actual con el nuevo valor de calidad.
            _qTable.SetQ(stateKey, action, newQ); //Lo pone en la tabla de calidad o Q-Tabla


            //throw new NotImplementedException();
        }

        /// <summary>
        /// Función de recompensa.
        /// Ejemplo orientativo:
        ///   si agent == other -> recompensa positiva grande (captura)
        ///   si no -> pequeña penalización negativa por cada paso.
        /// </summary>
        /// AHORA CALCULAR LA RECOMPENSA POR EJECUTAR UNA DETERMINADA ACCIÓN EN UN ESTADO DADO.
        private float ComputeReward(CellInfo agent, CellInfo other)
        {
            // TODO (alumno).
            // Ejemplo orientativo:
            // if (agent == other) return 10f;
            // else return -0.01f;

            //Quiero incetivar el movimiento del agente por todo el grid y para ello tambien penalizaré el recorrer las ultimas posiciones
            //varias veces, evitando asi survival loops  (1+1+1+1.... = infinito)

          
           
            if (agent.Equals(other))
            {
                return -100f; //Ha alcanzado al oponente.

            }else if (agent == null || !agent.Walkable) { 
                return -50f;  //Si la posicion a la que ha avanzado no es accesible, penalizamos.
            }
            else
            {
                
                if(ultimasCeldas.Contains(agent) && ultimasCeldas.Count>7)
                {
                    //ya ha recorrido 5 celdas y vuelve a pasar por una de ellas
                    ultimasCeldas.Dequeue(); //Eliminamos la celda más antigua de la cola(osea la primera celda que se guardó)
                    //Porque ya queda suficientemente lejos en el tiempo como para no penalizarlo por ello.
                    return -2f; //Si el agente ha pasado por esa celda recientemente, penalizamos.
                }
                //Es una posición transitable. Guardamos en la cola
                ultimasCeldas.Enqueue(agent);
                
                return 1f + 0.3f * agent.Distance(other, CellInfo.DistanceType.Euclidean); //paso intermedio sin ser pillado, recompensado. Más Recompensa a cuanta más distancia esté
            }

            
        }

        /// <summary>
        /// Condición de final de episodio.
        /// Lo más simple: cuando agente y oponente están en la misma celda.
        /// También puedes definir una probabilidad para el parámetro v visto en clase.
        /// </summary>
      
        //Considero que un estado terminal sería un estado de no retorno, es decir, un estado en el que el agente ha alcanzado al oponente y por lo tanto no puede seguir avanzando 
        //Por qué no considerar que atraviese una pared o salga del grid sería un punto de no retorno?
        private bool IsTerminalState(CellInfo agent, CellInfo other)
        {
            // TODO (alumno):
             return agent.Equals(other); //Si el agente y el oponente están en la misma celda, es un estado terminal
            //throw new NotImplementedException();
        }


        private CellInfo ApplyAction(CellInfo agentCell, QAction action)
        {
            int nx = agentCell.x;;
            int ny = agentCell.y;

            switch (action)
            {
                case QAction.Up:    ny += 1; break;
                case QAction.Down:  ny -= 1; break;
                case QAction.Right: nx += 1; break;
                case QAction.Left:  nx -= 1; break;
                case QAction.Stay:  return agentCell;
            }

            //Vamos a dejar los limites fisicos del movimiento dentro del grid y además aseguraremos de que si tratase de ir hacia esa posicion aunque no le dejemos, fuera negativo
            //Si deseamos que no haya limites fisicos y trabaje solo con recompensa , anotar lo de abajo.

            if (_worldInfo[nx, ny] == null || !_worldInfo[nx, ny].Walkable) //-Probando una cosa //Soluciona el problema de nullReference
            {
                return agentCell; //Si la celda a la que queremos movernos no es transitable, nos quedamos en la misma celda, osea un stay
            }
            else
            {
                return _worldInfo[nx, ny];
            }
            //-Hasta aqui
           // return _worldInfo[nx, ny];
        }


        private CellInfo MoveOpponent(CellInfo opponent, CellInfo target) 
        {
            var path = _navigationAlgorithm.GetPath(opponent, target, 1);
            if (path.Length > 0) //Aqui salta un error
                return path[0];

            return opponent;
        }
        #endregion
    }
}
