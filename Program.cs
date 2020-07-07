using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SchoolBusRoute
{
    class Program
    {
        static void Main(string[] args)
        {
            string answer = "n";
            do
            {
                Graph graph = new Graph();
                Start:
                Console.WriteLine("Press 1 for reading data from a file.");
                Console.WriteLine("Press 2 to generate a random graph.");
                int opt;
                bool result = int.TryParse(Console.ReadLine(), out opt);
                if(!result)
                {
                    Console.WriteLine("Please insert only numbers!");
                    goto Start;
                }
                if(opt == 1)
                {
                    try
                    {
                        graph.ReadFromFile();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        goto End;
                    }
                }
                else if(opt == 2)
                {
                    GetGraphData:
                    try
                    {
                        int nodes, garage, school, maxEdgeCost;
                        Console.Write("Nodes number: ");
                        nodes = int.Parse(Console.ReadLine());
                        Console.Write("Garage: ");
                        garage = int.Parse(Console.ReadLine());
                        Console.Write("School: ");
                        school = int.Parse(Console.ReadLine());
                        Console.Write("Maximum edge cost: ");
                        maxEdgeCost = int.Parse(Console.ReadLine());
                        graph.GenerateGraph(nodes, garage, school, maxEdgeCost);
                    }
                    catch(FormatException e)
                    {
                        Console.WriteLine(e.Message);
                        goto GetGraphData;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Option!");
                    goto Start;
                }
                AlgOpt:
                Console.WriteLine("Press 1 for backtracking.");
                Console.WriteLine("Press 2 for Iterated Hill-Climber algorithm.");
                result = int.TryParse(Console.ReadLine(), out opt);
                if(!result)
                {
                    Console.WriteLine("Insert only numbers!");
                    goto AlgOpt;
                }
                if(opt == 1)
                {
                    bruteForce(graph);
                }
                else if(opt == 2)
                {
                    iteratedHC(graph);
                }
                else
                {
                    Console.WriteLine("Invalid Option!");
                    goto AlgOpt;
                }
                Console.WriteLine("Do you wish to try another algorithm? (y/n): ");
                answer = Console.ReadLine();
                if (answer.Equals("y"))
                    goto AlgOpt;
                End:
                Console.WriteLine("Do you wish to try again? (y/n): ");
                answer = Console.ReadLine();
            } while (answer.Equals("y"));
        }
        private static void bruteForce(Graph graph)
        {
            TimeSpan timeSpan;
            Console.WriteLine("\nSearching the minimum cost route through backtracking...");
            int cost;
            int[] minCycle = graph.FindMinRouteBacktracking(out cost, out timeSpan);
            //outputData(cost, minCycle.ToList());
            printData(cost, minCycle, timeSpan);
           // outputDataToFile(cost, minCycle.ToList());
        }
        private static void iteratedHC(Graph graph)
        {
            TimeSpan timeSpan;
            Console.WriteLine("\nSearching the minimum cost route through Iterated Hill-Climber algorithm...");
            int cost;
            int[] minCycle = graph.IteratedHillClimber(out cost, out timeSpan);
            //outputData(cost, minCycle.ToList());
            printData(cost, minCycle, timeSpan);
           // outputDataToFile(cost, minCycle.ToList()) ;
        }
        static void outputDataToFile(int cost, List<int> cycle)
        {
            string path = @"C:\sbOut.txt";
            using (StreamWriter stream = new StreamWriter(path))
            {
                if (cost == int.MaxValue)
                {
                    stream.WriteLine("-1");
                    Console.WriteLine("Done!");
                }
                else
                {
                    stream.WriteLine(cost);
                    foreach (int node in cycle)
                    {
                        stream.Write(node + ", ");
                    }
                }
            }
            Console.WriteLine("Done!");
        }
        static void printData(int cost, int[] path, TimeSpan ts)
        {
            if(cost == int.MaxValue)
            {
                Console.WriteLine("-1");
                return;
            }
            Console.WriteLine("Cost: " + cost);
            Console.Write("Route: ");
            for (int i = 0; i < path.Length; i++)
            {
                path[i]++;
                Console.Write(" " + path[i]);
            }
            Console.WriteLine("\nTime: "+ts);
        }
    }
}
