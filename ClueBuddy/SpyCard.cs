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

	/// <summary>
	/// A clue from being able to peek at another player's card.
	/// </summary>
	[Serializable]
	public class SpyCard : Clue {
		#region Constants and Fields

		/// <summary>
		/// The card that was seen.
		/// </summary>
		private Card card;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SpyCard"/> class.
		/// </summary>
		public SpyCard() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="SpyCard"/> class.
		/// </summary>
		/// <param name="playerShowingCard">
		/// The player showing card.
		/// </param>
		/// <param name="cardSeen">
		/// The card seen.
		/// </param>
		public SpyCard(Player playerShowingCard, Card cardSeen)
			: base(playerShowingCard) {
			Contract.Requires<ArgumentNullException>(playerShowingCard != null, "player");
			Contract.Requires<ArgumentNullException>(cardSeen != null, "cardSeen");
			this.card = cardSeen;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the card shown.
		/// </summary>
		public Card Card {
			get {
				return this.card;
			}

			set {
				if (this.card != value) {
					this.card = value;
					this.OnPropertyChanged("Card");
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
					   where n.CardHolder == this.Player && (!n.IsSelected.HasValue || n.IsSelected.Value)
					   select n.Card;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"{0} has {1}.",
				this.Player != null ? this.Player.Name : "Someone",
				this.Card != null ? this.Card.Name : "some card");
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the constraints that can be inferred from the clue.
		/// </summary>
		/// <param name="nodes">
		/// The nodes from which to construct the constraints.
		/// </param>
		/// <returns>
		/// A sequence of constraints that the clue creates.
		/// </returns>
		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			if (this.Card != null && this.Player != null) {
				var constrainedNodes = nodes.Where(n => n.Card == this.Card && n.CardHolder == this.Player).OfType<INode>();
				if (constrainedNodes.Count() != 1) {
					throw new ArgumentException(Strings.IncompleteNodesList, "nodes");
				}
				yield return SelectionCountConstraint.ExactSelected(1, constrainedNodes);
			}
		}

		#endregion
	}
}
