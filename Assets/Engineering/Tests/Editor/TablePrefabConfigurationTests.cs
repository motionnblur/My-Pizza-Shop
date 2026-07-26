using Engineering.Scripts.Mono.Actors.Table;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Engineering.Tests
{
    public class TablePrefabConfigurationTests
    {
        private const string TablePrefabPath = "Assets/Engineering/Prefabs/Table.prefab";

        [Test]
        public void TablePrefab_HasTableWasteVisualsComponent()
        {
            var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
            Assert.That(tablePrefab, Is.Not.Null, $"Expected to find Table prefab at '{TablePrefabPath}'.");

            var wasteVisuals = tablePrefab.GetComponent<TableWasteVisuals>();
            Assert.That(wasteVisuals, Is.Not.Null, "Table prefab should have a TableWasteVisuals component.");
        }

        [Test]
        public void TableWasteVisuals_LeftoverPrefab_IsAssigned()
        {
            var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
            Assert.That(tablePrefab, Is.Not.Null);

            var wasteVisuals = tablePrefab.GetComponent<TableWasteVisuals>();
            Assert.That(wasteVisuals, Is.Not.Null);

            var prefabField = typeof(TableWasteVisuals).GetField("leftoverPrefab",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(prefabField, Is.Not.Null, "Expected TableWasteVisuals to have a 'leftoverPrefab' field.");

            var leftoverPrefab = prefabField.GetValue(wasteVisuals) as GameObject;
            Assert.That(leftoverPrefab, Is.Not.Null,
                "TableWasteVisuals.leftoverPrefab should be assigned to the leftover.prefab asset.");
        }

        [Test]
        public void TableWasteVisuals_LeftoverStackAnchor_IsAssigned()
        {
            var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
            Assert.That(tablePrefab, Is.Not.Null);

            var wasteVisuals = tablePrefab.GetComponent<TableWasteVisuals>();
            Assert.That(wasteVisuals, Is.Not.Null);

            var anchorField = typeof(TableWasteVisuals).GetField("leftoverStackAnchor",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(anchorField, Is.Not.Null, "Expected TableWasteVisuals to have a 'leftoverStackAnchor' field.");

            var anchor = anchorField.GetValue(wasteVisuals) as Transform;
            Assert.That(anchor, Is.Not.Null,
                "TableWasteVisuals.leftoverStackAnchor should be assigned to the LeftoverStackAnchor child transform.");
        }

        [Test]
        public void TableWasteVisuals_LeftoverStackSpacing_IsPositive()
        {
            var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
            Assert.That(tablePrefab, Is.Not.Null);

            var wasteVisuals = tablePrefab.GetComponent<TableWasteVisuals>();
            Assert.That(wasteVisuals, Is.Not.Null);

            var spacingField = typeof(TableWasteVisuals).GetField("leftoverStackSpacing",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(spacingField, Is.Not.Null, "Expected TableWasteVisuals to have a 'leftoverStackSpacing' field.");

            var spacing = (float)spacingField.GetValue(wasteVisuals);
            Assert.That(spacing, Is.GreaterThan(0f),
                "TableWasteVisuals.leftoverStackSpacing should be a positive value.");
        }

        [Test]
        public void Table_WasteVisuals_ReferencesTableWasteVisuals()
        {
            var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
            Assert.That(tablePrefab, Is.Not.Null);

            var table = tablePrefab.GetComponent<Table>();
            Assert.That(table, Is.Not.Null, "Table prefab should have a Table component.");

            var wasteVisuals = tablePrefab.GetComponent<TableWasteVisuals>();
            Assert.That(wasteVisuals, Is.Not.Null);

            var field = typeof(Table).GetField("wasteVisuals",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "Expected Table to have a 'wasteVisuals' field.");

            var assignedVisuals = field.GetValue(table) as TableWasteVisuals;
            Assert.That(assignedVisuals, Is.SameAs(wasteVisuals),
                "Table.wasteVisuals should reference the TableWasteVisuals component on the same GameObject.");
        }

        [Test]
        public void LeftoverStackAnchor_ChildExists()
        {
            var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
            Assert.That(tablePrefab, Is.Not.Null);

            var anchorTransform = tablePrefab.transform.Find("LeftoverStackAnchor");
            Assert.That(anchorTransform, Is.Not.Null,
                "Table prefab should have a child Transform named 'LeftoverStackAnchor'.");
            Assert.That(anchorTransform.name, Is.EqualTo("LeftoverStackAnchor"));
        }
    }
}
