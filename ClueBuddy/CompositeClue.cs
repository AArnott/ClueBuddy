// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompositeClue.cs" company="">
//   
// </copyright>
// <summary>
//   The suggestion response.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClueBuddy {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Text;

	using NerdBank.Algorithms.NodeConstraintSelection;

	/// <summary>
	/// The composite clue.
	/// </summary>
	[Serializable]
	public class CompositeClue : Clue {
		#region Constants and Fields

		/// <summary>
		/// The responses.
		/// </summary>
		private readonly Dictionary<Player, SuggestionResponse> responses = new Dictionary<Player, SuggestionResponse>();

		/// <summary>
		/// The suspicion.
		/// </summary>
		private Suspicion suspicion = new Suspicion();

		#endregion

		#region Properties

		/// <summary>
		/// The player the clue is regarding.
		/// Either the player making the suggestion or the player whose card was seen.
		/// </summary>
		public override Player Player {
			get {
				return base.Player;
			}

			set {
				base.Player = value;
				this.SetupOpponents();
			}
		}

		/// <summary>
		/// Gets the responses.
		/// </summary>
		/// <value>The responses.</value>
		public Dictionary<Player, SuggestionResponse> Responses {
			get { return this.responses; }
		}

		/// <summary>
		/// Gets or sets which three cards are trying to be disproven.
		/// </summary>
		public Suspicion Suspicion {
			get {
				return this.suspicion;
			}

			set
			{
				if (this.suspicion == value)
				{
					return;
				}
				if (this.suspicion != null)
				{
					this.suspicion.PropertyChanged -= this.SuspicionPropertyChanged;
				}
				this.suspicion = value;
				this.OnPropertyChanged("Suspicion");
				if (this.suspicion != null) {
					this.suspicion.PropertyChanged += this.SuspicionPropertyChanged;
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
			StringBuilder responsesStringBuilder = new StringBuilder();
			foreach (var pair in this.Responses) {
				if (pair.Value.Disproved.HasValue) {
					if (!pair.Value.Disproved.Value) {
						responsesStringBuilder.Append("!");
					}
					responsesStringBuilder.Append(pair.Key.Name);
					if (pair.Value.Alabi != null) {
						responsesStringBuilder.AppendFormat(":{0}", pair.Value.Alabi.Name);
					}
					responsesStringBuilder.Append(", ");
				}
			}
			responsesStringBuilder.Length -= 2;
			return string.Format("{0} suggested {1}.  Responses: {2}", this.Player, this.Suspicion, responsesStringBuilder);
		}

		#endregion

		#region Methods

		/// <summary>
		/// The constraints that can be inferred from the clue.
		/// </summary>
		/// <param name="nodes">The nodes from which to construct the constraints.</param>
		/// <returns>
		/// A sequence of constraints that the clue creates.
		/// </returns>
		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			return from clue in this.GenerateClues()
				   from constraint in clue.GetConstraints(nodes)
				   select constraint;
		}

		/// <summary>
		/// Generates the individual clues that make up this composite clue.
		/// </summary>
		/// <returns>A sequence of clues.</returns>
		private IEnumerable<Clue> GenerateClues() {
			foreach (KeyValuePair<Player, SuggestionResponse> pair in this.Responses) {
				SuggestionResponse response = pair.Value;
				if (response.Disproved.HasValue) {
					if (response.Disproved.Value) {
						yield return new Disproved(pair.Key, this.Suspicion, pair.Value.Alabi);
					} else {
						yield return new CannotDisprove(pair.Key, this.Suspicion);
					}
				}
			}
		}

		/// <summary>
		/// Setups the opponents.
		/// </summary>
		private void SetupOpponents() {
			if (this.Player == null || this.Player.Game == null) {
				this.Responses.Clear();
			} else {
				Game g = this.Player.Game;

				// produce a clean dictionary of players, while preserving any hints we
				// had before
				var template = new Dictionary<Player, SuggestionResponse>();
				foreach (Player p in g.Players) {
					if (p != this.Player) {
						template[p] = this.Responses.ContainsKey(p) ? this.Responses[p] : new SuggestionResponse();
					}
				}

				// now clean out the actual dictionary we're using and inject the clean version.
				this.Responses.Clear();
				foreach (var pair in template) {
					this.Responses.Add(pair.Key, pair.Value);
				}
			}
			this.OnConstraintsChanged();
		}

		/// <summary>
		/// Handles the PropertyChanged event of the suspicion control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		private void SuspicionPropertyChanged(object sender, PropertyChangedEventArgs e) {
			this.OnConstraintsChanged();
		}

		#endregion
	}
}
