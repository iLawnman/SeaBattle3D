using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;

public class MobileController : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern bool IsMobileBrowser();
    [DllImport("__Internal")]
    private static extern Vector2 outScreen();

    public GameObject PlayerFeild;
    public GameObject otherPlayerField;
    public Transform playerHposition;
    public Transform otherHposition;
    public Transform playerVposition;
    public Transform otherVposition;
    public DeviceType pladform;
    public Text infoText;
    [Multiline]
    public string mobileTxt;
    [Multiline]
    public string nonMobileTxt;

    // Start is called before the first frame update
    void Start()
    {
        if (IsMobileBrowser())
        {
            infoText.text = mobileTxt;
            infoText.text = outScreen().ToString();
        }
        else
            infoText.text = nonMobileTxt;
    }

    // Update is called once per frame
    void Update()
    {
        // if mobile
        if (IsMobileBrowser())
        {
            var xy = outScreen();
            infoText.text = xy.ToString();

            if (xy.x > xy.y)
            {
                ChangePortrait();
            }
            if (xy.x < xy.y)
            {
                ChangeLandscape();
            }
        }
        else
            this.enabled = false;
    }

    void ChangePortrait ()
    {
        PlayerFeild.transform.position = playerHposition.position;
        otherPlayerField.transform.position = otherHposition.position;
        Camera.main.ResetAspect();
    }
    void ChangeLandscape ()
    {
        PlayerFeild.transform.position = playerVposition.position;
        otherPlayerField.transform.position = otherVposition.position;
        Camera.main.ResetAspect();

    }
}
