using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AI_Assignment
{
    class KCluster
    {
        private static class Globals
        {
            public static XmlData XmlData = new XmlData();
        }

        //contains data from the XML file
        private class XmlData
        {
            private XmlDocument doc;
            private int numberOfNodes;
            private int numberOfCentroids;
            private int width;
            private int height;
            private int threshold;

            public int NumberOfNodes { get { return numberOfNodes; } }
            public int NumberOfCentroids { get { return numberOfCentroids; } }
            public int Width { get { return width; } }
            public int Height { get { return height; } }
            public int Threshold { get { return threshold; } }

            public XmlData()
            {
                doc = new XmlDocument();
                doc.Load("KClusterSettings.xml");
                try
                {
                    numberOfNodes = Convert.ToInt32(doc.SelectSingleNode("/root/NumberOfNodes").InnerText);
                    numberOfCentroids = Convert.ToInt32(doc.SelectSingleNode("/root/NumberOfCentroids").InnerText);
                    width = Convert.ToInt32(doc.SelectSingleNode("/root/Width").InnerText);
                    height = Convert.ToInt32(doc.SelectSingleNode("/root/Height").InnerText);
                    threshold = Convert.ToInt32(doc.SelectSingleNode("/root/Threshold").InnerText);
                }
                catch
                {
                    Console.WriteLine("Inproper XML input, press enter to exit.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }

            public void PrintXMLData()
            {
                Console.WriteLine("Number of nodes: {0}, number of centroids: {1}, Width: {2}, Height: {3}", NumberOfNodes, NumberOfCentroids, Width, Height);
            }
        }

        private class Node
        {
            public int xpos, ypos;      //Position on the graph
            public Centroid centroid;   //The centroid this node belongs to

            public Node(int x, int y)
            {
                xpos = x;
                ypos = y;
            }

            //Used to find the centroid this node belongs to
            public void AssignCentroid(List<Centroid> newCentroids)
            {
                centroid = null;
                double distanceToAssignedCentroid = 10000;

                //Loop through all centroids
                foreach (Centroid NewCentroid in newCentroids)
                {
                    //calculate distance to new centrodi
                    double distanceToNewCentroid = Math.Sqrt(Math.Pow(NewCentroid.Xpos - xpos, 2) + Math.Pow(NewCentroid.Ypos - ypos, 2));

                    if (centroid == null && distanceToNewCentroid <= Globals.XmlData.Threshold)
                    {
                        //This node is not assigned to a centroid, assign it to one if is within the threshold
                        centroid = NewCentroid;
                        centroid.nodes.Add(this);
                        //keep track of distance to assigned centroid
                        distanceToAssignedCentroid = Math.Sqrt(Math.Pow(centroid.Xpos - xpos, 2) + Math.Pow(centroid.Ypos - ypos, 2));
                    }
                    else if (centroid != null && distanceToNewCentroid <= distanceToAssignedCentroid && distanceToNewCentroid <= Globals.XmlData.Threshold)
                    {
                        //The new centroid is closer than the old centroid, remove this node from old and assign to new
                        centroid.nodes.Remove(this);
                        centroid = NewCentroid;
                        centroid.nodes.Add(this);
                        distanceToAssignedCentroid = Math.Sqrt(Math.Pow(centroid.Xpos - xpos, 2) + Math.Pow(centroid.Ypos - ypos, 2));
                    }
                }
            }
        }

        private class Centroid
        {
            public List<Node> nodes;    //Nodes that belong to this centroid
            private double xpos, ypos;
            public double Xpos
            {
                get { return Math.Round(xpos, 3); }
                set { xpos = value; }
            }
            public double Ypos
            {
                get { return Math.Round(ypos, 3); }
                set { ypos = value; }
            }

            //Initalizer
            public Centroid(int x, int y)
            {
                nodes = new List<Node>();
                Xpos = x;
                Ypos = y;
            }

            //This repositions this centroids, by calculating the average x and y values that belong to this centroid
            //Returns ture if the position changed, else false
            public bool Reposition()
            {
                if (nodes.Count == 0)
                    return false;

                double averageX = 0, averageY = 0;
                int count = 0;
                foreach(Node node in nodes)
                {
                    averageX += node.xpos;
                    averageY += node.ypos;
                    count++;
                }

                //Check if this node will change position
                if (Xpos == Math.Round(averageX / count, 3) && Ypos == Math.Round(averageY / count, 3))
                {
                    return false;
                }
                
                Xpos = Math.Round(averageX / count, 3);
                Ypos = Math.Round(averageY / count, 3);

                return true;
            }
        }

        private class Graph
        {
            int width, heigth, numberOfNodes, numberOfCentroids;
            List<Node> nodes;           //Nodes on the graph
            List<Centroid> centroids;   //Centroids on the graph

            public Graph()
            {
                //Initalize data
                width = Globals.XmlData.Width;
                heigth = Globals.XmlData.Height;
                numberOfNodes = Globals.XmlData.NumberOfNodes;
                numberOfCentroids = Globals.XmlData.NumberOfCentroids;
                nodes = new List<Node>();
                centroids = new List<Centroid>();

                //randomly generate our nodes
                Random rand = new Random();
                for (int i = 0; i < numberOfNodes; i++)
                {
                    //Set the new node to a random node within the grid
                    nodes.Add(new Node(rand.Next(width + 1), rand.Next(heigth + 1)));
                }

                //randomly generate our centroids
                for (int i = 0; i < numberOfCentroids; i++)
                {
                    //Set the new centroid to a random node within the grid
                    centroids.Add(new Centroid(rand.Next(width + 1), rand.Next(heigth + 1)));
                }
            }

            //Calculates and repositions centroids, untill no changes are made
            public void SolveKCluster()
            {
                bool changed = true;
                while (changed)
                {
                    changed = false;

                    //Calculate centroid that every node belongs to
                    foreach (Node node in nodes)
                    {
                        node.AssignCentroid(centroids);
                    }

                    Print();    //Print before repositioning the centroids

                    //Reposition the centroids. As soon as changed = true, we will need another round
                    foreach (Centroid centroid in centroids)
                    {
                        if (changed == false)
                        {
                            changed = centroid.Reposition();
                        }
                        else
                        {
                            centroid.Reposition();
                        }
                    }

                    //We will need to recalculate distances, remove nodes from centroid lists so we can start over
                    if (changed)    //If nothing changed, skip this step
                    {
                        foreach (Centroid centroid in centroids)
                        {
                            centroid.nodes = new List<Node>();
                        }
                    }
                }

                Print();    //Print the final result
            }
            
            public void Print()
            {
                //Print nodes that belong to a centroid
                foreach (Centroid centroid in centroids)
                {
                    Console.WriteLine("The nodes that belong to the centroid {0}, {1} are:", centroid.Xpos, centroid.Ypos);
                    foreach(Node node in centroid.nodes)
                    {
                        Console.WriteLine("{0}, {1}", node.xpos, node.ypos);
                    }
                    Console.WriteLine("");  //Make a line break
                }

                //Print free nodes
                Console.WriteLine("The outlier nodes are:");
                foreach (Node node in nodes)
                {
                    bool isFreeNode = true;
                    foreach (Centroid centroid in centroids)
                    {
                        if (centroid.nodes.Contains(node))
                        {
                            isFreeNode = false;
                        }
                    }
                    if (isFreeNode) { Console.WriteLine("{0}, {1}", node.xpos, node.ypos); }
                }
                Console.WriteLine("");
            }
        }

        static public void KClusterMain()
        {
            Graph graph = new Graph();
            graph.SolveKCluster();

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}
