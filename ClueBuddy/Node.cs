using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NerdBank.Algorithms.NodeConstraintSelection;

namespace ClueBuddy {
	[Serializable]
	class Node : NodeBase {
		public Node(ICardHolder cardHolder, Card card) {
			if (cardHolder == null) throw new ArgumentNullException("cardHolder");
			if (card == null) throw new ArgumentNullException("card");

			this.CardHolder = cardHolder;
			this.Card = card;
		}

		private Card card;
		/// <summary>
		/// The card that may be held by <see cref="Player"/>.
		/// </summary>
		public Card Card {
			get { return card; }
			private set { card = value; }
		}

		private ICardHolder cardHolder;
		/// <summary>
		/// The player or Case File who may hold <see cref="Card"/>.
		/// </summary>
		public ICardHolder CardHolder {
			get { return cardHolder; }
			private set { cardHolder = value; }
		}

		public override string ToString() {
			return string.Format("({0}, {1}) = {2}", CardHolder, Card, IsSelected.HasValue ? IsSelected.Value.ToString() : "?");
		}
	}
}
