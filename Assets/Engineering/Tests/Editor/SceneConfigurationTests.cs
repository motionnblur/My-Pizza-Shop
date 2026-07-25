using Engineering.Scripts.Mono.Items;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using Engineering.ScriptableObjects;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Engineering.Tests
{
    public class BuildSceneConfigurationTests
    {
        [Test]
        public void OnlyEnabledBuildScene_IsMainScene()
        {
            var scenes = EditorBuildSettings.scenes;

            Assert.That(scenes, Is.Not.Null.And.Not.Empty, "Expected at least one build scene.");

            SceneAsset mainScene = null;
            foreach (var scene in scenes)
            {
                if (scene.enabled)
                {
                    Assert.That(mainScene, Is.Null, "Expected exactly one enabled build scene.");
                    mainScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                }
            }

            Assert.That(mainScene, Is.Not.Null, "Expected an enabled build scene.");
            Assert.That(AssetDatabase.GetAssetPath(mainScene),
                Is.EqualTo("Assets/Scenes/MainScene.unity"));
        }
    }

    public class SoundManagerConfigurationTests
    {
        private Scene _mainScene;
        private Scene _previousActiveScene;

        [SetUp]
        public void SetUp()
        {
            _previousActiveScene = SceneManager.GetActiveScene();
            _mainScene = EditorSceneManager.OpenScene(
                "Assets/Scenes/MainScene.unity",
                OpenSceneMode.Additive);
        }

        [TearDown]
        public void TearDown()
        {
            if (_mainScene.isLoaded)
            {
                EditorSceneManager.CloseScene(_mainScene, true);
            }

            if (_previousActiveScene.IsValid())
            {
                SceneManager.SetActiveScene(_previousActiveScene);
            }
        }

        [Test]
        public void SoundManager_HasPizzaServedEventAssigned()
        {
            var soundManager = Object.FindFirstObjectByType<SoundManager>();
            Assert.That(soundManager, Is.Not.Null,
                "Expected a SoundManager in MainScene.");

            var serializedObject = new SerializedObject(soundManager);
            var pizzaServedProp = serializedObject.FindProperty("pizzaServedEvent");

            Assert.That(pizzaServedProp, Is.Not.Null,
                "Expected 'pizzaServedEvent' serialized property on SoundManager.");
            Assert.That(pizzaServedProp.objectReferenceValue, Is.Not.Null,
                "Expected pizzaServedEvent to be assigned.");
        }

        [Test]
        public void SoundManager_PizzaServedEvent_ReferencesCorrectAsset()
        {
            var pizzaServedAsset = AssetDatabase.LoadAssetAtPath<SVoidEventChannel>(
                "Assets/Engineering/ScriptableObjects/PizzaServed.asset");

            Assert.That(pizzaServedAsset, Is.Not.Null,
                "Expected PizzaServed.asset to exist.");

            var soundManager = Object.FindFirstObjectByType<SoundManager>();
            Assert.That(soundManager, Is.Not.Null,
                "Expected a SoundManager in MainScene.");

            var serializedObject = new SerializedObject(soundManager);
            var pizzaServedProp = serializedObject.FindProperty("pizzaServedEvent");

            Assert.That(pizzaServedProp.objectReferenceValue,
                Is.SameAs(pizzaServedAsset),
                "SoundManager's pizzaServedEvent must reference the same PizzaServed.asset " +
                "that ServingStation.prefab uses.");
        }
    }

    public class MainSceneEconomyIntegrationTests
    {
        private Scene _mainScene;
        private Scene _previousActiveScene;

        [SetUp]
        public void SetUp()
        {
            _previousActiveScene = SceneManager.GetActiveScene();
            _mainScene = EditorSceneManager.OpenScene(
                "Assets/Scenes/MainScene.unity",
                OpenSceneMode.Additive);
        }

        [TearDown]
        public void TearDown()
        {
            if (_mainScene.isLoaded)
            {
                EditorSceneManager.CloseScene(_mainScene, true);
            }

            if (_previousActiveScene.IsValid())
            {
                SceneManager.SetActiveScene(_previousActiveScene);
            }
        }

        [Test]
        public void MainScene_ProductionEconomyWiring_UsesSingleCurrencyServiceAndSharedMoneyAnimationChannel()
        {
            var currencyService = GetSingleSceneComponent<CurrencyService>(out var currencyServiceCount);
            var playerWallet = GetSingleSceneComponent<PlayerWallet>(out var playerWalletCount);
            var uiManager = GetSingleSceneComponent<UIManager>(out var uiManagerCount);
            var animationManager = GetSingleSceneComponent<AnimationManager>(out var animationManagerCount);
            var economyManager = GetSingleSceneComponent<EconomyManager>(out var economyManagerCount);

            Assert.That(currencyServiceCount, Is.EqualTo(1),
                "Expected exactly one CurrencyService in MainScene.");
            Assert.That(playerWalletCount, Is.EqualTo(1),
                "Expected exactly one PlayerWallet in MainScene.");
            Assert.That(uiManagerCount, Is.EqualTo(1),
                "Expected exactly one UIManager in MainScene.");
            Assert.That(animationManagerCount, Is.EqualTo(1),
                "Expected exactly one AnimationManager in MainScene.");
            Assert.That(economyManagerCount, Is.EqualTo(1),
                "Expected exactly one EconomyManager in MainScene.");

            Assert.That(currencyService, Is.Not.Null,
                "Expected a CurrencyService in MainScene.");
            Assert.That(playerWallet, Is.Not.Null,
                "Expected a PlayerWallet in MainScene.");
            Assert.That(uiManager, Is.Not.Null,
                "Expected a UIManager in MainScene.");
            Assert.That(animationManager, Is.Not.Null,
                "Expected an AnimationManager in MainScene.");
            Assert.That(economyManager, Is.Not.Null,
                "Expected an EconomyManager in MainScene.");

            var moneyAnimationRequestedAsset = AssetDatabase.LoadAssetAtPath<SMoneyAnimationEventChannel>(
                "Assets/Engineering/ScriptableObjects/Events/MoneyAnimationRequested.asset");

            Assert.That(moneyAnimationRequestedAsset, Is.Not.Null,
                "Expected MoneyAnimationRequested.asset to exist.");

            Assert.That(GetObjectReference(currencyService, "_playerWallet"),
                Is.SameAs(playerWallet),
                "CurrencyService must reference the scene PlayerWallet.");
            Assert.That(GetObjectReference(uiManager, "_playerWallet"),
                Is.SameAs(playerWallet),
                "UIManager must reference the scene PlayerWallet.");
            Assert.That(GetObjectReference(animationManager, "moneyAnimationRequested"),
                Is.SameAs(moneyAnimationRequestedAsset),
                "AnimationManager must use MoneyAnimationRequested.asset.");
            Assert.That(GetObjectReference(economyManager, "moneyAnimationRequested"),
                Is.SameAs(moneyAnimationRequestedAsset),
                "EconomyManager must use MoneyAnimationRequested.asset.");
        }

        private T GetSingleSceneComponent<T>(out int count)
            where T : Component
        {
            count = 0;
            T found = null;

            foreach (var rootGameObject in _mainScene.GetRootGameObjects())
            {
                var components = rootGameObject.GetComponentsInChildren<T>(true);
                count += components.Length;

                if (found == null && components.Length > 0)
                    found = components[0];
            }

            return found;
        }

        private static Object GetObjectReference(Object target, string propertyPath)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyPath);

            Assert.That(property, Is.Not.Null,
                $"Expected serialized property '{propertyPath}' on {target.name}.");

            return property.objectReferenceValue;
        }
    }

}
