using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image progressBarFill; 

    public void SetProgress(float progress)
        {
            progressBarFill.fillAmount = progress;
        }

}
