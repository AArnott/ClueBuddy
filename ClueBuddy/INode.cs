using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClueBuddy {
	interface INode {
		bool? IsSelected { get; set; }
		bool IsSimulating { get; }
		/// <summary>
		/// Pushes one level deeper in the simulation.
		/// </summary>
		void PushSimulation();
		/// <summary>
		/// Pops off the deepest simulation level.
		/// </summary>
		/// <returns>
		/// True if the simulation level is 0 (no simulation).
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown when no simulation is on the stack.
		/// </exception>
		bool PopSimulation();
		event EventHandler<ValueChangedEventArgs<bool?>> IsSelectedChanged;
	}
}
