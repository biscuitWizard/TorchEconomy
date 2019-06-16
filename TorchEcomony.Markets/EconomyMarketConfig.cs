using System.Collections.Generic;
using Torch;
using VRage.Game;

namespace TorchEconomy.Markets
{
	public class EconomyMarketConfig : ViewModel
	{
		public class ValueDefinitionBinding
		{
			public string RawDefinitionId { get; set; }
			public MyDefinitionId DefinitionId { get => MyDefinitionId.Parse(RawDefinitionId); }
			public double Value { get; set; }
			/// <summary>
			/// If true, this cannot be removed from the data table.
			/// </summary>
			public bool Protected { get; set; }

			public ValueDefinitionBinding()
			{
				Protected = true;
			}
		}
		
		public EconomyMarketConfig()
		{			
			EnergySecondsValue = 10;
			DefaultMarketRange = 3000;
			
			ValueDefinitionBindings = new List<ValueDefinitionBinding>
			{
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Iron", Value = .02},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Nickel", Value = .034},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Silicon", Value = .02},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Cobalt", Value = .05},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Gold", Value = .07},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Silver", Value = .088},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Magnesium", Value = .04},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Stone", Value = 0},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Uranium", Value = .08},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Default", Value = .01},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Platinum", Value = .09},
				new ValueDefinitionBinding { RawDefinitionId = "Component/Research_AdvancedMedicine", Value = 1000},
				new ValueDefinitionBinding { RawDefinitionId = "Component/Research_DataCore", Value = 1000},
				new ValueDefinitionBinding { RawDefinitionId = "Component/Research_AncientUrn", Value = 1000},
				new ValueDefinitionBinding { RawDefinitionId = "Component/Research_PrototypeTechnology", Value = 1000},
				new ValueDefinitionBinding { RawDefinitionId = "Component/Research_ExperimentalChemicals", Value = 1000},
				
				new ValueDefinitionBinding { RawDefinitionId = "Component/Industrial_RadiationBaffle", Value = 10000},
				new ValueDefinitionBinding { RawDefinitionId = "Component/Industrial_ThermalControlUnit", Value = 10000},
				new ValueDefinitionBinding { RawDefinitionId = "Component/Industrial_PowerConverter", Value = 10000},
				new ValueDefinitionBinding { RawDefinitionId = "Component/Industrial_MiningEquipment", Value = 10000},
			};
		}
		private decimal _energySecondsValue;
		public decimal EnergySecondsValue
		{
			get => _energySecondsValue;
			set => SetValue(ref _energySecondsValue, value);
		}

		private float _defaultMarketRange;
		public float DefaultMarketRange
		{
			get => _defaultMarketRange;
			set => SetValue(ref _defaultMarketRange, value);
		}

		private List<ValueDefinitionBinding> _valueDefinitionBindings;
		public List<ValueDefinitionBinding> ValueDefinitionBindings
		{
			get => _valueDefinitionBindings;
			set => SetValue(ref _valueDefinitionBindings, value);
		}
	}
}