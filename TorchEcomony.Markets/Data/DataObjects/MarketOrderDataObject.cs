using Sandbox.Definitions;
using TorchEconomy.Data;
using TorchEconomy.Data.DataObjects;
using TorchEconomy.Data.Schema;
using TorchEconomy.Markets.Data.Types;
using TorchEconomy.Markets.Managers;
using VRage.Game;

namespace TorchEconomy.Markets.Data.DataObjects
{
    public class MarketOrderDataObject : IDataObject
    {
        [PrimaryKey]
        public long Id { get; set; }
        [EnumAsByte]
        [Required]
        public BuyOrderType OrderType { get; set; }

        [Required] [StringLength(128)]
        public string DefinitionId { get; set; }

        private MyDefinitionId? _cachedDefinitionId;
        [Ignore]
        public MyDefinitionId MyDefinitionId
        {
            get
            {
                if(_cachedDefinitionId == null)
                    _cachedDefinitionId = MyDefinitionId.Parse(DefinitionId);
                return _cachedDefinitionId.Value;
            }
        }
        [Required]
        public decimal Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public long MarketId { get; set; }
        [Required]
        public int CreatedOn { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }

        public string ToString(MarketSimManager simManager, int maxNameLength)
        {
            var definition = MyDefinitionManager.Static.GetDefinition(MyDefinitionId);
            var orderQuantity = (Quantity + "x").PadLeft(6);
            var valueDifference =
                simManager.GetOrCalculateUniversalItemValue(MyDefinitionId);
            return
                $"{definition.DisplayNameText.PadRight(maxNameLength)} {orderQuantity}: {Utilities.FriendlyFormatCurrency(Price)} ({Utilities.FriendlyFormatCurrency(valueDifference)})";
        }
    }
}
