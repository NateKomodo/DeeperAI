using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DeeperAI
{
    public class NeuralNetwork : IComparable<NeuralNetwork>
    {
        //Neural Network critical variables / data
        private int[] layers;
        private float[][] neurons;
        private float[][][] weights;
        private float fitness;

        public NeuralNetwork(int[] layers)
        {
            //Init layers
            this.layers = new int[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                this.layers[i] = layers[i];
            }

            //Init Matrix
            InitNeurons();
            InitWeights();
        }

        //For copying other networks, literally a copy paste of NeuralNetwork(int[] layers) with copy weights
        public NeuralNetwork(NeuralNetwork copyNetwork)
        {
            this.layers = new int[copyNetwork.layers.Length];
            for (int i = 0; i < copyNetwork.layers.Length; i++)
            {
                this.layers[i] = copyNetwork.layers[i];
            }

            InitNeurons();
            InitWeights();
            CopyWeights(copyNetwork.weights);
        }

        //save the network to file
        public void SaveNetwork(string neuronPath, string weightPath)
        {
            var formatter = new BinaryFormatter();

            if (!File.Exists(weightPath)) File.Create(weightPath).Close();
            FileStream stream = File.OpenWrite(weightPath);
            formatter.Serialize(stream, weights);
            stream.Close();

            if (!File.Exists(neuronPath)) File.Create(neuronPath).Close();
            FileStream stream2 = File.OpenWrite(neuronPath);
            formatter.Serialize(stream2, neurons);
            stream2.Close();
        }

        //save the network to file
        public void LoadNetwork(string weightPath, string neuronPath)
        {
            if (File.Exists(weightPath) && File.Exists(neuronPath))
            {
                var formatter = new BinaryFormatter();

                FileStream stream = File.OpenRead(weightPath);
                weights = (float[][][])formatter.Deserialize(stream);
                stream.Close();

                FileStream stream2 = File.OpenRead(neuronPath);
                neurons = (float[][])formatter.Deserialize(stream2);
                stream2.Close();
            }
        }

        //When copying other networks, merge its weights with this
        private void CopyWeights(float[][][] copyWeights)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = copyWeights[i][j][k];
                    }
                }
            }
        }

        private void InitNeurons()
        {
            //Neuron Initilization
            List<float[]> neuronsList = new List<float[]>();
            for (int i = 0; i < layers.Length; i++) //run through all layers
            {
                neuronsList.Add(new float[layers[i]]); //add layer to neuron list
            }
            neurons = neuronsList.ToArray(); //convert list to array
        }

        //Create weights matrix.
        private void InitWeights()
        {
            List<float[][]> weightsList = new List<float[][]>(); //weights list which will later will converted into a weights 3D array

            //itterate over all neurons that have a weight connection
            for (int i = 1; i < layers.Length; i++)
            {
                List<float[]> layerWeightsList = new List<float[]>(); //layer weight list for this current layer (will be converted to 2D array)
                int neuronsInPreviousLayer = layers[i - 1];

                //itterate over all neurons in this current layer
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

                    //itterate over all neurons in the previous layer and set the weights randomly between 0.5f and -0.5
                    for (int k = 0; k < neuronsInPreviousLayer; k++)
                    {
                        //give random weights to neuron weights
                        neuronWeights[k] = GetRandomNumber(-0.5f, 0.5f);
                    }
                    layerWeightsList.Add(neuronWeights); //add neuron weights of this current layer to layer weights
                }
                weightsList.Add(layerWeightsList.ToArray()); //add this layers weights converted into 2D array into weights list
            }
            weights = weightsList.ToArray(); //convert to 3D array
        }

        /// Feed forward this neural network with a given input array
        public float[] FeedForward(float[] inputs)
        {
            //Add inputs to the neuron matrix
            for (int i = 0; i < inputs.Length; i++)
            {
                neurons[0][i] = inputs[i];
            }

            //itterate over all neurons and compute feedforward values 
            for (int i = 1; i < layers.Length; i++)
            {
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    float value = 0.25f;

                    for (int k = 0; k < neurons[i - 1].Length; k++)
                    {
                        value += weights[i - 1][j][k] * neurons[i - 1][k]; //sum off all weights connections of this neuron weight their values in previous layer
                    }

                    neurons[i][j] = (float)Math.Tanh(value); //Hyperbolic tangent activation
                }
            }

            return neurons[neurons.Length - 1]; //return output layer
        }

        /// Mutate neural network weights
        public void Mutate()
        {
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        float weight = weights[i][j][k];

                        //mutate weight value 
                        float randomNumber = GetRandomNumber(0f, 100f);

                        if (randomNumber <= 2f)
                        { //if 1
                          //flip sign of weight
                            weight *= -1f;
                        }
                        else if (randomNumber <= 4f)
                        { //if 2
                          //pick random weight between -1 and 1
                            weight = GetRandomNumber(-0.5f, 0.5f);
                        }
                        else if (randomNumber <= 6f)
                        { //if 3
                          //randomly increase by 0% to 100%
                            float factor = GetRandomNumber(0f, 1f) + 1f;
                            weight *= factor;
                        }
                        else if (randomNumber <= 8f)
                        { //if 4
                          //randomly decrease by 0% to 100%
                            float factor = GetRandomNumber(0f, 1f);
                            weight *= factor;
                        }

                        weights[i][j][k] = weight;
                    }
                }
            }
        }

        //Add fitness to this network
        public void AddFitness(float fit)
        {
            fitness += fit;
        }

        //Set the fitness of this network
        public void SetFitness(float fit)
        {
            fitness = fit;
        }

        //Get the fitness of this network
        public float GetFitness()
        {
            return fitness;
        }

        /// Compare two neural networks and sort based on fitness
        public int CompareTo(NeuralNetwork other)
        {
            if (other == null) return 1;

            if (fitness > other.fitness)
                return 1;
            else if (fitness < other.fitness)
                return -1;
            else
                return 0;
        }

        //Generate a random float
        public float GetRandomNumber(float minimum, float maximum)
        {
            Random random = new Random();
            return (float)random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
