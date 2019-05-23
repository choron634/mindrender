using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private int step = 0;
    private int Step { get { return step; } set { step = value; } }

    public bool IsDone { get; private set; }
    public float rewardvalue;
    public float Reward { get { return Mathf.Max(rewardvalue, 0); } private set { Reward = value; } }


    public void SetReward(float reward) {
        rewardvalue = reward;
    }

    public void AddReward(float reward) {
        rewardvalue += reward;
    }

    public virtual List<float> CollectObservations() {
        return new List<float>();
    }

    public virtual void AgentAction(float[] vectorAction) {
    }

    public virtual void AgentReset() {
    }

    public void Reset() {
        IsDone = false;
        AgentReset();
    }

    public void Done() {
        IsDone = true;
        Step++;
    }
}
