using System;
using CodeBase.Services.Score;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.HUD
{
    public class HUD : MonoBehaviour
    {
         [Header("Score")]
        [SerializeField] private TMP_Text scoreText;

        [Header("Castle HP")]
        [SerializeField] private TMP_Text castleHpText;

        private IScoreService _scoreService;
        private Destructible _castle;

        public void Init(IScoreService scoreService, Destructible castle)
        {
            Unsubscribe();

            _scoreService = scoreService;
            _castle = castle;

            Subscribe();

            UpdateScore(_scoreService != null ? _scoreService.Score : 0);

            if (_castle != null)
                UpdateCastleHp(_castle.hitPointsCurrent, _castle.hitPoints);
        }

        private void OnEnable()
        {
            Subscribe();

            if (_scoreService != null)
                UpdateScore(_scoreService.Score);

            if (_castle != null)
                UpdateCastleHp(_castle.hitPointsCurrent, _castle.hitPoints);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_scoreService != null)
            {
                _scoreService.OnScoreChanged -= UpdateScore;
                _scoreService.OnScoreChanged += UpdateScore;
            }

            if (_castle != null)
            {
                _castle.OnHealthChanged -= UpdateCastleHp;
                _castle.OnHealthChanged += UpdateCastleHp;
            }
        }

        private void Unsubscribe()
        {
            if (_scoreService != null)
                _scoreService.OnScoreChanged -= UpdateScore;

            if (_castle != null)
                _castle.OnHealthChanged -= UpdateCastleHp;
        }

        private void UpdateScore(int value)
        {
            if (scoreText != null)
                scoreText.SetText("Score: {0}", value);
        }

        private void UpdateCastleHp(float current, float max)
        {
            if (castleHpText == null)
                return;

            int c = Mathf.CeilToInt(current);
            int m = Mathf.CeilToInt(max);

            castleHpText.SetText("Castle Health: {0}/{1}", c, m);
        }
    }
}