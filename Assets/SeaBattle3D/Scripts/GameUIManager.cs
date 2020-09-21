﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public GameObject startUI;
    public GameObject winUI;
    public GameObject loseUI;
    public BattleGameManager battleManager;

    // Start is called before the first frame update
    void Start()
    {
        battleManager = FindObjectOfType<BattleGameManager>();
        startUI.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            startUI.SetActive(false);
            battleManager.currentMode = BattleGameManager.GameMode.edit;
        }

        if ((winUI.activeSelf || loseUI.activeSelf) && Input.GetKey(KeyCode.Space))
        {
            winUI.SetActive(false);
            loseUI.SetActive(false);
            battleManager.NewGame();
        }
    }
}
