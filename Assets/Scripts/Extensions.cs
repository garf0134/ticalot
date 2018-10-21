using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Extensions
{
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
