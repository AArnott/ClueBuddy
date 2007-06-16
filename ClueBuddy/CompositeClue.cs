using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	[Serializable]
	public class SuggestionResponse : INotifyPropertyChanged {
		bool? disproved;

		public bool? Disproved {
			get { return disproved; }
			set {
				if (disproved == value) return;
				disproved = value;
				OnPropertyChanged("Disproved");
			}
		}
		Card alabi;

		public Card Alabi {
			get { return alabi; }
			set {
				if (alabi == value) return;
				alabi = value;
				OnPropertyChanged("Alabi");
			}
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

	[Serializable]
	public class CompositeClue : Clue {
		public override Player Player {
			get {
				return base.Player;
			}
			set {
				base.Player = value;
				setupOpponents();
			}
		}

		void setupOpponents() {
			if (Player == null || Player.Game == null) {
				Responses.Clear();
			} else {
				Game g = Player.Game;
				// produce a clean dictionary of players, while preserving any hints we
				// had before
				var template = new Dictionary<Player, SuggestionResponse>();
				foreach (Player p in g.Players) {
					if (p != Player) {
						template[p] = Responses.ContainsKey(p) ? Responses[p] : new SuggestionResponse();
					}
				}
				// now clean out the actual dictionary we're using and inject the clean version.
				Responses.Clear();
				foreach (var pair in template) {
					Responses.Add(pair.Key, pair.Value);
				}
			}
			OnConstraintsChanged();
		}

		Suspicion suspicion = new Suspicion();
		/// <summary>
		/// Which three cards are trying to be disproven.
		/// </summary>
		public Suspicion Suspicion {
			get { return suspicion; }
			set {
				if (suspicion == value) return;
				if (suspicion != null) {
					suspicion.PropertyChanged -= new PropertyChangedEventHandler(suspicion_PropertyChanged);
				}
				suspicion = value;
				OnPropertyChanged("Suspicion");
				if (suspicion != null) {
					suspicion.PropertyChanged += new PropertyChangedEventHandler(suspicion_PropertyChanged);
				}
			}
		}

		void suspicion_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			OnConstraintsChanged();
		}

		Dictionary<Player, SuggestionResponse> responses = new Dictionary<Player, SuggestionResponse>();
		public Dictionary<Player, SuggestionResponse> Responses {
			get { return responses; }
		}

		IEnumerable<Clue> generateClues() {
			foreach (KeyValuePair<Player, SuggestionResponse> pair in Responses) {
				SuggestionResponse response = pair.Value;
				if (response.Disproved.HasValue) {
					if (response.Disproved.Value) {
						yield return new Disproved(pair.Key, Suspicion, pair.Value.Alabi);
					} else {
						yield return new CannotDisprove(pair.Key, Suspicion);
					}
				}
			}
		}

		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			return from clue in generateClues()
				   from constraint in clue.GetConstraints(nodes)
				   select constraint;
		}

		public override string ToString() {
			StringBuilder responsesStringBuilder = new StringBuilder();
			foreach (var pair in Responses) {
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
			return string.Format("{0} suggested {1}.  Responses: {2}", Player, Suspicion, responsesStringBuilder);
		}
	}
}
