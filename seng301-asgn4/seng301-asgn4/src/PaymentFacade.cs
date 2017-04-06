using Frontend4.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seng301_asgn4.src
{
	class PaymentFacade
	{
		private HardwareFacade hardware;
		public event EventHandler<CoinEventArgs> CoinInserted;

		public PaymentFacade(HardwareFacade hf)
		{
			hardware = hf;
			hardware.CoinReceptacle.CoinAdded += new EventHandler<CoinEventArgs>(CoinAdded);		
		}

		// Coin was inserted, notify BusinessRule
		private void CoinAdded(object sender, CoinEventArgs e)
		{
			if(this.CoinInserted != null)
			{
				this.CoinInserted(this, new CoinEventArgs() {Coin = e.Coin});
			}
		}

		// Checks whether or not certain coinRack has coins left
		public Boolean hasRemaining(int index)
		{
			if(this.hardware.CoinRacks[index].Count > 0)
				return true;
			return false;
		}

		// Moves a coin from input index of coinRack to delivery
		public void releaseCoin(int index)
		{
			this.hardware.CoinRacks[index].ReleaseCoin();
		}

		// Move coins from receptacle to racks/bin
		public void storeCoins()
		{
			this.hardware.CoinReceptacle.StoreCoins();				// Attempt to store coins
		}
		
	}
}
