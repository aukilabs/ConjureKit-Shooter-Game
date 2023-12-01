using TMPro;
using UnityEngine;

namespace ConjureKitShooter.UI
{
    public class ScoreEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText, scoreText;

        public void UpdateContent(string name, int score)
        {
            nameText.SetText(name);
            scoreText.SetText(score.ToString("0000000"));
        }
    }
}