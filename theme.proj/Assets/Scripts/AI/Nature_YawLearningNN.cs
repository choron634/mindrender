﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using System.Text;


public class Nature_YawLearningNN : MonoBehaviour
{
    [SerializeField] private int totalPopulation = 500;
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

    public NN BestOfBest { get; set; }
    public NN BestOfBestYaw { get; set; }

    public NN BestBrainYaw { get; set; }
    public NN SecondBrainYaw { get; set; }


    private float BestRecord { get; set; }
    private float BestRecordYaw { get; set; }

    private int EliteNumber = 10;



    private int CurrentPopCount { get; set; }
    private int Generation { get; set; }


    [SerializeField] private Agent agent = null;
    private Agent NNAgent { get { return agent; } }

    private List<NN> Children { get; set; } = new List<NN>();
    private List<NN> ChildrenYaw { get; set; } = new List<NN>();


    [SerializeField] private int tournamentSelection = 85;
    private int TournamentSlection { get { return tournamentSelection; } }


    public Text Populationtext;

    private void Start()
    {
        for (int i = 0; i < TotalPopulation; i++)
        {
            ChildrenYaw.Add(new NN(InputSize, HiddenSize1, HiddenSize2, OutputSize));
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
        if (NNAgent.IsDone) {//ベストの更新
            ChildrenYaw[CurrentPopCount].Reward = NNAgent.Reward;

            if (NNAgent.Reward > BestReward) {//一位の塗り替え
                SecondReward = BestReward;
                SecondBrainYaw = BestBrainYaw;
                BestReward = NNAgent.Reward;
                BestBrainYaw = ChildrenYaw[CurrentPopCount];
            }
            else if(NNAgent.Reward > SecondReward) {//二位の塗り替え
                SecondReward = NNAgent.Reward;
                SecondBrainYaw = ChildrenYaw[CurrentPopCount];
            }

            if(BestRecord < BestReward) {//全体を通じてのベスト
                BestRecord = BestReward;
                BestOfBestYaw = BestBrainYaw;
            }

            if (BestOfBestYaw != null)//ベストのセーブ
            {
                SaveBestBrain(BestOfBestYaw, "OnlyYaw.txt");
            }

            CurrentPopCount++;

           
            if (CurrentPopCount == TotalPopulation) {

                ///トーナメント選択
                var YawTournamentMembers = ChildrenYaw.AsEnumerable().OrderBy(x => Guid.NewGuid()).Take(ChildrenYaw.Count).ToList();

                var temp2 = YawTournamentMembers[0];

                YawTournamentMembers.Sort((a, b) => (int)b.Reward - (int)a.Reward);

                var BestBrainInYawTournament = YawTournamentMembers[0];

                var SecondBrainInYawTournament = YawTournamentMembers[1];

                Generation++;
                CurrentPopCount = 0;
                BestReward = 0;
                SecondReward = 0;
                Children.Clear();
                ChildrenYaw.Clear();

                while (ChildrenYaw.Count < TotalPopulation) //if you want to update the yaw NN, remove this coment out.
                {
                    var (c1, c2) = BestBrainInYawTournament.Crossover(SecondBrainInYawTournament);//トーナメント上位2個体の交叉結果の子供
                    ChildrenYaw.Add(c1);
                    ChildrenYaw.Add(c2);
                }
                
                BestBrain = SecondBrain = BestBrainYaw = SecondBrainYaw = null;
                
            }

            NNAgent.Reset();
        }

        var currentYawNN = ChildrenYaw[CurrentPopCount];
        var obsevationsYaw = NNAgent.CollectYawObservations();

        var Yawactions = currentYawNN.Predict(obsevationsYaw.ToArray());//Y軸回りの入力を入れる
        NNAgent.AgentAction(Yawactions);//outputをunity上のagentのactionに
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

    public void SaveBestRecord(int generation, float bestrecord) //create a text file for plot.
    {
        var text = string.Format("{0},{1}\n", generation, bestrecord);
        File.AppendAllText(@"BestRecordforPlot.txt", text);
    }

    public NN ImportNN(string filename)
    {
        FileStream fs = new FileStream(filename, FileMode.Open);
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