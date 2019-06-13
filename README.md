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

## TorchEconomy.Markets (In Progress)

### Features
* Can make stations into Trade Zones capable of supporting buy and sell orders.
* Items sold to the station are deposited directly into the station inventory.
* Items bought from a station are deposited directly into player's ship's inventory or personal inventory.
* NPC markets can be attached to stations and will automatically populate with inventory.
* Configurable through Torch's WPF UI, allowing the adjustment of base ore prices.
* Prices rise as players buy from NPCs, prices lower as players sell to NPCs, with prices normalizing over time.

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
