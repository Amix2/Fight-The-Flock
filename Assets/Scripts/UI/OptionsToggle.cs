using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsToggle : MonoBehaviour
{

    public GameObject settilgsList;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Toggle>().onValueChanged.AddListener((bool val) =>
        {
            settilgsList.SetActive(val);
        });
    }

}
