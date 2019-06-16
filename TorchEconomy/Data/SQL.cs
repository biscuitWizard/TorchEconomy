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

        public const string SELECT_MARKET_BY_NAME_AND_OWNER
            = @"SELECT * FROM `Market` WHERE CreatorId=@creatorId  
                         AND (`Name` LIKE @marketNameOrId OR `Id`=@marketNameOrId)";

        public const string MUTATE_MARKET_OPEN
            = @"UPDATE `Market` SET IsOpen=@isOpen WHERE `Id`=@id;";

        public const string MUTATE_MARKET_ACCOUNT
            = @"UPDATE `Market` SET AccountId=@accountId WHERE `Id`=@id;"; 
        #endregion
        
        #region Market Orders

        public const string SELECT_MARKET_ORDERS =
            @"SELECT * FROM `MarketOrder` WHERE `MarketId`=@marketId AND `IsDeleted`=0;";

        public const string INSERT_OR_UPDATE_MARKET_ORDER =
            @"INSERT IGNORE INTO `MarketOrder` (`OrderType`,`DefinitionId`,`Quantity`,`Price`,`MarketId`)
                VALUES(@orderType,@definitionId,0,0,@marketId);
              UPDATE `MarketOrder` SET `Quantity`=@quantity, `Price`=@price,`CreatedOn`=@createdOn
                WHERE `MarketId`=@marketId AND `DefinitionId`=@definitionId AND `OrderType`=@orderType;";

        public const string SELECT_MARKET_ORDER_BY_ITEM =
            @"SELECT * FROM `MarketOrder` WHERE `MarketId`=@marketId AND `IsDeleted`=0 
                              AND `DefinitionId`=@definitionId AND `OrderType`=@orderType";

        #endregion

    }
}
