using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField] private int step = 0;
    private int Step { get { return step; } set { step = value; } }

    public bool IsDone { get; private set; }
    public float rewardvalue;
    public float Reward { get { return Mathf.Max(rewardvalue, 0); } set{ rewardvalue = value; } }


    public void SetReward(float reward) {
        rewardvalue = reward;
    }

    public void AddReward(float reward) {
        rewardvalue += reward;
    }

    public virtual List<float> CollectYawObservations()
    {
        return new List<float>();
    }

    public virtual List<float> CollectObservations() {
        return new List<float>();
    }

    public virtual void AgentAction(float[] vectorAction)
    {
    }
    public virtual void AgentAction(float[] vectorAction, float[] vectorAction2) {
    }

    public virtual void AgentReset(bool GenerationChangeflug) {
    }

    public void Reset(bool GenerationChange) {
        IsDone = false;
        AgentReset(GenerationChange);
    }

    public void Done() {
        IsDone = true;
        Step++;
    }
}
