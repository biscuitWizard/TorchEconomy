using System.Collections.Generic;
using Torch;
using VRage.Collections;
using VRage.Game;

namespace TorchEconomy.Markets
{
	public class EconomyMarketConfig : ViewModel
	{
		public class BlacklistItem
		{
			public string Value { get; set; }
		}
		
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
			
			ValueDefinitionBindings = new ObservableCollection<ValueDefinitionBinding>
			{
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Ice", Value = .01},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Iron", Value = .02},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Nickel", Value = .034},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Silicon", Value = .02},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Cobalt", Value = .05},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Gold", Value = .07},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Silver", Value = .088},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Magnesium", Value = .04},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Stone", Value = 0},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Ice", Value = .01},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Uranium", Value = .08},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Default", Value = .01},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Ore/Platinum", Value = .09},
				
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Component/Research_AdvancedMedicine", Value = 1000},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Component/Research_DataCore", Value = 1000},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Component/Research_AncientUrn", Value = 1000},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Component/Research_PrototypeTechnology", Value = 1000},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Component/Research_ExperimentalChemicals", Value = 1000},
				
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Component/Industrial_RadiationBaffle", Value = 10000},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Component/Industrial_ThermalControlUnit", Value = 10000},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Component/Industrial_PowerConverter", Value = 10000},
				new ValueDefinitionBinding { RawDefinitionId = "MyObjectBuilder_Component/Industrial_MiningEquipment", Value = 10000},
			};
			Blacklist = new ObservableCollection<BlacklistItem>();
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

//		/// <summary>
//		/// How many markets may a player own?
//		/// </summary>
//		private int _playerMarketLimit;
//		public int PlayerMarketLimit
//		{
//			get => _playerMarketLimit;
//			set => SetValue(ref _playerMarketLimit, value);
//		}

		/// <summary>
		/// How much does it cost to open a market?
		/// </summary>
		private decimal _createMarketCost;
		public decimal CreateMarketCost
		{
			get => _createMarketCost;
			set => SetValue(ref _createMarketCost, value);
		}

		/// <summary>
		/// Can players own markets?
		/// </summary>
		private bool _playerOwnedMarkets;
		public bool PlayerOwnedMarkets
		{
			get => _playerOwnedMarkets;
			set => SetValue(ref _playerOwnedMarkets, value);
		}

		private ObservableCollection<ValueDefinitionBinding> _valueDefinitionBindings;
		public ObservableCollection<ValueDefinitionBinding> ValueDefinitionBindings
		{
			get => _valueDefinitionBindings;
			set => SetValue(ref _valueDefinitionBindings, value);
		}

		private ObservableCollection<BlacklistItem> _blacklist;
		public ObservableCollection<BlacklistItem> Blacklist
		{
			get => _blacklist;
			set => SetValue(ref _blacklist, value);
		}
	}
}