using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AI_Assignment
{
    class MainClass
    {
        static void Main()
        {
            string input;
            Console.WriteLine("Hello!\nPress 1 for HMM\nPress 2 for Single Layer NN\nPress 3 for K-means clustering\nPress 4 for linear regression analysis");
            input = Console.ReadLine();
            if (input == "1")
            {
                HMM.HiddenMarkovModel();
            }
            else if (input == "2")
            {
                NN.BuildNN();
            }
            else if (input == "3")
            {
                KCluster.KClusterMain();
            }
            else
            {
                LRA.LRAMain();
            }
        }
    }
}
