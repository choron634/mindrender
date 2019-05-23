using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

public class Nature : MonoBehaviour
{
    [SerializeField] private int totalPopulation = 10;
    private int TotalPopulation { get { return totalPopulation; } }

    [SerializeField] private int inputSize = 20;
    private int InputSize { get { return inputSize; } }

    [SerializeField] private int hiddenSize = 15;
    private int HiddenSize { get { return hiddenSize; } }

    [SerializeField] private int outputSize = 4;
    private int OutputSize { get { return outputSize; } }

    private float BestReward { get; set; }
    private float SecondReward { get; set; }
    public NN BestBrain { get; set; }
    private NN SecondBrain { get; set; }

    private float BestRecord { get; set; }


    private int CurrentPopCount { get; set; }
    private int Generation { get; set; }

    [SerializeField] private Agent agent = null;
    private Agent NNAgent { get { return agent; } }

    private List<NN> Children { get; set; } = new List<NN>();

    [SerializeField] private int tournamentSelection = 85;
    private int TournamentSlection { get { return tournamentSelection; } }

    private HelicopterAgent HeliAgent { get; set; }


    //private List<NN> Selected { get; set; } = new List<NN>();

    //private List<float> Rewards { get; set; } = new List<float>();


    public Text Populationtext;

    private void Start() {//
        for(int i = 0; i < TotalPopulation; i++) {
            Children.Add(new NN(InputSize, HiddenSize, OutputSize));
        }
    }

    // Update is called once per frame
    void Update() {
        Populationtext.text = "Population: " + (CurrentPopCount + 1) + "/" + TotalPopulation
            + "\nGeneration: " + (Generation + 1)
            + "\nBest Record: " + BestRecord
            + "\nBest Distance: " + BestReward
            + "\nSecond Best Distance: " + SecondReward;
    }
    
    private void FixedUpdate() {//時間ごとに呼び出される
        if(NNAgent.IsDone) {//ベストの更新
            Children[CurrentPopCount].Reward = NNAgent.Reward/HeliAgent.DriveTime;
            if(NNAgent.Reward / HeliAgent.DriveTime > BestReward) {//一位の塗り替え
                SecondReward = BestReward;
                SecondBrain = BestBrain;
                BestReward = NNAgent.Reward / HeliAgent.DriveTime;
                BestBrain = Children[CurrentPopCount];
            }
            else if(NNAgent.Reward > SecondReward) {//二位の塗り替え
                SecondReward = NNAgent.Reward / HeliAgent.DriveTime;
                SecondBrain = Children[CurrentPopCount];
            }

            if(BestRecord < BestReward) {
                BestRecord = BestReward;
            }

            CurrentPopCount++;
            //Debug.Log(BestReward);

            // 世代交代(ルーレット選択）
            if (CurrentPopCount == TotalPopulation) {
                /*var sum = 0.0f;
                for (int i = 0; i < Rewards.Count; i++)
                {
                    sum += Rewards[i];
                }
                for (int i = 0; i < Rewards.Count; i++)
                {
                    Rewards[i] = Rewards[i] / sum;
                }

                Selected.Add(BestBrain);//エリート保存

                for (int i = 1; i < Children.Count; i++)
                {
                    float sum_score = 0.0f;
                    Random.InitState(System.DateTime.Now.Millisecond);
                    float rdm = UnityEngine.Random.Range(0.0f, 1.0f);

                    for (int j = 0; j < Rewards.Count; j++)
                    {
                        sum_score += Rewards[j];
                        if (sum_score > rdm)
                        {
                            Selected.Add(Children[j]);
                            break;
                        }
                    }

                }

                for(int i = 0; i < Children.Count; i++)
                {
                    Children[i] = Selected[i];
                }

                Selected.Clear();
                Rewards.Clear();
                BestBrain = SecondBrain = null;
                */

                ///トーナメント選択
                var TournamentMembers = Children.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(tournamentSelection).ToList();
                var temp1 = TournamentMembers[0];

                TournamentMembers.Sort((a, b) => (int)b.Reward / (int)HeliAgent.DriveTime - (int)a.Reward / (int)HeliAgent.DriveTime);
                var BestBrainInTournament = TournamentMembers[0];
                var SecondBrainInTournament = TournamentMembers[1];

                Generation++;
                CurrentPopCount = 0;
                BestReward = 0;
                SecondReward = 0;
                Children.Clear();

                //Debug.Log(BestBrain);
                
                Children.Add(BestBrain);//エリート保存

                while(Children.Count < TotalPopulation) {
                    var (c1, c2) = BestBrainInTournament.Crossover(SecondBrainInTournament);//トーナメント上位2個体の交叉結果の子供
                    Children.Add(c1);
                    Children.Add(c2);
                }
                BestBrain = SecondBrain = null;
                
            }

            NNAgent.Reset();
        }

        var currentNN = Children[CurrentPopCount];
        var observations = NNAgent.CollectObservations();

        var actions = currentNN.Predict(observations.ToArray());//学習済みのNNにセンサーからの入力を入れる
        NNAgent.AgentAction(actions);//outputをunity上のagentのactionに//5/12
    }
}
