using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Frontend2.Hardware;
using System.Collections.Generic;
using Frontend2;

namespace A3
{
	// This test class contains tests regarding the accuracy of delivery items
	[TestClass]
	public class UnitTestDelivery
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
		public void T04_good_press_no_insert()
		{
			// Tests the pressing of buttons with no coins inserted
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {5, 10, 25, 100}, 3, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			vendingList[0].Configure(new List<string> {"Coke", "water", "stuff"}, new List<int> {250, 250, 205});
			vendingList[0].CoinRacks[0].LoadCoins(new List<Coin> {new Coin(5)});
			vendingList[0].CoinRacks[1].LoadCoins(new List<Coin> {new Coin(10)});
			vendingList[0].CoinRacks[2].LoadCoins(new List<Coin> {new Coin(25), new Coin(25)});
			vendingList[0].PopCanRacks[0].LoadPops(new List<PopCan> {new PopCan("Coke")});
			vendingList[0].PopCanRacks[1].LoadPops(new List<PopCan> {new PopCan("water")});
			vendingList[0].PopCanRacks[2].LoadPops(new List<PopCan> {new PopCan("stuff")});

			vendingList[0].SelectionButtons[0].Press();

			IDeliverable[] extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 0, "N/A"));
			
			VendingMachineStoredContents unloaded = unload(vendingList[0]);
			Assert.IsTrue(checkTeardown(unloaded, 65, 0, new List<string> {"Coke", "water", "stuff"}));
		}
		
		[TestMethod]
		public void T06_extract_before_sale()
		{
			// Tests extraction before inserting coins
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {100, 5, 25, 10}, 3, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			vendingList[0].Configure(new List<string> {"Coke", "water", "stuff"}, new List<int> {250, 250, 205});
			vendingList[0].CoinRacks[1].LoadCoins(new List<Coin> {new Coin(5)});
			vendingList[0].CoinRacks[2].LoadCoins(new List<Coin> {new Coin(25), new Coin(25)});
			vendingList[0].CoinRacks[3].LoadCoins(new List<Coin> {new Coin(10)});
			vendingList[0].PopCanRacks[0].LoadPops(new List<PopCan> {new PopCan("Coke")});
			vendingList[0].PopCanRacks[1].LoadPops(new List<PopCan> {new PopCan("water")});
			vendingList[0].PopCanRacks[2].LoadPops(new List<PopCan> {new PopCan("stuff")});

			vendingList[0].SelectionButtons[0].Press();

			IDeliverable[] extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 0, "N/A"));

			vendingList[0].CoinSlot.AddCoin(new Coin(100));
			vendingList[0].CoinSlot.AddCoin(new Coin(100));
			vendingList[0].CoinSlot.AddCoin(new Coin(100));

			extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 0, "N/A"));

			VendingMachineStoredContents unloaded = unload(vendingList[0]);
			Assert.IsTrue(checkTeardown(unloaded, 65, 0, new List<string> {"Coke", "water", "stuff"}));
		}

		[TestMethod]
		public void T11_extract_before_sale()
		{
			// Tests extracting before coins are inserted
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {100, 5, 25, 10}, 3, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			vendingList[0].Configure(new List<string> {"Coke", "water", "stuff"}, new List<int> {250, 250, 205});
			vendingList[0].CoinRacks[1].LoadCoins(new List<Coin> {new Coin(5)});
			vendingList[0].CoinRacks[2].LoadCoins(new List<Coin> {new Coin(25), new Coin(25)});
			vendingList[0].CoinRacks[3].LoadCoins(new List<Coin> {new Coin(10)});
			vendingList[0].PopCanRacks[0].LoadPops(new List<PopCan> {new PopCan("Coke")});
			vendingList[0].PopCanRacks[1].LoadPops(new List<PopCan> {new PopCan("water")});
			vendingList[0].PopCanRacks[2].LoadPops(new List<PopCan> {new PopCan("stuff")});

			vendingList[0].SelectionButtons[0].Press();
			
			IDeliverable[] extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 0, "N/A"));

			vendingList[0].CoinSlot.AddCoin(new Coin(100));
			vendingList[0].CoinSlot.AddCoin(new Coin(100));
			vendingList[0].CoinSlot.AddCoin(new Coin(100));

			extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 0, "N/A"));

			VendingMachineStoredContents unloaded = unload(vendingList[0]);
			Assert.IsTrue(checkTeardown(unloaded, 65, 0, new List<string> {"Coke", "water", "stuff"}));

			vendingList[0].CoinRacks[1].LoadCoins(new List<Coin> {new Coin(5)});
			vendingList[0].CoinRacks[2].LoadCoins(new List<Coin> {new Coin(25), new Coin(25)});
			vendingList[0].CoinRacks[3].LoadCoins(new List<Coin> {new Coin(10)});
			vendingList[0].PopCanRacks[0].LoadPops(new List<PopCan> {new PopCan("Coke")});
			vendingList[0].PopCanRacks[1].LoadPops(new List<PopCan> {new PopCan("water")});
			vendingList[0].PopCanRacks[2].LoadPops(new List<PopCan> {new PopCan("stuff")});

			vendingList[0].SelectionButtons[0].Press();
			
			extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 50, "Coke"));

			unloaded = unload(vendingList[0]);
			Assert.IsTrue(checkTeardown(unloaded, 315, 0, new List<string> {"water", "stuff"}));

			machine = new VendingMachine(new int[] {100, 5, 25, 10}, 3, 10, 10, 10);
			logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			vendingList[1].Configure(new List<string> {"Coke", "water", "stuff"}, new List<int> {250, 250, 205});
			vendingList[1].Configure(new List<string> {"A", "B", "C"}, new List<int> {5, 10, 25});

			unloaded = unload(vendingList[1]);
			Assert.IsTrue(checkTeardown(unloaded, 0, 0, new List<string>()));

			vendingList[1].CoinRacks[1].LoadCoins(new List<Coin> {new Coin(5)});
			vendingList[1].CoinRacks[2].LoadCoins(new List<Coin> {new Coin(25), new Coin(25)});
			vendingList[1].CoinRacks[3].LoadCoins(new List<Coin> {new Coin(10)});
			vendingList[1].PopCanRacks[0].LoadPops(new List<PopCan> {new PopCan("A")});
			vendingList[1].PopCanRacks[1].LoadPops(new List<PopCan> {new PopCan("B")});
			vendingList[1].PopCanRacks[2].LoadPops(new List<PopCan> {new PopCan("C")});

			vendingList[1].CoinSlot.AddCoin(new Coin(10));
			vendingList[1].CoinSlot.AddCoin(new Coin(5));
			vendingList[1].CoinSlot.AddCoin(new Coin(10));
			vendingList[1].SelectionButtons[2].Press();

			extracted = vendingList[1].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 0, "C"));

			unloaded = unload(vendingList[1]);
			Assert.IsTrue(checkTeardown(unloaded, 90, 0, new List<string> {"A", "B"}));
		}

	}
}
