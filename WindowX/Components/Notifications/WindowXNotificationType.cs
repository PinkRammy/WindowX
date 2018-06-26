using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowX.Components.Notifications
{
    /// <summary>
    /// The WindowX notification types.
    /// </summary>
    internal enum WindowXNotificationType
    {
        /// <summary>
        /// A notification containing general information.
        /// </summary>
        Information = 0,

        /// <summary>
        /// A notification describing the success of an action.
        /// </summary>
        Success = 1,

        /// <summary>
        /// A notification describing a warning about an action.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// A notification describing an error.
        /// </summary>
        Error = 3
    }
}
