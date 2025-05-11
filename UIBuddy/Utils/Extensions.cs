using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UIBuddy.Utils
{
    internal static class Extensions
    {
        public static Color GetWithOpacity(this Color color, float opacity)
        {
            return new Color(color.r, color.g, color.b, opacity);
        }
    }
}
