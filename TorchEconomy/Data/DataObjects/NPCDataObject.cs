using TorchEconomy.Data.Schema;
using TorchEconomy.Data.Types;

namespace TorchEconomy.Data.DataObjects
{
    public class NPCDataObject : IDataObject
    {
        [PrimaryKey]
        public long Id { get; set; }
        [Required]
        [StringLength(64)]
        public string Name { get; set; }
        [Required]
        [DefaultValue(false)]
        public bool IsDeleted { get; set; }
        
        /// <summary>
        /// Variable for NPC AI-stuff.
        /// </summary>
        [EnumAsByte]
        public IndustryTypeEnum IndustryType { get; set; }
    }
}