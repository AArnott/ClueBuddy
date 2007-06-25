using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	[Serializable]
	public struct GameRules {
		/// <summary>
		/// Whether a player's turn ends at the first disproval resulting from a suspicion.
		/// </summary>
		public bool DisprovalEndsTurn;
		/// <summary>
		/// Whether the board has certain spaces that allow a player to look at a random card
		/// of another player.
		/// </summary>
		public bool HasSpyglass;
	}
}
