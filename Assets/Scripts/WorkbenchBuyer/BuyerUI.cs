using Helpers;
using TMPro;
using UI;
using UnityEngine;

namespace WorkbenchBuyer
{
    // TODO: переделать под новую архитектуру
    public class BuyerUI : InfoUI
    {
        [SerializeField] private BuyerController buyerController;
        [SerializeField] private TextMeshProUGUI priceTag;

        protected override void OnEnable()
        {
            base.OnEnable();
            buyerController.OnPriceChange += UpdatePriceTag;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            buyerController.OnPriceChange -= UpdatePriceTag;
        }

        private void UpdatePriceTag(long price)
        {
            priceTag.text = $"Цена: {ResourceCountHelper.CountShortener(price)}";
        }
    }
}
