using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public int MaxSteps { get; set; }
    public int CurrentStep { get; set; }
    public int EpisodeCount { get; set; }

    [SerializeField] private float timeScale = 1.0f;
    private float TimeScale {
        get {
            return timeScale;
        }
        set {
            timeScale = value;
            if(Application.isPlaying) {
                Time.timeScale = timeScale;
            }
        }
    }

    [SerializeField] private Brain brain = null;
    public Brain Brain { get { return brain; } }

    private List<Agent> Agents { get; set; } = new List<Agent>();

    private void Start() {
        Time.timeScale = TimeScale;
        Brain.CreateTable();
        Reset();
    }

    private void FixedUpdate() {
        Step();
    }

    public virtual void Step() {
        CurrentStep++;
        if(MaxSteps > 0) {
            if(CurrentStep >= MaxSteps) {
                Reset();
                return;
            }
        }

        AgentResetIfDone();

        SendSate();

        AgentAction();
    }

    public virtual void Reset() {
        Agents.ForEach(a => a.Reset());
        CurrentStep = 0;
        EpisodeCount++;

        EnvironmentReset();
        EndReset();
    }

    public void SetTimeScale(float timeScale) {
        TimeScale = timeScale;
    }

    public virtual void EnvironmentReset() {

    }

    public virtual void EndReset() {
        Agents.Clear();
        Agents.AddRange(FindObjectsOfType<Agent>());

        SendSate();
    }

    private void SendSate() {
        Agents.ForEach(a => {
            Brain.SendState(a.CollectObservations(), a.Reward, a.IsDone);
        });
    }

    private void AgentResetIfDone() {
        foreach(var a in Agents.Where(a => a.IsDone)) {
            a.Reset();
        }
    }

    private void AgentAction() {
        var action = Brain.GetAction();
        Agents.ForEach(a => a.AgentAction(action));
    }

    public void SaveBrain() {
        
    }
}
