//-----------------------------------------------------------------------
// <copyright file="ClueContract.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;

	using NerdBank.Algorithms.NodeConstraintSelection;

	/// <summary>
	/// Contract class for the <see cref="Clue"/> class.
	/// </summary>
	[ContractClassFor(typeof(Clue))]
	internal abstract class ClueContract : Clue {
		/// <summary>
		/// Prevents a default instance of the <see cref="ClueContract"/> class from being created.
		/// </summary>
		private ClueContract() {
		}

		/// <summary>
		/// Gets the constraints that can be inferred from the clue.
		/// </summary>
		/// <param name="nodes">The nodes from which to construct the constraints.</param>
		/// <returns>
		/// A sequence of constraints that the clue creates.
		/// </returns>
		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			Contract.Requires<ArgumentNullException>(nodes != null, "nodes");
			throw new NotImplementedException();
		}
	}
}