using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SchoolBusRoute
{
    class Graph
    {
        private int garage;
        private int school;
        private int size;
        private List<List<GraphNode>> adjList;
        public void ReadFromFile()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\in.txt");
            StreamReader reader = new StreamReader(path);
            adjList = new List<List<GraphNode>>();
            List<string> lines = new List<string>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
            //extrag nodul de start si scoala
            garage = 0;
            string[] strArr = lines[0].Split(' ');
            int nodesNumb = int.Parse(strArr[0]);
            school = nodesNumb - 1;
            //construiesc lista de adiacenta
            for(short i = 0; i < nodesNumb; i++)
            {
                adjList.Add(new List<GraphNode>());
            }
            for (int i = 1; i < lines.Count; i++)
            {
                line = lines[i];
                strArr = line.Split(' ');
                int[] arr = new int[3];
                for(int j=0; j<3; j++)
                {
                    arr[j] = int.Parse(strArr[j]);
                }
                adjList[arr[0]-1].Add(new GraphNode(arr[1]-1, arr[2]));
                adjList[arr[1] - 1].Add(new GraphNode(arr[0] - 1, arr[2]));
            }
            //daca nu exista muchie intre garaj si scoala
            if (!adjList[garage].Any(n => n.Value == school))
                throw new Exception("There is no edge between from school to garage!");
            size = adjList.Count;
        }
        public void GenerateGraph(int nodesNumber, int start, int stop, int maxEdgeCost)
        {
            garage = start - 1;
            school = stop - 1;
            adjList = new List<List<GraphNode>>(nodesNumber);
            Random edgeRand = new Random();
            Random costGenerator = new Random();
            for (int i = 0; i<nodesNumber;i++)
            {
                adjList.Add(new List<GraphNode>());
            }
            for (int i = 0; i<nodesNumber - 1; i++)
            {

                for (int j = i+1; j < nodesNumber; j++)
                {
                    if(i == garage && j == school)
                    {
                        int cost = costGenerator.Next(maxEdgeCost)+1;
                        adjList[i].Add(new GraphNode(j, cost));
                        adjList[j].Add(new GraphNode(i, cost));
                        continue;
                    }
                    if(edgeRand.Next(10) >= 2)
                    {
                        int cost = costGenerator.Next(maxEdgeCost)+1;
                        adjList[i].Add(new GraphNode(j, cost));
                        adjList[j].Add(new GraphNode(i, cost));
                    }
                }
            }
            size = adjList.Count;
        }
        /// <summary>
        /// Finds the minimum route through backtracking. The "garage" will appear only once but the cost will include
        /// the edge [school, garage]
        /// </summary>
        /// <param name="minCost">Will store the route's cost </param>
        /// <param name="ts">Stores the amount of time it took to find the minimum cost route.</param>
        /// <returns></returns>
        public int[] FindMinRouteBacktracking(out int minCost, out TimeSpan ts)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            minCost = int.MaxValue;
            int[] solution = new int[size];
            int[] permutation = new int[size];
            solution[0] = garage;
            solution[size - 1] = school;
            if(size == 2)
            {
                minCost = adjList[0].Find(n => n.Value == school).Cost;
                minCost += minCost;
                stopwatch.Stop();
                ts = stopwatch.Elapsed;
                return solution;
            }
            permutation[0] = garage;
            permutation[size - 1] = school;
            permutation[1] = -1;
            int k = 1;
            while(k > 0)
            {
                bool found = false;
                while(permutation[k] < size - 1 && found == false)
                {
                    permutation[k]++;
                    if(permutation[k] != garage && permutation[k] != school)
                    {
                        bool areEqual = false;
                        for (int i = 1; i < k; i++)
                        {
                            if(permutation[i] == permutation[k])
                            {
                                areEqual = true;
                                break;
                            }
                        }
                        if (areEqual == false)
                        {
                            if (adjList[permutation[k-1]].Contains(new GraphNode(permutation[k])))
                            {
                                found = true;
                            }
                            if(k == size - 2)
                            {
                                if(adjList[permutation[k]].Contains(new GraphNode(permutation[school]))==false)
                                {
                                    found = false;
                                }
                            }
                        }
                    }
                }
                if(found == true)
                {
                    if(k == size - 2)
                    {
                        int cycleCost = getCycleCost(permutation);
                        if (minCost > cycleCost)
                        {
                            permutation.CopyTo(solution, 0);
                            minCost = cycleCost;
                        }
                    }
                    else
                    {
                        k++;
                        permutation[k] = -1;
                    }
                }
                else
                {
                    k--;
                }
            }
            stopwatch.Stop();
            ts = stopwatch.Elapsed;
            return solution;
        }
        public int[] IteratedHillClimber(out int cost, out TimeSpan ts)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            double maxIterations = 0;
            if (size == 4)
                maxIterations = 1;
            else if (size > 4 && size <= 7)
                maxIterations = Math.Pow((size - 2), 2);
            else if (size >= 8)
                maxIterations = Math.Pow(size, 2);

            double t = 0;
            bool end = false;
            int[] bestRoute = new int[size];
            bestRoute[0] = garage;
            bestRoute[size - 1] = school;
            int bestCost = findFirstRouteBacktracking(bestRoute, 1);
            if(size==3 || bestCost == int.MaxValue)
            {
                cost = bestCost;
                stopwatch.Stop();
                ts = stopwatch.Elapsed;
                return bestRoute;
            }
  
            do
            {
                bool local = false;
                int[] currentRoute = new int[size];
                currentRoute[0] = garage;
                currentRoute[size - 1] = school;
                int steps = (size - 2) / 2;
                Random rand = new Random();
                int k = 0;//index for currentRoute
                List<int> unvisitedNodes = new List<int>();
                int currCost = int.MaxValue;
                //generate random neigbours(routes)
                do
                {
                    k = 0;
                    for (int i = 0; i < size; i++)
                    {
                        if (i == garage || i == school)
                            continue;
                        unvisitedNodes.Add(i);
                    }
                    do
                    {
                        int x = currentRoute[k];
                        int i = rand.Next(unvisitedNodes.Count);
                        int y = unvisitedNodes[i];
                        if (adjList[x].Contains(new GraphNode(y)))
                        {
                            k++;
                            currentRoute[k] = y;
                            unvisitedNodes.RemoveAt(i);
                        }
                        else
                            unvisitedNodes.RemoveAt(i);
                    } while (k < steps && unvisitedNodes.Count > 0);

                    if (unvisitedNodes.Count == 0)
                    {
                        if (k == 1)
                        {
                            end = true;
                            break;
                        }
                        currentRoute[k] = 0;
                        k--;
                        currentRoute[k] = 0;
                    }
                    currCost = findFirstRouteBacktracking(currentRoute, k);
                    if (currCost == int.MaxValue)
                        unvisitedNodes.Clear();
                } while (currCost==int.MaxValue);

                if (end)
                    break;

                do
                {   //create and select the neighbors with the minimum cost
                    int[] x = new int[size];
                    int startIndex = k+1;
                    int xCost = int.MaxValue;
                    for(int i=startIndex; i<=size-2; i++)
                    {
                        int[] neighbour = new int[size];
                        currentRoute.CopyTo(neighbour, 0);
                        int pathCost = findFirstRouteBacktracking(neighbour, i);
                        if(pathCost < xCost)
                        {
                            x = neighbour;
                            xCost = pathCost;
                        }
                    }
                    if (currCost > xCost)
                    {
                        currentRoute = x;
                        currCost = xCost;
                    }
                    else
                        local = true;
                } while (!local);
                t++;
                if(bestCost > currCost)
                {
                    bestCost = currCost;
                    bestRoute = currentRoute;
                }

            } while (t < maxIterations && end==false);

            stopwatch.Stop();
            ts = stopwatch.Elapsed;
            cost = bestCost;
            return bestRoute;
        }
       
        private int getCycleCost(int[] permutation)
        {
            int cost = 0;
            for(int i = 0; i < size - 1; i++)
            {
                GraphNode y = adjList[permutation[i]].Find(n => n.Value == permutation[i + 1]);
                if (y == null)
                    return int.MaxValue;

                cost += y.Cost;
            }
            if (!adjList[permutation[school]].Contains(new GraphNode(garage)))
                return int.MaxValue;
            cost += adjList[school].Find(n => n.Value == garage).Cost;
            return cost;
        }
        /// <summary>
        /// Finds a valid route throug backtracking.
        /// </summary>
        /// <param name="path">The array that stores the path.</param>
        /// <param name="start">The starting node.</param>
        /// <returns>The route's cost.</returns>
        private int findFirstRouteBacktracking(int[] path, int start)
        {
            int minCost = int.MaxValue;
            if (size == 2)
            {
                minCost = adjList[0].Find(n => n.Value == school).Cost;
                minCost += minCost;
                return minCost;
            }
            int k = start;
            while (k >= start)
            {
                bool found = false;
                while (path[k] < size - 1 && found == false)
                {
                    path[k]++;
                    if (path[k] != garage && path[k] != school)
                    {
                        bool areEqual = false;
                        for (int i = 1; i < k; i++)
                        {
                            if (path[i] == path[k])
                            {
                                areEqual = true;
                                break;
                            }
                        }
                        if (areEqual == false)
                        {
                            if (adjList[path[k - 1]].Contains(new GraphNode(path[k])))
                            {
                                found = true;
                            }
                            if (k == size - 2)
                            {
                                if (adjList[path[k]].Contains(new GraphNode(path[school])) == false)
                                {
                                    found = false;
                                }
                            }
                        }
                    }
                }
                if (found == true)
                {
                    if (k == size - 2)
                    {
                        int cycleCost = getCycleCost(path);
                        if (minCost > cycleCost)
                        {
                            minCost = cycleCost;
                            return minCost;
                        }
                    }
                    else
                    {
                        k++;
                        path[k] = -1;
                    }
                }
                else
                {
                    k--;
                }
            }
            return minCost;
        }
    }
}
