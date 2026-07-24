using Engineering.Scripts.Mono.Managers;
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
}
