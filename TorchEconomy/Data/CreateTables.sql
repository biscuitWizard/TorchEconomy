-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               8.0.16 - MySQL Community Server - GPL
-- Server OS:                    Win64
-- HeidiSQL Version:             9.5.0.5196
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Dumping structure for table space_engineers.Account
CREATE TABLE IF NOT EXISTS `Account` (
  `Id` bigint(8) NOT NULL AUTO_INCREMENT,
  `PlayerId` decimal(20,0) NOT NULL,
  `Balance` float NOT NULL,
  `IsNPC` tinyint(1) NOT NULL DEFAULT '0',
  `IsPrimary` tinyint(1) NOT NULL DEFAULT '0',
  `IsDeleted` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
-- Dumping structure for table space_engineers.Transaction
CREATE TABLE IF NOT EXISTS `Transaction` (
  `Id` bigint(8) NOT NULL AUTO_INCREMENT,
  `ToAccountId` bigint(8) NOT NULL,
  `FromAccountId` bigint(8) NOT NULL,
  `TransactionAmount` float NOT NULL,
  `TransactedOn` int(8) NOT NULL,
  `Reason` varchar(256) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Data exporting was unselected.
-- Dumping structure for table space_engineers.Market
CREATE TABLE IF NOT EXISTS `Market` (
  `Id` bigint(8) NOT NULL AUTO_INCREMENT,
  `Name` varchar(64) NOT NULL,
  `Range` float NOT NULL,
  `ParentGridId` decimal(20,0) NOT NULL,
  `AccountId` bigint(8) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- Data exporting was unselected.
-- Dumping structure for table space_engineers.MarketOrder
CREATE TABLE IF NOT EXISTS `MarketOrder` (
  `Id` bigint(8) NOT NULL AUTO_INCREMENT,
  `MarketId` bigint(8) NOT NULL,
  `BuyOrderType` int(8) NOT NULL,
  `DefinitionId` varchar(128) NOT NULL,
  `Quantity` int(8) NOT NULL,
  `Price` float NOT NULL,
  `CreatedOn` int(8) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;


-- Data exporting was unselected.
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
