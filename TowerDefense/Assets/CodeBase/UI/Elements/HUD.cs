using System;
using CodeBase.StaticData;
using CodeBase.UI.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private OpenWindowButton inventoryButton;
        [SerializeField] private OpenWindowButton potionsButton;
        [SerializeField] private OpenWindowButton formulaButton;
        [SerializeField] private Image _overImageSprite;
        private bool _isHoldItem;
        
        public void Hide()
        {
            if (inventoryButton)
            {
                inventoryButton.Button.enabled = false;
                inventoryButton.Button.image.enabled = false;
            }

            if (formulaButton)
            {
                formulaButton.Button.enabled = false;
                formulaButton.Button.image.enabled = false;
            }
        }

        public void Show()
        {
            if (inventoryButton)
            {
                inventoryButton.Button.enabled = true;
                inventoryButton.Button.image.enabled = true;
            }

            if (formulaButton)
            {
                formulaButton.Button.enabled = true;
                formulaButton.Button.image.enabled = true;
            }
        }

        private void Update()
        {
            if (_isHoldItem)
            {
                _overImageSprite.transform.position = new Vector3(Input.mousePosition.x,
                    Input.mousePosition.y, 0);
            }
        }

        public void SetOverCursorImage(Sprite sprite)
        {
            _overImageSprite.sprite = sprite;
            _overImageSprite.color = Color.white;
            _isHoldItem = true;
        }

        public void SetClearOverCursorImage()
        {
            _overImageSprite.color = Color.clear;

            _isHoldItem = false;
        }
    }
}