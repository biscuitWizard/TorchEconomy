namespace TorchEconomy.Data
{
    public static class SQL
    { 
        #region Accounts
        public const string SELECT_ACCOUNTS 
            = "SELECT * FROM `Account` WHERE PlayerId = @playerId AND `IsDeleted`=0;";

        public const string SELECT_PRIMARY_ACCOUNT
            = "SELECT * FROM `Account` WHERE PlayerId = @playerId AND IsPrimary=1 AND `IsDeleted`=0;";

        public const string SELECT_ACCOUNT
            = "SELECT * FROM `Account` WHERE Id = @id AND `IsDeleted`=0";
        
        public const string INSERT_ACCOUNT 
            = "INSERT INTO `Account` (`PlayerId`,`Balance`,`IsNPC`,`IsPrimary`,`Nickname`) VALUES(@playerId,@balance,@isNPC,@isPrimary,@nickname);";
        
        public const string MUTATE_ACCOUNT_BALANCE 
            = "UPDATE `Account` SET `Balance` = `Balance` + @amount WHERE `Id` = @id;";
            
        public const string MUTATE_ACCOUNT_PRIMARY
            = @"
            UPDATE `Account` SET `IsPrimary` = 0 WHERE `PlayerId` = @playerId AND `Id` > 0 AND `IsDeleted`=0; 
            UPDATE `Account` SET `IsPrimary` = 1 WHERE `Id`=@id;";
        
        #endregion
        
        #region Transactions

        public const string SELECT_TRANSACTIONS
            = "SELECT * FROM `Transaction` WHERE `FromAccountId`=@accountId OR `ToAccountId`=@accountId;";
        
        public const string INSERT_TRANSACTION
            = @"INSERT INTO `Transaction` (`FromAccountId`,`ToAccountId`,`TransactionAmount`,`TransactedOn`,`Reason`) 
             VALUES(@fromAccountId,@toAccountId,@transactionAmount,@transactedOn,@reason);";
        #endregion
        
        #region Markets
        public const string INSERT_MARKET
            = @"
            INSERT INTO `Market` (`Name`,`ParentGridId`,`Range`,`CreatorId`) 
            VALUES(@name,@parentGridId,@range,@creatorId);";
        
        public const string SELECT_MARKETS
            = @"SELECT * FROM `Market` WHERE `IsDeleted`=0;";

        public const string SELECT_MARKET_BY_GRID
            = @"SELECT * FROM `Market` WHERE `ParentGridId`=@parentGridId AND `IsDeleted`=0;";
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
