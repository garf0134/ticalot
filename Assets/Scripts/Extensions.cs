using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Extensions
{
  /// <summary>
  /// A convenience method to reduce the hassle of obtaining the
  /// attribute instance associated with an enum value.
  /// </summary>
  /// <typeparam name="TAttribute">An attribute type</typeparam>
  /// <param name="value">The enum value </param>
  /// <returns>The <typeparamref name="TAttribute"/>instance associated 
  /// with the <paramref name="value"/> or null if no attribute exists</returns>
  public static TAttribute GetAttribute<TAttribute>(this System.Enum value)
        where TAttribute : System.Attribute
  {
    var type = value.GetType();
    var name = System.Enum.GetName(type, value);
    return type.GetField(name) // I prefer to get attributes this way
        .GetCustomAttributes(false)
        .OfType<TAttribute>()
        .SingleOrDefault();
  }
}
