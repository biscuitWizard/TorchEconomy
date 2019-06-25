using Torch.API;
using TorchEconomy.Data;

namespace TorchEconomy.Managers
{
    public abstract class BaseManager
    {
        protected IConnectionFactory ConnectionFactory { get; }
        protected ITorchBase Torch
        {
            get { return EconomyPlugin.Instance.Torch; }
        }

        protected EconomyConfig Config
        {
            get { return EconomyPlugin.Instance.Config; }
        }

        public BaseManager(IConnectionFactory connectionFactory)
        {
            ConnectionFactory = connectionFactory;
        }
        
        public virtual void Update() {}
        public virtual void Stop() {}
        /// <summary>
        /// Final initialization.
        /// </summary>
        public virtual void Start() {}
        /// <summary>
        /// Called before Start, for initialization.
        /// </summary>
        public virtual void Awake() {}
        public virtual void Save() {}
    }
}
