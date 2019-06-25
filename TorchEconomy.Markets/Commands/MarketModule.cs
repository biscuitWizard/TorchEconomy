using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.ModAPI;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Mod;
using Torch.Mod.Messages;
using TorchEconomy.Data.Types;
using TorchEconomy.Managers;
using TorchEconomy.Markets.Data;
using TorchEconomy.Markets.Data.Models;
using TorchEconomy.Markets.Data.Types;
using TorchEconomy.Markets.Managers;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;

namespace TorchEconomy.Markets.Commands
{
    [Category("econ")]
    public class MarketModule : EconomyCommandModule
    {
        private static readonly Logger Log = LogManager.GetLogger("Economy.Commands.Markets");
        
        [Command("list", "Lists available goods to buy.")]
        [Permission(MyPromoteLevel.None)]
        public void List()
        {
            var marketManager = EconomyPlugin.GetManager<MarketManager>();
            var marketOrderManager = EconomyPlugin.GetManager<MarketOrderManager>();
            var marketSimManager = EconomyPlugin.GetManager<MarketSimulationManager>();
            
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You cannot do that while dead.");
                return;
            }
            
            var controllingCube = Context.Player.Controller.ControlledEntity as IMyCubeBlock;
            if (controllingCube == null)
            {
                Context.Respond("Trading by hand is not supported.");
                return;
            }

            marketManager.GetConnectedMarket(controllingCube.CubeGrid)
                .Then(market =>
                {
                    if (market == null)
                    {
                        Context.Respond("Unable to find any connected markets. Have you docked to a market?");
                        return;
                    }

                    marketOrderManager
                        .GetMarketOrders(market.Id)
                        .Then(orders =>
                        {
                            var dataProvider = GetDataProvider<MarketSimulationProvider>();
                            var responseBuilder = new StringBuilder();
                            responseBuilder.AppendLine();

                            var maxNameLength = 30;
                            
                            responseBuilder.AppendLine("+-- Buy Orders:");
                            var buyOrders = orders.Where(o => o.OrderType == BuyOrderType.Buy 
                                                              || o.OrderType == BuyOrderType.BuyShip).ToArray();
                            if (buyOrders.Length == 0)
                                responseBuilder.AppendLine("None");
                            else
                            {
                                foreach (var order in buyOrders)
                                {
                                    responseBuilder.AppendLine(
                                        $"+ {order.ToString(dataProvider, maxNameLength)}");
                                }
                            }

                            responseBuilder.AppendLine("+-- Sell Orders:");
                            var sellOrders = orders.Where(o => o.OrderType == BuyOrderType.Sell
                                                               || o.OrderType == BuyOrderType.SellShip).ToArray();
                            if (sellOrders.Length == 0)
                                responseBuilder.AppendLine("None");
                            else
                            {
                                foreach (var order in sellOrders)
                                {
                                    responseBuilder.AppendLine(
                                        $"+ {order.ToString(dataProvider, maxNameLength)}");
                                }
                            }

                            responseBuilder.AppendLine($"Total Orders: {orders.Length}");
                            ModCommunication.SendMessageTo(
                                new DialogMessage($"Mrkt#{market.Id} ({market.Name}) Inventory:", null, null,
                                    responseBuilder.ToString()), Context.Player.SteamUserId);
                        })
                        .Catch(HandleError);
                })
                .Catch(HandleError);
        }
        
