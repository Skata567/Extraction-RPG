using UnityEngine;
using UnityEngine.UI;

namespace PrototypeRT
{
    public class PotionCountUI : MonoBehaviour
    {
        [SerializeField] private Text countText;
        [SerializeField] private string format = "Potion: {0}/{1}";

        public void Configure(Text text)
        {
            countText = text;
        }

        private void Awake()
        {
            if (countText == null)
                countText = GetComponent<Text>();
        }

        private void OnEnable()
        {
            PrototypeRTEvents.OnPotionCountChanged += Refresh;
        }

        private void OnDisable()
        {
            PrototypeRTEvents.OnPotionCountChanged -= Refresh;
        }

        private void Refresh(int current, int max)
        {
            if (countText == null) return;
            countText.text = string.Format(format, current, max);
        }
    }
}
