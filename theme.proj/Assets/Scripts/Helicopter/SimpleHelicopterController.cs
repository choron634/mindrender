using UnityEngine;

public class SimpleHelicopterController : MonoBehaviour
{
    public AudioSource HelicopterSound;
    public Rigidbody HelicopterModel;
    public HeliRotorController MainRotorController;
    public HeliRotorController SubRotorController;

    public float TurnForce = 3f;
    public float ForwardForce = 10f;
    public float ForwardTiltForce = 20f;
    public float TurnTiltForce = 30f;
    public float EffectiveHeight = 150f;

    public float turnTiltForcePercent = 1.5f;
    public float turnForcePercent = 1.3f;

    public float Torque { get; set; }

    private float _engineForce;
    public float EngineForce {
        get { return _engineForce; }
        set {
            MainRotorController.RotarSpeed = value * 80;
            SubRotorController.RotarSpeed = value * 40;
            HelicopterSound.pitch = Mathf.Clamp(value / 40, 0, 1.2f);
            _engineForce = value;
        }
    }

    private Vector2 hMove = Vector2.zero;
    private Vector2 hTilt = Vector2.zero;
    private float hTurn = 0f;
    public bool IsOnGround = true;

    // Use this for initialization
    void Start() {
    }

    void Update() {
    }

    void FixedUpdate() {
        LiftProcess();
        MoveProcess();
        TiltProcess();
    }

    private void MoveProcess() {
        var turn = TurnForce * Mathf.Lerp(hMove.x, hMove.x * (turnTiltForcePercent - Mathf.Abs(hMove.y)), Mathf.Max(0f, hMove.y));
        hTurn = Mathf.Lerp(hTurn, turn, Time.fixedDeltaTime * TurnForce);
        HelicopterModel.AddRelativeTorque(0f, hTurn * HelicopterModel.mass, 0f);
        HelicopterModel.AddRelativeForce(Vector3.forward * Mathf.Max(0f, hMove.y * ForwardForce * HelicopterModel.mass));
    }

    private void LiftProcess() {
        var upForce = 1 - Mathf.Clamp(HelicopterModel.transform.position.y / EffectiveHeight, 0, 1);
        upForce = Mathf.Lerp(0f, EngineForce, upForce) * HelicopterModel.mass;
        HelicopterModel.AddRelativeForce(Vector3.up * upForce);
    }

