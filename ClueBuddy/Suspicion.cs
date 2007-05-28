using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	public class Suspicion {
		public Suspicion(Suspect suspect, Weapon weapon, Location location) {
			if (suspect == null) throw new ArgumentNullException("suspect");
			if (weapon == null) throw new ArgumentNullException("weapon");
			if (location == null) throw new ArgumentNullException("location");
			this.Suspect = suspect;
			this.Weapon = weapon;
			this.Location = location;
		}
		public readonly Suspect Suspect;
		public readonly Weapon Weapon;
		public readonly Location Location;
		public IEnumerable<Card> Cards {
			get {
				yield return Suspect;
				yield return Weapon;
				yield return Location;
			}
		}

		public override string ToString() {
			return string.Format("[{0}, {1}, {2}]", Suspect, Weapon, Location);
		}
	}
}
