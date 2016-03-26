using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace thelab.core
{

    /// <summary>
    /// Enumeration to define the type format used in Math
    /// </summary>
    public enum MathType
    {
        None = 0,
        Float,
        Int,
        Vector2,
        Vector3,
        Vector4,
        Color,
        Quaternion,
        Transform,
        Rect,
    }

}