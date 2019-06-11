using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using System.Text;


public class Nature : MonoBehaviour
{
    [SerializeField] private int totalPopulation = 10;
    private int TotalPopulation { get { return totalPopulation; } }

    [SerializeField] private int inputSize = 19;
    private int InputSize { get { return inputSize; } }

    [SerializeField] private int hiddenSize1 = 16;
    private int HiddenSize1 { get { return hiddenSize1; } }

    [SerializeField] private int hiddenSize2 = 16;
    private int HiddenSize2 { get { return hiddenSize2; } }

    [SerializeField] private int outputSize = 4;
    private int OutputSize { get { return outputSize; } }

    private float BestReward { get; set; }
    private float SecondReward { get; set; }
    public NN BestBrain { get; set; }
    private NN SecondBrain { get; set; }

    public NN BestBrainYaw { get; set; }
    public NN SecondBrainYaw { get; set; }


    private float BestRecord { get; set; }
    private float BestRecordYaw { get; set; }



    private int CurrentPopCount { get; set; }
    private int Generation { get; set; }

    private bool GenerationChange;

    [SerializeField] private Agent agent = null;
    private Agent NNAgent { get { return agent; } }

    private List<NN> Children { get; set; } = new List<NN>();
    private List<NN> ChildrenYaw { get; set; } = new List<NN>();


    [SerializeField] private int tournamentSelection = 85;
    private int TournamentSlection { get { return tournamentSelection; } }



    //private List<NN> Selected { get; set; } = new List<NN>();

    //private List<float> Rewards { get; set; } = new List<float>();


    public Text Populationtext;

    private void Start() {//
        for(int i = 0; i < TotalPopulation; i++) {
            Children.Add(new NN(InputSize, HiddenSize1, HiddenSize2, OutputSize));
        }
        for (int i = 0; i < TotalPopulation; i++)
        {
            ChildrenYaw.Add(ImportNN());
        }
    }

    // Update is called once per frame
    void Update() {
        Populationtext.text = "Population: " + (CurrentPopCount + 1) + "/" + TotalPopulation
            + "\nGeneration: " + (Generation + 1)
            + "\nBest Record: " + BestRecord
            + "\nBest of this generation: " + BestReward
            + "\nSecond Best of this generation: " + SecondReward;
    }
    
