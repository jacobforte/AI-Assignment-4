using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AI_Assignment
{
    class LRA
    {
        private static class Globals
        {
            public static XmlData XmlData = new XmlData();
        }

        //contains data from the XML file
        private class XmlData
        {
            private XmlDocument doc;
            private int width;
            private int height;
            private int[,] node;
            
            public int Width { get { return width; } }
            public int Height { get { return height; } }
            public int[,] Node { get { return node; } }

            public XmlData()
            {
                doc = new XmlDocument();
                doc.Load("LRASettings.xml");
                try
                {
                    node = new int[doc.SelectNodes("/root/nodes/node").Count, 2];
                    width = Convert.ToInt32(doc.SelectSingleNode("/root/width").InnerText);
                    height = Convert.ToInt32(doc.SelectSingleNode("/root/height").InnerText);

                    for(int xmlNode = 0; xmlNode < doc.SelectNodes("/root/nodes/node").Count; xmlNode++)
                    {
                        node[xmlNode, 0] = Convert.ToInt32(doc.SelectNodes("/root/nodes/node")[xmlNode].Attributes["x"].Value);
                        node[xmlNode, 1] = Convert.ToInt32(doc.SelectNodes("/root/nodes/node")[xmlNode].Attributes["y"].Value);
                    }
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
                
            }
        }

        class Node
        {
            public int xpos, ypos;

            public Node(int x, int y)
            {
                xpos = x;
                ypos = y;
            }
        }

        class Graph
        {
            private List<Node> nodes;
            private int width, height;
            private double slope, constant;

            public Graph()
            {
                nodes = new List<Node>();
                width = Globals.XmlData.Width;
                height = Globals.XmlData.Height;

                //Fetch the list of nodes
                for (int i = 0; i < Globals.XmlData.Node.Length / 2; i++)
                {
                    nodes.Add(new Node(Globals.XmlData.Node[i, 0], Globals.XmlData.Node[i, 1]));
                }

                //Calculate the optimum line
                CalculateOptimumLine();
                Print();
            }

            //Given an independent variable, predict the dependent variable
            public double Prediction (double input)
            {
                return slope * input + constant;
            }

            public void Print()
            {
                Console.WriteLine("The optimum line is {0}X + {1}", Math.Round(slope, 3), Math.Round(constant, 3));   //aX + b
            }

            private void CalculateOptimumLine()
            {
                double XSampleMean = 0, XStandardDeviation = 0, YSampleMean = 0; //YStandardDeviation = 0;
                slope = 0;
                constant = 0;

                //Calculate sample means
                int count = 0;
                while (count < nodes.Count)
                {
                    XSampleMean += nodes[count].xpos;
                    YSampleMean += nodes[count].ypos;
                    count++;
                }
                XSampleMean = XSampleMean / count;
                YSampleMean = YSampleMean / count;

                //Calculate standard deviation
                count = 0;
                while (count < nodes.Count)
                {
                    XStandardDeviation += Math.Pow(nodes[count].xpos - XSampleMean, 2);
                    //YStandardDeviation += Math.Pow(nodes[count].ypos - YSampleMean, 2); //This is used for corlation factor, which we don't need
                    count++;
                }

                //Find slope
                count = 0;
                while (count < nodes.Count)
                {
                    slope += (nodes[count].xpos - XSampleMean) * (nodes[count].ypos - YSampleMean);
                    count++;
                }
                slope = slope / XStandardDeviation;

                //Find the constant
                constant = YSampleMean - (slope * XSampleMean);
            }
        }

        static public void LRAMain()
        {
            Graph graph = new Graph();
            Console.WriteLine("Input any number to predict it's dependent variable. Input any string to exit.", Globals.XmlData.Width);
            double prediction = 0;
            do
            {
                try
                {
                    prediction = Convert.ToDouble(Console.ReadLine());
                    Console.WriteLine("Your predicted number is {0}", graph.Prediction(prediction));
                }
                catch
                {
                    Console.WriteLine("Inproper input, press enter to exit.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            } while (true);
        }
    }
}
