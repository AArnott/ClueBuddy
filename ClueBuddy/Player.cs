using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ClueBuddy {
	/// <summary>
	/// A human player in the game.  Not a suspect.
	/// </summary>
	public class Player : ICardHolder {
		public Player(string name) {
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			this.name = name;
		}

		private string name;
		/// <summary>
		/// The name of the human player.
		/// </summary>
		public string Name {
			get { return name; }
			set { name = value; }
		}

		public override string ToString() {
			return Name;
		}

		private int cardsHeldCount;
		/// <summary>
		/// The number of cards in the player's hand.
		/// </summary>
		public int CardsHeldCount {
			get { return cardsHeldCount; }
			set {
				if (Game != null) throw new InvalidOperationException(Strings.IllegalAfterGameIsStarted);
				cardsHeldCount = value;
			}
		}

		private Game game;
		/// <summary>
		/// The game this player belongs to.
		/// </summary>
		public Game Game {
			get { return game; }
			internal set { game = value; }
		}
	}
}
