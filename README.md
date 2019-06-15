# What is TorchEconomy?
Torch Economy is a collection of economy plugins for the Torch API that provides an extra layer of gameplay for Space Engineers. It is based heavily on (and makes use of) 
Frontier Economy's code, expanding and modifying it to make it a server-side only modification.


## TorchEconomy

### Features
* Player & NPC Currency Tracking
* Multiple bank accounts are allowed per player/NPC, with one selected as primary.
* APIGateway integration to allow client-side or other server-side modifications to interact with the economy.
* Security provided by way of a server-provided Transaction Key that can be used to sign APIGateway messages for authenticity.
* Extended gameplay provided through addon plugins.
* Fully configurable through Torch's WPF UI (with currency names, and more being tracked this way.)
* ADO.NET backend integration. All data is stored on SQL providers allowing external integration through web portals.
* Full transaction logs for every monetary exchange that takes place.

### Commands
* !econ balance: Displays the balance for all accounts attached to your player.
* !econ logs <account#OrName>: Displays a transaction log for your account limited to the last 50 transactions.
* !econ accounts primary <account#OrName>: Sets the provided account # as your primary account to send/receive money from.
* !econ accounts close <account#OrName>: Closes the account # provided it has 0 money.
* !econ accounts open <accountName>: Opens a new account with the provided nickname. Players are limited to 10 of these.
* !econ transfer <playerNameOrId> <amount>: Transfers money from your primary account to the targeted player's primary account.
* !econ move <fromAccount> <toAccount> <amount>: Manually transfer money between two accounts you own.

### Admin Commands
* !admin accounts give <playerNameOrId> <amount>: Admin command to give a player money. Sends it to their primary account. Amount may be negative.

### Using the API
Send requests to DS using the normal APIGateway (the Protobuf classes are under TorchEconomy/Messages). Messages from other mods should include the TransactionKey if ForceTransactionCheck is enabled in Torch Economy. Responses from Torch will be returned specifically to the client that requested them.

It is currently possibly to interact directly with accounts using the API if properly authorized by the server's TransactionKey.

## TorchEconomy.Markets (In Progress)

### Features
* Can make stations into Trade Zones capable of supporting buy and sell orders.
* Items sold to the station are deposited directly into the station inventory.
* Items bought from a station are deposited directly into player's ship's inventory or personal inventory.
* NPC markets can be attached to stations and will automatically populate with inventory.
* Configurable through Torch's WPF UI, allowing the adjustment of base ore prices.
* Prices rise as players buy from NPCs, prices lower as players sell to NPCs, with prices normalizing over time.
* Player tradezones can be configured to use a specific bank account, preventing overdrafting and griefing by mass sales.

### Commands
* !econ buy <itemName> <quantity>: Purchases a quantity of items from the market your ship is currently docked to.
* !econ sell <itemName> <quantity>: Sells a quantity of items from your ship inventory to the market your ship is docked to.
* !econ markets list: Lists all markets that you have permission to modify.
* !econ markets create <gridName> <marketName>: Creates a named market linked to the provided gridName. You must own a majority of the station.
* !econ markets buy <marketNameOrId> <itemName> <pricePer1> <quantity>: Creates a buy order on the specified market. Must own market.
* !econ markets sell <marketNameOrId> <itemName> <pricePer1> <quantity>: Creates a sell order on the specified market. Must own market.
* !econ markets open <marketNameOrId>: Opens the specified market for business.
* !econ markets close <marketNameOrId>: Closes the specified market for business.
* !econ markets account <marketNameOrId> <accountNameOrId>: Links an account to specified market to act as a coffer.
* !econ markets setprice <marketNameOrId> <itemName> <newPricePer1>: Sets a price on a specified item at the specified market.

### Admin Commands
* !admin markets delete <marketName>: Deletes a market.  
* !admin markets createNPC <gridName> <marketName> <industry>: Creates an NPC market of the specified industry type at the provider grid.
  
### About NPC Markets
There are four market types:
* Industrial: Buys military products high, Sells ore/ingots cheap
* Consumer: Buys ore/ingots high, Sells components cheap
* Research: Buys components high, Sells research commodity items cheap
* Military: Buys research commodity items high, and sells military commodity items cheap.

If you'll notice, markets sell Industrial -> Consumer -> Research -> Military -> Industrial.


## TorchEconomy.ShipTrading (Incomplete)

### Features
* Ships can be sold by their owners and bought by other players.
* Extra PCU granted explicitly for ships in escrow.
* NPC ship pads that will respawn sellable ships over time as players buy them.
* Fully configurable through Torch's WPF UI

## TorchEconomy.Storage (Incomplete)

### Features
* Ships can be stowed offline for a nominal fee and cooldown period.
* Can charge regular fees to keep a ship stowed.
* Fully configurable through Torch's WPF UI

## TorchEconomy.WebAPI (Incomplete)

### Features
* A full REST API that gives read/write access to functionalities of the plugin.
* Packaged vue.js web application that gives steam users the ability to interact with markets and manage accounts via a web interface.

# Installation

* Get the latest Torch release here: https://torchapi.net/download
* Unzip the Torch release into its own directory and run the executable. It will automatically download the SE DS and generate the other necessary files.
  - If you already have a DS installed you can unzip the Torch files into the folder that contains the DedicatedServer64 folder.
* Get the latest TorchEconomy release here: No builds currently available
* Place the TorchEconomy.zip plugins you desire into the /plugins folder.

# Building
To build TorchEconomy you must first have a complete SE Dedicated installation somewhere and a Torch installation. Before you open the solution, run the Setup batch file and enter the path of that installation's DedicatedServer64 folder. The script will make a symlink to that folder so the Torch solution can find the DLL references it needs.

In both cases you will need to set the InstancePath in TorchConfig.xml to an existing dedicated server instance as Torch can't fully generate it on its own yet.

If you have a more enjoyable server experience because of Torch, please consider supporting them on Patreon.
[![Patreon](http://i.imgur.com/VzzIMgn.png)](https://www.patreon.com/bePatron?u=847269)!
