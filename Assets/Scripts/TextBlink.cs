using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextBlink : MonoBehaviour {

    Text component;


    // Use this for initialization
    void Start () {
        component = GetComponent<Text>();
    }
    
    // Update is called once per frame
    void Update () {
        //component.color.a = ;
        component.color = new Color(component.color.r, component.color.g, component.color.b, 0.5f + Mathf.Cos(Time.frameCount * (Mathf.PI / 60))/2);
    }
}
