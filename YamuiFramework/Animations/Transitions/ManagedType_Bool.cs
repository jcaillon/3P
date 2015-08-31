using System;

namespace YamuiFramework.Animations.Transitions
{
    internal class ManagedType_Bool : IManagedType
    {
        #region IManagedType Members

        /// <summary>
        /// Returns the type we're managing.
        /// </summary>
        public Type getManagedType()
        {
            return typeof(bool);
        }

        /// <summary>
        /// Returns a copy of the float passed in.
        /// </summary>
        public object copy(object o)
        {
            bool value = (bool)o;
            return value;
        }

        /// <summary>
        /// Returns the interpolated value for the percentage passed in.
        /// </summary>
        public object getIntermediateValue(object start, object end, double dPercentage)
        {
            return (dPercentage > 0.5);
        }

        #endregion
    }
}
