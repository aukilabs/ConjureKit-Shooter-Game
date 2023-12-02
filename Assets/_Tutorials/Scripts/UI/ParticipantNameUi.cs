using UnityEngine;

namespace ConjureKitShooter.UI
{
    public class ParticipantNameUi : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text nameDisplay;
        [SerializeField] private TMPro.TMP_Text scoreDisplay;

        public void SetName(string playerName)
        {
            nameDisplay.SetText(playerName);
        }

        public void SetScore(string score)
        {
            scoreDisplay.SetText(score);
        }

        public string GetName()
        {
            return nameDisplay.text;
        }
    }
}