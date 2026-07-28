using Engineering.ScriptableObjects;
using NUnit.Framework;
using UnityEngine;

namespace Engineering.Tests
{
    public class SCameraSettingsEditModeTests
    {
        [Test]
        public void DefaultXDamping_IsPositive()
        {
            var settings = ScriptableObject.CreateInstance<SCameraSettings>();
            Assert.That(settings.XDamping, Is.GreaterThan(0f));
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void DefaultYDamping_IsPositive()
        {
            var settings = ScriptableObject.CreateInstance<SCameraSettings>();
            Assert.That(settings.YDamping, Is.GreaterThan(0f));
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void DefaultZDamping_IsPositive()
        {
            var settings = ScriptableObject.CreateInstance<SCameraSettings>();
            Assert.That(settings.ZDamping, Is.GreaterThan(0f));
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void DefaultMaxFollowSpeed_IsZero()
        {
            var settings = ScriptableObject.CreateInstance<SCameraSettings>();
            Assert.That(settings.MaxFollowSpeed, Is.Zero);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void XDamping_HasExpectedDefault()
        {
            var settings = ScriptableObject.CreateInstance<SCameraSettings>();
            Assert.That(settings.XDamping, Is.EqualTo(0.18f).Within(0.0001f));
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void YDamping_HasExpectedDefault()
        {
            var settings = ScriptableObject.CreateInstance<SCameraSettings>();
            Assert.That(settings.YDamping, Is.EqualTo(0.25f).Within(0.0001f));
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void ZDamping_HasExpectedDefault()
        {
            var settings = ScriptableObject.CreateInstance<SCameraSettings>();
            Assert.That(settings.ZDamping, Is.EqualTo(0.18f).Within(0.0001f));
            Object.DestroyImmediate(settings);
        }
    }
}
