using Helpers;
using TMPro;
using UI;
using UnityEngine;

namespace WorkbenchBuyer
{
    public class BuyerUI : InfoUI
    {
        [SerializeField] private BuyerController buyerController;
        [SerializeField] private TextMeshProUGUI priceTag;

        protected override void Start()
        {
            base.Start();
            UpdatePriceTag(buyerController.GetCurrentPrice());
        }
        
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
            var data = ResourceCountHelper.CountShortener(price, "Цена: ");
            priceTag.SetText(data.formatTemplate, data.value);
        }
    }
}
