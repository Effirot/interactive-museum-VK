using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Canvas canvas;

    public TextMeshProUGUI TextMeshPro;

    public Image image;

    public Slider slider;

    public TextMeshProUGUI interactionInfo;
    int interactionInfoTime;

    public void setInteractionInfo(string text)
    {
        interactionInfo.text = text;
        interactionInfoTime = 50;
    }

    private void FixedUpdate()
    {
        
        if (interactionInfoTime <= 0)
        {
            interactionInfo.text = "";
        }
        else
        {
            interactionInfoTime -= 1;
            interactionInfo.color = new Color(1F, 1F, 1F, interactionInfoTime / 50F);
        }
    }

    void Start()
    {
        //TextMeshPro.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        //TextMeshPro.text += "1";

        TextMeshPro.rectTransform.position = new Vector3(TextMeshPro.rectTransform.position.x, slider.value * 250F, TextMeshPro.rectTransform.position.z);
    }
}
