
using System;

namespace _3PA.NppCore {
    internal static partial class Sci {
        /// <summary>
        /// A Marker handle.
        /// </summary>
        /// <remarks>
        /// This is an opaque type, meaning it can be used by a Scintilla control but
        /// otherwise has no public members of its own.
        /// </remarks>
        public struct MarkerHandle {
            internal IntPtr Value;

            /// <summary>
            /// Returns a value indicating whether this instance is equal to a specified object.
            /// </summary>
            /// <param name="obj">An object to compare with this instance or null.</param>
            /// <returns>
            /// true if <paramref name="obj" /> is an instance of MarkerHandle and equals the value of this instance;
            /// otherwise, false.
            /// </returns>
            public override bool Equals(object obj) {
                return obj is IntPtr && Value == ((MarkerHandle) obj).Value;
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>A 32-bit signed integer hash code.</returns>
            public override int GetHashCode() {
                return Value.GetHashCode();
            }

            /// <summary>
            /// Determines whether two specified instances of MarkerHandle are equal.
            /// </summary>
            /// <param name="a">The first handle to compare.</param>
            /// <param name="b">The second handle to compare.</param>
            /// <returns>true if <paramref name="a" /> equals <paramref name="b" />; otherwise, false.</returns>
            public static bool operator ==(MarkerHandle a, MarkerHandle b) {
                return a.Value == b.Value;
            }

            /// <summary>
            /// Determines whether two specified instances of MarkerHandle are not equal.
            /// </summary>
            /// <param name="a">The first handle to compare.</param>
            /// <param name="b">The second handle to compare.</param>
            /// <returns>true if <paramref name="a" /> does not equal <paramref name="b" />; otherwise, false.</returns>
            public static bool operator !=(MarkerHandle a, MarkerHandle b) {
                return a.Value != b.Value;
            }
        }
    }
}
