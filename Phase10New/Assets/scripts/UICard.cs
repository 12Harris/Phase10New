using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class UICard : MonoBehaviour
{
    private Vector3 initialPosition;
    public Vector3 InitialPosition {get => initialPosition; set => initialPosition = value;}
    private bool initialized = false;
    public bool Initialized => initialized;
    private Image displayImage;
    public Image DisplayImage => displayImage;
    private TextMeshProUGUI displayText;
    public TextMeshProUGUI  DisplayText => displayText;
    private Player owner;
    public Player Owner {get => owner; set => owner = value;}

    public static event Action _onUICardInitialized;

    public void Start()
    {

    }

    public void Initialize()
    {
        initialPosition = transform.position;
        displayImage = transform.Find("Image").GetComponent<Image>();
        displayText = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        enableHighLight(false);
        _onUICardInitialized?.Invoke();
    }

    public void Update()
    {
        if(!initialized)
        {
            Initialize();
            
            initialized = true;
        }
    }

    public void enableHighLight(bool enable)
    {
        transform.Find("HighLight").GetComponent<Image>().enabled = enable;
    }

    public bool Highlighted()
    {
        return transform.Find("HighLight").GetComponent<Image>().enabled;
    }
}