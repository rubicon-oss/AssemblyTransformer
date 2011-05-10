using System;

namespace GenericAttributeGenerator
{
  /// <summary>
  /// This attribute is used to mark a custom "attribute", which is a generic class, to be transformed to a
  /// "real" attribute. This means, extend System.Attribute.
  /// </summary>
  public class GenericAttributeMarkerAttribute : Attribute
  {
    public GenericAttributeMarkerAttribute ()
    {
    }
  }

  public class GenericAttribute
  {
  }

  public class MyAttribute<T> : GenericAttribute
  {
  }
}