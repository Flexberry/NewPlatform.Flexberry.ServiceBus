namespace NewPlatform.Flexberry.ServiceBus.IntegratedTests.Components
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    internal class ObjectRepositoryTestHelper
    {
        internal static void CheckValue<T>(IEnumerable<T> actualList, string propertyToCheck, object valueToCheck, int expectedCount)
        {
            var isActualListContainsTWithCondition = actualList.Count(t => CheckProperty(t, propertyToCheck, valueToCheck)) == expectedCount;
            Assert.True(isActualListContainsTWithCondition);
        }

        internal static void CheckPropertiesOfAllObjects<T>(IEnumerable<T> actualList, IEnumerable objsToCheck, string[] propertiesToCheck)
        {
            foreach (var objToCheck in objsToCheck)
            {
                if (objToCheck is T)
                {
                    foreach (var propertyToCheck in propertiesToCheck)
                    {
                        var isActualListContainsT = actualList.Count(t => CheckProperty(t, (T)objToCheck, propertyToCheck)) == 1;
                        Assert.True(isActualListContainsT);
                    }
                }
            }
        }

        internal static bool CheckProperty<T>(T t, string propertyName, object valueToCheck)
        {
            var value = GetValueOfProperty(t, propertyName);
            return value.Equals(valueToCheck);
        }

        internal static bool CheckProperty<T>(T t1, T t2, string propertyName)
        {
            var value1 = GetValueOfProperty(t1, propertyName);
            var value2 = GetValueOfProperty(t2, propertyName);

            return value1.Equals(value2);
        }

        internal static object GetValueOfProperty<T>(T originObject, string propertyName)
        {
            var type = originObject.GetType();
            if (propertyName.Contains("."))
            {
                var propertiesNames = propertyName.Split('.');
                object propertyValue = originObject;
                foreach (var anotherProperty in propertiesNames)
                {
                    var propertyInfo = type.GetProperty(anotherProperty);
                    propertyValue = propertyInfo.GetValue(propertyValue);
                    type = propertyInfo.PropertyType;
                }

                return propertyValue;
            }

            return type.GetProperty(propertyName).GetValue(originObject);
        }

        internal static int GetObjectsOfSpecifiedTypeCount<T>(IEnumerable objects)
        {
            var count = 0;
            foreach (var obj in objects)
            {
                if (obj is T)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
