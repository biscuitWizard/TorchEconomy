﻿using System;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace TorchEconomy
{
    public static class Utilities
    {
        public static string FriendlyFormatCurrency(decimal amount)
        {
            var stringAmount = amount.ToString("#,##0.##");
            return $"{stringAmount}{EconomyPlugin.Instance.Config.CurrencyAbbreviation}";
        }
        
        public static double DistanceFrom(this VRageMath.Vector3 start, VRageMath.Vector3 end)
        {
            var x = Math.Pow(end.X - start.X, 2);
            var y = Math.Pow(end.Y - start.Y, 2);
            var z = Math.Pow(end.Z - start.Z, 2);

            return Math.Abs(Math.Sqrt(x + y + z));
        }
        
        public static bool HasBlockType(this IMyCubeGrid grid, string typeName)
        {
            foreach (var block in ((MyCubeGrid)grid).GetFatBlocks())

                if (string.Compare(block.BlockDefinition.Id.TypeId.ToString().Substring(16), typeName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return true;

            return false;
        }

        public static bool HasBlockSubtype(this IMyCubeGrid grid, string subtypeName)
        {
            foreach (var block in ((MyCubeGrid)grid).GetFatBlocks())
                if (string.Compare(block.BlockDefinition.Id.SubtypeName, subtypeName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return true;

            return false;
        }

        public static bool TryGetEntityByNameOrId(string nameOrId, out IMyEntity entity)
        {
            if (long.TryParse(nameOrId, out long id))
                return MyAPIGateway.Entities.TryGetEntityById(id, out entity);

            foreach (var ent in MyEntities.GetEntities())
            {
                if (ent.DisplayName == nameOrId)
                {
                    entity = ent;
                    return true;
                }
            }

            entity = null;
            return false;
        }

        public static IMyPlayer GetPlayerByNameOrId(string nameOrPlayerId)
        {
            if (!long.TryParse(nameOrPlayerId, out long id))
            {
                foreach (var identity in MySession.Static.Players.GetAllIdentities())
                {
                    if (identity.DisplayName == nameOrPlayerId)
                    {
                        id = identity.IdentityId;
                    }
                }
            }

            if (MySession.Static.Players.TryGetPlayerId(id, out MyPlayer.PlayerId playerId))
            {
                if (MySession.Static.Players.TryGetPlayerById(playerId, out MyPlayer player))
                {
                    return player;
                }
            }

            return null;
        }

        public static string FormatDataSize(double size)
        {
            string p = MyUtils.FormatByteSizePrefix(ref size);
            return $"{size:N}{p}B";
        }
    }
}