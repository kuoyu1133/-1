using UnityEngine;
using TMPro;

public class StatRowUI : MonoBehaviour
{
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI valueText;

    public void SetInfo(string label, string value)
    {
        labelText.text = label;
        valueText.text = value;
    }
}