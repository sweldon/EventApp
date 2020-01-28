using System;
using System.Collections.Generic;
using System.Text;

namespace EventApp
{
    public static class Utils
    {
         public static string GetCelebrationImage(bool is_celebrating)
        {
            if (is_celebrating)
            {
                return "celebrate_active.png";
            }
            else
            {
                return "celebrate.png";
            }
        }
    }
}
