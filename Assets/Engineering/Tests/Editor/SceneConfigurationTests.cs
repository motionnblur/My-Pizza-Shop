using Engineering.Scripts.Mono.Items;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using Engineering.Scripts.Mono.Actors.ServeStation;
using Engineering.Scripts.Mono.Areas;
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

        [Test]
        public void EconomyManager_ReferencesSceneCurrencyService()
        {
            var economyManager = GetSingleSceneComponent<EconomyManager>(out _);
            var currencyService = GetSingleSceneComponent<CurrencyService>(out _);

            Assert.That(economyManager, Is.Not.Null);
            Assert.That(currencyService, Is.Not.Null);

            var referenced = GetObjectReference(economyManager, "currencyService");
            Assert.That(referenced, Is.Not.Null,
                "EconomyManager must have a CurrencyService reference assigned.");
            Assert.That(referenced, Is.SameAs(currencyService),
                "EconomyManager must reference the scene CurrencyService.");
        }

        [Test]
        public void AllBuyingAreas_ReferenceSceneEconomyManager()
        {
            var economyManager = GetSingleSceneComponent<EconomyManager>(out _);
            Assert.That(economyManager, Is.Not.Null,
                "Expected an EconomyManager in MainScene.");

            var allBuyingAreas = GetAllSceneComponents<BuyingArea>();
            foreach (var area in allBuyingAreas)
            {
                var referenced = GetObjectReference(area, "economyManager");
                Assert.That(referenced, Is.Not.Null,
                    $"BuyingArea '{area.name}' must have an EconomyManager reference assigned.");
                Assert.That(referenced, Is.SameAs(economyManager),
                    $"BuyingArea '{area.name}' must reference the scene EconomyManager.");
            }
        }

        [Test]
        public void AllServeStations_ReferenceSceneCurrencyService()
        {
            var currencyService = GetSingleSceneComponent<CurrencyService>(out _);
            Assert.That(currencyService, Is.Not.Null,
                "Expected a CurrencyService in MainScene.");

            var allServeStations = GetAllSceneComponents<ServeStation>();
            foreach (var station in allServeStations)
            {
                var referenced = GetObjectReference(station, "currencyService");
                Assert.That(referenced, Is.Not.Null,
                    $"ServeStation '{station.name}' must have a CurrencyService reference assigned.");
                Assert.That(referenced, Is.SameAs(currencyService),
                    $"ServeStation '{station.name}' must reference the scene CurrencyService.");
            }
        }

        [Test]
        public void AllMoneyToCollect_ReferenceSceneCurrencyService()
        {
            var currencyService = GetSingleSceneComponent<CurrencyService>(out _);
            Assert.That(currencyService, Is.Not.Null,
                "Expected a CurrencyService in MainScene.");

            var allMoneyToCollect = GetAllSceneComponents<MoneyToCollect>();
            foreach (var pickup in allMoneyToCollect)
            {
                var referenced = GetObjectReference(pickup, "currencyService");
                Assert.That(referenced, Is.Not.Null,
                    $"MoneyToCollect '{pickup.name}' must have a CurrencyService reference assigned.");
                Assert.That(referenced, Is.SameAs(currencyService),
                    $"MoneyToCollect '{pickup.name}' must reference the scene CurrencyService.");
            }
        }

        [Test]
        public void AllRequiredEventChannels_AreAssigned()
        {
            var economyManager = GetSingleSceneComponent<EconomyManager>(out _);
            Assert.That(economyManager, Is.Not.Null);
            Assert.That(GetObjectReference(economyManager, "buyingAreaPurchasedEvent"), Is.Not.Null,
                "EconomyManager must have buyingAreaPurchasedEvent assigned.");
            Assert.That(GetObjectReference(economyManager, "moneyAnimationRequested"), Is.Not.Null,
                "EconomyManager must have moneyAnimationRequested assigned.");

            var animationManager = GetSingleSceneComponent<AnimationManager>(out _);
            Assert.That(animationManager, Is.Not.Null);
            Assert.That(GetObjectReference(animationManager, "moneyAnimationRequested"), Is.Not.Null,
                "AnimationManager must have moneyAnimationRequested assigned.");

            var soundManager = Object.FindFirstObjectByType<SoundManager>();
            Assert.That(soundManager, Is.Not.Null);
            Assert.That(GetObjectReference(soundManager, "groundMoneyCollectedEvent"), Is.Not.Null,
                "SoundManager must have groundMoneyCollectedEvent assigned.");
            Assert.That(GetObjectReference(soundManager, "buyingAreaPurchasedEvent"), Is.Not.Null,
                "SoundManager must have buyingAreaPurchasedEvent assigned.");
            Assert.That(GetObjectReference(soundManager, "pizzaServedEvent"), Is.Not.Null,
                "SoundManager must have pizzaServedEvent assigned.");
            Assert.That(GetObjectReference(soundManager, "pizzaTrashedEvent"), Is.Not.Null,
                "SoundManager must have pizzaTrashedEvent assigned.");
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

        private T[] GetAllSceneComponents<T>()
            where T : Component
        {
            var results = new System.Collections.Generic.List<T>();
            foreach (var rootGameObject in _mainScene.GetRootGameObjects())
            {
                results.AddRange(rootGameObject.GetComponentsInChildren<T>(true));
            }
            return results.ToArray();
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
