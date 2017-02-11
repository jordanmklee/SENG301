using Frontend1;
using System.Collections.Generic;

namespace seng301_asgn1
{
	public class VendingMachine
	{
		// Vending Machine properties and data
		public List<int> coinKinds { get; set; }
		public int selectionButtonCount { get; set; }
		public List<string> popNames { get; set; }
		public List<int> popCosts { get; set; }
		public List<int> indexDescending { get; }
		
		// Inventory
		public List<Queue<Coin>> coinChutes;	// Coins available for change
		public List<Queue<Pop>> popChutes;		// Pops available to be sold
		public List<Coin> moneyEarned;			// All coins entered into the machine

		// User I/O
		public List<Coin> coinSlot;				// Temporary List of coins entered into the machine
		public List<Deliverable> deliveryChute;	// List of items currently in the delivery chute

		public VendingMachine()
		{
			// Initialize instance variables
			coinKinds = new List<int>();
			selectionButtonCount = new int();
			popNames = new List<string>();
			popCosts = new List<int>();
			indexDescending = new List<int>();

			coinChutes = new List<Queue<Coin>>();
			popChutes = new List<Queue<Pop>>();
			moneyEarned = new List<Coin>();

			coinSlot = new List<Coin>();
			deliveryChute = new List<Deliverable>();
		}

		public int getIndexOfMax(List<int> exclude)
		{
			int indexOfMax = new int();
			int max = 0;
			foreach(int value in coinKinds)
			{
				if(!exclude.Contains(coinKinds.IndexOf(value)))		// Check if index is excluded
				{
					if(value > max)		// New largest value
					{
						max = value;	// Reset max
						indexOfMax = coinKinds.IndexOf(value);	// Reset indexOfMax
					}
				}
			}
			return indexOfMax;
		}

		public void sortCoinKinds()
		{
			while(indexDescending.Count != coinKinds.Count)
				indexDescending.Add(getIndexOfMax(indexDescending));
		}

	}
}