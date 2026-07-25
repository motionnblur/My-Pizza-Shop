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

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Expected {target.GetType().Name} to define '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
