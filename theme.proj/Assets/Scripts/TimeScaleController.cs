﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    public void OnTimeScaleChanged(float timeScale) {
        Time.timeScale = timeScale;
    }
}
