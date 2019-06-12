using Draygo.API;
using TorchEconomySE.Data;

namespace TorchEconomySE.Managers
{
    public class TextHudAPIManager : BaseManager
    {
        private HudAPIv2 _textApi;
        
        public TextHudAPIManager(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public override void Start()
        {
            base.Start();
            
            _textApi = new HudAPIv2(OnRegisteredCallback);
        }

        private void OnRegisteredCallback()
        {
        }
    }
}