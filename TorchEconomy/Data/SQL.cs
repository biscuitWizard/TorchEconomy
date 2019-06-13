﻿namespace TorchEconomy.Data
{
    public static class SQL
    { 
        #region Accounts
        public const string SELECT_ACCOUNTS 
            = "SELECT * FROM `Account` WHERE PlayerIdentity = @playerId;";

        public const string SELECT_PRIMARY_ACCOUNT
            = "SELECT * FROM `Account` WHERE PlayerIdentity = @playerId AND IsPrimary=1;";

        public const string SELECT_ACCOUNT
            = "SELECT * FROM `Account` WHERE Id = @id";
        
        public const string INSERT_ACCOUNT 
            = "INSERT INTO `Account` (`PlayerIdentity`,`Balance`,`IsNPC`,`IsPrimary`) VALUES(@playerId,@balance,@isNPC,@isPrimary);";
        
        public const string MUTATE_ACCOUNT_BALANCE 
            = "UPDATE `Account` SET `Balance` = `Balance` + @amount WHERE `Id` = @id;";
            
        public const string MUTATE_ACCOUNT_PRIMARY
            = @"
            UPDATE `Account` SET `IsPrimary` = 0 WHERE `PlayerIdentity` = (SELECT `PlayerIdentity` FROM `Account` WHERE `Id`=@id) AND `Id` > 0; 
            UPDATE `Account` SET `IsPrimary` = @isPrimary WHERE `Id`=@id;";
        
        #endregion
        
        #region Trade Zones
        public const string INSERT_TRADEZONE
            = @"
            INSERT INTO `TradeZone` (`Name`,`PositionX`,`PositionY`,`PositionZ`,`Range`,`OwnerId`) 
            VALUES(@name,@positionX,@positionY,@positionZ,@range,@ownerPlayerId);";
        public const string SELECT_TRADEZONES
            = @"SELECT * FROM `TradeZone`;";
        #endregion
        
        #region Market Orders
        public const string INSERT_MARKET_ORDER
            = @"
            INSERT INTO `MarketOrder` (`DefinitionIdHash`,`Quantity`,`Price`,`OrderType`,`TradeZoneId`)
            VALUES(@definitionIdHash,@quantity,@price,@orderType,@tradeZoneId);";
        
        public const string SELECT_MARKET_ORDERS_FOR_TRADEZONE
            = @"SELECT * FROM `MarketOrder` WHERE `TradeZoneId`=@tradeZoneId;";
        #endregion
        
    }
}
