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
            private double[,] neuralNetwokWeights = new double[4, 3];

            public double Threshold { get { return threshold; } }
            public double[,] NeuralNetwokWeights { get { return neuralNetwokWeights; } }

            public XmlData()
            {
                doc = new XmlDocument();
                doc.Load("NNSettings.xml");
                try
                {
                    int inputCount = 0;
                    int outputCount = 0;

                    //If we needed more layers, this would be a 3d array
                    threshold = Convert.ToDouble(doc.SelectSingleNode("/root/threshold").InnerText);
                    foreach (XmlNode input in doc.SelectNodes("/root/input"))
                    {
                        foreach (XmlNode output in input.ChildNodes)
                        {
                            neuralNetwokWeights[inputCount, outputCount] = Convert.ToDouble(output.InnerText);
                            outputCount++;
                        }
                        outputCount = 0;
                        inputCount++;
                    }
                    inputCount = 0;
                }
                catch
                {
                    Console.WriteLine("Inproper XML input, press enter to exit.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }

            public void PrintWeights()
            {
                //DEBUGING Use this to show all weights
                foreach (double weight in neuralNetwokWeights)
                {
                    Console.WriteLine(weight + " ");
                }
                Console.ReadLine();
            }
        }

        //Contains one set of inputs and expected outputs each
        private class NeuralData
        {
            public List<int> Input;
            public List<int> ExpectedOutput;

            public NeuralData()
            {
                Input = new List<int>();
                ExpectedOutput = new List<int>();
            }

            //Add all inputs
            public void AddInputs(List<int> inputs)
            {
                Input = inputs;
            }

            //Add all outputs
            public void AddExpectedOutputs(List<int> outputs)
            {
                ExpectedOutput = outputs;
            }
        }

        //Contains data for the input of a neuron
        private class WeightedInput
        {
            public double inputValue;
            public double weight;
        }

        //Contains data for each neuron
        private class Neuron
        {
            //Contains a list of input to this neuron and their weights
            public List<WeightedInput> WeightedInputs;
            //Each neuron only has one ouput value
            public double Output;
            //Used to influence the weights of the inputs

            public Neuron()
            {
                WeightedInputs = new List<WeightedInput>();
            }

            //This calculates the neurons output value, compares it to the threshold, then sets output to one or zero.
            public void Fire()
            {
                double value = 0.0f;
                foreach (WeightedInput input in WeightedInputs)
                {
                    value += input.inputValue * input.weight;
                }

                //If the value is grather than the threshold the neuron fires
                if (value >= Globals.XmlData.Threshold)
                {
                    Output = value;
                }
                else
                {
                    Output = 0;
                }
            }

            //Adjust the weights of the inputs
            public void AdjustWeights(double rate, int delta)
            {
                //Loop through each connection
                foreach (WeightedInput input in WeightedInputs)
                {
                    switch (delta)
                    {
                        case 1: //input = 0
                            if (input.inputValue == 0) //the link is good, raise weight
                            {
                                input.weight += rate;
                            }
                            else    //the link is bad, lower weight
                            {
                                input.weight -= rate;
                            }
                            break;
                        case -1:    //input = 1
                            if (input.inputValue == 1) //the link is good, raise weight
                            {
                                input.weight += rate;
                            }
                            else    //the link is bad, lower weight
                            {
                                input.weight -= rate;
                            }
                            break;
                        case 0: //input = output
                            if (input.inputValue != this.Output)    //the link is bad, lower weight
                            {
                                input.weight -= rate;
                            }
                            else
                            {
                                input.weight += rate;
                            }
                            break;
                        default:
                            break;
                    }
                    //if (input.weight > 1) { input.weight = 1; }
                    //else if (input.weight < 0) { input.weight = 0; }
                }
            }
        }

        //Used to store data about each NeuralLayer
        private class NeuralLayer
        {
            //Stors a list of neurons within this layer
            public List<Neuron> Neurons;
            public int LayerNumber;

            //Initalize the layer
            public NeuralLayer(int numberOfNeurons, int layerNumber)
            {
                //Add as many neuons as needed
                Neurons = new List<Neuron>();
                for (int i = 0; i < numberOfNeurons; i++)
                {
                    Neurons.Add(new Neuron());
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
                int i = 0;
                foreach (NeuralLayer neuralLayer in NeuralLayers)
                {
                    //The last layer doesn't connect to anything, break
                    if (i >= NeuralLayers.Count - 1)
                    {
                        break;
                    }

                    //Connect all neurons in the current layer to all the neurons in the next layer
                    NeuralLayer nextLayer = NeuralLayers[i + 1];
                    int inputCount = 0;
                    int outputCount = 0;
                    foreach (Neuron ToNeuron in nextLayer.Neurons)
                    {
                        for (int j = 0; j < neuralLayer.Neurons.Count; j++)
                        {
                            //Make the initial connection between current input neuron and current output neuron
                            ToNeuron.WeightedInputs.Add(new WeightedInput()
                            {
                                inputValue = neuralLayer.Neurons[j].Output,
                                weight = Globals.XmlData.NeuralNetwokWeights[inputCount, outputCount]   //Get weights from the XML file
                            });
                            inputCount++;
                        }
                        inputCount = 0;
                        outputCount++;
                    }
                    i++;
                }
            }

            //This function will train the neural network
            //The expected output will follow the following rules
            //1 and 2 = true, 1 = true
            //2 and 3 = true, 2 = true
            //3 and 4 = true, 3 = true
            public void Train(int iterations, double learningRate)
            {
                //Generate 100 training datas
                List<NeuralData> neuralDatas = new List<NeuralData>();
                Random rnd = new Random();  //Used to generate random numbers. Seeded from system clock.
                for (int i = 0; i < iterations; i++)
                {
                    neuralDatas.Add(new NeuralData());
                    List<int> inputs = new List<int>();
                    List<int> outputs = new List<int>();

                    //Randomly generate the inputs
                    for (int j = 0; j < 4; j++) //The problem specificly asks for 4 inputs
                    {
                        inputs.Add(rnd.Next(0, 2)); //Upper bound is exclusive
                    }
                    neuralDatas[i].AddInputs(inputs);

                    //Calculate the expected outputs
                    if (neuralDatas[i].Input[0] == 1 && neuralDatas[i].Input[1] == 1)
                    {
                        outputs.Add(1);
                    }
                    else
                    {
                        outputs.Add(0);
                    }
                    if (neuralDatas[i].Input[1] == 1 && neuralDatas[i].Input[2] == 1)
                    {
                        outputs.Add(1);
                    }
                    else
                    {
                        outputs.Add(0);
                    }
                    if (neuralDatas[i].Input[2] == 1 && neuralDatas[i].Input[3] == 1)
                    {
                        outputs.Add(1);
                    }
                    else
                    {
                        outputs.Add(0);
                    }
                    neuralDatas[i].AddExpectedOutputs(outputs);
                }

                //Loop through each generated input/output values (100)
                for (int currentData = 0; currentData < neuralDatas.Count; currentData++)
                {
                    NeuralLayer inputLayer = NeuralLayers[0];
                    List<double> actualOutputs = new List<double>();

                    //Set the input data into the first layer
                    for (int j = 0; j < neuralDatas[currentData].Input.Count; j++)
                    {
                        inputLayer.Neurons[j].Output = neuralDatas[currentData].Input[j];
                    }

                    //Calculate the outputs
                    bool first = true;  //This skips the first layer, we alread have the outputs
                    foreach (NeuralLayer layer in NeuralLayers)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            //Set proper input values for the second
                            for(int outNeuron = 0; outNeuron < inputLayer.Neurons.Count; outNeuron++)
                            {
                                for(int inNeuron = 0; inNeuron < layer.Neurons.Count; inNeuron++)
                                {
                                    layer.Neurons[inNeuron].WeightedInputs[outNeuron].inputValue = inputLayer.Neurons[outNeuron].Output;
                                }
                            }
                            //This attempts to fire all nodes in the current layer,
                            //Fireing nodes checks if the sum of the weighted inputs is larger than the threshold, then sets ouput to zero or one
                            layer.FireNodes();
                        }
                    }

                    //Set the calculated outputs to actualOutput
                    foreach (Neuron neuron in NeuralLayers.Last().Neurons)  //for each neuron in output layer
                    {
                        if (neuron.Output > Globals.XmlData.Threshold)
                        {
                            actualOutputs.Add(1);
                        }
                        else
                        {
                            actualOutputs.Add(0);
                        }
                    }

                    //Adjust each incorect node
                    for (int i = 0; i < actualOutputs.Count; i++)
                    {
                        if (actualOutputs[i] > neuralDatas[currentData].ExpectedOutput[i]) //actual = 1, expected = 0
                        {
                            NeuralLayers.Last().Neurons[i].AdjustWeights(learningRate, 1);
                        }
                        else if (actualOutputs[i] < neuralDatas[currentData].ExpectedOutput[i]) //actual = 0, expected = 1
                        {
                            NeuralLayers.Last().Neurons[i].AdjustWeights(learningRate, -1);
                        }
                        else    //expect = actual
                        {
                            NeuralLayers.Last().Neurons[i].AdjustWeights(learningRate, 0);
                        }
                    }
                }
            }

            public void NormalInput(NeuralData data)
            {
                NeuralLayer inputLayer = NeuralLayers[0];
                List<int> actualOutputs = new List<int>();

                //Set the input data into the first layer
                for (int j = 0; j < data.Input.Count; j++)
                {
                    inputLayer.Neurons[j].Output = data.Input[j];
                }

                //Calculate the outputs
                bool first = true;  //This skips the first layer, we alread have the outputs
                foreach (NeuralLayer layer in NeuralLayers)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        //Set proper input values for the second
                        for (int outNeuron = 0; outNeuron < inputLayer.Neurons.Count; outNeuron++)
                        {
                            for (int inNeuron = 0; inNeuron < layer.Neurons.Count; inNeuron++)
                            {
                                layer.Neurons[inNeuron].WeightedInputs[outNeuron].inputValue = inputLayer.Neurons[outNeuron].Output;
                            }
                        }
                        //This attempts to fire all nodes in the current layer,
                        //Fireing nodes checks if the sum of the weighted inputs is larger than the threshold, then sets ouput to zero or one
                        layer.FireNodes();
                    }
                }

                //Print the output nodes
                string outString = "";
                foreach (Neuron neuron in NeuralLayers.Last().Neurons)  //for each neuron in output layer
                {
                    if (neuron.Output > Globals.XmlData.Threshold)
                    {
                        outString = outString + 1;
                    }
                    else
                    {
                        outString = outString + 0;
                    }
                }
                Console.WriteLine(outString);
            }
        }

            static public void BuildNN()
            {
                NeuralNetwork network = new NeuralNetwork();
                network.NeuralLayers.Add(new NeuralLayer(4, 0));
                network.NeuralLayers.Add(new NeuralLayer(3, 1));

                network.BuildNetwork();
                Console.WriteLine("Begin training.");
                network.Train(100, 0.1);
                Console.WriteLine("Done training.");
                Console.WriteLine("Enter 4 different numbers, must be 1 or 0. Press enter after each number");
                while (true)
                {
                    NeuralData data = new NeuralData();
                    List<int> input = new List<int>();
                    for (int i = 0; i < 4; i++)
                    {
                        try
                        {
                            input.Add(new int());
                            input[i] = Convert.ToInt32(Console.ReadLine());
                            if (input[i] != 0)
                            {
                                input[i] = 1;
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Inproper input, press enter to exit.");
                            Console.ReadLine();
                            Environment.Exit(0);
                        }
                    }
                    data.AddInputs(input);
                    network.NormalInput(data);
                    Console.WriteLine("Enter another input.");
                }
            }
        }
    }