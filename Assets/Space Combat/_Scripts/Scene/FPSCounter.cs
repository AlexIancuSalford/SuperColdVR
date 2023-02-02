using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private float[] frameDeltaTimeArray;
    private int lastFrameIndex = 0;

    private void Awake()
    {
        frameDeltaTimeArray = new float[50];
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

        Debug.Log(Mathf.RoundToInt(CalculateFPS()).ToString());
    }

    private float CalculateFPS()
    {
        float fps = 0;

        foreach (var frame in frameDeltaTimeArray) 
        {
            fps += frame;
        }

        return frameDeltaTimeArray.Length / fps;
    }
}
