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
        readonly int[] layers = new int[] { 7, 16, 16, 4 };

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
            DoReset();
            net = nets.First();
        }

        NeuralNetwork net;
        int netCount = 0;
        public void ProccessNet()
        {
            float[] rays = GetRaycasts();
            float xscale = ModEngineVariables.Submarine.GetComponent<MoveSubmarine>().desiredXScale;
            float pitch = ModEngineVariables.Submarine.transform.rotation.z * xscale;
            float speed = ModEngineVariables.Submarine.GetComponent<MoveSubmarine>().subSpeed / 1000;
            float[] result = net.FeedForward(new float[] { rays[0], rays[1], rays[2], rays[3], xscale, pitch, speed });
            PushRotation(ModEngineVariables.Submarine.GetComponent<MoveSubmarine>(), result[0]);
            PushFlip(ModEngineVariables.Submarine.GetComponent<MoveSubmarine>(), result[1]);
            PushEngineChange(ModEngineVariables.Submarine.GetComponent<MoveSubmarine>(), result[2]);
            if (Timeout())
            {
                net.SetFitness(Math.Abs(ModEngineVariables.Submarine.transform.position.y));
                DoReset();
                if (netCount >= 20)
                {
                    netCount = 0;
                    Finish();
                }
                else
                {
                    netCount++;
                    new ModEngineChatMessage("Fitness: " + net.GetFitness(), PlayerNetworking.ChatMessageType.BOT);
                    new ModEngineChatMessage($"Next net (#{netCount}) of current gen", PlayerNetworking.ChatMessageType.BOT);
                    net = nets[netCount + 1];
                }
            }
        }

        private void Finish() //Test the network and return fitness
        {
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
            new ModEngineChatMessage("Next generation", PlayerNetworking.ChatMessageType.BOT);
            Begin();
        }

        int callCount = 0;
        float prevDepth;
        private bool Timeout()
        {
            if (ModEngineVariables.Substats.NetworksubHealth < 10) return true;
            if (ModEngineVariables.Substats.currentMaxDepthTraveled > 1000) return true;
            if (callCount >= 300 && prevDepth == Math.Round(Math.Abs(ModEngineVariables.Submarine.transform.position.y)))
            {
                callCount = 0;
                return true;
            }
            else if (callCount >= 300)
            {
                callCount = 0;
                return false;
            }
            else
            {
                callCount++;
                prevDepth = (int)Math.Round(Math.Abs(ModEngineVariables.Submarine.transform.position.y));
                return false;
            }
        }

        private float[] GetRaycasts()
        {
            float[] raycastsDist = new float[4];
            raycastsDist[0] = CastRay(Vector2.up);
            raycastsDist[1] = CastRay(Vector2.right);
            raycastsDist[2] = CastRay(Vector2.down);
            raycastsDist[3] = CastRay(Vector2.left);
            return raycastsDist;
        }

        private float CastRay(Vector2 dir)
        {
            var submarine = ModEngineVariables.Submarine;
            var origin = submarine.transform.position;
            var direction = submarine.transform.TransformDirection(dir);

            int layerMask = 1 << 27;

            var hit = Physics2D.Raycast(origin, direction, 500, layerMask);

            if (hit.transform == null) return 1;
            if (hit.transform.gameObject == null) return 1;

            return hit.distance / 500;
        }

        private void DoReset()
        {
            callCount = 0;
            ModEngineVariables.Submarine.transform.position = new Vector2(250f, -214.8f);
            ModEngineVariables.AIDM.NetworkcurrentWaterType = 1;
            foreach (var brk in ModEngineVariables.Breaks) brk.GetComponent<BreakBehavior>().Networkhealth = -1;
            foreach (var brk in ModEngineVariables.ExteriorEnemies) brk.GetComponent<ExteriorEnemyHealth>().Networkhealth = -1;
        }

        private void PushRotation(MoveSubmarine sub, float value)
        {
            value = value * 1000;
            sub.subRotation = 0f;
            sub.counterTorque = 0f;
            if (sub.subStats != null)
            {
                float num = Mathf.Clamp((float)sub.subStats.subEnginePower, 0f, 2.8f);
                if (Mathf.Sign(sub.transform.localScale.x) == Mathf.Sign(sub.localScaleXPrevious))
                {
                    if (Mathf.Abs(value) > 0f)
                    {
                        sub.subRotation = Mathf.Round(value) * (sub.subRotationSpeed + (1f + num) / 8f) * sub.horizDir * -1f * sub.torqueScalar;
                    }
                    else
                    {
                        float num2 = Mathf.Sign(Mathf.DeltaAngle(sub.currentAngle, 90f));
                        float num3 = Mathf.Abs(Mathf.DeltaAngle(sub.currentAngle, 90f));
                        float num4 = 1f / (1f + Mathf.Pow(2.78128f, -num3 + sub.levelBounceDampening));
                        sub.subRotation = num2 * sub.subRotationSpeed * num4 * sub.torqueScalar;
                    }
                    float f = Mathf.DeltaAngle(90f, sub.currentAngle);
                    sub.counterTorque = Mathf.Abs(Mathf.DeltaAngle(90f, sub.currentAngle));
                    sub.counterTorque = 5f / (1f + Mathf.Pow(2.78128f, -0.3f * sub.counterTorque + 0.3f * sub.subAngleLimit));
                    sub.counterTorque = sub.counterTorque * sub.subRotationSpeed * Mathf.Sign(f) * 1.05f * sub.torqueScalar;
                    sub.subRotation -= sub.counterTorque;
                    if (GameControllerBehavior.playerInNavigation == NetworkManagerBehavior.myLocalPlayer || (GameControllerBehavior.playerInNavigation == null && sub.isServer))
                    {
                        sub.syncSubRotation = sub.subRotation;
                    }
                }
            }
        }

        private void PushFlip(MoveSubmarine sub, float value)
        {
            if (value < 0 && !sub.flipping)
            {
                sub.horizDir = -1f;
                sub.desiredXScale = -1f;
                sub.flipping = true;
            }
            else if (value > 0 && !sub.flipping)
            {
                sub.horizDir = 1f;
                sub.desiredXScale = 1f;
                sub.flipping = true;
            }
        }

        private void PushEngineChange(MoveSubmarine sub, float value)
        {
            if (value > 0 && sub.subStats.NetworksubEnginePower != 3)
            {
                sub.subStats.NetworksubEnginePower += 1;
            }
            else if (value < 1 && sub.subStats.NetworksubEnginePower != 0)
            {
                sub.subStats.NetworksubEnginePower -= 1;
            }
        }
    }
}
