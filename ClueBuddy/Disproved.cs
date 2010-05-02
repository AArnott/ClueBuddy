// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Disproved.cs" company="">
//   
// </copyright>
// <summary>
//   A clue that a player can disprove a given suspicion.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;

	using NerdBank.Algorithms.NodeConstraintSelection;

	/// <summary>
	/// A clue that a player can disprove a given suggestion.
	/// </summary>
	[Serializable]
	public class Disproved : Clue {
		#region Constants and Fields

		/// <summary>
		/// The card shown.
		/// </summary>
		private Card cardShown;

		/// <summary>
		/// The suspicion.
		/// </summary>
		private Suspicion suspicion;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Disproved"/> class. 
		/// Creates a clue from when an opponent disproves the <see cref="Suspicion"/>
		/// of another opponent (besides the interacting <see cref="Player"/>.)
		/// </summary>
		/// <param name="disprovingPlayer">
		/// The player disproving the <see cref="Suspicion">suggestion</see>.
		/// </param>
		/// <param name="suggestion">
		/// The weapon, suspect and place being suspected.
		/// </param>
		public Disproved(Player disprovingPlayer, Suspicion suggestion)
			: this(disprovingPlayer, suggestion, null) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Disproved"/> class. 
		/// Creates a clue from when an opponent disproves the <see cref="Suspicion"/>
		/// of the interacting <see cref="Player"/>.
		/// </summary>
		/// <param name="opponent">
		/// The player who disproved the <see cref="Suspicion"/>.
		/// </param>
		/// <param name="suggestion">
		/// The weapon, suspect and place being suspected.
		/// </param>
		/// <param name="cardShown">
		/// The card that the opponent showed to disprove the <see cref="Suspicion"/>.
		/// </param>
		public Disproved(Player opponent, Suspicion suggestion, Card cardShown)
			: base(opponent) {
			Contract.Requires<ArgumentNullException>(suggestion != null, "suggestion");
			this.suspicion = suggestion;
			if (cardShown != null) {
				if (!suggestion.Cards.Contains(cardShown))
					throw new ArgumentException(string.Format(Strings.DisprovingCardNotInSuspicion, "cardShown", "suggestion"));
				this.cardShown = cardShown;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// The card shown to disprove the suspicion, if applicable.
		/// </summary>
		public Card CardShown {
			get {
				return this.cardShown;
			}

			private set {
				if (this.cardShown != value) {
					this.cardShown = value;
					this.OnPropertyChanged("CardShown");
				}
			}
		}

		/// <summary>
		/// The set of three cards involved in the suggestion.
		/// </summary>
		public Suspicion Suspicion {
			get {
				return this.suspicion;
			}

			set {
				if (this.suspicion != value) {
					this.suspicion = value;
					this.OnPropertyChanged("Suspicion");
				}
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
			if (this.CardShown != null) {
				return string.Format("{0} disproved {1} by showing {2}.", this.Player, this.Suspicion, this.CardShown);
			}

			return string.Format("{0} disproved {1}.", this.Player, this.Suspicion);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the constraints that can be inferred from the clue.
		/// </summary>
		/// <param name="nodes">The nodes from which to construct the constraints.</param>
		/// <returns>
		/// A sequence of constraints that the clue creates.
		/// </returns>
		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			if (this.CardShown == null) {
				var constrainedNodes = nodes.Where(n => n.CardHolder == this.Player && this.Suspicion.Cards.Contains(n.Card)).OfType<INode>();
				if (constrainedNodes.Count() != this.Suspicion.Cards.Count()) {
					throw new ArgumentException(Strings.IncompleteNodesList, "nodes");
				}
				yield return SelectionCountConstraint.MinSelected(1, constrainedNodes);
			} else {
				var constrainedNodes = nodes.Where(n => n.CardHolder == this.Player && n.Card == this.CardShown).OfType<INode>();
				if (constrainedNodes.Count() != 1) {
					throw new ArgumentException(Strings.IncompleteNodesList, "nodes");
				}
				yield return SelectionCountConstraint.ExactSelected(1, constrainedNodes);
			}
		}

		#endregion
	}
}
