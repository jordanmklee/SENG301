using System;
using System.Collections.Generic;
using Frontend2;
using Frontend2.Hardware;
using seng301_asgn2.src;

public class VendingMachineFactory : IVendingMachineFactory {

	private List<VendingMachine> vendingList;

	public VendingMachineFactory()
	{
		vendingList = new List<VendingMachine>();
	}

    public int CreateVendingMachine(List<int> coinKinds, int selectionButtonCount, int coinRackCapacity, int popRackCapacity, int receptacleCapacity) {
		// Converts coinKinds into an array (to match parameter signature)
		int[] convertedCoinKinds = new int[coinKinds.Count];
		int index = 0;
		foreach(int kind in coinKinds)
			convertedCoinKinds[index++] = kind;
		
		// Creates new vending machine, and add to list
		VendingMachine newMachine = new VendingMachine(convertedCoinKinds, selectionButtonCount, coinRackCapacity, popRackCapacity, receptacleCapacity);
		vendingList.Add(newMachine);

		// Creates associated logic class for machine
		Logic newLogic = new Logic(newMachine, coinKinds);

        return 0;
    }

    public void ConfigureVendingMachine(int vmIndex, List<string> popNames, List<int> popCosts) {
		vendingList[vmIndex].Configure(popNames, popCosts);
    }

    public void LoadCoins(int vmIndex, int coinKindIndex, List<Coin> coins) {
		vendingList[vmIndex].CoinRacks[coinKindIndex].LoadCoins(coins);
    }

    public void LoadPops(int vmIndex, int popKindIndex, List<PopCan> pops) {
		vendingList[vmIndex].PopCanRacks[popKindIndex].LoadPops(pops);
    }

    public void InsertCoin(int vmIndex, Coin coin) {
		vendingList[vmIndex].CoinSlot.AddCoin(coin);
    }

    public void PressButton(int vmIndex, int value) {
		vendingList[vmIndex].SelectionButtons[value].Press();
    }

    public List<IDeliverable> ExtractFromDeliveryChute(int vmIndex) {
		// Append every item in the delivery chute to a list, and return it
		List<IDeliverable> extracted = new List<IDeliverable>();
		foreach(IDeliverable item in vendingList[vmIndex].DeliveryChute.RemoveItems())
			extracted.Add(item);

        return extracted;
    }

    public VendingMachineStoredContents UnloadVendingMachine(int vmIndex) {
		VendingMachineStoredContents unloaded = new VendingMachineStoredContents();

		// Unload every coin rack
		foreach(CoinRack rack in vendingList[vmIndex].CoinRacks)
			unloaded.CoinsInCoinRacks.Add(rack.Unload());

		// Unload every pop can rack
		foreach(PopCanRack rack in vendingList[vmIndex].PopCanRacks)
			unloaded.PopCansInPopCanRacks.Add(rack.Unload());
		
		// Unload storage bin coins
		foreach(Coin coin in vendingList[vmIndex].StorageBin.Unload())
			unloaded.PaymentCoinsInStorageBin.Add(coin);

        return unloaded;
    }
}