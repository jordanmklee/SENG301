using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Frontend2;
using Frontend2.Hardware;
using System.Collections.Generic;

namespace A3
{
	// This test class's tests are grouped on vending machine input
	[TestClass]
	public class UnitTestInput
	{
		
		public Boolean checkDelivery(IDeliverable[] extracted, int expectedCoinValue, String expectedPopName)
		{
			int extractedCoins = 0;
			PopCan extractedPop = new PopCan("N/A");
			foreach(IDeliverable item in extracted)		// Traverse every item in extracted
			{
				if(item.GetType() == typeof(Coin))		// Current item is a Coin
				{
					Coin coin = (Coin)item;
					extractedCoins += coin.Value;		// ADd coin value to counter
				}
				if(item.GetType() == typeof(PopCan))	// Current item is a PopCan
				{
					PopCan popcan = (PopCan)item;
					extractedPop = popcan;				// There is only ever one pop in the delivery, set name
				}	
			}

			// Return Boolean based on whether or not actual is the same as expected
			return (expectedCoinValue == extractedCoins && expectedPopName.Equals(extractedPop.Name));
		}

		public VendingMachineStoredContents unload(VendingMachine vm)
		{
			// Unloads everything in vm
			VendingMachineStoredContents unloaded = new VendingMachineStoredContents();
			foreach(CoinRack rack in vm.CoinRacks)
				unloaded.CoinsInCoinRacks.Add(rack.Unload());
			unloaded.PaymentCoinsInStorageBin.AddRange(vm.StorageBin.Unload());
			foreach(PopCanRack rack in vm.PopCanRacks)
				unloaded.PopCansInPopCanRacks.Add(rack.Unload());
			return unloaded;
		}

		public Boolean checkTeardown(VendingMachineStoredContents unloaded, int expectedCoinValue, int expectedStorageValue, List<string> expectedPopNames)
		{
			// Organize teardown
			int unloadedCoinRack = 0;
			int unloadedStorage = 0;
			List<PopCan> unloadedPopRack = new List<PopCan>();
			foreach(List<Coin> list in unloaded.CoinsInCoinRacks)
			{
				foreach(Coin coin in list)
					unloadedCoinRack += coin.Value;
			}
			foreach(Coin coin in unloaded.PaymentCoinsInStorageBin)
				unloadedStorage += coin.Value;

			foreach(List<PopCan> list in unloaded.PopCansInPopCanRacks)
			{
				foreach(PopCan popcan in list)
					unloadedPopRack.Add(popcan);
			}
			
			// Expected coins is not the same; fail
			if(expectedCoinValue != unloadedCoinRack)
				return false;

			// Coinslot test passed
			// Expected storage value is not the same; fail
			if(expectedStorageValue != unloadedStorage)
				return false;

			// Storage Bin test passed
			// If the expected size differs from the actual size, then fail
			if(unloadedPopRack.Count != expectedPopNames.Count)
				return false;

			// From here on, assume that the expected and actual have the same size
			// Check if the pop names are the same
			foreach(PopCan popcan in unloadedPopRack)		// Traverses every unloaded pop
			{
				Boolean found = false;
				int nameIndex = 0;
				while(!found)
				{
					if(nameIndex >= expectedPopNames.Count)
						break;
					String popname = expectedPopNames[nameIndex];
					if(popcan.Name == popname)
					{
						expectedPopNames.Remove(popname);	// Remove matching from expected list
						found = true;
					}
					nameIndex++;
				}
			}
			// Expected List is not empty (ie. different from actual); fail
			if (expectedPopNames.Count != 0)
				return false;

			return true;	// All tests passed
		}

		[TestMethod]
		public void T10_invalid_coin()
		{
			// Tests for unrecognized coins
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {5, 10, 25, 100}, 1, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			vendingList[0].Configure(new List<string> {"stuff"}, new List<int> {140});
			vendingList[0].CoinRacks[0].LoadCoins(new List<Coin> {new Coin(5)});
			vendingList[0].CoinRacks[1].LoadCoins(new List<Coin> {new Coin(10), new Coin(10), new Coin(10), new Coin(10), new Coin(10), new Coin(10)});
			vendingList[0].CoinRacks[2].LoadCoins(new List<Coin> {new Coin(25)});
			vendingList[0].CoinRacks[3].LoadCoins(new List<Coin> {new Coin(100)});
			vendingList[0].PopCanRacks[0].LoadPops(new List<PopCan> {new PopCan("stuff")});

			vendingList[0].CoinSlot.AddCoin(new Coin(1));
			vendingList[0].CoinSlot.AddCoin(new Coin(139));
			vendingList[0].SelectionButtons[0].Press();
			
			IDeliverable[] extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 140, "N/A"));

			VendingMachineStoredContents unloaded = unload(vendingList[0]);
			Assert.IsTrue(checkTeardown(unloaded, 190, 0, new List<string> {"stuff"}));
		}
		
		[TestMethod]
		public void T13_storage_bin()
		{
			// Tests storage bin functionality
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {5, 10, 25, 100}, 1, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			vendingList[0].Configure(new List<string> {"stuff"}, new List<int> {135});
			vendingList[0].CoinRacks[0].LoadCoins(new List<Coin> {new Coin(5), new Coin(5), new Coin(5), new Coin(5), new Coin(5), new Coin(5), new Coin(5), new Coin(5), new Coin(5), new Coin(5)});
			vendingList[0].CoinRacks[1].LoadCoins(new List<Coin> {new Coin(10), new Coin(10), new Coin(10), new Coin(10), new Coin(10), new Coin(10), new Coin(10), new Coin(10), new Coin(10), new Coin(10)});
			vendingList[0].CoinRacks[2].LoadCoins(new List<Coin> {new Coin(25), new Coin(25), new Coin(25), new Coin(25), new Coin(25), new Coin(25), new Coin(25), new Coin(25), new Coin(25), new Coin(25)});
			vendingList[0].CoinRacks[3].LoadCoins(new List<Coin> {new Coin(100), new Coin(100), new Coin(100), new Coin(100), new Coin(100), new Coin(100), new Coin(100), new Coin(100), new Coin(100), new Coin(100)});
			vendingList[0].PopCanRacks[0].LoadPops(new List<PopCan> {new PopCan("stuff")});

			vendingList[0].CoinSlot.AddCoin(new Coin(25));
			vendingList[0].CoinSlot.AddCoin(new Coin(100));
			vendingList[0].CoinSlot.AddCoin(new Coin(10));
			vendingList[0].SelectionButtons[0].Press();

			IDeliverable[] extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 0, "stuff"));

			VendingMachineStoredContents unloaded = unload(vendingList[0]);
			Assert.IsTrue(checkTeardown(unloaded, 1400, 135, new List<string> ()));
		}		

	}
}
