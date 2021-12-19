using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;
using System.Linq;

namespace AIAlgorithm.AStar
{
    public class Node
    {
        public int x, y, G, H;
        public Node parentNode;

        public int F
        {
            get { return G + H; }
        }
        public Vector2Int Pos
        {
            get { return new Vector2Int(x, y); }
        }

        public Node(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
    }

    public class AStar : MonoBehaviour
    {
        [Header("Variable")]
        [SerializeField] private float moveDuration = 1;

        [Header("Component")]
        private Grid grid;
        private EntityController entityController;

        private List<Node> nodeList, openList, closedList, finalList;
        private Vector3Int start;
        private Vector2Int startPos, endPos;

        public void SetTargetPosition(Vector3Int start, Vector3Int end, Grid grid, EntityController entityController)
        {
            StopCoroutine("Move");

            this.start = start;
            startPos = (Vector2Int)start;
            endPos = (Vector2Int)end;
            this.grid = grid;
            this.entityController = entityController;

            PathFinding();
        }

        /// <summary>
        /// 오브젝트의 이동처리를 하는 함수
        /// </summary>
        /// <returns></returns>
        private IEnumerator Move()
        {
            bool isMove = false;
            Sequence sequence = null;
            Vector3Int cellPos = start;
            Vector3Int currentPos = entityController.currentPos;

            for (int i = 0; i < finalList.Count; )
            {
                if (!entityController.IsInMoveRange(finalList[i].x, finalList[i].y))
                {
                    i++;
                    continue;
                }

                if (!isMove)
                {
                    sequence = DOTween.Sequence();

                    TileData tileData = MapGenerator.Instance.GetMapData().ground.Get(new Vector2Int(finalList[i].x, finalList[i].y));

                    cellPos = new Vector3Int(finalList[i].x, finalList[i].y, entityController.GetOffsetZ() + MapGenerator.Instance.GetTileHeight(tileData));
                    Vector3 targetPos = grid.CellToWorld(cellPos);

                    sequence.Insert(0, transform.DOMove(targetPos, moveDuration));
                    isMove = true;
                }

                sequence.OnComplete(() =>
                {
                    currentPos = cellPos;
                    isMove = false;
                    i++;
                });

                yield return null;
            }

            entityController.currentPos = currentPos;
            entityController.MoveEnd();
        }

        /// <summary>
        /// 길을 찾는 함수
        /// </summary>
        private void PathFinding()
        {
            nodeList = new List<Node>();
            openList = new List<Node>();
            closedList = new List<Node>();
            finalList = new List<Node>();

            //열린 목록에 시작 노드 추가
            openList.Add(new Node(startPos.x, startPos.y));

            //열린 목록에 아무것도 없다면 길이 없음을 의미
            while (openList.Count > 0)
            {
                Node currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    //열린 목록 중에서 이동 비용이 가장 낮은 노드를 선택
                    if ((openList[i].F <= currentNode.F) && openList[i].H < currentNode.H)
                    {
                        currentNode = openList[i];
                    }
                }
                //선택한 노드를 열린 목록에서 지우고 닫힌 목록에 추가
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode.Pos == endPos)
                {
                    //선택한 노드가 목표 지점과 같다면 시작 지점이 될 때까지 부모 노드로 거슬러 올라감
                    while (currentNode.Pos != startPos)
                    {
                        finalList.Add(currentNode);
                        currentNode = currentNode.parentNode;
                    }
                    finalList.Add(new Node(startPos.x, startPos.y));
                    //거꾸로 추가 되었기 때문에 Reverse함수로 뒤집어 줌
                    finalList.Reverse();

                    StartCoroutine("Move");
                    return;
                }

                //인접한 노드 중 갈 수 있는 노드를 찾는 과정
                AddNodeToOpenList(currentNode, GetOrCreateNode(currentNode.x + 1, currentNode.y));
                AddNodeToOpenList(currentNode, GetOrCreateNode(currentNode.x - 1, currentNode.y));
                AddNodeToOpenList(currentNode, GetOrCreateNode(currentNode.x, currentNode.y + 1));
                AddNodeToOpenList(currentNode, GetOrCreateNode(currentNode.x, currentNode.y - 1));

                if (openList.Count == 0)
                {
                    currentNode = closedList.Where(x => x.H != 0).OrderBy(x => x.H).First();

                    //선택한 노드가 목표 지점과 같다면 시작 지점이 될 때까지 부모 노드로 거슬러 올라감
                    while (currentNode.Pos != startPos)
                    {
                        finalList.Add(currentNode);
                        currentNode = currentNode.parentNode;
                    }
                    finalList.Add(new Node(startPos.x, startPos.y));
                    //거꾸로 추가 되었기 때문에 Reverse함수로 뒤집어 줌
                    finalList.Reverse();

                    StartCoroutine("Move");
                    return;
                }
            }
        }

        private Node GetOrCreateNode(int x, int y)
        {
            Node node = nodeList.FirstOrDefault(n => n.x == x && n.y == y);
            if (node == null)
            {
                node = new Node(x, y);
                nodeList.Add(node);
            }
            return node;
        }

        /// <summary>
        /// 노드 상태를 확인하고 열린 목록에 추가하는 함수
        /// </summary>
        /// <param name="currentNode">현재 노드</param>
        /// <param name="newNode">인접 노드</param>
        private void AddNodeToOpenList(Node currentNode, Node newNode)
        {
            //벽인지 검사
            if (!MapGenerator.Instance.IsMovableTile(new Vector3Int(newNode.x, newNode.y, 0))) return;
            //벽에 인접한 노드를 대각선으로 가로지르는지 검사
            if (!MapGenerator.Instance.IsMovableTile(new Vector3Int(newNode.x, newNode.y, 0)) || !MapGenerator.Instance.IsMovableTile(new Vector3Int(newNode.x, newNode.y, 0))) return;
            //닫힌 목록에 이미 존재하는지 검사
            if (closedList.Contains(newNode)) return;

            int G;
            //직선
            if (currentNode.x - newNode.x == 0 || currentNode.y - newNode.y == 0)
            {
                G = currentNode.G + 10;
            }
            //대각선
            else
            {
                G = currentNode.G + 14;
            }

            //이동 비용이 더 낫거나 열린 목록에 없다면 이동 비용을 계산하고 부모 노드를 현재 노드로 지정
            if (G < newNode.G || !openList.Contains(newNode))
            {
                newNode.G = G;

                newNode.H = (Mathf.Abs(newNode.x - endPos.x) + Mathf.Abs(newNode.y - endPos.y)) * 10;
                newNode.parentNode = currentNode;

                openList.Add(newNode);
            }
        }
    }
}