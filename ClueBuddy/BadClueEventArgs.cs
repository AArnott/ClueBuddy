//-----------------------------------------------------------------------
// <copyright file="BadClueEventArgs.cs" company="Andrew Arnott">
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
	/// Arguments sent to event handlers listening for bad clues.
	/// </summary>
	public class BadClueEventArgs : EventArgs {
		/// <summary>
		/// The game
		/// </summary>
		private readonly Game game;

		/// <summary>
		/// Initializes a new instance of the <see cref="BadClueEventArgs"/> class.
		/// </summary>
		/// <param name="game">The game.</param>
		/// <param name="ex">The exception describing the broken constraint.</param>
		internal BadClueEventArgs(Game game, BrokenConstraintException ex) {
			Contract.Requires<ArgumentNullException>(game != null, "game");
			Contract.Requires<ArgumentNullException>(ex != null, "ex");

			this.game = game;
			this.Exception = ex;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="BadClueEventArgs"/> is handled.
		/// </summary>
		/// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
		public bool Handled { get; private set; }

		/// <summary>
		/// Gets or sets the exception describing the constraint violation.
		/// </summary>
		public BrokenConstraintException Exception { get; private set; }

		public IEnumerable<Clue> SuspectClues {
			get { return this.game.FindContradictingClues(); }
		}

		/// <summary>
		/// Sets the <see cref="Handled"/> property to true.
		/// </summary>
		public void SetHandled() {
			this.Handled = true;
		}

		[ContractInvariantMethod]
		private void ObjectInvariant() {
			Contract.Invariant(this.game != null);
			Contract.Invariant(this.Exception != null);
		}

	}
}
