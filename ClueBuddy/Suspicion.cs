using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ClueBuddy {
	[Serializable]
	public class Suspicion : INotifyPropertyChanged {
		public Suspicion() { }
		public Suspicion(Suspect suspect, Weapon weapon, Place place) {
			if (suspect == null) throw new ArgumentNullException("suspect");
			if (weapon == null) throw new ArgumentNullException("weapon");
			if (place == null) throw new ArgumentNullException("place");
			this.suspect = suspect;
			this.weapon = weapon;
			this.place = place;
		}

		Suspect suspect;
		public Suspect Suspect {
			get { return suspect; }
			set {
				if (suspect == value) return;
				suspect = value;
				OnPropertyChanged("Suspect");
			}
		}
		Weapon weapon;
		public Weapon Weapon {
			get { return weapon; }
			set {
				if (weapon == value) return;
				weapon = value;
				OnPropertyChanged("Weapon");
			}
		}
		Place place;
		public Place Place {
			get { return place; }
			set {
				if (place == value) return;
				place = value;
				OnPropertyChanged("Place");
			}
		}

		public IEnumerable<Card> Cards {
			get {
				yield return Suspect;
				yield return Weapon;
				yield return Place;
			}
		}

		public override string ToString() {
			return string.Format("[{0}, {1}, {2}]", Suspect, Weapon, Place);
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
