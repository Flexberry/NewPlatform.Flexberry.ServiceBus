namespace NewPlatform.Flexberry.ServiceBus.Tests.Utils
{
    using NewPlatform.Flexberry.ServiceBus.Utils;

    using Xunit;

    /// <summary>
    /// Tests for <see cref="KeyLocker{T1, T2}"/>.
    /// </summary>
    public class KeyLockerTest
    {
        /// <summary>
        /// Tests for <see cref="KeyLocker{T1, T2}.GetLock(T1)"/> and <see cref="KeyLocker{T1, T2}.FreeLock(T1)"/>.
        /// </summary>
        [Fact]
        public void TestKeyLocker()
        {
            // Arrange.
            var locker = new KeyLocker<int, object>();

            // Act.
            var lock_1_1 = locker.GetLock(1);
            var lock_1_2 = locker.GetLock(1);
            var lock_2_1 = locker.GetLock(2);

            locker.FreeLock(1);
            locker.FreeLock(1);

            // Assert.
            Assert.True(lock_1_1 == lock_1_2);
            Assert.True(lock_1_1 != lock_2_1);
            Assert.True(lock_1_1 != locker.GetLock(1));
        }
    }
}
