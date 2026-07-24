using System.Collections;
using System.Reflection;
using Engineering.Scripts.Mono.Managers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Engineering.Tests
{
    public class UIManagerPlayModeTests
    {
        private GameObject _gameObject;
        private UIManager _uiManager;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("UIManagerTest");
            _uiManager = _gameObject.AddComponent<UIManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
            typeof(UIManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                ?.SetValue(null, null);
        }

        [UnityTest]
        public IEnumerator Awake_SetsStaticInstance()
        {
            InvokeAwake(_uiManager);
            yield return null;

            Assert.That(UIManager.Instance, Is.SameAs(_uiManager));
        }

        [UnityTest]
        public IEnumerator Awake_DestroysDuplicateInstance()
        {
            InvokeAwake(_uiManager);
            yield return null;

            var duplicateObject = new GameObject("UIManagerDuplicate");
            var duplicate = duplicateObject.AddComponent<UIManager>();
            InvokeAwake(duplicate);
            yield return null;

            Assert.That(duplicate == null, Is.True);

            Object.DestroyImmediate(duplicateObject);
        }

        [UnityTest]
        public IEnumerator UpdateMoneyText_UpdatesTextComponent()
        {
            var textObject = new GameObject("MoneyText");
            var text = textObject.AddComponent<Text>();
            SetPrivateField(_uiManager, "moneyText", text);
            yield return null;

            _uiManager.UpdateMoneyText(42);

            Assert.That(text.text, Is.EqualTo("42"));

            Object.DestroyImmediate(textObject);
        }

        [UnityTest]
        public IEnumerator UpdateMoneyText_SafeWhenMoneyTextNull()
        {
            Assert.DoesNotThrow(() => _uiManager.UpdateMoneyText(42));
            yield return null;
        }

        private static void InvokeAwake(UIManager target)
        {
            var method = typeof(UIManager).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Expected UIManager to define 'Awake'.");
            method.Invoke(target, null);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
