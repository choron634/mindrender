using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayBestBrain : Nature
{
    [SerializeField] private Agent agent = null;
    private Agent NNAgent { get { return agent; } }
    public void OnToggle(bool b)
    {
        if (b)
        {
            var observations = NNAgent.CollectObservations();

            var actions = BestBrain.Predict(observations.ToArray());//学習済みのNNにセンサーからの入力を入れる
            NNAgent.AgentAction(actions);//outputをunity上のagentのactionに//5/12
        }
    }
}
