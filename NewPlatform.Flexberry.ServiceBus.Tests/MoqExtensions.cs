namespace NewPlatform.Flexberry.ServiceBus.Tests
{
    using System;
    using System.Collections;
    using System.Reflection;
    using Moq;
    using Moq.Language;
    using Moq.Language.Flow;

    /// <summary>
    /// Методы расширения для Moq для поддержки ref-параметров (грязные хаки).
    /// Примеры взяты отсюда:
    /// http://stackoverflow.com/questions/1068095/assigning-out-ref-parameters-in-moq
    /// и отсюда: https://github.com/moq/moq4/issues/105.
    /// </summary>
    public static class MoqExtensions
    {
        /// <summary>
        /// Делегат для <see cref="Action{T}"/> с ref-параметром.
        /// </summary>
        /// <typeparam name="TRef">Тип параметров.</typeparam>
        /// <param name="refVal">Параметр, передаваемый по ссылке.</param>
        public delegate void RefAction<TRef>(ref TRef refVal);

        /// <summary>
        /// Делегат для <see cref="Action{T, T}"/> с двумя параметрами, первый из которых передается по ссылке.
        /// </summary>
        /// <typeparam name="TRef">Тип параметров передаваемых по ссылке.</typeparam>
        /// <typeparam name="TParam">Тип обычных парамветров.</typeparam>
        /// <param name="refVal">Параметр, передаваемый по ссылке.</param>
        /// <param name="param">Обычный параметр.</param>
        public delegate void RefValAction<TRef, TParam>(ref TRef refVal, TParam param);

        /// <summary>
        /// Метод расширения для <see cref="Moq"/>, который позволяет подписываться на Callback методов, параметром которых является ref-объект (вариант с возвращаемым значением).
        /// </summary>
        /// <param name="mock">
        /// <see cref="Moq"/>, для которого нужно вызвать Callback.
        /// </param>
        /// <param name="action">
        /// <see cref="Action{T}"/> с ref-параметром.
        /// </param>
        /// <typeparam name="TMock">
        /// Moq для какого типа.
        /// </typeparam>
        /// <typeparam name="TReturn">
        /// Тип возвращаемого значения.
        /// </typeparam>
        /// <typeparam name="TRef">
        /// Тип ref-параметра.
        /// </typeparam>
        /// <returns>
        /// Возвращаем структуру для поддержки цепочечного вызова функции.
        /// </returns>
        public static IReturnsThrows<TMock, TReturn> RefCallback<TMock, TReturn, TRef>(this ICallback<TMock, TReturn> mock, RefAction<TRef> action)
            where TMock : class
        {
            return RefCallbackInternal(mock, action);
        }

        /// <summary>
        /// Метод расширения для <see cref="Moq"/>, который отключает проверку параметров для Callback-функций (вариант с возвращаемым значением).
        /// </summary>
        /// <param name="mock">
        /// <see cref="Moq"/>, для которого нужно отключить проверку параметров.
        /// </param>
        /// <returns>
        /// Для поддержки цепочечных вызовов, возвращаем <paramref name="mock"/>.
        /// </returns>
        public static IThrowsResult IgnoreRefMatching(this IThrowsResult mock)
        {
            try
            {
                FieldInfo matcherField = typeof(Mock).GetTypeInfo().Assembly.GetType("Moq.MethodCall").GetField("argumentMatchers", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);

                IList argumentMatchers = (IList)matcherField.GetValue(mock);
                Type refMatcherType = typeof(Mock).GetTypeInfo().Assembly.GetType("Moq.Matchers.RefMatcher");
                FieldInfo equalField = refMatcherType.GetField("equals", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);

                foreach (object matcher in argumentMatchers)
                {
                    if (matcher.GetType() == refMatcherType)
                        equalField.SetValue(matcher, new Func<object, bool>(o => true));
                }

                return mock;
            }
            catch (NullReferenceException)
            {
                return mock;
            }
        }

        /// <summary>
        ///  Метод расширения для <see cref="Moq"/>, который позволяет подписываться на Callback методов, параметром которых является ref-объект (вариант без возвращаемого значения).
        /// </summary>
        /// <param name="mock">
        /// <see cref="Moq"/>, для которого нужно вызвать Callback.
        /// </param>
        /// <param name="action">
        /// <see cref="Action{T}"/> с ref-параметром.
        /// </param>
        /// <typeparam name="TRef">
        /// Тип ref-параметра.
        /// </typeparam>
        /// <returns>
        /// Результат выполнения Callback.
        /// </returns>
        public static ICallbackResult RefCallback<TRef>(this ICallback mock, RefAction<TRef> action)
        {
            return RefCallbackInternal(mock, action);
        }

        /// <summary>
        ///  Метод расширения для <see cref="Moq"/>, который позволяет подписываться на Callback методов с двумя параметрами, первый из которых ref-объект (вариант без возвращаемого значения).
        /// </summary>
        /// <param name="mock"><see cref="Moq"/>, для которого нужно вызвать Callback.</param>
        /// <param name="action"><see cref="Action{T, T}"/> с ref-параметром.</param>
        /// <typeparam name="TRef">Тип параметров передаваемых по ссылке.</typeparam>
        /// <typeparam name="TParam">Тип обычных параметров.</typeparam>
        /// <returns>Результат выполнения Callback.</returns>
        public static ICallbackResult RefValCallback<TRef, TParam>(this ICallback mock, RefValAction<TRef, TParam> action)
        {
            return RefCallbackInternal(mock, action);
        }

        /// <summary>
        /// Метод расширения для <see cref="Moq"/>, который отключает проверку параметров для Callback-функций (вариант без возвращаемого значения).
        /// </summary>
        /// <param name="mock">
        /// <see cref="Moq"/>, для которого нужно отключить проверку параметров.
        /// </param>
        /// <returns>
        /// Для поддержки цепочечных вызовов, возвращаем <paramref name="mock"/>.
        /// </returns>
        public static ICallback IgnoreRefMatching(this ICallback mock)
        {
            try
            {
                FieldInfo matcherField = typeof(Mock).GetTypeInfo().Assembly.GetType("Moq.MethodCall").GetField("argumentMatchers", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);

                IList argumentMatchers = (IList)matcherField.GetValue(mock);
                Type refMatcherType = typeof(Mock).GetTypeInfo().Assembly.GetType("Moq.Matchers.RefMatcher");
                FieldInfo equalField = refMatcherType.GetField("equals", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance);

                foreach (object matcher in argumentMatchers)
                {
                    if (matcher.GetType() == refMatcherType)
                        equalField.SetValue(matcher, new Func<object, bool>(o => true));
                }

                return mock;
            }
            catch (NullReferenceException)
            {
                return mock;
            }
        }

        /// <summary>
        /// Вызов внутренней "магии" Moq для подписки на нужный Callback (вариант с возвращаемым значением).
        /// </summary>
        /// <param name="mock">
        /// <see cref="Moq"/>, для которого нужно вызвать "магию".
        /// </param>
        /// <param name="action">
        /// <see cref="Action{T}"/> с ref-параметром.
        /// </param>
        /// <typeparam name="TMock">
        /// Moq для какого типа.
        /// </typeparam>
        /// <typeparam name="TReturn">
        /// Тип возвращаемого значения.
        /// </typeparam>
        /// <returns>
        /// Возвращаем структуру для поддержки цепочечного вызова функции.
        /// </returns>
        private static IReturnsThrows<TMock, TReturn> RefCallbackInternal<TMock, TReturn>(ICallback<TMock, TReturn> mock, object action)
            where TMock : class
        {
            mock.GetType().Assembly.GetType("Moq.MethodCall").InvokeMember("SetCallbackWithArguments", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, mock, new[] { action });
            return mock as IReturnsThrows<TMock, TReturn>;
        }

        /// <summary>
        /// Вызов внутренней "магии" Moq для подписки на нужный Callback (вариант без возвращаемого значения).
        /// </summary>
        /// <param name="mock">
        /// <see cref="Moq"/>, для которого нужно вызвать "магию".
        /// </param>
        /// <param name="action">
        /// <see cref="Action{T}"/> с ref-параметром.
        /// </param>
        /// <returns>
        /// Результат выполнения Callback.
        /// </returns>
        private static ICallbackResult RefCallbackInternal(ICallback mock, object action)
        {
            mock.GetType().Assembly.GetType("Moq.MethodCall")
                .InvokeMember("SetCallbackWithArguments", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, mock, new[] { action });
            return (ICallbackResult)mock;
        }
    }
}
