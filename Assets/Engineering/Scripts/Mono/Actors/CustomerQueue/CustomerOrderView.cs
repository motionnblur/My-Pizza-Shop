using TMPro;
using UnityEngine;

namespace Engineering.Scripts.Mono.Actors.CustomerQueue
{
    public class CustomerOrderView : MonoBehaviour
    {
        [SerializeField] private TMP_Text orderText;
        [SerializeField] private string orderTextFormat = "Pizza: {0}";
        [SerializeField] private string noSeatMessage = "NO SEAT!";

        public void ShowRemainingPizzas(int remaining)
        {
            if (orderText == null)
                return;

            orderText.text = string.Format(orderTextFormat, remaining);
            orderText.gameObject.SetActive(remaining > 0);
        }

        public void ShowNoSeatMessage()
        {
            if (orderText == null)
                return;

            orderText.text = noSeatMessage;
            orderText.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (orderText == null)
                return;

            orderText.gameObject.SetActive(false);
        }
    }
}
