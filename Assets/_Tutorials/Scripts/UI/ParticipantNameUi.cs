using UnityEngine;

namespace ConjureKitShooter.UI
{
    public class ParticipantNameUi : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshPro nameDisplay;
        [SerializeField] private TMPro.TextMeshPro scoreDisplay;

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