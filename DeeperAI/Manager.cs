using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeNeedToModDeeperEngine;

namespace DeeperAI
{
    public class Manager
    {
        //Critical vars
        List<NeuralNetwork> nets = new List<NeuralNetwork>();
        readonly int populationSize = 10;
        readonly int[] layers = new int[] { 8, 16, 16, 6 };

        public Manager()
        {
            for (int i = 0; i < populationSize; i++) //Populate nets with neural nets
            {
                NeuralNetwork net = new NeuralNetwork(layers);
                net.Mutate();
                nets.Add(net);
            }
            Begin();
        }

        private void Begin()
        {
            foreach (var net in nets)
            {
                //Test the networks and assign fitness
                net.SetFitness(DoFeed(net));
            }
            //Sort nets, and cull
            nets.Sort();
            for (int i = 0; i < populationSize / 2; i++) //Loop through the population, mutating the best ones, and overwriting the worst with the best 
            { //This basically means the good ones asexually reproduce / mutate into new nets and the parents stay in case the children are retards
                nets[i] = new NeuralNetwork(nets[i + (populationSize / 2)]);
                nets[i].Mutate();
                nets[i + (populationSize / 2)] = new NeuralNetwork(nets[i]);
            }
            for (int i = 0; i < populationSize; i++) //Reset fitness
            {
                nets[i].SetFitness(0f);
            }
        }

        public float DoFeed(NeuralNetwork net) //Test the network and return fitness
        {
            bool flag = false;
            while (!flag)
            {
                //TODO net.FeedForward with the correct values and do action, with timeout if no change, and some stopping conidtion
            }
            float depth = ModEngineVariables.Substats.currentMaxDepthTraveled; //Current max depth is based off progress in the biome, which is determined by submarine.y
            DoReset();
            return depth;
        }

        public void DoReset()
        {
            ModEngineVariables.Submarine.transform.position = new Vector2(191f, -5.29f);
            ModEngineVariables.AIDM.NetworkcurrentWaterType = 1;
        }
    }
}
