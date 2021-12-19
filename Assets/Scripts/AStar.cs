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
        /// ������Ʈ�� �̵�ó���� �ϴ� �Լ�
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
        /// ���� ã�� �Լ�
        /// </summary>
        private void PathFinding()
        {
            nodeList = new List<Node>();
            openList = new List<Node>();
            closedList = new List<Node>();
            finalList = new List<Node>();

            //���� ��Ͽ� ���� ��� �߰�
            openList.Add(new Node(startPos.x, startPos.y));

            //���� ��Ͽ� �ƹ��͵� ���ٸ� ���� ������ �ǹ�
            while (openList.Count > 0)
            {
                Node currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    //���� ��� �߿��� �̵� ����� ���� ���� ��带 ����
                    if ((openList[i].F <= currentNode.F) && openList[i].H < currentNode.H)
                    {
                        currentNode = openList[i];
                    }
                }
                //������ ��带 ���� ��Ͽ��� ����� ���� ��Ͽ� �߰�
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode.Pos == endPos)
                {
                    //������ ��尡 ��ǥ ������ ���ٸ� ���� ������ �� ������ �θ� ���� �Ž��� �ö�
                    while (currentNode.Pos != startPos)
                    {
                        finalList.Add(currentNode);
                        currentNode = currentNode.parentNode;
                    }
                    finalList.Add(new Node(startPos.x, startPos.y));
                    //�Ųٷ� �߰� �Ǿ��� ������ Reverse�Լ��� ������ ��
                    finalList.Reverse();

                    StartCoroutine("Move");
                    return;
                }

                //������ ��� �� �� �� �ִ� ��带 ã�� ����
                AddNodeToOpenList(currentNode, GetOrCreateNode(currentNode.x + 1, currentNode.y));
                AddNodeToOpenList(currentNode, GetOrCreateNode(currentNode.x - 1, currentNode.y));
                AddNodeToOpenList(currentNode, GetOrCreateNode(currentNode.x, currentNode.y + 1));
                AddNodeToOpenList(currentNode, GetOrCreateNode(currentNode.x, currentNode.y - 1));

                if (openList.Count == 0)
                {
                    currentNode = closedList.Where(x => x.H != 0).OrderBy(x => x.H).First();

                    //������ ��尡 ��ǥ ������ ���ٸ� ���� ������ �� ������ �θ� ���� �Ž��� �ö�
                    while (currentNode.Pos != startPos)
                    {
                        finalList.Add(currentNode);
                        currentNode = currentNode.parentNode;
                    }
                    finalList.Add(new Node(startPos.x, startPos.y));
                    //�Ųٷ� �߰� �Ǿ��� ������ Reverse�Լ��� ������ ��
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
        /// ��� ���¸� Ȯ���ϰ� ���� ��Ͽ� �߰��ϴ� �Լ�
        /// </summary>
        /// <param name="currentNode">���� ���</param>
        /// <param name="newNode">���� ���</param>
        private void AddNodeToOpenList(Node currentNode, Node newNode)
        {
            //������ �˻�
            if (!MapGenerator.Instance.IsMovableTile(new Vector3Int(newNode.x, newNode.y, 0))) return;
            //���� ������ ��带 �밢������ ������������ �˻�
            if (!MapGenerator.Instance.IsMovableTile(new Vector3Int(newNode.x, newNode.y, 0)) || !MapGenerator.Instance.IsMovableTile(new Vector3Int(newNode.x, newNode.y, 0))) return;
            //���� ��Ͽ� �̹� �����ϴ��� �˻�
            if (closedList.Contains(newNode)) return;

            int G;
            //����
            if (currentNode.x - newNode.x == 0 || currentNode.y - newNode.y == 0)
            {
                G = currentNode.G + 10;
            }
            //�밢��
            else
            {
                G = currentNode.G + 14;
            }

            //�̵� ����� �� ���ų� ���� ��Ͽ� ���ٸ� �̵� ����� ����ϰ� �θ� ��带 ���� ���� ����
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