using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthRotation : MonoBehaviour {

    // Use this for initialization
    Transform transformComponent;

    void Start () {
        transformComponent = GetComponent<Transform>();
    }
    
    // Update is called once per frame
    void Update () {
        transformComponent.Rotate(new Vector3(0, 0, 1), -30f * Time.deltaTime);
    }
}
