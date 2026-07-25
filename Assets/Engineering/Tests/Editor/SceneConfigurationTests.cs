using Engineering.Scripts.Mono.Bootstrap;
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

    public class MainSceneInstallerConfigurationTests
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
        public void MainScene_ContainsExactlyOneInstaller()
        {
            var installers = GetAllSceneComponents<MainSceneInstaller>();
            Assert.That(installers.Length, Is.EqualTo(1),
                "Expected exactly one MainSceneInstaller in MainScene.");
        }

        [Test]
        public void Installer_HasAllCoreServiceReferencesAssigned()
        {
            var installer = GetSingleSceneComponent<MainSceneInstaller>(out _);
            Assert.That(installer, Is.Not.Null, "Expected a MainSceneInstaller in MainScene.");

            var so = new SerializedObject(installer);

            AssertField(so, "inputManager", "InputManager");
            AssertField(so, "playerWallet", "PlayerWallet");
            AssertField(so, "currencyService", "CurrencyService");
            AssertField(so, "economyManager", "EconomyManager");
            AssertField(so, "playerMovement", "PlayerMovement");
            AssertField(so, "uiManager", "UIManager");
        }

        [Test]
        public void Installer_ConsumerArrays_HaveNoNullsOrDuplicates()
        {
            var installer = GetSingleSceneComponent<MainSceneInstaller>(out _);
            Assert.That(installer, Is.Not.Null, "Expected a MainSceneInstaller in MainScene.");

            var so = new SerializedObject(installer);

            AssertConsumerArrayValid(so, "buyingAreas", "BuyingArea");
            AssertConsumerArrayValid(so, "moneyPickups", "MoneyToCollect");
            AssertConsumerArrayValid(so, "serveStations", "ServeStation");
        }

        [Test]
        public void Installer_ListsEveryBuyingAreaInScene()
        {
            var installer = GetSingleSceneComponent<MainSceneInstaller>(out _);
            var so = new SerializedObject(installer);
            var arrayProp = so.FindProperty("buyingAreas");

            var sceneBuyingAreas = GetAllSceneComponents<BuyingArea>();
            Assert.That(arrayProp.arraySize, Is.EqualTo(sceneBuyingAreas.Length),
                "Installer buyingAreas count must match the scene BuyingArea count.");

            var installerAreas = new System.Collections.Generic.HashSet<BuyingArea>();
            for (var i = 0; i < arrayProp.arraySize; i++)
            {
                var element = arrayProp.GetArrayElementAtIndex(i).objectReferenceValue as BuyingArea;
                Assert.That(element, Is.Not.Null, $"buyingAreas[{i}] must not be null.");
                Assert.That(installerAreas.Add(element), Is.True,
                    $"buyingAreas[{i}] duplicates an earlier entry.");
            }

            foreach (var area in sceneBuyingAreas)
            {
                Assert.That(installerAreas.Contains(area), Is.True,
                    $"Scene BuyingArea '{area.name}' must be listed in the installer.");
            }
        }

        [Test]
        public void Installer_ListsEveryMoneyToCollectInScene()
        {
            var installer = GetSingleSceneComponent<MainSceneInstaller>(out _);
            var so = new SerializedObject(installer);
            var arrayProp = so.FindProperty("moneyPickups");

            var scenePickups = GetAllSceneComponents<MoneyToCollect>();
            Assert.That(arrayProp.arraySize, Is.EqualTo(scenePickups.Length),
                "Installer moneyPickups count must match the scene MoneyToCollect count.");

            var installerPickups = new System.Collections.Generic.HashSet<MoneyToCollect>();
            for (var i = 0; i < arrayProp.arraySize; i++)
            {
                var element = arrayProp.GetArrayElementAtIndex(i).objectReferenceValue as MoneyToCollect;
                Assert.That(element, Is.Not.Null, $"moneyPickups[{i}] must not be null.");
                Assert.That(installerPickups.Add(element), Is.True,
                    $"moneyPickups[{i}] duplicates an earlier entry.");
            }

            foreach (var pickup in scenePickups)
            {
                Assert.That(installerPickups.Contains(pickup), Is.True,
                    $"Scene MoneyToCollect '{pickup.name}' must be listed in the installer.");
            }
        }

        [Test]
        public void Installer_ListsEveryServeStationInScene()
        {
            var installer = GetSingleSceneComponent<MainSceneInstaller>(out _);
            var so = new SerializedObject(installer);
            var arrayProp = so.FindProperty("serveStations");

            var sceneStations = GetAllSceneComponents<ServeStation>();
            Assert.That(arrayProp.arraySize, Is.EqualTo(sceneStations.Length),
                "Installer serveStations count must match the scene ServeStation count.");

            var installerStations = new System.Collections.Generic.HashSet<ServeStation>();
            for (var i = 0; i < arrayProp.arraySize; i++)
            {
                var element = arrayProp.GetArrayElementAtIndex(i).objectReferenceValue as ServeStation;
                Assert.That(element, Is.Not.Null, $"serveStations[{i}] must not be null.");
                Assert.That(installerStations.Add(element), Is.True,
                    $"serveStations[{i}] duplicates an earlier entry.");
            }

            foreach (var station in sceneStations)
            {
                Assert.That(installerStations.Contains(station), Is.True,
                    $"Scene ServeStation '{station.name}' must be listed in the installer.");
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

        private static void AssertField(SerializedObject so, string propertyName, string displayName)
        {
            var prop = so.FindProperty(propertyName);
            Assert.That(prop, Is.Not.Null,
                $"Expected '{propertyName}' serialized property on MainSceneInstaller.");
            Assert.That(prop.objectReferenceValue, Is.Not.Null,
                $"MainSceneInstaller '{displayName}' must be assigned.");
        }

        private static void AssertConsumerArrayValid(SerializedObject so, string arrayPropertyName, string displayName)
        {
            var arrayProp = so.FindProperty(arrayPropertyName);
            Assert.That(arrayProp, Is.Not.Null,
                $"Expected '{arrayPropertyName}' serialized property on MainSceneInstaller.");

            var seen = new System.Collections.Generic.HashSet<int>();
            for (var i = 0; i < arrayProp.arraySize; i++)
            {
                var element = arrayProp.GetArrayElementAtIndex(i);
                Assert.That(element.objectReferenceValue, Is.Not.Null,
                    $"MainSceneInstaller.{arrayPropertyName}[{i}] must not be null.");
                var id = element.objectReferenceValue.GetInstanceID();
                Assert.That(seen.Add(id), Is.True,
                    $"MainSceneInstaller.{arrayPropertyName}[{i}] duplicates an earlier entry.");
            }
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
