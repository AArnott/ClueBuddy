using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public abstract class Clue : INotifyPropertyChanged {
		protected Clue() { }
		protected Clue(Player player) {
			if (player == null) throw new ArgumentNullException("player");
			this.player = player;
		}
		Player player;
		/// <summary>
		/// The player the clue is regarding.
		/// </summary>
		public Player Player {
			get { return player; }
			set {
				if (player == value) return; // nothing to change
				player = value;
				OnPropertyChanged("Player");
			}
		}

		/// <summary>
		/// The constraints that can be inferred from the clue.
		/// </summary>
		internal abstract IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes);

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
