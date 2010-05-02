//-----------------------------------------------------------------------
// <copyright file="CannotDisprove.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;

	using NerdBank.Algorithms.NodeConstraintSelection;

	/// <summary>
	/// A clue that a player cannot disprove some suggestion.
	/// </summary>
	[Serializable]
	public class CannotDisprove : Clue {
		/// <summary>
		/// Backing field for the <see cref="Suspicion"/> property.
		/// </summary>
		private Suspicion suspicion;

		/// <summary>
		/// Initializes a new instance of the <see cref="CannotDisprove"/> class.
		/// </summary>
		/// <param name="unablePlayer">The player that cannot disprove the suspicion.</param>
		/// <param name="suspicion">The suspicion.</param>
		public CannotDisprove(Player unablePlayer, Suspicion suspicion)
			: base(unablePlayer) {
			Contract.Requires<ArgumentNullException>(unablePlayer != null, "unablePlayer");
			Contract.Requires<ArgumentNullException>(suspicion != null, "suspicion");
			this.suspicion = suspicion;
		}

		/// <summary>
		/// Gets or sets the suggestion that cannot be disproven.
		/// </summary>
		public Suspicion Suspicion {
			get {
				return suspicion;
			}

			set
			{
				if (suspicion != value)
				{
					suspicion = value;
					OnPropertyChanged("Suspicion");
				}
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString() {
			return string.Format("{0} could not disprove {1}", Player, Suspicion);
		}

		/// <summary>
		/// Gets the constraints that can be inferred from the clue.
		/// </summary>
		/// <param name="nodes">The nodes from which to construct the constraints.</param>
		/// <returns>
		/// A sequence of constraints that the clue creates.
		/// </returns>
		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			var constrainedNodes = nodes.Where(n => n.CardHolder == Player && Suspicion.Cards.Contains(n.Card)).OfType<INode>();
			if (constrainedNodes.Count() != Suspicion.Cards.Count())
			{
				throw new ArgumentException(Strings.IncompleteNodesList, "nodes");
			}
			yield return SelectionCountConstraint.MaxSelected(0, constrainedNodes);
		}
	}
}
