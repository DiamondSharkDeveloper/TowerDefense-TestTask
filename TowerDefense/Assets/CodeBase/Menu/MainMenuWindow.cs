using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace CodeBase.Menu
{
    public class MainMenuWindow : WindowBase
    {
        [SerializeField] private Button buttonPrefab;
        [SerializeField] private GameObject content;
        private List<Button> _buttons=new List<Button>();
        private bool _isGameRun;
        

        public void Init(List<MenuButtons> buttonsList,bool isGameRun)
        {
            _isGameRun = isGameRun;
            foreach (MenuButtons menuButtons in buttonsList)
            {
                Button button = Instantiate(buttonPrefab, content.transform);
                button.name = menuButtons.Name;
                button.onClick.AddListener(menuButtons.Action.Invoke);
                button.GetComponentInChildren<TextMeshProUGUI>().text = menuButtons.Name;
                button.gameObject.SetActive(true);
                _buttons.Add(button);
            }
        }
        
    }
}