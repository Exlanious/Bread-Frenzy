using UnityEngine;

public class TimeScaleDebug : MonoBehaviour
{
    private float last;

    void Update()
    {
        if (!Mathf.Approximately(last, Time.timeScale))
        {
            last = Time.timeScale;
            Debug.Log("TimeScale changed to: " + last);
        }
    }
}