    private void TiltProcess() {
        hTilt.x = Mathf.Lerp(hTilt.x, hMove.x * TurnTiltForce, Time.deltaTime);
        hTilt.y = Mathf.Lerp(hTilt.y, hMove.y * ForwardTiltForce, Time.deltaTime);
        HelicopterModel.transform.localRotation = Quaternion.Euler(hTilt.y, HelicopterModel.transform.localEulerAngles.y, -hTilt.x);
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="x">x軸の移動量</param>
    /// <param name="y">y軸の移動量</param>
    /// <param name="DeltaEngineForce">EngineForceの変化</param>
    /// <param name="torque">回転速度</param>
    public void Move(float x, float y, float DeltaEngineForce, float torque) {
        Torque = torque;
        EngineForce += Mathf.Max(DeltaEngineForce/5, 0);
        float tempY = 0;
        float tempX = 0;

        if (hMove.y > 0) {
            hMove.y -= Time.fixedDeltaTime;
        }
        else if(hMove.y < 0) {
            hMove.y += Time.fixedDeltaTime;
        }

        if(hMove.x > 0) {
            hMove.x -= Time.fixedDeltaTime;
        }
        else if(hMove.x < 0) {
            hMove.x += Time.fixedDeltaTime;
        }

        if (!IsOnGround)
        {
            tempX = x;
        }

        if (!IsOnGround)
        {
            tempY = y;
        }


        if (!IsOnGround)
        {
            var force = torque * (turnForcePercent-hMove.y)*HelicopterModel.mass;
            HelicopterModel.AddRelativeTorque(0f, force, 0);
        }

        /*
        if (x > 0.3)
        {
            if (!IsOnGround)
            {
                tempX = Time.fixedDeltaTime;
            }
        }
        if (x < -0.3)
        {
            if (!IsOnGround)
            {
                tempX = -Time.fixedDeltaTime;
            }
        }

        if (y > 0.3)
        {
            if (!IsOnGround)
            {
                tempY = Time.fixedDeltaTime;
            }
        }
        if (y < -0.3)
        {
            if (!IsOnGround)
            {
                tempY = -Time.fixedDeltaTime;
            }
        }

        if(torque >= 0.2)
        {
            if (!IsOnGround)
            {
                var force = (turnForcePercent - Mathf.Abs(hMove.y)) * HelicopterModel.mass;
                HelicopterModel.AddRelativeTorque(0f, force, 0);
            }
        }
        else if(torque <= -0.2)
        {
            if (!IsOnGround)
            {
                var force = -(turnForcePercent - Mathf.Abs(hMove.y)) * HelicopterModel.mass;
                HelicopterModel.AddRelativeTorque(0f, force, 0);
            }

        }*/

        hMove.x += tempX;
        hMove.y += tempY;

        hMove.x = Mathf.Clamp(hMove.x, -1, 1);
        hMove.y = Mathf.Clamp(hMove.y, -1, 1);

    }

    public void Action(PressedKeyCode[] obj) {
        float tempY = 0;
        float tempX = 0;

        // stable forward
        if(hMove.y > 0)
            tempY = -Time.fixedDeltaTime;
        else
            if(hMove.y < 0)
            tempY = Time.fixedDeltaTime;

        // stable lurn
        if(hMove.x > 0)
            tempX = -Time.fixedDeltaTime;
        else
            if(hMove.x < 0)
            tempX = Time.fixedDeltaTime;


        foreach(var pressedKeyCode in obj) {
            switch(pressedKeyCode) {
                case PressedKeyCode.SpeedUpPressed:

                    EngineForce += 0.1f;
                    break;
                case PressedKeyCode.SpeedDownPressed:

                    EngineForce -= 0.12f;
                    if(EngineForce < 0) EngineForce = 0;
                    break;

                case PressedKeyCode.ForwardPressed:

                    if(IsOnGround) break;
                    tempY = Time.fixedDeltaTime;
                    break;
                case PressedKeyCode.BackPressed:

                    if(IsOnGround) break;
                    tempY = -Time.fixedDeltaTime;
                    break;
                case PressedKeyCode.LeftPressed:

                    if(IsOnGround) break;
                    tempX = -Time.fixedDeltaTime;
                    break;
                case PressedKeyCode.RightPressed:

                    if(IsOnGround) break;
                    tempX = Time.fixedDeltaTime;
                    break;
                case PressedKeyCode.TurnRightPressed: {
                        if(IsOnGround) break;
                        var force = (turnForcePercent - Mathf.Abs(hMove.y)) * HelicopterModel.mass;
                        HelicopterModel.AddRelativeTorque(0f, force, 0);
                    }
                    break;
                case PressedKeyCode.TurnLeftPressed: {
                        if(IsOnGround) break;

                        var force = -(turnForcePercent - Mathf.Abs(hMove.y)) * HelicopterModel.mass;
                        HelicopterModel.AddRelativeTorque(0f, force, 0);
                    }
                    break;

            }
        }

        hMove.x += tempX;
        hMove.x = Mathf.Clamp(hMove.x, -1, 1);

        hMove.y += tempY;
        hMove.y = Mathf.Clamp(hMove.y, -1, 1);

    }

    public void Stop() {
        hTilt = Vector2.zero;
        hMove = Vector2.zero;
        EngineForce = 0;
        HelicopterModel.velocity = Vector3.zero;
        HelicopterModel.angularVelocity = Vector3.zero;
    }

    private void OnCollisionEnter() {
        IsOnGround = true;
    }

    private void OnCollisionExit() {
        IsOnGround = false;
    }
}
