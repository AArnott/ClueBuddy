using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NerdBank.Algorithms.NodeConstraintSelection;

namespace ClueBuddy {
	public class BadClueEventArgs:EventArgs {
		private Game game;

		internal BadClueEventArgs(Game game, BrokenConstraintException ex) {
			this.game = game;
			this.Exception = ex;
		}

		public bool Handled { get; private set; }

		public BrokenConstraintException Exception { get; private set; }

		public IEnumerable<Clue> SuspectClues {
			get { return game.FindContradictingClues(); }
		}

		public void SetHandled() {
			this.Handled = true;
		}
	}
}
