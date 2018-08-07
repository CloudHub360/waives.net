using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace DocoptNet
{
    public static class DocoptExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this ValueObject option, Func<object, T> itemProjection)
        {
            var items = new List<T>();
            foreach (var item in option.AsList)
            {
                items.Add(itemProjection(item));
            }

            return items;
        }
    }
}