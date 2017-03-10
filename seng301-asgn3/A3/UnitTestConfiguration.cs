using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Frontend2;
using Frontend2.Hardware;
using System.Collections.Generic;

namespace A3
{
	// This test class contains tests that ensure the configuration functionality is correct
	[TestClass]
	public class UnitTestConfiguration
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
		public void T05_scrambled_coins()
		{
			// Tests configuration of scrambled coins
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {100, 5, 25, 10}, 3, 2, 10, 10);
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

			vendingList[0].SelectionButtons[0].Press();

			extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 50, "Coke"));

			VendingMachineStoredContents unloaded = unload(vendingList[0]);
			Assert.IsTrue(checkTeardown(unloaded, 215, 100, new List<string> {"water", "stuff"}));
		}

		[TestMethod]
		public void T07_change_config()
		{
			// Tests reconfiguration
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {5, 10, 25, 100}, 3, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			vendingList[0].Configure(new List<string> {"A", "B", "C"}, new List<int> {5, 10, 25});
			vendingList[0].CoinRacks[0].LoadCoins(new List<Coin> {new Coin(5)});
			vendingList[0].CoinRacks[1].LoadCoins(new List<Coin> {new Coin(10)});
			vendingList[0].CoinRacks[2].LoadCoins(new List<Coin> {new Coin(25), new Coin(25)});
			vendingList[0].PopCanRacks[0].LoadPops(new List<PopCan> {new PopCan("A")});
			vendingList[0].PopCanRacks[1].LoadPops(new List<PopCan> {new PopCan("B")});
			vendingList[0].PopCanRacks[2].LoadPops(new List<PopCan> {new PopCan("C")});

			vendingList[0].Configure(new List<string> {"Coke", "water", "stuff"}, new List<int> {250, 250, 205});

			vendingList[0].SelectionButtons[0].Press();

			IDeliverable[] extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 0, "N/A"));

			vendingList[0].CoinSlot.AddCoin(new Coin(100));
			vendingList[0].CoinSlot.AddCoin(new Coin(100));
			vendingList[0].CoinSlot.AddCoin(new Coin(100));
			
			vendingList[0].SelectionButtons[0].Press();

			extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 50, "A"));

			VendingMachineStoredContents unloaded = unload(vendingList[0]);
			Assert.IsTrue(checkTeardown(unloaded, 315, 0, new List<string> {"B", "C"}));

			vendingList[0].CoinRacks[0].LoadCoins(new List<Coin> {new Coin(5)});
			vendingList[0].CoinRacks[1].LoadCoins(new List<Coin> {new Coin(10)});
			vendingList[0].CoinRacks[2].LoadCoins(new List<Coin> {new Coin(25), new Coin(25)});
			vendingList[0].PopCanRacks[0].LoadPops(new List<PopCan> {new PopCan("Coke")});
			vendingList[0].PopCanRacks[1].LoadPops(new List<PopCan> {new PopCan("water")});
			vendingList[0].PopCanRacks[2].LoadPops(new List<PopCan> {new PopCan("stuff")});
			
			vendingList[0].CoinSlot.AddCoin(new Coin(100));
			vendingList[0].CoinSlot.AddCoin(new Coin(100));
			vendingList[0].CoinSlot.AddCoin(new Coin(100));
			vendingList[0].SelectionButtons[0].Press();

			extracted = vendingList[0].DeliveryChute.RemoveItems();
			Assert.IsTrue(checkDelivery(extracted, 50, "Coke"));

			unloaded = unload(vendingList[0]);
			Assert.IsTrue(checkTeardown(unloaded, 315, 0, new List<string> {"water", "stuff"}));
		}

		[TestMethod]
		[ExpectedException (typeof(ArgumentOutOfRangeException))]
		public void U01_configure_before_creation()
		{
			// Tests configuration before creation of vending machine
			List<VendingMachine> vendingList = new List<VendingMachine>();
			// Error: VendingMachine 0 has not been created yet
			vendingList[0].Configure(new List<string> {"Coke", "water", "stuff"}, new List<int> {250, 250, 205});
			
			VendingMachine machine = new VendingMachine(new int[] {5, 10, 25, 100}, 3, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			vendingList[0].CoinRacks[0].LoadCoins(new List<Coin> {new Coin(5)});
			vendingList[0].CoinRacks[1].LoadCoins(new List<Coin> {new Coin(10)});
			vendingList[0].CoinRacks[2].LoadCoins(new List<Coin> {new Coin(25), new Coin(25)});
			vendingList[0].PopCanRacks[0].LoadPops(new List<PopCan> {new PopCan("Coke")});
			vendingList[0].PopCanRacks[1].LoadPops(new List<PopCan> {new PopCan("water")});
			vendingList[0].PopCanRacks[2].LoadPops(new List<PopCan> {new PopCan("stuff")});

			VendingMachineStoredContents unloaded = unload(vendingList[0]);
			Assert.IsTrue(checkTeardown(unloaded, 65, 0, new List<string> {"Coke", "water", "stuff"}));
			
		}

		[TestMethod]
		[ExpectedException (typeof(Exception))]
		public void U02_bad_costs()
		{
			// Tests invalid costs
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {5, 10, 25, 100}, 3, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			// Error: Stuff cost is 0
			vendingList[0].Configure(new List<string> {"Coke", "water", "stuff"}, new List<int> {250, 250, 0});
			
			vendingList[0].CoinRacks[0].LoadCoins(new List<Coin> {new Coin(5)});
			vendingList[0].CoinRacks[1].LoadCoins(new List<Coin> {new Coin(10)});
			vendingList[0].CoinRacks[2].LoadCoins(new List<Coin> {new Coin(25), new Coin(25)});
			vendingList[0].PopCanRacks[0].LoadPops(new List<PopCan> {new PopCan("Coke")});
			vendingList[0].PopCanRacks[1].LoadPops(new List<PopCan> {new PopCan("water")});
			vendingList[0].PopCanRacks[2].LoadPops(new List<PopCan> {new PopCan("stuff")});

			VendingMachineStoredContents unloaded = unload(vendingList[0]);
			Assert.IsTrue(checkTeardown(unloaded, 0, 0, new List<string> ()));
		}

		[TestMethod]
		[ExpectedException (typeof(Exception))]
		public void U03_bad_names()
		{
			// Tests invalid number of pop names
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {5, 10, 25, 100}, 3, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			// Error: Not enough pops/costs
			vendingList[0].Configure(new List<string> {"Coke", "water"}, new List<int> {250, 250});
		}

		[TestMethod]
		[ExpectedException (typeof(Exception))]
		public void U04_bad_coin()
		{
			// Tests non-unique denominations
			List<VendingMachine> vendingList = new List<VendingMachine>();
			// Error: Non-unique denominations
			VendingMachine machine = new VendingMachine(new int[] {1, 1}, 1, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);
		}

		[TestMethod]
		[ExpectedException (typeof(Exception))]
		public void U05_bad_coin_kind()
		{
			// Tests invalid denominations
			List<VendingMachine> vendingList = new List<VendingMachine>();
			// Error: Bad denomination
			VendingMachine machine = new VendingMachine(new int[] {0}, 1, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);
		}

		[TestMethod]
		[ExpectedException (typeof(IndexOutOfRangeException))]
		public void U06_bad_button()
		{
			// Tests invalid button press
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {5, 10, 25, 100}, 3, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			// Error: Bad button number
			vendingList[0].SelectionButtons[3].Press();
		}

		[TestMethod]
		[ExpectedException (typeof(IndexOutOfRangeException))]
		public void U07_bad_button_2()
		{
			// Tests invalid button press
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {5, 10, 25, 100}, 3, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			// Error: Bad button number
			vendingList[0].SelectionButtons[-1].Press();
		}

		[TestMethod]
		[ExpectedException (typeof(IndexOutOfRangeException))]
		public void U08_bad_button_3()
		{
			// Tests invalid button press
			List<VendingMachine> vendingList = new List<VendingMachine>();
			VendingMachine machine = new VendingMachine(new int[] {5, 10, 25, 100}, 3, 10, 10, 10);
			VendingMachineLogic logic = new VendingMachineLogic(machine);
			vendingList.Add(machine);

			// Error: Bad button number
			vendingList[0].SelectionButtons[4].Press();
		}
	}
}
