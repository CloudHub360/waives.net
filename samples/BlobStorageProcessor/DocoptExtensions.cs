using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
namespace DocoptNet
{
    public static class DocoptExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this ValueObject option, Func<object, T> itemProjection)
        {
            return option.AsList.Cast<object>().Select(itemProjection).ToList();
        }
    }
}
// ReSharper restore IdentifierTypo