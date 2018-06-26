using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowX
{
    /// <summary>
    /// The available WindowX styles.
    /// </summary>
    public enum WindowXType
    {
        /// <summary>
        /// The default WindowX style.
        /// </summary>
        Window = 0,

        /// <summary>
        /// No resizing functionality.
        /// </summary>
        NoResize = 1,

        /// <summary>
        /// Modal window. Focused until closing and no minimize.
        /// </summary>
        Modal = 2,
    }
}
