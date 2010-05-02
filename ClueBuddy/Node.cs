namespace ClueBuddy {
	using System;
	using System.Diagnostics.Contracts;

	using NerdBank.Algorithms.NodeConstraintSelection;

	/// <summary>
	/// Represents the possibility that a particular card is held by a player or the case file.
	/// </summary>
	[Serializable]
	internal class Node : NodeBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="Node"/> class.
		/// </summary>
		/// <param name="cardHolder">The card holder.</param>
		/// <param name="card">The card.</param>
		public Node(ICardHolder cardHolder, Card card) {
			Contract.Requires<ArgumentNullException>(cardHolder != null, "cardHolder");
			Contract.Requires<ArgumentNullException>(card != null, "card");

			this.CardHolder = cardHolder;
			this.Card = card;
		}

		/// <summary>
		/// Gets the card that may be held by <see cref="Player"/>.
		/// </summary>
		public Card Card { get; private set; }

		/// <summary>
		/// Gets the player or Case File who may hold <see cref="Card"/>.
		/// </summary>
		public ICardHolder CardHolder { get; private set; }

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString() {
			return string.Format("({0}, {1}) = {2}", this.CardHolder, this.Card, IsSelected.HasValue ? IsSelected.Value.ToString() : "?");
		}

		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.Card != null);
			Contract.Invariant(this.CardHolder != null);
		}
	}
}
