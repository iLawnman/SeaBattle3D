using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileController : MonoBehaviour
{
    public GameObject PlayerFeild;
    public GameObject otherPlayerField;
    public Transform playerHposition;
    public Transform otherHposition;
    public Transform playerVposition;
    public Transform otherVposition;
    public DeviceType pladform;

    // Start is called before the first frame update
    void Start()
    {
        pladform = SystemInfo.deviceType;
        Debug.Log(pladform);
    }

    // Update is called once per frame
    void Update()
    {
        // if mobile
        if (pladform == DeviceType.Handheld)
        {
            if (Input.deviceOrientation == DeviceOrientation.Portrait)
                ChangePortrait();
            if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
                ChangeLandscape();
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