        [Command("buy", "<itemNameOrIndex> <quantity>: Purchases a quantity of items from nearby tradezones at the lowest prices available.")]
        [Permission(MyPromoteLevel.None)]
        public void Buy(string itemName, decimal quantity)
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You cannot do this while dead.");
                return;
            }
            
            if (quantity < new decimal(0.01))
            {
                Context.Respond($"{quantity} is too low. Please use a higher value.");
                return;
            }
            
            if (!DefinitionResolver.TryGetDefinitionByName(itemName, out var itemDefinition))
            {
                Context.Respond($"Unable to find an item by the name of {itemName}");
                return;
            }
            
            var controllingCube = Context.Player.Controller.ControlledEntity as IMyCubeBlock;
            if (controllingCube == null)
            {
                Context.Respond("Trading by hand is not supported.");
                return;
            }


            var marketManager = GetManager<MarketManager>();
            var orderManager = GetManager<MarketOrderManager>();
            var accountManager = GetManager<AccountsManager>();
            marketManager.GetConnectedMarket(controllingCube.CubeGrid)
                .Then(market =>
                {
                    orderManager.GetMarketOrder(BuyOrderType.Sell, market.Id, itemDefinition.Id)
                        .Then(order =>
                        {
                            if (order == null)
                            {
                                Context.Respond($"Unable to find any sell orders for {itemDefinition.DisplayNameText}.");
                                return;
                            }

                            if (order.Quantity != -1 && order.Quantity < quantity)
                            {
                                Context.Respond($"Trying to buy more than the seller has. Seller only has {order.Quantity}.");
                                return;
                            }

                            if (order.Quantity != -1)
                            {
                                orderManager.UpdateOrderQuantity(order.Id, order.Quantity - quantity)
                                    .Catch(error => Log.Error(error));
                            }

                            // Actually do the transfer... fuck.
                            var stationGrid = MyAPIGateway.Entities.GetEntityById(market.ParentGridId);
                            
                            accountManager.GetPrimaryAccount(Context.Player.SteamUserId)
                                .Then(account =>
                                {
                                    var sellAmount = quantity * order.Price;
                                    TransferInventory(stationGrid as IMyCubeGrid, controllingCube.CubeGrid,
                                        itemDefinition as MyPhysicalItemDefinition, quantity, market.IsNPC, false)
                                        .Then(() =>
                                        {
                                            accountManager.AdjustAccountBalance(market.AccountId.Value, sellAmount,
                                                account.Id,
                                                $"Exchanged {quantity}x {itemDefinition.DisplayNameText} for {Utilities.FriendlyFormatCurrency(sellAmount)}.");
                                            Context.Respond($"Exchanged {quantity}x {itemDefinition.DisplayNameText} for {Utilities.FriendlyFormatCurrency(sellAmount)}.");
                                        })
                                        .Catch(HandleError);
                                    
                                })
                                .Catch(HandleError);
                            
                        })
                        .Catch(HandleError);
                })
                .Catch(HandleError);
        }

        [Command("sell", "<itemName> <quantity>: Attempts to sell a quantity of items to nearby trade zones at best possible price from character inventory and ship inventory.")]
        [Permission(MyPromoteLevel.None)]
        public void Sell(string itemName, decimal quantity)
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You cannot do this while dead.");
                return;
            }
            
            if (quantity < new decimal(0.01))
            {
                Context.Respond($"{quantity} is too low. Please use a higher value.");
                return;
            }
            
            if (!DefinitionResolver.TryGetDefinitionByName(itemName, out var itemDefinition))
            {
                Context.Respond($"Unable to find an item by the name of {itemName}");
                return;
            }
            
            var controllingCube = Context.Player.Controller.ControlledEntity as IMyCubeBlock;
            if (controllingCube == null)
            {
                Context.Respond("Trading by hand is not supported.");
                return;
            }


            var marketManager = GetManager<MarketManager>();
            var orderManager = GetManager<MarketOrderManager>();
            var accountManager = GetManager<AccountsManager>();
            marketManager.GetConnectedMarket(controllingCube.CubeGrid)
                .Then(market =>
                {
                    if (market == null)
                    {
                        Context.Respond("Unable to find any connected markets. Have you docked to a market?");
                        return;
                    }

                    orderManager.GetMarketOrder(BuyOrderType.Buy, market.Id, itemDefinition.Id)
                        .Then(order =>
                        {
                            if (order == null)
                            {
                                Context.Respond($"Unable to find any buy orders for {itemDefinition.DisplayNameText}.");
                                return;
                            }

                            if (order.Quantity != -1 && order.Quantity < quantity)
                            {
                                Context.Respond($"Trying to sell more than the order desires by {quantity - order.Quantity}. Buy order only wants {order.Quantity}.");
                                return;
                            }

                            if (order.Quantity != -1)
                            {
                                // If quantity isn't infinite. Update order quantity.
                                orderManager.UpdateOrderQuantity(order.Id, order.Quantity - quantity)
                                    .Catch(error => Log.Error(error));
                            }

                            // Actually do the transfer... fuck.
                            var stationGrid = MyAPIGateway.Entities.GetEntityById(market.ParentGridId);
                            
                            accountManager.GetPrimaryAccount(Context.Player.SteamUserId)
                                .Then(account =>
                                {
                                    var sellAmount = quantity * order.Price;
                                    TransferInventory(controllingCube.CubeGrid, stationGrid as IMyCubeGrid,
                                        itemDefinition as MyPhysicalItemDefinition, quantity, false, market.IsNPC)
                                        .Then(() =>
                                        {
                                            accountManager.AdjustAccountBalance(account.Id, sellAmount,
                                                market.AccountId,
                                                $"Exchanged {quantity}x {itemDefinition.DisplayNameText} for {Utilities.FriendlyFormatCurrency(sellAmount)}.");
                                            Context.Respond($"Exchanged {quantity}x {itemDefinition.DisplayNameText} for {Utilities.FriendlyFormatCurrency(sellAmount)}.");
                                        })
                                        .Catch(HandleError);
                                })
                                .Catch(HandleError);
                            
                        })
                        .Catch(HandleError);
                })
                .Catch(HandleError);
        }

        private Promise TransferInventory(IMyCubeGrid fromGrid, IMyCubeGrid toGrid, MyPhysicalItemDefinition itemDefinition,
            decimal quantity, bool fromGridIsNpc = false, bool toGridIsNpc = false)
        {
            return new Promise((resolve, reject) =>
            {
                var payloadMass = quantity * (decimal)itemDefinition.Mass;
                
                if (!fromGridIsNpc)
                {
                    var fromTerminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(fromGrid);
                    var fromBlocks = new List<IMyCargoContainer>();
                    fromTerminalSystem.GetBlocksOfType(fromBlocks);

                    var storedAmount =
                        fromBlocks
                            .Select(c => c.GetInventory())
                            .Sum(i => (decimal) i.GetItemAmount(itemDefinition.Id));
                    if (storedAmount < quantity)
                    {
                        reject(new LogicLevelException(
                            $"Inventory lacks {quantity - storedAmount}x {itemDefinition.DisplayNameText} to sell. Inventory only has {storedAmount}."));
                        return;
                    }
                }

                if (!toGridIsNpc)
                {
                    var toTerminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(toGrid);
                    var toBlocks = new List<IMyCargoContainer>();
                    toTerminalSystem.GetBlocksOfType(toBlocks);
                    
                    var availableMass =
                        toBlocks
                            .Select(c => c.GetInventory())
                            .Where(i => !i.IsFull)
                            .Sum(i => (decimal) (i.MaxVolume - i.CurrentVolume));
                    if (availableMass < payloadMass)
                    {
                        reject(new LogicLevelException(
                            $"Inventory lacks the required space to hold the payload. Required Mass: {payloadMass}, Available Mass: {availableMass}."));
                        return;
                    }
                }
                
                // We're clear to change inventories...
                if (!fromGridIsNpc)
                    RemoveItems(fromGrid, itemDefinition, quantity);
                if(!toGridIsNpc)
                    AddItems(toGrid, itemDefinition, quantity);

                resolve();
            });
        }

        private void RemoveItems(IMyCubeGrid fromGrid, MyPhysicalItemDefinition itemDefinition, decimal quantity)
        {
            var terminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(fromGrid);
            var blocks = new List<IMyCargoContainer>();
            terminalSystem.GetBlocksOfType(blocks);

            var remaining = quantity;
            foreach (var cargoBlock in blocks.Where(b => b.CubeGrid.EntityId == fromGrid.EntityId))
            {
                var inventory = cargoBlock.GetInventory();
                var available = (decimal)inventory.GetItemAmount(itemDefinition.Id);

                if (available > remaining)
                {
                    inventory.RemoveItemsOfType((MyFixedPoint)remaining, itemDefinition.Id);
                    remaining = 0;
                }
                else
                {
                    inventory.RemoveItemsOfType((MyFixedPoint)available, itemDefinition.Id);
                    remaining -= available;
                }

                if (remaining == 0)
                    return;
            }
        }

        private void AddItems(IMyCubeGrid toGrid, MyPhysicalItemDefinition itemDefinition, decimal quantity)
        {
            var terminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(toGrid);
            var blocks = new List<IMyCargoContainer>();
            terminalSystem.GetBlocksOfType(blocks);
            
            var remaining = quantity;
            foreach (var cargoBlock in blocks.Where(b => b.CubeGrid.EntityId == toGrid.EntityId))
            {
                var inventory = (MyInventoryBase)cargoBlock.GetInventory();
                var available = (decimal)inventory.ComputeAmountThatFits(itemDefinition.Id);

                if (available >= remaining)
                {
                    ((IMyInventory) inventory).BetterAddItems((MyFixedPoint)remaining, itemDefinition.Id);
                    remaining = 0;
                }
                else if(available > 0)
                {
                    ((IMyInventory) inventory).BetterAddItems((MyFixedPoint)available, itemDefinition.Id);
                    remaining -= available;
                }

                if (remaining == 0)
                    return;
            }
        }
        
        [Command("refuel", "<gasType>: Refuels the hydrogen on your ship.")]
        [Permission(MyPromoteLevel.None)]
        public void Refuel(string gasType)
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You cannot do this while dead.");
                return;
            }

            if (!gasType.Equals("o2", StringComparison.InvariantCultureIgnoreCase)
                && !gasType.Equals("h2", StringComparison.InvariantCultureIgnoreCase))
            {
                Context.Respond("Incorrect gas type specified. Options are: H2, and O2.");
                return;
            }
            
            var controllingCube = Context.Player.Controller.ControlledEntity as IMyCubeBlock;
            if (controllingCube == null)
            {
                Context.Respond("Trading by hand is not supported.");
                return;
            }

            var gasDefinition = gasType.Equals("o2", StringComparison.InvariantCultureIgnoreCase)
                ? new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Oxygen")
                : new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Hydrogen");

            var marketManager = GetManager<MarketManager>();
            var npcManager = GetManager<NPCManager>();
            var accountManager = GetManager<AccountsManager>();
            marketManager.GetConnectedMarket(controllingCube.CubeGrid)
                .Then(market =>
                {
                    if (market == null)
                    {
                        Context.Respond("Unable to find any connected markets. Have you docked to a market?");
                        return;
                    }

                    if (!market.IsNPC)
                    {
                        Context.Respond("Only Service Stations can refuel ships.");
                        return;
                    }

                    npcManager.GetNPC((long) market.CreatorId)
                        .Then(npc =>
                        {
                            if (npc.IndustryType != IndustryTypeEnum.Service)
                            {
                                Context.Respond("Only Service Stations can refuel ships.");
                                return;
                            }

                            accountManager.GetPrimaryAccount(Context.Player.SteamUserId)
                                .Then(account =>
                                {
                                     var terminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(controllingCube.CubeGrid);
                                    var blocks = new List<IMyGasTank>();
                                    terminalSystem.GetBlocksOfType(blocks);

                                    double neededGas = 0;
                                    foreach (var tank in blocks.Cast<MyGasTank>())
                                    {
                                        if (tank.BlockDefinition.StoredGasId != gasDefinition)
                                            continue;
                                        neededGas += tank.GasCapacity * tank.FilledRatio;
                                    }

                                    var simulationProvider = EconomyPlugin.GetDataProvider<MarketSimulationProvider>();
                                    var iceValue = simulationProvider.GetUniversalItemValue("Ice");
                                    var costToFill = new decimal(neededGas) / (iceValue * 9);
                                    
                                    var cost = Math.Min(costToFill, account.Balance);
                                    var purchasedGasAmount = cost * (iceValue * 9);
                                    
                                    var remainingGasAmount = purchasedGasAmount;
                                    foreach (var tank in blocks.Cast<MyGasTank>())
                                    {
                                        if (tank.BlockDefinition.StoredGasId != gasDefinition)
                                            continue;
                                        
                                        var gas = new decimal(tank.GasCapacity * (1 - tank.FilledRatio));
                                        if (gas < remainingGasAmount)
                                        {
                                            tank.ChangeFillRatioAmount(1);
                                            remainingGasAmount -= gas;
                                        }
                                        else
                                        {
                                            var newRatio = (double) (remainingGasAmount / new decimal(tank.GasCapacity))
                                                           + tank.FilledRatio;
                                            tank.ChangeFillRatioAmount(newRatio);
                                            break;
                                        }
                                    }

                                    var friendlyBoughtAmount = Math.Round(purchasedGasAmount, 2);
                                    accountManager.AdjustAccountBalance(account.Id, cost * -1, null,
                                        $"Bought {friendlyBoughtAmount}kg of {gasType}.");

                                    Context.Respond(
                                        $"Bought {friendlyBoughtAmount}kg of {gasType} for {Utilities.FriendlyFormatCurrency(cost)}.");
                                }).Catch(HandleError);
                        })
                        .Catch(HandleError);
                })
                .Catch(HandleError);
        }
        
        [Command("recharge", "Recharges the batteries on your ship.")]
        [Permission(MyPromoteLevel.None)]
        public void Recharge()
        {
            var character = Context.Player.Character;
            if (character == null)
            {
                Context.Respond("You cannot do this while dead.");
                return;
            }
            
            var controllingCube = Context.Player.Controller.ControlledEntity as IMyCubeBlock;
            if (controllingCube == null)
            {
                Context.Respond("Trading by hand is not supported.");
                return;
            }

            var marketManager = GetManager<MarketManager>();
            var npcManager = GetManager<NPCManager>();
            var accountManager = GetManager<AccountsManager>();
            marketManager.GetConnectedMarket(controllingCube.CubeGrid)
                .Then(market =>
                {
                    if (market == null)
                    {
                        Context.Respond("Unable to find any connected markets. Have you docked to a market?");
                        return;
                    }

                    if (!market.IsNPC)
                    {
                        Context.Respond("Only Service Stations can recharge ships.");
                        return;
                    }

                    npcManager.GetNPC((long) market.CreatorId)
                        .Then(npc =>
                        {
                            if (npc.IndustryType != IndustryTypeEnum.Service)
                            {
                                Context.Respond("Only Service Stations can recharge ships.");
                                return;
                            }


                            accountManager.GetPrimaryAccount(Context.Player.SteamUserId)
                                .Then(account =>
                                {
                                    var terminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(controllingCube.CubeGrid);
                                    var blocks = new List<IMyBatteryBlock>();
                                    terminalSystem.GetBlocksOfType(blocks);

                                    var energyDeficit = 0f;
                                    foreach (var battery in blocks.Cast<MyBatteryBlock>())
                                    {
                                        energyDeficit += battery.MaxStoredPower - battery.CurrentStoredPower;
                                    }
                                    
                                    var energyCost = EconomyMarketsPlugin.Instance.Config.EnergySecondsValue;
                                    var maxPossibleCharge = account.Balance / (energyCost / 1000m);

                                    var chargeCredit = Math.Min((float)maxPossibleCharge, energyDeficit);
                                    var cost = (decimal)chargeCredit * (energyCost / 1000m);
                                    foreach (var battery in blocks.Cast<MyBatteryBlock>())
                                    {
                                        var chargeNeeded = battery.MaxStoredPower - battery.CurrentStoredPower;
                                        if (chargeNeeded <= chargeCredit)
                                        {
                                            battery.CurrentStoredPower = battery.CurrentStoredPower + chargeNeeded;
                                            chargeCredit -= chargeNeeded;
                                        }
                                        else
                                        {
                                            battery.CurrentStoredPower = battery.CurrentStoredPower + chargeCredit;
                                            break;
                                        }
                                    }

                                    accountManager.AdjustAccountBalance(account.Id, cost * -1, null,
                                        $"Recharge for {energyDeficit}Mw.");
                                    Context.Respond($"Your ship has been charged {energyDeficit}MW for {Utilities.FriendlyFormatCurrency(cost)}.");
                                }).Catch(HandleError);
                        })
                        .Catch(HandleError);
                })
                .Catch(HandleError);
        }
    }
}