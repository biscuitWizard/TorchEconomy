namespace TorchEconomySE.Data
{
    public static class SQL
    { 
        public const string SELECT_ACCOUNT = "SELECT * FROM `Account` WHERE PlayerIdentity = @playerId;";
        public const string INSERT_ACCOUNT = "INSERT INTO `Account` (`PlayerIdentity`,`Balance`) VALUES(@playerId,@balance);";
        public const string MODIFY_ACCOUNT_BALANCE = "UPDATE `Account` SET `Balance` = `Balance` + @amount WHERE `PlayerIdentity` = @playerId;";
    }
}
