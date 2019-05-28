
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class NN
{
    private float MutationRate { get; set; } = 0.05f;
    private float RandomMin { get; set; } = -1;
    private float RandomMax { get; set; } = 1;

    public Matrix InputBias { get; private set; }
    public Matrix InputWeights { get; private set; }
    public Matrix HiddenBias1 { get; private set; }
    public Matrix HiddenBias2{ get; private set; }
    public Matrix HiddenWeights1 { get; private set; }
    public Matrix HiddenWeights2 { get; private set; }


    public int InputSize { get; private set; }
    public int HiddenSize1 { get; private set; }
    public int HiddenSize2 { get; private set; }
    public int OutputSize { get; private set; }

    public float Reward { get; set; }

    public NN(int inputSize, int hiddenSize1, int hiddensize2, int outputSize) {
        CreateMatrix(inputSize, hiddenSize1, hiddensize2, outputSize);
        InitAllMatrix();//行列をランダムに初期化する
    }

    public NN(NN other) {
        CreateMatrix(other.InputSize, other.HiddenSize1, other.HiddenSize2, other.OutputSize);
        InputBias = other.InputBias.Copy();
        HiddenBias1 = other.HiddenBias1.Copy();
        HiddenBias2 = other.HiddenBias2.Copy();
        InputWeights = other.InputWeights.Copy();
        HiddenWeights1 = other.HiddenWeights1.Copy();
        HiddenWeights2 = other.HiddenWeights2.Copy();
    }

    public (NN child1, NN child2) Crossover(NN other) {
        var c1 = new NN(this);
        var c2 = new NN(other);
        var dna1 = c1.ToDNA();
        var dna2 = c2.ToDNA();

        var cutpoints = c1.Cut(2, dna1.Length);//適当な二つの順位を選んでる

        var ndna1 = new float[dna1.Length];
        var ndna2 = new float[dna2.Length];

        for(int i = 0; i < cutpoints.Length - 1; i++) {//カットポイントでDNAを切って交叉
            var start = cutpoints[i];
            var end = cutpoints[i + 1];
            Array.Copy((i % 2) == 0 ? dna1 : dna2, start, ndna1, start, end - start);
            Array.Copy((i % 2) == 0 ? dna2 : dna1, start, ndna2, start, end - start);
        }

        c1.SetDNA(ndna1);//元の行列の形に戻す
        c2.SetDNA(ndna2);//元の行列の形に戻す

        /*
        for(int i = 0; i < dna1.Length; i++) {
            if(UnityEngine.Random.value < 0.5f) {
                var temp = dna1[i];
                dna1[i] = dna2[i];
                dna2[i] = temp;
            }
        }

        c1.SetDNA(dna1);
        c2.SetDNA(dna2);
        */
        return (c1, c2);//上位二つのNNの子供NNがふたつできた。
    }

    public float[] Predict(float[] inputs) {
        var m = new Matrix(inputs);
        //Debug.Log(m.Row);
        //Debug.Log(m.Colmun);
        //Debug.Log(InputWeights.Colmun);
        var firstLayer = m.Mul(InputWeights);
        for(int c = 0; c < firstLayer.Colmun; c++) {
            firstLayer[0, c] = Tanh(firstLayer[0, c] + InputBias[0, c]);
        }

        var secondLayer = firstLayer.Mul(HiddenWeights1);
        for (int c = 0; c < secondLayer.Colmun; c++)
        {
            secondLayer[0, c] = Tanh(secondLayer[0, c] + HiddenBias1[0, c]);
        }


        var lastLayer = secondLayer.Mul(HiddenWeights2);
        var outputs = new float[OutputSize];
        for(int c = 0; c < OutputSize; c++) {
            outputs[c] = Tanh(lastLayer[0, c] + HiddenBias2[0, c]);
        }

        return outputs;
    }

    private float Sigmoid(double x) {
        return 1 / (1 - Mathf.Exp(-1 * (float)x));
    }

    private float Tanh(float x)
    {
        return (float)System.Math.Tanh(x);
    }

    private void SetDNA(float[] dna, bool mutation = true) {//DNAの形にしたものをもとの意味を持つ行列群に戻す。
        var index = SetDNA(InputBias, dna, 0, mutation);
        index = SetDNA(HiddenBias1, dna, index, mutation);
        index = SetDNA(HiddenBias2, dna, index, mutation);
        index = SetDNA(InputWeights, dna, index, mutation);
        index = SetDNA(HiddenWeights1, dna, index, mutation);
        index = SetDNA(HiddenWeights2, dna, index, mutation);
    }

    public float[] ToDNA() {//dna:[inputbias[], hiddenbias[], inputweights[], hiddenweights[]]
        var dna = new List<float>();
        dna.AddRange(InputBias.ToArray());
        dna.AddRange(HiddenBias1.ToArray());
        dna.AddRange(HiddenBias2.ToArray());
        dna.AddRange(InputWeights.ToArray());
        dna.AddRange(HiddenWeights1.ToArray());
        dna.AddRange(HiddenWeights2.ToArray());
        return dna.ToArray();
    }

    public void Save(string path) {
        using(var bw = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write))) {
            bw.Write(InputSize);
            bw.Write(HiddenSize1);
            bw.Write(HiddenSize2);
            bw.Write(OutputSize);

            var dna = ToDNA();
            bw.Write(dna.Length);
            for(int i = 0; i < dna.Length; i++) {
                bw.Write(dna[i]);
            }
        }
    }

    public void Load(string path) {
        using(var br = new BinaryReader(new FileStream(path, FileMode.Create))) {
            int inputSize = br.ReadInt32();
            int hiddenSize1 = br.ReadInt32();
            int hiddenSize2 = br.ReadInt32();
            int outputSize = br.ReadInt32();
            Debug.Log(inputSize);
            Debug.Log(hiddenSize1);
            Debug.Log(hiddenSize2);
            Debug.Log(outputSize);

            CreateMatrix(inputSize, hiddenSize1, hiddenSize2, outputSize);

            var length = br.ReadInt32();
            var dna = new float[length];
            for(int i = 0; i < length; i++) {
                dna[i] = br.ReadSingle();
            }

            SetDNA(dna, false);
        }
    }

    private void CreateMatrix(int inputSize, int hiddenSize1, int hiddenSize2, int outputSize) {
        InputSize = inputSize;
        HiddenSize1 = hiddenSize1;
        HiddenSize2 = hiddenSize2;
        OutputSize = outputSize;

        InputBias = new Matrix(1, hiddenSize1);
        InputWeights = new Matrix(inputSize, hiddenSize1);
        HiddenBias1 = new Matrix(1, HiddenSize1);
        HiddenWeights1 = new Matrix(HiddenSize1, HiddenSize2);
        HiddenBias2 = new Matrix(1, hiddenSize2);
        HiddenWeights2 = new Matrix(hiddenSize2, outputSize);
    }

    private void InitAllMatrix() {
        InitMatrix(InputBias);
        InitMatrix(HiddenBias1);
        InitMatrix(HiddenBias2);
        InitMatrix(InputWeights);
        InitMatrix(HiddenWeights1);
        InitMatrix(HiddenWeights2);
    }

    private void InitMatrix(Matrix m) {//行列をランダムに初期化
        for(int r = 0; r < m.Row; r++) {
            for(int c = 0; c < m.Colmun; c++) {
                m[r, c] = UnityEngine.Random.Range(RandomMin, RandomMax);
            }
        }
    }

    private int SetDNA(Matrix m, float[] dna, int index, bool mutation) {//突然変異を加味しながらもとの行列に戻している。
        for(int r = 0; r < m.Row; r++) {
            for(int c = 0; c < m.Colmun; c++) {
                m[r, c] = dna[index];
                if(mutation) {
                    if(UnityEngine.Random.value < MutationRate) {
                        m[r, c] = UnityEngine.Random.Range(RandomMin, RandomMax);
                    }
                }
                index++;
            }
        }

        return index;
    }

    private int[] Cut(int n, int length) {//1~lengthの間のランダムな整数が重複なく入った配列を返す。（順序をランダムにする？）
        var set = new SortedSet<int>();

        set.Add(0);
        while(set.Count < n + 1) {
            set.Add(UnityEngine.Random.Range(1, length));
        }
        set.Add(length);
        /*
        var text = "";
        foreach(var a in set) {
            text += string.Format("{0},", a);
        }

        Debug.Log(text);
        */
        var points = new int[set.Count];
        set.CopyTo(points);
        return points;
    }
}
