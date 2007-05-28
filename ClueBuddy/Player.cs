using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ClueBuddy {
	/// <summary>
	/// A human player in the game.  Not a suspect.
	/// </summary>
	public class Player : ICardHolder, INotifyPropertyChanged {
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
			set {
				if (name == value) return;
				name = value;
				OnPropertyChanged("Name");
			}
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
				if (cardsHeldCount == value) return;
				cardsHeldCount = value;
				OnPropertyChanged("CardsHeldCount");
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

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName) {
			PropertyChangedEventHandler propertyChanged = PropertyChanged;
			if (propertyChanged != null) {
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion
	}
}
