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
	}
}
