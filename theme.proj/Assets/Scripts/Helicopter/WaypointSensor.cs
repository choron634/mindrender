using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WaypointSensor : MonoBehaviour
{
    [SerializeField] private string layerName = "wall";
    private string LayerName { get { return layerName; } }

    [SerializeField] public GameObject[] points = null;
    public GameObject[] Points { get { return points; } }


    public List<Vector3> GetPosition()
    {
        var list = new List<Vector3>();
        Array.ForEach(points, p =>
        {
            if (p.activeSelf)
            {
                list.Add(p.transform.position);
            }
            else
            {
                list.Add(Vector3.zero);//これはびみょうかも
            }
        });
        return list;
    }

    public List<int> GetObjects() {
        var list = new List<int>();
        Array.ForEach(Points, p => {
            if(p.activeSelf) {
                var dir = (p.transform.position - transform.position).normalized;
                var distance = Vector3.Distance(p.transform.position, transform.position);
                var hits = Physics.RaycastAll(transform.position, dir, distance, 1 << LayerMask.NameToLayer(LayerName));
                list.Add(hits.Length);
#if UNITY_EDITOR
                Debug.DrawRay(transform.position, dir * distance, Color.blue, 0.1f);
#endif
            }
            else {
                list.Add(0);
            }
        });
        return list;
    }

    public List<float> GetDistance() {
        var list = new List<float>();
        Array.ForEach(Points, p => {
            if(p.activeSelf) {
                list.Add(Vector3.Distance(transform.position, p.transform.position));
            }
            else {
                list.Add(0);
            }
        });

        return list;
    }

    public List<Vector3> GetVector()
    {
        var list = new List<Vector3>();
        Array.ForEach(Points, p =>
        {
            if (p.activeSelf)
            {
                list.Add(p.transform.position - transform.position);
            }
            else
            {
                list.Add(Vector3.zero);//これはびみょうかも
            }
        });
        return list;
    }

    public List<float> GetHorizontalAngles()
    {
        var position = new Vector2(transform.position.x, transform.position.z);
        var forward = new Vector2(transform.forward.x, transform.forward.z);
        var list = new List<float>();
        Array.ForEach(Points, p => {
            if (p.activeSelf)
            {
                var v = new Vector2(p.transform.position.x, p.transform.position.z);
                var a = forward;
                var b = v - position;

                var axis = Vector2Cross(a, b);

                var angle = Vector2.Angle(a, b)* (axis < 0 ? -1 : 1);
                list.Add(angle / 180.0f);
            }
            else
            {
                list.Add(0);
            }
        });
        return list;
    }

    public List<float> GetVelocityAngles()
    {
        var HeliRb = this.transform.GetComponent<Rigidbody>();
        var position = transform.position;
        var velocity = HeliRb.velocity;
        //var horizontalvelocity = new Vector2(velocity.x, velocity.y);
        var list = new List<float>();
        Array.ForEach(Points, p => {
            if (p.activeSelf)
            {
                var v = p.transform.position;
                var a = velocity;
                var b = v - position;

                var axis = Vector3.Cross(a, b);

                var angle = Vector3.Angle(a, b) * (axis.y < 0 ? -1 : 1);
                list.Add(angle / 180.0f);
            }
            else
            {
                list.Add(0);
            }
        });
        return list;
    }


    public void WaypointReset() {
        Array.ForEach(Points, p => p.SetActive(true));
    }

    public static float Vector2Cross(Vector2 lhs, Vector2 rhs)
    {
        return lhs.x * rhs.y - rhs.x * lhs.y;
    }
}
