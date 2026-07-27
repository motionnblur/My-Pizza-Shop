using Engineering.Scripts.Mono.Actors.CustomerQueue;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Engineering.Tests
{
    public class CustomerBotPrefabContractTests
    {
        private const string PrefabPath = "Assets/Engineering/Prefabs/CustomerBot.prefab";

        [Test]
        public void CustomerBotPrefab_CanBeLoaded()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null,
                $"CustomerBot prefab must exist at '{PrefabPath}'.");
        }

        [Test]
        public void CustomerBot_HasAssignedOrderText()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);

            var bot = prefab.GetComponent<CustomerBot>();
            Assert.That(bot, Is.Not.Null,
                "CustomerBot component must exist on the prefab root.");

            var serializedBot = new SerializedObject(bot);
            var orderTextProp = serializedBot.FindProperty("orderText");
            Assert.That(orderTextProp, Is.Not.Null,
                "CustomerBot must have a serialized 'orderText' field.");
            Assert.That(orderTextProp.objectReferenceValue, Is.Not.Null,
                "CustomerBot.prefab 'orderText' must be assigned in the Inspector.");
        }

        [Test]
        public void CustomerBot_OrderTextFormatIsExactString()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);

            var bot = prefab.GetComponent<CustomerBot>();
            Assert.That(bot, Is.Not.Null);

            var serializedBot = new SerializedObject(bot);
            var formatProp = serializedBot.FindProperty("orderTextFormat");
            Assert.That(formatProp, Is.Not.Null,
                "CustomerBot must have a serialized 'orderTextFormat' field.");
            Assert.That(formatProp.stringValue, Is.EqualTo("{0}"),
                "CustomerBot.prefab 'orderTextFormat' must be exactly '{0}' for clean number display.");
        }

        [Test]
        public void CustomerBot_OrderTextRaycastTargetIsDisabled()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);

            var bot = prefab.GetComponent<CustomerBot>();
            Assert.That(bot, Is.Not.Null);

            var serializedBot = new SerializedObject(bot);
            var orderTextProp = serializedBot.FindProperty("orderText");
            Assert.That(orderTextProp, Is.Not.Null);
            Assert.That(orderTextProp.objectReferenceValue, Is.Not.Null,
                "Cannot verify raycast target when 'orderText' is unassigned.");

            var serializedText = new SerializedObject(orderTextProp.objectReferenceValue);
            var raycastProp = serializedText.FindProperty("m_RaycastTarget");
            Assert.That(raycastProp, Is.Not.Null,
                "TMP_Text component must expose 'm_RaycastTarget' property.");
            Assert.That(raycastProp.boolValue, Is.False,
                "CustomerBot order text must have Raycast Target disabled to avoid blocking scene interaction.");
        }

        [Test]
        public void CustomerBot_OrderUICanvasHasNoGraphicRaycaster()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);

            var bot = prefab.GetComponent<CustomerBot>();
            Assert.That(bot, Is.Not.Null);

            var serializedBot = new SerializedObject(bot);
            var orderTextProp = serializedBot.FindProperty("orderText");
            Assert.That(orderTextProp, Is.Not.Null);
            Assert.That(orderTextProp.objectReferenceValue, Is.Not.Null,
                "Cannot verify canvas when 'orderText' is unassigned.");

            var textComponent = orderTextProp.objectReferenceValue as Component;
            Assert.That(textComponent, Is.Not.Null,
                "'orderText' must reference a Component (TMP_Text).");

            var canvas = textComponent.transform.parent.GetComponent<Canvas>();
            Assert.That(canvas, Is.Not.Null,
                "Order text must be a child of a Canvas GameObject.");

            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            Assert.That(raycaster, Is.Null,
                "The world-space Canvas containing CustomerBot order text must not have a GraphicRaycaster " +
                "to prevent unnecessary raycast processing.");
        }
    }
}
