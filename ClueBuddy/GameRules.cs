//-----------------------------------------------------------------------
// <copyright file="GameRules.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ClueBuddy {
	using System;

	/// <summary>
	/// The rules for a particular variety of the game.
	/// </summary>
	[Serializable]
	public struct GameRules {
		/// <summary>
		/// Gets or sets a value indicating whether a player's turn ends at the first disproval resulting from a suspicion.
		/// </summary>
		public bool DisprovalEndsTurn { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the board has certain spaces that allow a player to look at a random card
		/// of another player.
		/// </summary>
		public bool HasSpyglass { get; set; }
	}
}
