using UnityEngine;
using UnityEngine.UI;

namespace FLOBUK.StoreSimulator
{
    [DisallowMultipleComponent]
    public class ExpansionTabButtonLink : MonoBehaviour
    {
        private Button button;
        private UIShopCategoryHelper categoryHelper;
        private GameObject targetPanel;

        public void Configure(UIShopCategoryHelper helper, GameObject panel)
        {
            categoryHelper = helper;
            targetPanel = panel;
            EnsureListener();
        }

        void Awake()
        {
            EnsureListener();
        }

        void OnEnable()
        {
            EnsureListener();
        }

        void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnButtonClicked);
        }

        private void EnsureListener()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (button == null)
                return;

            button.onClick.RemoveListener(OnButtonClicked);
            button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (categoryHelper != null && targetPanel != null)
                categoryHelper.Show(targetPanel);
        }
    }
}
