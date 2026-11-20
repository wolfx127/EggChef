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
            startTable = startNodeList;
            endTable = new (seatList);
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
        public List<RoadNode> FindPath(RoadNode startNode, RoadNode endNode)
        {
            int start = 0;
            int end = 0;
            if (!startIndex.TryGetValue(startNode, out start))
            {
                start = nodeTable.IndexOf(startNode);
                if (start < 0 || start >= nodeTable.Count) return new List<RoadNode>();
                startIndex[startNode] = start;
            }
            if (!endIndex.TryGetValue(endNode, out end))
            {
                end = nodeTable.IndexOf(endNode);
                if(end < 0 || end >= nodeTable.Count) return new List<RoadNode>();
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

            return path;
        }
    }
}
