using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class VacuumEnv : MonoBehaviour
{
    [SerializeField] private DustController dust = null;
    private DustController Dust { get { return dust; } }

    [SerializeField] private VacuumAgent agent = null;
    private VacuumAgent Agent { get { return agent; } }

    public void FixedUpdate() {
        // お掃除ロボット、ゴミをリセット
        if(Agent.IsDead) {
            Agent.ResetVacuum();
            Dust.ResetDust();
        }
    }
}
