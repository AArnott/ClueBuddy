using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NerdBank.Algorithms.NodeConstraintSelection;

namespace ClueBuddy {
	[Serializable]
	public class SpyCard : Clue {
		public SpyCard() { }
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

		public IEnumerable<Card> PossiblySeenCards {
			get {
				if (Player != null && Player.Game != null) {
					return from n in Player.Game.Nodes
						   where n.CardHolder == Player && (!n.IsSelected.HasValue || n.IsSelected.Value)
						   select n.Card;
				} else {
					throw new InvalidOperationException(string.Format(Strings.PropertyMustBeSetFirst, "Player.Game"));
				}
			}
		}

		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			if (nodes == null) throw new ArgumentNullException("nodes");
			if (Card != null && Player != null) {
				var constrainedNodes = nodes.Where(n => n.Card == Card && n.CardHolder == Player).OfType<INode>();
				if (constrainedNodes.Count() != 1)
					throw new ArgumentException(Strings.IncompleteNodesList, "nodes");
				yield return SelectionCountConstraint.ExactSelected(1, constrainedNodes);
			}
		}

		public override string ToString() {
			return string.Format("{0} has {1}.",
				Player != null ? Player.Name : "Someone",
				Card != null ? Card.Name : "some card");
		}
	}
}
