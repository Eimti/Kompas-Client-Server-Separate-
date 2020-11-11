﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KompasClient.UI
{
    public class ClientEscapeMenuUIController : MonoBehaviour
    {
        public void Update()
        {
            //if this is active and the player hits escape, go away
            if (Input.GetKeyDown(KeyCode.Escape)) Cancel();
        }

        public void Enable() => gameObject.SetActive(true);

        public void GoToMainMenu() => SceneManager.LoadScene(MainMenuUICtrl.MainMenuScene);

        public void Exit() => Application.Quit();

        public void Cancel() => gameObject.SetActive(false);
    }
}