using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    public GameObject panel;

    public void OptionsAppear()
    {
        panel.SetActive(true);
    }

    public void OptionsDisappear()
    {
        panel.SetActive(false);
    }
}