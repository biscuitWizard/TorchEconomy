using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Networking;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Torch.Commands;
using TorchEconomy.Markets.Data;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.Network;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace TorchEconomy.ShipTrading.Commands
{
	[Category("econ")]
	public class ShipModule : EconomyCommandModule
	{
		[Command("ships buy", "<gridNameOrId>: Purchases a ship for sale.")]
		public void BuyShip()
		{
			
		}

		[Command("ships info", "Gets information on a ship for sale.")]
		public void InfoShip()
		{
			const int range = 150;
			var character = Context.Player.Character;
			var worldMatrix = character.GetHeadMatrix(true, true, false); // dead center of player cross hairs, or the direction the player is looking with ALT.
			var startPosition = worldMatrix.Translation + worldMatrix.Forward * 0.5f;
			var endPosition = worldMatrix.Translation + worldMatrix.Forward * (range + 0.5f);

			if (!MyAPIGateway.Physics.CastRay(startPosition, endPosition, out var hit))
			{
				Context.Respond("Unable to find ship.");
				return;
			}

			InfoShip(hit.HitEntity as IMyCubeGrid);
		}

		[Command("ships info", "<gridNameOrId>: Gets information on a ship for sale.")]
		public void InfoShip(string gridNameOrId)
		{
			MyCubeGrid shipEntity = null;
			if (!Utilities.TryGetEntityByNameOrId(gridNameOrId, out IMyEntity entity))
			{
				Context.Respond($"Unable to find a station by the name of '{gridNameOrId}'.");
				return;
			}
            
			shipEntity = entity as MyCubeGrid;
			if (shipEntity == null
			    || !shipEntity.IsStatic)
			{
				Context.Respond($"Unable to find a station by the name of '{gridNameOrId}'.");
				return;
			}
			
			InfoShip(shipEntity);
		}

		private void InfoShip(IMyCubeGrid shipGrid)
		{
			
		}
		
		[Command("ships appraise", "Appraises a ship's approximate value.")]
		public void AppraiseShip()
		{
			const int range = 150;
			var character = Context.Player.Character;
			var worldMatrix = character.GetHeadMatrix(true, true, false); // dead center of player cross hairs, or the direction the player is looking with ALT.
			var startPosition = worldMatrix.Translation + worldMatrix.Forward * 0.5f;
			var endPosition = worldMatrix.Translation + worldMatrix.Forward * (range + 0.5f);

			if (!MyAPIGateway.Physics.CastRay(startPosition, endPosition, out var hit))
			{
				Context.Respond("Unable to find ship.");
				return;
			}

			AppraiseShip(hit.HitEntity as IMyCubeGrid);
		}

		[Command("ships appraise", "<gridNameOrId>: Appraises a ship's approximate value.")]
		public void AppraiseShip(string gridNameOrId)
		{
			MyCubeGrid shipEntity = null;
			if (!Utilities.TryGetEntityByNameOrId(gridNameOrId, out IMyEntity entity))
			{
				Context.Respond($"Unable to find a station by the name of '{gridNameOrId}'.");
				return;
			}
            
			shipEntity = entity as MyCubeGrid;
			if (shipEntity == null
			    || !shipEntity.IsStatic)
			{
				Context.Respond($"Unable to find a station by the name of '{gridNameOrId}'.");
				return;
			}
			
			AppraiseShip(shipEntity);
		}

		private void AppraiseShip(IMyCubeGrid shipGrid)
		{
			var marketProvider = EconomyPlugin.GetDataProvider<MarketSimulationProvider>();

			var pricePromise = new Promise((resolve, reject) =>
				{
					int terminalBlocks = 0;
					int armorBlocks = 0;
					decimal shipValue = 0;
					decimal inventoryValue = 0;
					int gridCount = 0;

					var gridComponents = new Dictionary<MyDefinitionId, decimal>();
					var inventoryComponents = new Dictionary<MyDefinitionId, decimal>();

					var grids = shipGrid.GetAttachedGrids(AttachedGrids.Static);
					gridCount = grids.Count;
					foreach (var grid in grids)
					{
						var blocks = new List<IMySlimBlock>();
						grid.GetBlocks(blocks);

						foreach (var block in blocks)
						{
							MyCubeBlockDefinition blockDefintion;
							if (block.FatBlock == null)
							{
								armorBlocks++;
								blockDefintion = MyDefinitionManager.Static.GetCubeBlockDefinition(block.GetObjectBuilder());
							}
							else
							{
								terminalBlocks++;
								blockDefintion = MyDefinitionManager.Static.GetCubeBlockDefinition(block.FatBlock.BlockDefinition);
							}

							//EconomyScript.Instance.ServerLogger.Write("Cube Worth '{0}' '{1}' {2} {3}.", blockDefintion.Id.TypeId, blockDefintion.Id.SubtypeName, block.BuildIntegrity, block.BuildLevelRatio);

							#region Go through component List based on construction level.

							foreach (var component in blockDefintion.Components)
							{
								//EconomyScript.Instance.ServerLogger.Write("Component Worth '{0}' '{1}' x {2}.", component.Definition.Id.TypeId, component.Definition.Id.SubtypeName, component.Count);

								if (!gridComponents.ContainsKey(component.Definition.Id))
									gridComponents.Add(component.Definition.Id, 0);
								gridComponents[component.Definition.Id] += component.Count;
							}

							// This will subtract off components missing from a partially built cube.
							// This also includes the Construction Inventory.
							var missingComponents = new Dictionary<string, int>();
							block.GetMissingComponents(missingComponents);
							foreach (var kvp in missingComponents)
							{
								var definitionid = new MyDefinitionId(typeof(MyObjectBuilder_Component), kvp.Key);
								gridComponents[definitionid] -= kvp.Value;
							}

							#endregion

							if (block.FatBlock != null)
							{
								var cube = (MyEntity) block.FatBlock;

								#region Go through Gasses for tanks and cockpits.

								var tank = cube as IMyGasTank;
								var gasTankDefintion = blockDefintion as MyGasTankDefinition;

								if (gasTankDefintion != null && tank != null)
								{
									decimal volume = (decimal) gasTankDefintion.Capacity * (decimal) tank.FilledRatio;
									if (!inventoryComponents.ContainsKey(gasTankDefintion.StoredGasId))
										inventoryComponents.Add(gasTankDefintion.StoredGasId, 0);
									inventoryComponents[gasTankDefintion.StoredGasId] += volume;
									//MessageClientTextMessage.SendMessage(SenderSteamId, "GAS tank", "{0} detected {1}", gasTankDefintion.StoredGasId, volume);
								}

								// Check through Cockpits.
								var cockpit =
									cube as Sandbox.Game.Entities.MyCockpit; // For some reason, the o2 is on the MyCockpit Class. There is no Interface.
								if (cockpit != null)
								{
									// Hardcoded, because Oxygen and Hydrogen do not have available defintions.
									var oxygenDefintion = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen");
									if (!inventoryComponents.ContainsKey(oxygenDefintion))
										inventoryComponents.Add(oxygenDefintion, 0);
									inventoryComponents[oxygenDefintion] += (decimal) cockpit.OxygenAmount;
									//MessageClientTextMessage.SendMessage(SenderSteamId, "COCKPIT tank", "{0} detected {1}", null, cockpit.OxygenAmount);
								}

								#endregion

								#region Go through all other Inventories for components/items.

								// Inventory check based on normal game access.
								var relation = block.FatBlock.GetUserRelationToOwner(Context.Player.IdentityId);
								if (relation != MyRelationsBetweenPlayerAndBlock.Enemies
								    && relation != MyRelationsBetweenPlayerAndBlock.Neutral)
								{
									for (var i = 0; i < cube.InventoryCount; i++)
									{
										var inventory = cube.GetInventory(i);
										var list = inventory.GetItems();
										foreach (var item in list)
										{
											var id = item.Content.GetId();
											if (!inventoryComponents.ContainsKey(id))
												inventoryComponents.Add(id, 0);
											inventoryComponents[id] += (decimal) item.Amount;
										}
									}
								}

								#endregion
							}
						}
					}

					shipValue = SumComponents(marketProvider, gridComponents);
					inventoryValue = SumComponents(marketProvider, inventoryComponents);

					Context.Respond(
						$"Ship Worth: {shipValue}, Contents Worth: {inventoryValue}, Total Worth: {shipValue + inventoryValue}");
				})
				.Catch(HandleError);
		}

		public void SpawnShip()
		{
			
//			MyEntities.FindFreePlace()
//			MyMultiplayer
//				.RaiseStaticEvent(
//					s =>
//						new Action<List<MyObjectBuilder_CubeGrid>, bool, Vector3, bool, bool, MyCubeGrid.RelativeOffset>(
//							MyCubeGrid.TryPasteGrid_Implementation), list, false, Vector3.Zero, false, true, arg, default(EndpointId),
//					null);
		}
		
		private static decimal SumComponents(MarketSimulationProvider provider, 
			Dictionary<MyDefinitionId, decimal> accumulatedComponents)
		{
			decimal total = 0;
			foreach (var kvp in accumulatedComponents)
			{
				var itemPrice = provider.GetUniversalItemValue(kvp.Key);
				total += kvp.Value * itemPrice;
			}
			return total;
		}
	}
}