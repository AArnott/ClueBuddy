using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public interface ICardHolder {
		/// <summary>
		/// The number of cards in the player's hand or in the Case File.
		/// </summary>
		int CardsHeldCount { get; }
		Game Game { get; }
	}
}
