using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public abstract class Clue {
		protected Clue(Player player) {
			if (player == null) throw new ArgumentNullException("player");
			this.Player = player;
		}
		/// <summary>
		/// The player the clue is regarding.
		/// </summary>
		public readonly Player Player;

		/// <summary>
		/// The constraints that can be inferred from the clue.
		/// </summary>
		internal abstract IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes);
	}
}
