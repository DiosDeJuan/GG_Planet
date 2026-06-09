//Adaptado por POMPIC 20100333
namespace FLOBUK.StoreSimulator
{
    public class ShoplifterInteractableProxy : Interactable
    {
        private ShoplifterAgent agent;

        public void Configure(ShoplifterAgent shoplifterAgent)
        {
            agent = shoplifterAgent;
        }

        public override void OnBecameFocus()
        {
            if (agent != null)
                agent.OnBecameFocus();
        }

        public override bool Interact(string actionName)
        {
            return agent != null && agent.Interact(actionName);
        }

        public override void OnLostFocus()
        {
            if (agent != null)
                agent.OnLostFocus();
        }
    }
}
