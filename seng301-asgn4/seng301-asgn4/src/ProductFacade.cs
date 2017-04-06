using Frontend4.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seng301_asgn4.src
{
	class ProductFacade
	{
		private HardwareFacade hardware;

		public ProductFacade(HardwareFacade hf)
		{
			hardware = hf;
		}

		// Checks whether or not a product is sold out
		public Boolean checkStock(int index)
		{
			if(this.hardware.ProductRacks[index].Count > 0)
				return true;
			return false;
		}

		// Moves product from index chute to delivery
		public void dispense(int index)
		{
			this.hardware.ProductRacks[index].DispenseProduct();	// Send product to delivery
		}
	}
}
