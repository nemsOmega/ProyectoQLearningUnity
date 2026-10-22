using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;

namespace GrupoT
{
    public class QMindTester : IQMind
    {
        private WorldInfo _worldInfo;
        private QTableStorage _qStorage;
        private QTable _qTable;

        public void Initialize(WorldInfo worldInfo)
        {
            _worldInfo = worldInfo;

            _qStorage = new QTableStorage("TablaQ.csv");
            _qTable = new QTable(_qStorage);
        }

        public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
        {
            string stateKey = BuildStateKey(currentPosition, otherPosition, _worldInfo);

            QAction bestAction = _qTable.GetBestAction(stateKey);

            CellInfo nextPosition = ApplyAction(currentPosition, bestAction);

            return nextPosition;
        }

        private string BuildStateKey(CellInfo agent, CellInfo other, WorldInfo _worldInfo)
        {
            var state = new QState(agent, other, _worldInfo);
            return state.ToKey();
        }

        private CellInfo ApplyAction(CellInfo agentCell, QAction action)  //Versión con límites
        {
            CellInfo nextCell = agentCell;
            switch (action)
            {
                case QAction.Up:
                    nextCell = _worldInfo[agentCell.x, agentCell.y + 1];
                    break;
                case QAction.Down:
                    nextCell = _worldInfo[agentCell.x, agentCell.y - 1];
                    break;
                case QAction.Right:
                    nextCell = _worldInfo[agentCell.x + 1, agentCell.y];
                    break;
                case QAction.Left:
                    nextCell = _worldInfo[agentCell.x - 1, agentCell.y];
                    break;
                default:
                    nextCell = _worldInfo[agentCell.x, agentCell.y];
                    break;
            }


            //Vamos a dejar los limites fisicos del movimiento dentro del grid y además aseguraremos de que si tratase de ir hacia esa posicion aunque no le dejemos, fuera negativo
            //Si deseamos que no haya limites fisicos y trabaje solo con recompensa , anotar lo de abajo.
            if (nextCell == null || !nextCell.Walkable)
            {
                return agentCell;
            }

            return nextCell;
        }
    }
}