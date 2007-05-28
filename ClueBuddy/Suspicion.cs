using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public class Suspicion {
		public Suspicion(Suspect suspect, Weapon weapon, Place place) {
			if (suspect == null) throw new ArgumentNullException("suspect");
			if (weapon == null) throw new ArgumentNullException("weapon");
			if (place == null) throw new ArgumentNullException("place");
			this.Suspect = suspect;
			this.Weapon = weapon;
			this.Place = place;
		}
		public readonly Suspect Suspect;
		public readonly Weapon Weapon;
		public readonly Place Place;
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
	}
}
