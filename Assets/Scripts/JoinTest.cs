using UnityEngine;
using TMPro;

public class JoinTest : MonoBehaviour
{
    public TMP_InputField inputField;

    public void OnJoinPressed()
    {
        Debug.Log("Entered Code: " + inputField.text);
    }
}
