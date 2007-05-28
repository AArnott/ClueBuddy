using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public class Disproved : Clue {
		/// <summary>
		/// Creates a clue from when an opponent disproves the <see cref="Suspicion"/>
		/// of another opponent (besides the interacting <see cref="Player"/>.)
		/// </summary>
		/// <param name="disprovingPlayer">The player disproving the <see cref="Suspicion">suggestion</see>.</param>
		/// <param name="suggestion">The weapon, suspect and location being suspected.</param>
		public Disproved(Player disprovingPlayer, Suspicion suggestion)
			: base(disprovingPlayer) {
			if (suggestion == null) throw new ArgumentNullException("suggestion");
			this.Suspicion = suggestion;
		}
		/// <summary>
		/// Creates a clue from when an opponent disproves the <see cref="Suspicion"/>
		/// of the interacting <see cref="Player"/>.
		/// </summary>
		/// <param name="opponent">The player who disproved the <see cref="Suspicion"/>.</param>
		/// <param name="suggestion">The weapon, suspect and location being suspected.</param>
		/// <param name="cardShown">The card that the opponent showed to disprove the <see cref="Suspicion"/>.</param>
		public Disproved(Player opponent, Suspicion suggestion, Card cardShown)
			: this(opponent, suggestion) {
			if (cardShown == null)
				throw new ArgumentNullException("cardShown");
			if (!suggestion.Cards.Contains(cardShown))
				throw new ArgumentException(string.Format(Strings.DisprovingCardNotInSuspicion, "cardShown", "suggestion"));
			this.CardShown = cardShown;
		}


		private Suspicion suspicion;
		/// <summary>
		/// The set of three cards involved in the suggestion.
		/// </summary>
		public Suspicion Suspicion {
			get { return suspicion; }
			private set { suspicion = value; }
		}

		public override string ToString() {
			if (CardShown != null)
				return string.Format("{0} disproved {1} by showing {2}.", Player, Suspicion, CardShown);
			else
				return string.Format("{0} disproved {1}.", Player, Suspicion);
		}

		private Card cardShown;
		/// <summary>
		/// The card shown to disprove the suspicion, if applicable.
		/// </summary>
		public Card CardShown {
			get { return cardShown; }
			private set { cardShown = value; }
		}

		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			if (nodes == null) throw new ArgumentNullException("nodes");
			if (CardShown == null) {
				var constrainedNodes = nodes.Where(n => n.CardHolder == Player && Suspicion.Cards.Contains(n.Card)).OfType<INode>();
				if (constrainedNodes.Count() != Suspicion.Cards.Count())
					throw new ArgumentException(Strings.IncompleteNodesList, "nodes");
				yield return SelectionCountConstraint.MinSelected(1, constrainedNodes);
			} else {
				var constrainedNodes = nodes.Where(n => n.CardHolder == Player && n.Card == CardShown).OfType<INode>();
				if (constrainedNodes.Count() != 1)
					throw new ArgumentException(Strings.IncompleteNodesList, "nodes");
				yield return SelectionCountConstraint.ExactSelected(1, constrainedNodes);
			}
		}
	}
}
