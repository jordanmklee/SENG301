using Frontend4;
using Frontend4.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seng301_asgn4.src
{
	class CommunicactionFacade
	{
		private HardwareFacade hardware;
		public event EventHandler buttonPressed;
	
		public CommunicactionFacade(HardwareFacade hf)
		{
			hardware = hf;
			// Sets up listeners for each button
			foreach(SelectionButton button in hardware.SelectionButtons)
				button.Pressed += new EventHandler(pressed);	
		}

		// Button was pressed, notify BusinessRule
		private void pressed(object sender, EventArgs e)
		{
			if(this.buttonPressed != null)
			{
				this.buttonPressed(this, e);
			}
		}

		// Returns the index of button of sender
		public int getIndex(object sender)
		{
			return Array.IndexOf(this.hardware.SelectionButtons, sender) + 1;
		}

		// Returns the cost of product at input index
		public int getPrice(int index)
		{
			return this.hardware.ProductKinds[index].Cost.Value;
		}


	}
}
