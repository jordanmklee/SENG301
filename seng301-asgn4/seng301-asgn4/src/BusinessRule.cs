using Frontend4;
using Frontend4.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seng301_asgn4.src
{
	class BusinessRule
	{
		PaymentFacade payment;
		ProductFacade product;
		CommunicactionFacade communication;

		private List<int> indexDescending;	// Order of coins from highest denomination to lowest
		private List<int> coinKindsList;	// List of coin denominations

		private int insertedValue;			// Counter of current credit that can be used

		public BusinessRule(PaymentFacade payment, ProductFacade product, CommunicactionFacade communication, Cents[] coinKinds)
		{
			this.payment = payment;
			this.product = product;
			this.communication = communication;

			// Set up list of denominations
			convertToList(coinKinds);
			sortCoinKinds(coinKindsList);

			insertedValue = 0;		// Set insertedValue to 0

			// Initialize listeners
			payment.CoinInserted += new EventHandler<CoinEventArgs>(CoinInserted);
			communication.buttonPressed += new EventHandler(ButtonPressed);
		}

		// Converts list of Cents to their values, for easier maniupulation
		private void convertToList(Cents[] coinKinds)
		{
			coinKindsList = new List<int>();
			foreach(Cents cent in coinKinds)
				coinKindsList.Add(cent.Value);
		}

		// Pressed button, if enough, then dispense product and change
		private void ButtonPressed(object sender, EventArgs e)
		{
			int buttonIndex = communication.getIndex(sender);			// Get index of button pressed
			int itemCost = communication.getPrice(buttonIndex);			// Get cost of product at that index
						
			if(insertedValue >= itemCost)								// Check if enough money has been inserted
			{
				if(product.checkStock(buttonIndex))						// Only proceed if there is stock left
				{
					product.dispense(buttonIndex);						// Dispaense product
					payment.storeCoins();								// Store coins that were inserted

					int difference = insertedValue - itemCost;			// Calculate change owed
					insertedValue = 0;									// Reset credit
					foreach(int value in indexDescending)
					{
						while(payment.hasRemaining(value))				// If coinRack has coins left, vend it
						{
							if(coinKindsList[value] <= difference)		// Check if coin value can be used to pay change
							{
								payment.releaseCoin(value);				// Release coin
								difference -= coinKindsList[value];		// Decrement difference owed
							}
							else
								break;
						}
					}
					insertedValue += difference;						// Add remaining credit back to value counter
				}
			}
		}

		// Increment counter of current inserted value
		private void CoinInserted(object sender, CoinEventArgs e)
		{
			insertedValue += e.Coin.Value.Value;
		}

		// Returns the order of indices st. the values are ordered highest to lowest
		private void sortCoinKinds(List<int> coinKinds)
		{
			indexDescending = new List<int>();
			while(indexDescending.Count != coinKinds.Count)
				indexDescending.Add(getIndexOfMax(indexDescending, coinKinds));
		}

		// Returns the highest value coin index, excluding specified coins
		private int getIndexOfMax(List<int> exclude, List<int> coinKinds)
		{
			int indexOfMax = new int();
			int max = 0;
			foreach(int value in coinKinds)
			{
				if(!exclude.Contains(coinKinds.IndexOf(value)))	// Check if index is excluded
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
	
	}
}
