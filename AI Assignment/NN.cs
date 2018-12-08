using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AI_Assignment
{
    class NN
    {
        //Used to store global variables
        private static class Globals
        {
            public static XmlData XmlData = new XmlData();
        }

        //contains data from the XML file
        private class XmlData
        {
            private XmlDocument doc;
            private double threshold;
            private double bias;
            private int inputs;
            private int outputs;
            private int layers;
            private int trainingIterations;

            public double Threshold { get { return threshold; } }
            public double Bias { get { return bias; } }
            public int Inputs { get { return inputs; } }
            public int Outputs { get { return outputs; } }
            public int Layers { get { return layers; } }
            public int TrainingIterations { get { return trainingIterations; } }

            public XmlData()
            {
                doc = new XmlDocument();
                doc.Load("NNSettings.xml");
                try
                {
                    threshold = Convert.ToDouble(doc.SelectSingleNode("/root/threshold").InnerText);
                    bias = Convert.ToDouble(doc.SelectSingleNode("/root/bias").InnerText);
                    inputs = Convert.ToInt32(doc.SelectSingleNode("/root/inputs").InnerText);
                    outputs = Convert.ToInt32(doc.SelectSingleNode("/root/outputs").InnerText);
                    layers = Convert.ToInt32(doc.SelectSingleNode("/root/layers").InnerText);
                    trainingIterations = Convert.ToInt32(doc.SelectSingleNode("/root/trainingIterations").InnerText);
                }
                catch
                {
                    Console.WriteLine("Inproper XML input, press enter to exit.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
        }

        //Contains one set of inputs and expected outputs each
        private class NeuralData
        {
            public List<int> Input;
            public List<int> TargetOutput;
            public List<double> ActualOutput;
            public double Error;

            public NeuralData()
            {
                Input = new List<int>();
                TargetOutput = new List<int>();
                ActualOutput = new List<double>();
            }

            public double CalculateError()
            {
                Error = 0;
                int i;
                for (i = 0; i < TargetOutput.Count; i++)
                {
                    Error += Math.Pow(ActualOutput[i] - TargetOutput[i], 2);
                }
                return Error = Error / i;
            }

            public void PrintInput()
            {
                string print = "Input: ";
                foreach (int i in Input)
                {
                    print += i;
                }
                Console.WriteLine(print);
            }

            public void PrintTarget()
            {
                string print = "Target output: ";
                foreach (int i in TargetOutput)
                {
                    print += i;
                }
                Console.WriteLine(print);
            }

            public void PrintActual()
            {
                string print = "Actual output: ";
                foreach (double i in ActualOutput)
                {
                    print += Math.Round(i, 3) + " ";
                }
                Console.WriteLine(print);
            }
        }

        //Contains the weight of a conection and the neuron that feeds the value for the conection.
        private class Conection
        {
            public Neuron ConectedFrom; //The neuron that the conection gets it's output value from.
            public double weight;
        }

        private class Neuron
        {
            public List<Conection> Conections;
            public double Output;
            public int NeuronNumber;    //Used to track it's position in a layer

            public Neuron(int number)
            {
                Conections = new List<Conection>();
                NeuronNumber = number;
            }

            //This calculates the neurons output value, compares it to the threshold, then sets output to either the calculated value or zero.
            public void Fire()
            {
                Output = 0;
                foreach (Conection conection in Conections)
                {
                    if (conection.ConectedFrom.Output > 0)
                    {
                        Output += conection.ConectedFrom.Output * conection.weight;
                    }
                }
                Output += Globals.XmlData.Bias;

                //If the value is greater than the threshold, the neuron fires. Else it outputs 0
                if (Output >= Globals.XmlData.Threshold)
                {
                    Output = 1/(1 + Math.Exp(Output));
                }
                else
                {
                    Output = 0;
                }
            }

            //Adjust the weights of the inputs
            public void AdjustWeights(NeuralNetwork neuralNetwork, int targetOutput, NeuralData neuralData, double learningRate)
            {
                double initialError = neuralData.Error;     //Store the initial Error
                Queue<double> initialWeights = new Queue<double>();
                GetWeights(initialWeights);

                int count = 10;
                while (neuralData.Error >= initialError)
                {
                    foreach (Conection conection in Conections)
                    {   //Loop through every conection
                        if ((conection.ConectedFrom.Output > 0 && targetOutput == 0))
                        {   //If the conected neuron fired and we wanted the neuron to not fire
                            conection.weight -= learningRate;
                            conection.ConectedFrom.AdjustWeights(targetOutput, learningRate);
                        }
                        else if ((conection.ConectedFrom.Output) == 0 && targetOutput == 1)
                        {   //If the conected neuron didn't fire and we wanted it to fire.
                            conection.weight += learningRate;
                            conection.ConectedFrom.AdjustWeights(targetOutput, learningRate);
                        }
                        neuralData = neuralNetwork.CalculateOutputs(neuralData);
                        neuralData.CalculateError();
                        if (neuralData.Error < initialError) { return; }
                    }
                    count--;
                    if (count < 0)
                    {
                        //we were unable to improve the error, we must fix the weights
                        SetWeights(initialWeights);
                        neuralNetwork.CalculateOutputs(neuralData);
                        neuralData.CalculateError();
                        break;
                    }
                }
            }

            private Queue<double> GetWeights(Queue<double> queue)
            {
                foreach (Conection conection in Conections)
                {
                    queue.Enqueue(conection.weight);
                    queue = conection.ConectedFrom.GetWeights(queue);
                }
                return queue;
            }

            private Queue<double> SetWeights(Queue<double> queue)
            {
                foreach (Conection conection in Conections)
                {
                    conection.weight = queue.Dequeue();
                    queue = conection.ConectedFrom.SetWeights(queue);
                }
                return queue;
            }

            private void AdjustWeights(int targetOutput, double learningRate)
            {   //Input nodes don't have conection. No need to handel them
                foreach (Conection conection in Conections)
                {   //Loop through every conection
                    if ((conection.ConectedFrom.Output > 0 && targetOutput == 0))
                    {   //If the conected neuron fired and we wanted the neuron to not fire
                        conection.weight -= learningRate;
                    }
                    else if ((conection.ConectedFrom.Output) == 0 && targetOutput == 1)
                    {   //If the conected neuron didn't fire and we wanted it to fire.
                        conection.weight += learningRate;
                    }
                    conection.ConectedFrom.AdjustWeights(targetOutput, learningRate);
                }
            }
        }

        //Used to store data about each NeuralLayer
        private class NeuralLayer
        {
            //Stors a list of neurons within this layer
            public List<Neuron> Neurons;
            public int LayerNumber;

            public NeuralLayer(int numberOfNeurons, int layerNumber)
            {
                //Add as many neuons as needed
                Neurons = new List<Neuron>();
                for (int i = 0; i < numberOfNeurons; i++)
                {
                    Neurons.Add(new Neuron(i));
                }

                //Set the layer number, 0 is the input layer, 1 is the output layer
                this.LayerNumber = layerNumber;
            }

            public void FireNodes()
            {
                foreach (Neuron neuron in Neurons)
                {
                    neuron.Fire();
                }
            }
        }

        private class NeuralNetwork
        {
            //Contains the data for each layer in the network.
            public List<NeuralLayer> NeuralLayers;

            //Constructor for the class
            public NeuralNetwork()
            {
                NeuralLayers = new List<NeuralLayer>();
            }

            //Connect all of the layers in the neural network, the netwok must have layers for this to work
            public void BuildNetwork()
            {
                //Break if only 1 or 0 layers
                if (NeuralLayers.Count < 2)
                {
                    Console.WriteLine("Not enough layers.");
                    return;
                }

                //Conects each layer to the next layer
                foreach (NeuralLayer neuralLayer in NeuralLayers)
                {
                    //The last layer doesn't connect to anything, break
                    if (neuralLayer == NeuralLayers.Last())
                    {
                        break;
                    }

                    //Fetch the next layer to connect to
                    NeuralLayer nextLayer = NeuralLayers[neuralLayer.LayerNumber + 1];
                    Random rand = new Random();
                    //Loop through all nodes from both layers and try to conect them
                    foreach (Neuron fromNeuron in neuralLayer.Neurons)
                    {
                        foreach (Neuron toNeuron in nextLayer.Neurons)
                        {
                            //Neurons with the same number will allways conect
                            if (fromNeuron.NeuronNumber == toNeuron.NeuronNumber)
                            {
                                toNeuron.Conections.Add(new Conection()
                                {
                                    ConectedFrom = fromNeuron,
                                    weight = rand.NextDouble() + 0.1
                                });
                            }
                            //Else, randomly assign conections, if the random number is zero, no conection
                            else if (rand.Next(0, Globals.XmlData.Inputs) != 0)
                            {
                                toNeuron.Conections.Add(new Conection()
                                {
                                    ConectedFrom = fromNeuron,
                                    weight = rand.NextDouble() + 0.1
                                });
                            }
                            //We need to worry about neurons not having a conection if next layer has more nodes than current layer
                            else if (toNeuron.NeuronNumber >= neuralLayer.Neurons.Count && fromNeuron == neuralLayer.Neurons.Last() && toNeuron.Conections.Count < 1)
                            {
                                toNeuron.Conections.Add(new Conection()
                                {
                                    ConectedFrom = fromNeuron,
                                    weight = rand.NextDouble() + 0.1
                                });
                            }
                        }
                    }
                }
            }

            public void Train()
            {
                Random rand = new Random();

                for (int i = 0; i < Globals.XmlData.TrainingIterations; i++)
                {
                    //Generate the input and target output
                    NeuralData neuralData = GenerateTrainingData(rand);

                    //Calculate the actual outputs from our generated inputs
                    neuralData = CalculateOutputs(neuralData);

                    //Calculate the error and print it
                    double initialError = neuralData.CalculateError();
                    neuralData.PrintInput();
                    neuralData.PrintTarget();
                    neuralData.PrintActual();
                    Console.WriteLine("Initial error: {0}", Math.Round(initialError, 3));

                    //Adjust the weights. If our error is zero, don't change anything
                    if (initialError > 0)
                    {
                        Console.WriteLine("Adjusting weights.");
                        //loop through each output and compare actual to target
                        for (int j = 0; j < neuralData.TargetOutput.Count; j++)
                        {
                            if (neuralData.ActualOutput[j] != neuralData.TargetOutput[j])
                            {   //Actual output does not match target output
                                NeuralLayers.Last().Neurons[j].AdjustWeights(this, neuralData.TargetOutput[j], neuralData, Math.Pow(neuralData.ActualOutput[j] - neuralData.TargetOutput[j], 2));
                            }
                        }
                        Console.WriteLine("New error: {0}", Math.Round(neuralData.CalculateError(), 3));
                        neuralData.PrintActual();
                    }
                    Console.WriteLine("");
                }
            }

            //Generate the input and target output
            private NeuralData GenerateTrainingData(Random rand)
            {
                NeuralData neuralData = new NeuralData();

                //Randomly generate the inputs
                for (int j = 0; j < Globals.XmlData.Inputs; j++)
                {
                    neuralData.Input.Add(rand.Next(0, 2));
                }

                //Randomly generate target outputs
                for (int j = 0; j < Globals.XmlData.Outputs; j++)
                {
                    neuralData.TargetOutput.Add(rand.Next(0, 2));
                }

                return neuralData;
            }

            public NeuralData CalculateOutputs(NeuralData neuralData)
            {
                //Assign the inputs to the first layer.
                for (int neuron = 0; neuron < NeuralLayers[0].Neurons.Count; neuron++)
                {
                    NeuralLayers[0].Neurons[neuron].Output = neuralData.Input[neuron];
                }

                //Attempt to fire all nodes. Skip the first layer. This will get out actual output
                bool FirstLayer = true;
                foreach (NeuralLayer neuralLayer in NeuralLayers)
                {
                    if (FirstLayer)
                    {
                        FirstLayer = false;
                    }
                    else
                    {
                        neuralLayer.FireNodes();
                    }
                }

                neuralData.ActualOutput.Clear();  //Need to clear the list, we will call this function multiple times
                //Fetch the actual output
                foreach (Neuron neuron in NeuralLayers.Last().Neurons)
                {
                    if (neuron.Output > 0)
                    {   //If the neuron fired.
                        neuralData.ActualOutput.Add(neuron.Output);
                    }
                    else
                    {   //The neuron didn't fire.
                        neuralData.ActualOutput.Add(0);
                    }
                }

                return neuralData;
            }
        }

        public static void NNMain()
        {
            NeuralNetwork neuralNetwork = new NeuralNetwork();
            Random rand = new Random();
            for (int i = 0; i < Globals.XmlData.Layers; i++)
            {
                if (i == 0)
                {   //The first layer has a set number of inputs
                    neuralNetwork.NeuralLayers.Add(new NeuralLayer(Globals.XmlData.Inputs, i));
                }
                else if (i == Globals.XmlData.Layers - 1)
                {   //The last layer has a set number of outputs
                    neuralNetwork.NeuralLayers.Add(new NeuralLayer(Globals.XmlData.Outputs, i));
                }
                else
                {   //The rest of the layers can hav a random number of neurons
                    neuralNetwork.NeuralLayers.Add(new NeuralLayer(rand.Next(Globals.XmlData.Inputs, Globals.XmlData.Outputs + 2), i));
                }
            }

            neuralNetwork.BuildNetwork();
            Console.WriteLine("Training Start.");
            neuralNetwork.Train();
            Console.WriteLine("Training End.");

            Console.ReadLine();
        }
    }
}