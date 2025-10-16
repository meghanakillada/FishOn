using UnityEngine;
using TMPro;

public class EndScreen : MonoBehaviour
{
    public TMP_Text finalText;
    public TMP_Text highText;

    void Start()
    {
        int last = PlayerPrefs.GetInt("LAST_SCORE", 0);
        int hi = PlayerPrefs.GetInt("LAST_HIGH", 0);
        if (finalText) finalText.text = $"Final Score: {last}";
        if (highText) highText.text = $"High Score: {hi}";
    }
}