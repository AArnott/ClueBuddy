//-----------------------------------------------------------------------
// <copyright file="BadAccusation.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;

	using NerdBank.Algorithms.NodeConstraintSelection;

	/// <summary>
	/// Represents an accusation made by another player that ended up being wrong.
	/// </summary>
	[Serializable]
	public class BadAccusation : Clue {
		/// <summary>
		/// The case file.
		/// </summary>
		private readonly CaseFile caseFile;
		
		/// <summary>
		/// The suggestion.
		/// </summary>
		private readonly Suspicion suggestion;

		/// <summary>
		/// Initializes a new instance of the <see cref="BadAccusation"/> class.
		/// </summary>
		/// <param name="suggestion">The suggestion.</param>
		/// <param name="caseFile">The case file.</param>
		public BadAccusation(Suspicion suggestion, CaseFile caseFile) {
			Contract.Requires<ArgumentNullException>(suggestion != null, "suggestion");
			Contract.Requires<ArgumentNullException>(caseFile != null, "caseFile");
			this.caseFile = caseFile;
			this.suggestion = suggestion;
		}

		/// <summary>
		/// Gets the constraints that can be inferred from the clue.
		/// </summary>
		/// <param name="nodes">The nodes from which to construct the constraints.</param>
		/// <returns>
		/// A sequence of constraints that the clue creates.
		/// </returns>
		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			yield return new SelectionCountConstraint(0, 2, true,
				nodes.Where(n => n.CardHolder == caseFile && suggestion.Cards.Contains(n.Card))
				.OfType<INode>()
			);
		}
	}
}
