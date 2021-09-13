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
            return mock.Callback(action);
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
            return mock.Callback(action);
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
            return mock.Callback(action);
        }
    }
}
