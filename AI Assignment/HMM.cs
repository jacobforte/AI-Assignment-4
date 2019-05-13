using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AI_Assignment
{
    class HMM
    {
        //Contains data about a path from one node to another node, each path only trravels in one direction
        class Path
        {
            public List<int> transitionSequence;    //The states are zero indexed
            public double probability;

            public Path(int integer = 0)
            {
                probability = 0;
                transitionSequence = new List<int>();
            }
        };

        //This function expects valid XML input, because checking the XML file will take a lot of work, I am not going to check it.
        public static void HiddenMarkovModel()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("HMMSettings.xml");

            int numberOfStates;
            int numberOfEmmissions;
            double[,] transitionMatrix;
            double[,] emissionMatrix;

            //Initialize values
            try
            {
                numberOfStates = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("/root/numberOfStates").InnerText);   //[toState, fromState]
                numberOfEmmissions = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("/root/numberOfEmmissions").InnerText);   //[state,emmission]       
                transitionMatrix = new double[numberOfStates, numberOfStates];
                emissionMatrix = new double[numberOfStates, numberOfEmmissions];

                //Populate emmission matrix
                {
                    int signalCount = 0;
                    int stateCount = 0;
                    foreach (XmlNode signal in doc.GetElementsByTagName("emmissionMatrix")[0].ChildNodes)
                    {
                        foreach (XmlNode state in signal.ChildNodes)
                        {
                            emissionMatrix[stateCount, signalCount] = Convert.ToDouble(state.InnerText);
                            stateCount++;
                        }
                        stateCount = 0;
                        signalCount++;
                    }
                }

                //populate the transition matrix
                {
                    int fromCount = 0;
                    int toCount = 0;
                    foreach (XmlNode fromState in doc.GetElementsByTagName("transitionMatrix")[0].ChildNodes)
                    {
                        foreach (XmlNode toState in fromState.ChildNodes)
                        {
                            transitionMatrix[toCount, fromCount] = Convert.ToDouble(toState.InnerText);
                            toCount++;
                        }
                        toCount = 0;
                        fromCount++;
                    }
                }
            }
            catch
            {
                Console.WriteLine("Inproper XML input, press enter to exit.");
                Console.ReadLine();
                return;
            }

            //Get the sequence of observations from the user
            Console.WriteLine("There are curently " + numberOfEmmissions + " different emissions.");
            Console.WriteLine("Enter a sequence of observations using the numbers 1 through " + numberOfEmmissions + ".");
            Console.WriteLine("Press enter after each emission, enter 0 to continue.");
            int temp = 0;
            List<int> observations = new List<int>();
            do
            {
                try //This is necesary in case the user enteres a string instead of a number
                {
                    temp = Convert.ToInt32(Console.ReadLine()); //Read the value entered by the user
                    if (temp > 0 && temp < numberOfEmmissions + 1)  //If temp is between one and the number of emissions
                    {
                        observations.Add(temp);
                    }
                    else if (temp != 0)
                    {
                        Console.WriteLine("Number not within the range 1 through " + numberOfEmmissions + ". Please try again.");
                    }
                }
                catch
                {
                    Console.WriteLine("Oops, that string wasn't a number. Please try again.");
                }
            } while (temp != 0);

            //If there areno obserations, end the program
            if (observations.Count < 1)
            {
                Console.WriteLine("No observations entered, press enter to exit.");
                Console.ReadLine();
                return;
            }

            //Use an external function to get every possible path.
            List<Path> paths = new List<Path>();
            paths.Add(new Path(0));  //Initalize a path object
            FindNextState(transitionMatrix, emissionMatrix, observations, numberOfStates, paths);

            //We have our list of paths, we now need to calculate every probability and pick the best one.
            for (int path = 0; path < paths.Count; path++)
            {
                //Perform the calculaion on every path
                for (int i = 0; i < observations.Count; i++)
                {
                    if (i == 0)
                    {
                        //Find the initial emmition probability
                        paths[path].probability = emissionMatrix[paths[path].transitionSequence[i], observations[i] - 1];
                    }
                    else
                    {
                        //Find the transition probability
                        paths[path].probability = paths[path].probability * transitionMatrix[paths[path].transitionSequence[i], paths[path].transitionSequence[i - 1]];
                        //Find the emmission probability
                        paths[path].probability = paths[path].probability * emissionMatrix[paths[path].transitionSequence[i], observations[i] - 1];
                    }
                }
            }

            //DEBUGING Use this to show all the calculated probabilities
            foreach (Path path in paths)
            {
                string output = "";
                foreach (int state in path.transitionSequence)
                {
                    temp = state + 1;
                    output = output + temp + " ";
                }

                output = output + path.probability;
                Console.WriteLine(output);
            }

            //Find the most probable path.
            Path mostProbablePath = new Path(0);
            foreach (Path path in paths)
            {
                if (path.probability > mostProbablePath.probability)
                {
                    mostProbablePath = path;
                }
            }

            //Print the result.
            {
                string output = "";
                foreach (int state in mostProbablePath.transitionSequence)
                {
                    temp = state + 1;
                    output = output + temp + " ";
                }
                Console.WriteLine("The most probable path is " + output + "with a probability of " + mostProbablePath.probability + ".");
                Console.WriteLine("Press enter to continue.");
                Console.ReadLine();
            }
        }

        static List<Path> FindNextState(double[,] transitionMatrix, double[,] emissionMatrix, List<int> observations, int numberOfStates, List<Path> paths, int currentDepth = 0)
        {
            if (currentDepth == 0)
            {
                //Find an initial state that emits the current emmission
                for (int nextState = 0; nextState < numberOfStates; nextState++)
                {
                    if (emissionMatrix[nextState, observations[currentDepth] - 1] > 0)  //observations are 1 index, because they must be inputted
                    {
                        //This state emmits the current observation (represented by depth)
                        paths[paths.Count - 1].transitionSequence.Add(nextState);   //Add the next state to our transition sequence.
                        paths = FindNextState(transitionMatrix, emissionMatrix, observations, numberOfStates, paths, currentDepth + 1);   //Go one level deeper
                    }
                }
                //We have iterated through all the states, the last path we generated is empty, remove it and return.
                paths.RemoveAt(paths.Count - 1);
            }
            else if (observations.Count == 1)
            {
                //We only have one observation, create a new, empty path and return
                paths.Add(new Path(0));
            }
            else
            {
                //Find the states that emmits the next observation
                for (int nextState = 0; nextState < numberOfStates; nextState++)
                {
                    if (emissionMatrix[nextState, observations[currentDepth] - 1] > 0)
                    {
                        //This state emits the current observation (represented by depth)
                        //Check if there is a path to this state from the current state
                        if (transitionMatrix[nextState, paths.Last<Path>().transitionSequence.Last<int>()] > 0)
                        {
                            //There is a path from the current state to the next state
                            if (currentDepth == observations.Count - 1)
                            {
                                //There are no more observations, we cannot do deeper.
                                paths.Last<Path>().transitionSequence.Add(nextState);   //Add the next state to our transition sequence.
                                paths.Add(new Path(0)); //Duplicate the current path object, then remove the nextState that we just added from our new object
                                foreach (int state in paths[paths.Count - 2].transitionSequence)
                                {
                                    paths.Last<Path>().transitionSequence.Add(state);
                                }
                                paths.Last<Path>().transitionSequence.RemoveAt(paths.Last<Path>().transitionSequence.Count - 1);    //Remove the state we just added from the duplicate
                            }
                            else
                            {
                                //There is another observation after this one, go deeper.
                                paths.Last<Path>().transitionSequence.Add(nextState);   //Add the next state to our transition sequence.
                                paths = FindNextState(transitionMatrix, emissionMatrix, observations, numberOfStates, paths, currentDepth + 1);   //Go one level deeper
                            }
                        }
                    }
                }
                paths.Last<Path>().transitionSequence.RemoveAt(paths.Last<Path>().transitionSequence.Count - 1);    //going down a level, go back one observation
            }
            return paths;
        }
    }
}
