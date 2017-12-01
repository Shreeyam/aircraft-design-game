using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class DesignerUIManager : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    private void Awake()
    {
        HideAllPanels();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SwitchToPanel(string name)
    {
        HideAllPanels();

        var canvasGroup = transform
            .Find(name)
            .GetComponent<CanvasGroup>();

        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

    }

    void HideAllPanels()
    {
        foreach (Transform child in transform)
        {
            if (child.name.EndsWith("Panel") && child.name != "MainPanel")
            {
                var canvasGroup = child.GetComponent<CanvasGroup>();

                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
