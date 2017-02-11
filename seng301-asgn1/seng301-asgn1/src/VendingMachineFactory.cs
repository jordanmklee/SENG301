using System.Collections;
using System.Collections.Generic;

using Frontend1;
using System;

namespace seng301_asgn1 {
    /// <summary>
    /// Represents the concrete virtual vending machine factory that you will implement.
    /// This implements the IVendingMachineFactory interface, and so all the functions
    /// are already stubbed out for you.
    /// 
    /// Your task will be to replace the TODO statements with actual code.
    /// 
    /// Pay particular attention to extractFromDeliveryChute and unloadVendingMachine:
    /// 
    /// 1. These are different: extractFromDeliveryChute means that you take out the stuff
    /// that has already been dispensed by the machine (e.g. pops, money) -- sometimes
    /// nothing will be dispensed yet; unloadVendingMachine is when you (virtually) open
    /// the thing up, and extract all of the stuff -- the money we've made, the money that's
    /// left over, and the unsold pops.
    /// 
    /// 2. Their return signatures are very particular. You need to adhere to this return
    /// signature to enable good integration with the other piece of code (remember:
    /// this was written by your boss). Right now, they return "empty" things, which is
    /// something you will ultimately need to modify.
    /// 
    /// 3. Each of these return signatures returns typed collections. For a quick primer
    /// on typed collections: https://www.youtube.com/watch?v=WtpoaacjLtI -- if it does not
    /// make sense, you can look up "Generic Collection" tutorials for C#.
    /// </summary>
    public class VendingMachineFactory : IVendingMachineFactory {

		private List<VendingMachine> vendingList;

        public VendingMachineFactory() {
			vendingList = new List<VendingMachine>();
        }

        public int createVendingMachine(List<int> coinKinds, int selectionButtonCount) {
			VendingMachine newMachine = new VendingMachine();

			foreach(int c in coinKinds)									// Traverses every coin type in parameter
			{
				if(c > 0)												// Adds coin type if non-zero, non-negative
				{
					if(!newMachine.coinKinds.Contains(c))				// Adds coin type if not already in list
					{
						newMachine.coinKinds.Add(c);
						newMachine.coinChutes.Add(new Queue<Coin>());	// Creates a coin chute for every coin type
					}
					else
						throw new Exception("Duplicate coin type");
				}
				else
					throw new Exception("Invalid coin type");
			}

			newMachine.sortCoinKinds();

			newMachine.selectionButtonCount = selectionButtonCount;
			for(int x = 0; x < newMachine.selectionButtonCount; x++)
				newMachine.popChutes.Add(new Queue<Pop>());		// Creates a pop chute for every expected pop

			vendingList.Add(newMachine);

			return (vendingList.Count - 1);
        }

        public void configureVendingMachine(int vmIndex, List<string> popNames, List<int> popCosts) {
			
			if(vmIndex < vendingList.Count)												// Check if index parameter is valid
			{
				if(popNames.Count == vendingList[vmIndex].selectionButtonCount
					&& popCosts.Count == vendingList[vmIndex].selectionButtonCount)		// Check if correct number of pops/costs have been input
				{
					// Clear previous (allow overwrite of previous configuration)
					vendingList[vmIndex].popNames.Clear();
					vendingList[vmIndex].popCosts.Clear();

					for(int x = 0; x < vendingList[vmIndex].selectionButtonCount; x++)	// Traverse through every button, associating costs and names
					{
						if(popCosts[x] > 0)												// Only add pop if it has a positive cost
						{
							vendingList[vmIndex].popNames.Add(popNames[x]);
							vendingList[vmIndex].popCosts.Add(popCosts[x]);
						}
						else
							throw new Exception("Invalid pop cost");
					}					
				}
				else
					throw new Exception("Wrong number of popNames/popCosts");				
			}
			else
				throw new Exception("Vending machine " + vmIndex + " does not exist");
			
        }

        public void loadCoins(int vmIndex, int coinKindIndex, List<Coin> coins) {
			if(vmIndex < vendingList.Count)									// Check if index parameter is valid
			{
				if(coinKindIndex < vendingList[vmIndex].coinKinds.Count)	// Check if chute index is valid
				{
					foreach(Coin c in coins)								// Add each coin of parameter into selected chute
						vendingList[vmIndex].coinChutes[coinKindIndex].Enqueue(c);
				}
				else
					throw new Exception("Invalid coinKindIndex");
			}
			else
				throw new Exception("Vending machine " + vmIndex + " does not exist");
        }

