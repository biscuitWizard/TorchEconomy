namespace TorchEconomySE.Data
{
    public static class SQL
    { 
        public const string SELECT_ACCOUNT 
            = "SELECT * FROM `Account` WHERE PlayerIdentity = @playerId;";
        public const string INSERT_ACCOUNT 
            = "INSERT INTO `Account` (`PlayerIdentity`,`Balance`) VALUES(@playerId,@balance);";
        public const string MODIFY_ACCOUNT_BALANCE 
            = "UPDATE `Account` SET `Balance` = `Balance` + @amount WHERE `PlayerIdentity` = @playerId;";

        public const string INSERT_TRADEZONE
            = @"
            INSERT INTO `TradeZone` (`Name`,`PositionX`,`PositionY`,`PositionZ`,`Range`,`OwnerId`) 
            VALUES(@name,@positionX,@positionY,@positionZ,@range,@ownerPlayerId);";
        public const string SELECT_TRADEZONES
            = @"SELECT * FROM `TradeZone`;";

        public const string INSERT_MARKET_ORDER
            = @"
            INSERT INTO `MarketOrder` (`DefinitionIdHash`,`Quantity`,`Price`,`OrderType`,`TradeZoneId`)
            VALUES(@definitionIdHash,@quantity,@price,@orderType,@tradeZoneId);";
        public const string SELECT_MARKET_ORDERS_FOR_TRADEZONE
            = @"SELECT * FROM `MarketOrder` WHERE `TradeZoneId`=@tradeZoneId;";
    }
}
