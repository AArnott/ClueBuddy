//-----------------------------------------------------------------------
// <copyright file="SpyCard.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;

	using NerdBank.Algorithms.NodeConstraintSelection;

	[Serializable]
	public class SpyCard : Clue {
		/// <summary>
		/// The card that was seen.
		/// </summary>
		private Card card;

		/// <summary>
		/// Initializes a new instance of the <see cref="SpyCard"/> class.
		/// </summary>
		public SpyCard() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="SpyCard"/> class.
		/// </summary>
		/// <param name="playerShowingCard">The player showing card.</param>
		/// <param name="cardSeen">The card seen.</param>
		public SpyCard(Player playerShowingCard, Card cardSeen)
			: base(playerShowingCard) {
			Contract.Requires<ArgumentNullException>(cardSeen != null, "cardSeen");
			this.card = cardSeen;
		}

		/// <summary>
		/// Gets or sets the card shown.
		/// </summary>
		public Card Card {
			get {
				return card;
			}

			set {
				if (card != value) {
					this.card = value;
					OnPropertyChanged("Card");
				}
			}
		}

		/// <summary>
		/// Gets the cards possibly seen by the player.
		/// </summary>
		public IEnumerable<Card> PossiblySeenCards {
			get {
				Contract.Requires<InvalidOperationException>(this.Player != null && this.Player.Game != null);
				return from n in this.Player.Game.Nodes
					   where n.CardHolder == Player && (!n.IsSelected.HasValue || n.IsSelected.Value)
					   select n.Card;
			}
		}

		/// <summary>
		/// Gets the constraints that can be inferred from the clue.
		/// </summary>
		/// <param name="nodes">The nodes from which to construct the constraints.</param>
		/// <returns>
		/// A sequence of constraints that the clue creates.
		/// </returns>
		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			if (Card != null && Player != null) {
				var constrainedNodes = nodes.Where(n => n.Card == Card && n.CardHolder == Player).OfType<INode>();
				if (constrainedNodes.Count() != 1) {
					throw new ArgumentException(Strings.IncompleteNodesList, "nodes");
				}
				yield return SelectionCountConstraint.ExactSelected(1, constrainedNodes);
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"{0} has {1}.",
				Player != null ? Player.Name : "Someone",
				Card != null ? Card.Name : "some card");
		}
	}
}
