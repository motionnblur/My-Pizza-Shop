using System;
using Engineering.ScriptableObjects;
using NUnit.Framework;
using UnityEngine;

namespace Engineering.Tests
{
    public class SVoidEventChannelTests
    {
        private SVoidEventChannel _eventChannel;

        [SetUp]
        public void SetUp()
        {
            _eventChannel = ScriptableObject.CreateInstance<SVoidEventChannel>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_eventChannel);
        }

        [Test]
        public void Raise_NotifiesRegisteredListener()
        {
            var invocationCount = 0;
            _eventChannel.RegisterListener(() => invocationCount++);

            _eventChannel.Raise();

            Assert.That(invocationCount, Is.EqualTo(1));
        }

        [Test]
        public void UnregisterListener_StopsNotifications()
        {
            var invocationCount = 0;
            Action listener = () => invocationCount++;
            _eventChannel.RegisterListener(listener);
            _eventChannel.UnregisterListener(listener);

            _eventChannel.Raise();

            Assert.That(invocationCount, Is.EqualTo(0));
        }
    }
}