    private void FixedUpdate() {//時間ごとに呼び出される
        GenerationChange = true;//if you don't want to keep the order of waypoints during one generation, turn off this.
        //GenerationChange = false;//if you want to keep the order of waypoints during one generation, turn off this.
        if (NNAgent.IsDone) {//ベストの更新
            Children[CurrentPopCount].Reward = NNAgent.Reward;
            ChildrenYaw[CurrentPopCount].Reward = NNAgent.Reward;

            if (NNAgent.Reward > BestReward) {//一位の塗り替え
                SecondReward = BestReward;
                SecondBrain = BestBrain;
                SecondBrainYaw = BestBrainYaw;
                BestReward = NNAgent.Reward;
                BestBrain = Children[CurrentPopCount];
                BestBrainYaw = ChildrenYaw[CurrentPopCount];
            }
            else if(NNAgent.Reward > SecondReward) {//二位の塗り替え
                SecondReward = NNAgent.Reward;
                SecondBrain = Children[CurrentPopCount];
                SecondBrainYaw = ChildrenYaw[CurrentPopCount];
            }

            if(BestRecord < BestReward) {
                BestRecord = BestReward;
            }

            if (BestBrain != null)
            {
                SaveBestBrain(BestBrain,"bestbrain.txt");
                SaveBestBrain(BestBrainYaw, "bestbrain_yaw.txt");
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
                    [.InitState(System.DateTime.Now.Millisecond);
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
                var YawTournamentMembers = ChildrenYaw.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(ChildrenYaw.Count).ToList();

                var temp1 = TournamentMembers[0];
                var temp2 = YawTournamentMembers[0];

                TournamentMembers.Sort((a, b) => (int)b.Reward - (int)a.Reward);
                YawTournamentMembers.Sort((a, b) => (int)b.Reward - (int)a.Reward);

                var BestBrainInTournament = TournamentMembers[0];
                var BestBrainInYawTournament = YawTournamentMembers[0];

                var SecondBrainInTournament = TournamentMembers[1];
                var SecondBrainInYawTournament = YawTournamentMembers[1];

                Generation++;
                CurrentPopCount = 0;
                BestReward = 0;
                SecondReward = 0;
                Children.Clear();
                ChildrenYaw.Clear();

                //Debug.Log(BestBrain);
                
                //Children.Add(BestBrain);//エリート保存
                //Children.Add(SecondBrain);

                while(Children.Count < TotalPopulation) {
                    var (c1, c2) = BestBrainInTournament.Crossover(SecondBrainInTournament);//トーナメント上位2個体の交叉結果の子供
                    //var (c1, c2) = BestBrainInTournament.Crossover(BestBrainInTournament);//トーナメント上位2個体の交叉結果の子供
                    //var (c3, c4) = SecondBrainInTournament.Crossover(SecondBrainInTournament);//トーナメント上位2個体の交叉結果の子供

                    Children.Add(c1);
                    Children.Add(c2);
                }
                while (ChildrenYaw.Count < TotalPopulation)
                {
                    var (c1, c2) = BestBrainInYawTournament.Crossover(SecondBrainInYawTournament);//トーナメント上位2個体の交叉結果の子供
                    //var (c1, c2) = BestBrainInTournament.Crossover(BestBrainInTournament);//トーナメント上位2個体の交叉結果の子供
                    //var (c3, c4) = SecondBrainInTournament.Crossover(SecondBrainInTournament);//トーナメント上位2個体の交叉結果の子供

                    ChildrenYaw.Add(c1);
                    ChildrenYaw.Add(c2);
                }
                BestBrain = SecondBrain = BestBrainYaw = SecondBrainYaw = null;
                GenerationChange = true;
                
            }

            NNAgent.Reset(GenerationChange);
        }

        var currentNN = Children[CurrentPopCount];
        var currentYawNN = ChildrenYaw[CurrentPopCount];
        var observations = NNAgent.CollectObservations();
        var obsevationsYaw = NNAgent.CollectYawObservations();

        var actions = currentNN.Predict(observations.ToArray());//学習済みのNNにセンサーからの入力を入れる
        var Yawactions = currentYawNN.Predict(obsevationsYaw.ToArray());
        NNAgent.AgentAction(actions,Yawactions);//outputをunity上のagentのactionに//5/12
    }

    private void SaveBest(NN bestbrain)
    {
        bestbrain.Save("savedata.txt");
    }

    private void SaveBestBrain(NN bestbrain, string filename)
    {
        FileStream fs = new FileStream(filename, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);

        try
        {

            var inputbias = bestbrain.InputBias;
            var inputweights = bestbrain.InputWeights;
            var hiddenbias1 = bestbrain.HiddenBias1;
            var hiddenbias2 = bestbrain.HiddenBias2;
            var hidenweights1 = bestbrain.HiddenWeights1;
            var hidenweights2 = bestbrain.HiddenWeights2;

            var inputsize = bestbrain.InputSize;
            var hiddensize1 = bestbrain.HiddenSize1;
            var hiddensize2 = bestbrain.HiddenSize2;
            var outputsize = bestbrain.OutputSize;

            sw.WriteLine(inputsize);
            sw.WriteLine(hiddensize1);
            sw.WriteLine(hiddensize2);
            sw.WriteLine(outputsize);


            for (int c = 0; c < inputbias.Colmun; c++)
            {
                var text = inputbias[0, c];
                sw.WriteLine(text);
            }
            for (int r = 0; r < inputweights.Row; r++)
            {
                for (int c = 0; c < inputweights.Colmun; c++)
                {
                    var text = inputweights[r, c];
                    sw.WriteLine(text);
                }
            }
            for (int c = 0; c < hiddenbias1.Colmun; c++)
            {
                var text = hiddenbias1[0, c];
                sw.WriteLine(text);
            }
            for (int r = 0; r < hidenweights1.Row; r++)
            {
                for (int c = 0; c < hidenweights1.Colmun; c++)
                {
                    var text = hidenweights1[r, c];
                    sw.WriteLine(text);
                }
            }
            for (int c = 0; c < hiddenbias2.Colmun; c++)
            {
                var text = hiddenbias2[0, c];
                sw.WriteLine(text);
            }
            for (int r = 0; r < hidenweights2.Row; r++)
            {
                for (int c = 0; c < hidenweights2.Colmun; c++)
                {
                    var text = hidenweights2[r, c];
                    sw.WriteLine(text);
                }
            }
        }
        finally
        {
        sw.Close();

        }

    }

    public NN ImportNN()
    {
        FileStream fs = new FileStream("learned_yaw.txt", FileMode.Open);
        var sr = new StreamReader(fs, Encoding.UTF8);

        var LearnedNN = new NN(0, 0, 0, 0);

        try
        {
            var inputsize = int.Parse(sr.ReadLine());
            var hiddensize1 = int.Parse(sr.ReadLine());
            var hiddensize2 = int.Parse(sr.ReadLine());
            var outputsize = int.Parse(sr.ReadLine());

            LearnedNN = new NN(inputsize, hiddensize1, hiddensize2, outputsize);

            var inputbias = LearnedNN.InputBias;
            var inputweights = LearnedNN.InputWeights;
            var hiddenbias1 = LearnedNN.HiddenBias1;
            var hiddenbias2 = LearnedNN.HiddenBias2;
            var hiddenweights1 = LearnedNN.HiddenWeights1;
            var hiddenweights2 = LearnedNN.HiddenWeights2;


            for (int c = 0; c < inputbias.Colmun; c++)
            {
                inputbias[0, c] = float.Parse(sr.ReadLine());
            }
            for (int r = 0; r < inputweights.Row; r++)
            {
                for (int c = 0; c < inputweights.Colmun; c++)
                {
                    inputweights[r, c] = float.Parse(sr.ReadLine());
                }
            }
            for (int c = 0; c < hiddenbias1.Colmun; c++)
            {
                hiddenbias1[0, c] = float.Parse(sr.ReadLine());

            }
            for (int r = 0; r < hiddenweights1.Row; r++)
            {
                for (int c = 0; c < hiddenweights1.Colmun; c++)
                {
                    hiddenweights1[r, c] = float.Parse(sr.ReadLine());

                }
            }
            for (int c = 0; c < hiddenbias2.Colmun; c++)
            {
                hiddenbias2[0, c] = float.Parse(sr.ReadLine());

            }
            for (int r = 0; r < hiddenweights2.Row; r++)
            {
                for (int c = 0; c < hiddenweights2.Colmun; c++)
                {
                    hiddenweights2[r, c] = float.Parse(sr.ReadLine());

                }
            }
        }
        finally
        {
            sr.Close();
        }
        return LearnedNN;

    }

}
