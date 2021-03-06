﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;

public class MobileController : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern bool IsMobileBrowser();

    public GameObject PlayerFeild;
    public GameObject otherPlayerField;
    public Transform playerHposition;
    public Transform otherHposition;
    public Transform playerVposition;
    public Transform otherVposition;
    public Text infoText;
    [Multiline]
    public string mobileTxt;
    [Multiline]
    public string nonMobileTxt;

    // Start is called before the first frame update
    void Start()
    {
        infoText.text = nonMobileTxt;

        if (IsMobileBrowser())
        {
            infoText.text = mobileTxt;
            //RotateButton.SetActive(true);
            //PlaceButton.SetActive(true);
        }
    }

    public void ReceivedBrowserData(int orientation)
    {
        if (orientation == 0)
            ChangeLandscape();
        if (orientation == 1)
            ChangePortrait();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ChangeLandscape ()
    {
        PlayerFeild.transform.position = playerHposition.position;
        otherPlayerField.transform.position = otherHposition.position;
        Camera.main.transform.position = new Vector3(0, 13, -3);
    }
    void ChangePortrait ()
    {
        PlayerFeild.transform.position = playerVposition.position;
        otherPlayerField.transform.position = otherVposition.position;
        Camera.main.transform.position = new Vector3(0.5f, 18, -4.5f);


    }
}
