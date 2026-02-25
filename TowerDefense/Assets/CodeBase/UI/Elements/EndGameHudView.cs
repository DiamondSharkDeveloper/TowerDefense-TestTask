using UnityEngine;

namespace CodeBase.UI.HUD
{
    public class EndGameHudView : MonoBehaviour
    {
        [SerializeField] private GameObject winRoot;
        [SerializeField] private GameObject loseRoot;

        public void HideAll()
        {
            if (winRoot != null) winRoot.SetActive(false);
            if (loseRoot != null) loseRoot.SetActive(false);
        }

        public void ShowWin()
        {
            HideAll();
            if (winRoot != null) winRoot.SetActive(true);
        }

        public void ShowLose()
        {
            HideAll();
            if (loseRoot != null) loseRoot.SetActive(true);
        }
    }
}