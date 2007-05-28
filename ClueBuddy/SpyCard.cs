using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public class SpyCard : Clue {
		public SpyCard(Player playerShowingCard, Card cardSeen)
			: base(playerShowingCard) {
			if (cardSeen == null) throw new ArgumentNullException("cardSeen");
			this.card = cardSeen;
		}

		Card card;
		/// <summary>
		/// The card shown.
		/// </summary>
		public Card Card {
			get { return card; }
			set {
				if (card == value) return; // nothing to change
				card = value;
				OnPropertyChanged("Card");
			}
		}

		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			if (nodes == null) throw new ArgumentNullException("nodes");
			var constrainedNodes = nodes.Where(n => n.Card == Card && n.CardHolder == Player).OfType<INode>();
			if (constrainedNodes.Count() != 1)
				throw new ArgumentException(Strings.IncompleteNodesList, "nodes");
			yield return SelectionCountConstraint.ExactSelected(1, constrainedNodes);
		}
	}
}