        public void loadPops(int vmIndex, int popKindIndex, List<Pop> pops) {
            if(vmIndex < vendingList.Count)
			{
				if(popKindIndex < vendingList[vmIndex].popNames.Count)
				{
					foreach(Pop p in pops)
						vendingList[vmIndex].popChutes[popKindIndex].Enqueue(p);
				}
				else
					throw new Exception("Invalid popKindIndex");
			}
			else
				throw new Exception("Vending machine " + vmIndex + " does not exist");
        }

        public void insertCoin(int vmIndex, Coin coin) {
			if (vmIndex < vendingList.Count)
			{
				if(vendingList[vmIndex].coinKinds.Contains(coin.Value))
					vendingList[vmIndex].coinSlot.Add(coin);			// Recognized; accept coin
				else
					vendingList[vmIndex].deliveryChute.Add(coin);		// Not recognized; return via deliveryChute
			}
			else
				throw new Exception("Vending machine " + vmIndex + " does not exist");
        }	

        public void pressButton(int vmIndex, int value) {
			if(vmIndex < vendingList.Count)
			{
				// Count money and calculate difference
				int moneyInserted = 0;
				foreach(Coin coin in vendingList[vmIndex].coinSlot)	
					moneyInserted += coin.Value;
				int difference = moneyInserted - vendingList[vmIndex].popCosts[value];

				// Enough money was paid, and there is a pop to be vended
				if(difference >= 0 && vendingList[vmIndex].popChutes[value].Count > 0)
				{
					vendingList[vmIndex].deliveryChute.Add(vendingList[vmIndex].popChutes[value].Dequeue());    // Add pop to deliveryChute

					foreach (Coin coin in vendingList[vmIndex].coinSlot)			// Remove money from coin slot
						vendingList[vmIndex].moneyEarned.Add(coin);

					// Add change to deliveryChute
					foreach(int index in vendingList[vmIndex].indexDescending)		// Cycle through the chutes in order of descending value
					{
						while (vendingList[vmIndex].coinChutes[index].Count > 0)	// Only operates if the current chute has coins
						{
							int currentCoinValue = vendingList[vmIndex].coinChutes[index].Peek().Value;

							if (difference >= currentCoinValue)						// Current chute coin value can be used to provide change
							{
								difference -= currentCoinValue;						// Update difference owed
								vendingList[vmIndex].deliveryChute.Add(vendingList[vmIndex].coinChutes[index].Dequeue());	// Dispense coin from current chute
							}
							else
								break;												// Move onto next chute since difference owed cannot be paid with current coin value
						}
					}
				}
			}
			else
				throw new Exception("Vending machine " + vmIndex + " does not exist");
        }

        public List<Deliverable> extractFromDeliveryChute(int vmIndex) {

			List<Deliverable> delivered = new List<Deliverable>();

			if (vmIndex < vendingList.Count)								// Valid machine
			{
				foreach (Deliverable thing in vendingList[vmIndex].deliveryChute)
					delivered.Add(thing);
			}
			else
				throw new Exception("Vending machine " + vmIndex + " does not exist");

            return delivered;
        }

        public List<IList> unloadVendingMachine(int vmIndex) {
			List<IList> unloaded  = new List<IList>();						// Initialize return list

			List<Coin> remainingChange = new List<Coin>();					// Initialize return sublist
			foreach(Queue<Coin> chute in vendingList[vmIndex].coinChutes)	// Travese each coin chute
			{
				while(chute.Count > 0)
					remainingChange.Add(chute.Dequeue());					// Move every coin in the chute to the return sublist
			}
			unloaded.Add(remainingChange);									// Add sublist to return list

			unloaded.Add(vendingList[vmIndex].moneyEarned);					// Add sublist to return list

			List<Pop> remainingPop = new List<Pop>();						// Initialize return sublist
			foreach(Queue<Pop> chute in vendingList[vmIndex].popChutes)		// Traverse each pop chute
			{
				while(chute.Count > 0)
					remainingPop.Add(chute.Dequeue());						// Move every pop in the chute to the return sublist
			}
			unloaded.Add(remainingPop);										// Add subist to return list

			vendingList.RemoveAt(vmIndex);									// Destroy machine

			return unloaded;
		}
    }
}