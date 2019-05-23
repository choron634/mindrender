using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Brain : ScriptableObject
{
    [SerializeField] private int stateSize = 1;
    private int StateSize { get { return stateSize; } }

    [SerializeField] private int actionSize = 1;
    private int ActionSize { get { return actionSize; } }

    [SerializeField] private float epsilon = 1.0f;
    [SerializeField] private float epsilonMin = 0.0f;

    private float[][] QTable { get; set; }
    // Initial epsilon value for random action selection.}
    private float Epsilon { get; set; } = 1.0f;
    // Lower bound of epsilon.
    private float EpsilonMin { get; set; } = 0.0f;
    // Number of steps to lower e to eMin.
    private int AnnealingSteps { get; set; } = 20000;
    // Discount factor for calculating Q-target.
    private float Gamma { get; set; } = 0.99f;
    // The rate at which to update the value estimates given a reward.
    private float ETA { get; set; } = 0.9f;
    private int LastState { get; set; }
    private int CurrentAction { get; set; }

    public void CreateTable() {
        Epsilon = epsilon;
        EpsilonMin = epsilonMin;
        LastState = 0;
        CurrentAction = -1;
        QTable = new float[StateSize][];

        for(int i = 0; i < StateSize; i++) {
            QTable[i] = new float[ActionSize];
        }
    }

    public float[] GetAction() {
        var action = QTable[LastState].ToList().IndexOf(QTable[LastState].Max());

        if(Epsilon <= Random.Range(0.0f, 1.0f)) {
            CurrentAction = QTable[LastState].ToList().IndexOf(QTable[LastState].Max());
        }
        else {
            CurrentAction = Random.Range(0, ActionSize);
        }

        if(Epsilon > EpsilonMin) {
            Epsilon = Epsilon - ((1f - EpsilonMin) / AnnealingSteps);
        }

        CurrentAction = action;
        return new float[1] { action };
    }

    public float[] GetValue() {
        float[] value_table = new float[QTable.Length];
        for(int i = 0; i < QTable.Length; i++) {
            value_table[i] = QTable[i].Average();
        }
        return value_table;
    }

    public void SendState(List<float> state, float reward, bool done) {
        int nextState = Mathf.FloorToInt(state.First());
        if(CurrentAction != -1) {
            if(done == true) {
                QTable[LastState][CurrentAction] += ETA * (reward - QTable[LastState][CurrentAction]);
            }
            else {
                QTable[LastState][CurrentAction] += ETA * (reward + Gamma * QTable[nextState].Max() - QTable[LastState][CurrentAction]);
            }
        }
        LastState = nextState;
    }

    public void BrainReset() {
        Epsilon = epsilon;
    }

    public void Save(string path) {
        using(var bw = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate))) {
            for(int i = 0; i < StateSize; i++) {
                for(int j = 0; j < ActionSize; j++) {
                    bw.Write(QTable[i][j]);
                }
            }
        }
    }

    public void Load(string path) {
        using(var br = new BinaryReader(new FileStream(path, FileMode.Open))) {
            QTable = new float[StateSize][];
            for(int i = 0; i < StateSize; i++) {
                QTable[i] = new float[ActionSize];
                for(int j = 0; j < ActionSize; j++) {
                    QTable[i][j] = br.ReadSingle();
                }
            }
        }
    }
}
