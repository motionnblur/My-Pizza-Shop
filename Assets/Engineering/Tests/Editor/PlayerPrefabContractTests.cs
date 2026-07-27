using Engineering.Scripts.Mono.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Engineering.Tests
{
    public class PlayerPrefabContractTests
    {
        private const string PrefabPath = "Assets/Engineering/Prefabs/Player.prefab";

        [Test]
        public void PlayerPrefab_HasDirectScriptsAndMeshChildren()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null, "Player.prefab not found at path.");

            var scriptsChild = prefab.transform.Find("Scripts");
            Assert.That(scriptsChild, Is.Not.Null, "Player must have a direct 'Scripts' child.");
            Assert.That(scriptsChild.parent, Is.EqualTo(prefab.transform),
                "'Scripts' must be a direct child of Player root.");

            var meshChild = prefab.transform.Find("Mesh");
            Assert.That(meshChild, Is.Not.Null, "Player must have a direct 'Mesh' child.");
            Assert.That(meshChild.parent, Is.EqualTo(prefab.transform),
                "'Mesh' must be a direct child of Player root.");
        }

        [Test]
        public void ScriptsChild_ContainsPlayerPizzaInventory()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var scriptsChild = prefab.transform.Find("Scripts");
            Assert.That(scriptsChild, Is.Not.Null);

            var pizzaInv = scriptsChild.GetComponent<PlayerPizzaInventory>();
            Assert.That(pizzaInv, Is.Not.Null,
                "Scripts must have PlayerPizzaInventory for trigger-based pizza collection.");
        }

        [Test]
        public void ScriptsChild_ContainsPlayerWasteInventory()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var scriptsChild = prefab.transform.Find("Scripts");
            Assert.That(scriptsChild, Is.Not.Null);

            var wasteInv = scriptsChild.GetComponent<PlayerWasteInventory>();
            Assert.That(wasteInv, Is.Not.Null,
                "Scripts must have PlayerWasteInventory for trigger-based waste collection.");
        }

        [Test]
        public void MeshChild_HasCapsuleCollider()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var meshChild = prefab.transform.Find("Mesh");
            Assert.That(meshChild, Is.Not.Null);

            var collider = meshChild.GetComponent<CapsuleCollider>();
            Assert.That(collider, Is.Not.Null,
                "Mesh must have a CapsuleCollider for gameplay physics interactions.");
            Assert.That(collider.isTrigger, Is.False,
                "Mesh collider must NOT be a trigger; it is the player's physics body.");
        }

        [Test]
        public void PlayerRoot_HasPlayerTag()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab.tag, Is.EqualTo("Player"),
                "Player root must be tagged 'Player' so trigger callbacks can identify player colliders.");
        }

        [Test]
        public void PlayerInventories_AreNotOnPlayerRoot()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

            var pizzaOnRoot = prefab.GetComponent<PlayerPizzaInventory>();
            Assert.That(pizzaOnRoot, Is.Null,
                "PlayerPizzaInventory must NOT be on the Player root. It belongs under Scripts.");

            var wasteOnRoot = prefab.GetComponent<PlayerWasteInventory>();
            Assert.That(wasteOnRoot, Is.Null,
                "PlayerWasteInventory must NOT be on the Player root. It belongs under Scripts.");
        }

        [Test]
        public void PlayerTriggerRelay_ReferencesPlayerTriggerOnScripts()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var meshChild = prefab.transform.Find("Mesh");
            var scriptsChild = prefab.transform.Find("Scripts");
            Assert.That(meshChild, Is.Not.Null);
            Assert.That(scriptsChild, Is.Not.Null);

            var relay = meshChild.GetComponent<PlayerTriggerRelay>();
            Assert.That(relay, Is.Not.Null,
                "Mesh must have PlayerTriggerRelay to forward trigger events from the physics collider.");

            var playerTrigger = scriptsChild.GetComponent<PlayerTrigger>();
            Assert.That(playerTrigger, Is.Not.Null,
                "Scripts must have PlayerTrigger to receive relayed trigger events.");

            var serializedRelay = new SerializedObject(relay);
            var triggerProp = serializedRelay.FindProperty("playerTrigger");
            Assert.That(triggerProp, Is.Not.Null,
                "PlayerTriggerRelay must expose serialized 'playerTrigger' field.");
            Assert.That(triggerProp.objectReferenceValue, Is.EqualTo(playerTrigger),
                "PlayerTriggerRelay must reference PlayerTrigger on Scripts.");
        }

        [Test]
        public void PrefabPath_RemainsStable()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null,
                $"Player prefab must exist at '{PrefabPath}'. " +
                "If moved, update all consumers that depend on this hierarchy structure.");
        }
    }
}
