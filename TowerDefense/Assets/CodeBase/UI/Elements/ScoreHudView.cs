using System;
using CodeBase.Services.Score;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.HUD
{
    public class ScoreHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;

        private IScoreService _scoreService;

        public void Init(IScoreService scoreService)
        {
            _scoreService = scoreService;
            UpdateText(_scoreService.Score);
        }

        private void OnEnable()
        {
            if (_scoreService != null)
            {
                _scoreService.OnScoreChanged -= UpdateText;
                _scoreService.OnScoreChanged += UpdateText;
                UpdateText(_scoreService.Score);
            }
        }

        private void OnDisable()
        {
            if (_scoreService != null)
                _scoreService.OnScoreChanged -= UpdateText;
        }

        private void UpdateText(int value)
        {
            if (scoreText != null)
                scoreText.text = value.ToString();
        }
    }
}