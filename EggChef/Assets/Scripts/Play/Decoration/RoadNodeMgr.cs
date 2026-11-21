using System;
using System.Collections.Generic;
using Framework;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Play.Decoration
{
    public class RoadNodeMgr
    {
        public List<RoadNode> startTable = new();
        public List<RoadNode> nodeTable = new();
        public List<RoadNode> endTable = new(); 
        
        private Dictionary<RoadNode, int> startIndex = new();
        private Dictionary<RoadNode, int> endIndex = new();
        
        public Dictionary<(int start,int end), List<RoadNode>> pathTable = new();
        private float minDis = 10f;//间距超过这个值的节点视为不连通

        public RoadNodeMgr(List<RoadNode> nodeList,  List<RoadNode> startNodeList, List<RoadNode> seatList)
        {
            nodeTable = new (nodeList);
            
            foreach (var n in startNodeList)
                if (n && !nodeTable.Contains(n)) nodeTable.Add(n);
            foreach (var n in seatList)
                if (n && !nodeTable.Contains(n)) nodeTable.Add(n);

            startTable = startNodeList;
            endTable = new (seatList);

            Debug.Log($"[RoadNodeMgr] road={nodeList.Count} start={startNodeList.Count} seat={seatList.Count} => nodeTable={nodeTable.Count}");
        }

        public RoadNode GetRandomStartNode()
        {
            return startTable[Random.Range(0, startTable.Count)];
        }

        public SeatData GetRandomSeat()
        {
            return endTable[Random.Range(0, endTable.Count)].GetComponent<SeatData>();
        }

        /// <summary>
        /// A*算法寻路
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns>索引0是起点，索引count-1是终点和seat语义相同</returns>
        public List<RoadNode> FindPath(RoadNode startNode, RoadNode endNode, float distance = 0)
        {
            if (distance > 1)
            {
                minDis = distance;
            }

            int start = 0;
            int end = 0;
            if (!startIndex.TryGetValue(startNode, out start))
            {
                start = nodeTable.IndexOf(startNode);
                if (start < 0 || start >= nodeTable.Count)
                {
                    Debug.LogError($"[FindPath] 起点不在 nodeTable: {startNode?.name}");
                    return new List<RoadNode>();
                }
                startIndex[startNode] = start;
            }
            if (!endIndex.TryGetValue(endNode, out end))
            {
                end = nodeTable.IndexOf(endNode);
                if(end < 0 || end >= nodeTable.Count)
                {
                    Debug.LogError($"[FindPath] 终点不在 nodeTable: {endNode?.name}（座位上那个 RoadNode）");
                    return new List<RoadNode>();
                }
                endIndex[endNode] = end;
            }
            
            if(pathTable.ContainsKey((start,end)))
                return pathTable[(start,end)];
            

            float[] G = new float[nodeTable.Count];
            Array.Fill(G, float.MaxValue);
            G[start] = 0;
            float H(int target) => Vector3.Distance(nodeTable[end].transform.position, nodeTable[target].transform.position);
            
            int[] comeFrom = new int[nodeTable.Count];
            Array.Fill(comeFrom, -1);
            List<RoadNode> path = new();
            bool[] closed = new bool[nodeTable.Count];
            BinaryHeap<int> open = new();
            open.Push(H(start), start);

            while (open.Count != 0)
            {
                int cur = open.Pop();
                if(closed[cur]) 
                    continue;
                closed[cur] = true;

                if (cur == end)
                {
                    while (comeFrom[cur] >= 0)
                    {
                        path.Add(nodeTable[cur]);
                        cur = comeFrom[cur];
                    }
                    path.Add(nodeTable[start]);
                    path.Reverse();
                    pathTable[(start, end)] = path;
                    Debug.Log($"[FindPath] {startNode?.name} -> {endNode?.name} 成功，path={path.Count}");
                    return path;
                }

                for (int i = 0; i < nodeTable.Count; i++)
                {
                    if (closed[i])
                        continue;
                    
                    float dis = Vector3.Distance(nodeTable[cur].transform.position, nodeTable[i].transform.position);
                    if(dis >= minDis)
                        continue;

                    float newG = G[cur] + dis;
                    if (G[i] <= newG)
                        continue;
                    
                    G[i] = newG;
                    comeFrom[i] = cur;
                    open.Push(G[i] + H(i), i);
                }
            }

            Debug.LogWarning($"[FindPath] {startNode?.name} -> {endNode?.name} 不连通（nodeTable={nodeTable.Count}, minDis={minDis}）");
            return path;
        }
    }
}
