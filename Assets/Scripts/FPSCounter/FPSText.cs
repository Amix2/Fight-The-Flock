using System;
using UnityEngine;
using UnityEngine.UI;

public class FPSText : MonoBehaviour
{
    private Text text;

    // Start is called before the first frame update
    private void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    private void Update()
    {
        text.text = $"{Math.Round(Time.smoothDeltaTime * 1000f, 1)}ms";
    }
}