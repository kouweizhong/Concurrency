﻿using System;
using System.Collections.Generic;

namespace MartinSu.Concurrency.Core
{
    internal static class Lists
    {
        public static void Swap(ref List<Action> a, ref List<Action> b)
        {
            List<Action> tmp = a;
            a = b;
            b = tmp;
        }
    }
}
