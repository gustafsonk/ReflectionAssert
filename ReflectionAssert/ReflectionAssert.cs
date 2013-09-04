using System;
using System.Collections;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AssertExtensions
{
    public static class ReflectionAssert
    {
        public static void AreEqual(object expected, object actual)
        {
            AssertEqual(expected, actual);
        }

        private static void AssertEqual(object expected, object actual, Property property = null)
        {
            // Check for nulls, stopping if needed.
            if (NullAssertEqual(expected, actual, property))
            {
                return;
            }

            // Check the object types.
            Type type = TypeAssertEqual(expected, actual);

            if (IsValueType(type)) // Value types should ignore the properties.
            {
                // Make a more explicit assert message if possible.
                if (property == null)
                {
                    property = new Property(null, type.Name);
                    ValueTypeAssertEqual(expected, actual, property);
                }
                else
                {
                    ValueTypeAssertEqual(expected, actual, property);
                }
            }
            else if (expected is IList) // Lists need to be iterated through.
            {
                ListAssertEqual((IList)expected, (IList)actual, property);
            }
            else // Otherwise, Check the object properties.
            {
                PropertiesAssertEqual(expected, actual, type);
            }
        }

        private static bool NullAssertEqual(object expected, object actual, Property property)
        {
            // Pass if both objects are null.
            if (expected == null && actual == null)
            {
                return true;
            }

            // Fail if only one object is null.
            if (expected == null || actual == null)
            {
                const string expectedNullMessage = "Expected: null, Actual: non-null.";
                const string actualNullMessage = "Expected: non-null, Actual: null.";
                string message = expected == null ? expectedNullMessage : actualNullMessage;

                // Make a more explicit assert message if possible.
                if (property == null)
                {
                    Assert.Fail("Objects are not the same nullness. " + message);
                }
                else
                {
                    Assert.Fail("{0}.{1} is not the same nullness. " + message, property.ParentName, property.Name);
                }
            }

            return false;
        }

        private static Type TypeAssertEqual(object expected, object actual)
        {
            // Fail if the objects are not the same type.
            Type expectedType = expected.GetType();
            Type actualType = actual.GetType();

            if (expectedType != actualType)
            {
                Assert.Fail("Objects are not the same type. Expected: {0}, Actual: {1}", expectedType, actualType);
            }

            return expectedType;
        }

        private static void ListAssertEqual(IList expecteds, IList actuals, Property property = null)
        {
            // Fail if the lists are not the same size.
            int expectedSize = expecteds.Count;
            int actualSize = actuals.Count;
            if (expectedSize != actualSize)
            {
                // Make a more explicit assert message if possible.
                if (property == null)
                {
                    Assert.Fail("ILists are not the same size. Expected size: {0}, Actual size: {1}", expectedSize, actualSize);
                }
                else
                {
                    Assert.Fail("{0}.{1} is not the same size. Expected size: {2}, Actual size: {3}", property.ParentName, property.Name, expectedSize, actualSize);
                }
            }

            // Check each object in the list.
            for (int i = 0; i < expectedSize; i++)
            {
                object expected = expecteds[i];
                object actual = actuals[i];
                AssertEqual(expected, actual, property);
            }
        }

        private static void PropertiesAssertEqual(object expected, object actual, Type type)
        {
            // Check each property's value.
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                object expectedValue = propertyInfo.GetValue(expected, null);
                object actualValue = propertyInfo.GetValue(actual, null);

                Property property = new Property(propertyInfo.DeclaringType.Name, propertyInfo.Name);

                PropertyAssertEqual(expectedValue, actualValue, property);
            }
        }

        private static void PropertyAssertEqual(object expected, object actual, Property property)
        {
            // Recursion.
            AssertEqual(expected, actual, property);
        }

        private static bool IsValueType(Type type)
        {
            // If the properties are comparable value types, or if it's the special case of a string type...
            return type.IsValueType || type == "".GetType();
        }

        private static void ValueTypeAssertEqual(object expected, object actual, Property property)
        {
            // Call the property type's Equals method.
            if (!Equals(expected, actual))
            {
                // Make a more explicit assert message if possible.
                if (string.IsNullOrEmpty(property.ParentName))
                {
                    Assert.Fail("{0} is not the same value. Expected: {1}, Actual: {2}", property.Name, expected, actual);
                }
                else
                {
                    Assert.Fail("{0}.{1} is not the same value. Expected: {2}, Actual: {3}", property.ParentName, property.Name, expected, actual);
                }
            }
        }
    }
}