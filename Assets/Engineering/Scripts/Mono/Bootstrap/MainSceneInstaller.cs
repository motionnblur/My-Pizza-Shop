using System;
using System.Collections.Generic;
using Engineering.Scripts.Mono.Areas;
using Engineering.Scripts.Mono.Actors.ServeStation;
using Engineering.Scripts.Mono.Actors.Table;
using Engineering.Scripts.Mono.Items;
using Engineering.Scripts.Mono.Managers;
using Engineering.Scripts.Mono.Player;
using UnityEngine;

namespace Engineering.Scripts.Mono.Bootstrap
{
    [DefaultExecutionOrder(-1000)]
    public sealed class MainSceneInstaller : MonoBehaviour
    {
        [Header("Core Services")]
        [SerializeField] private InputManager inputManager;
        [SerializeField] private PlayerWallet playerWallet;
        [SerializeField] private CurrencyService currencyService;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private UIManager uiManager;

        [Header("Scene Consumers")]
        [SerializeField] private BuyingArea[] buyingAreas;
        [SerializeField] private MoneyToCollect[] moneyPickups;
        [SerializeField] private ServeStation[] serveStations;
        [SerializeField] private TableManager tableManager;

        private void Awake()
        {
            ValidateAllReferences();
            InitializeComponents();
        }

        private void ValidateAllReferences()
        {
            ValidateCoreReference(inputManager, nameof(inputManager));
            ValidateCoreReference(playerWallet, nameof(playerWallet));
            ValidateCoreReference(currencyService, nameof(currencyService));
            ValidateCoreReference(economyManager, nameof(economyManager));
            ValidateCoreReference(playerMovement, nameof(playerMovement));
            ValidateCoreReference(uiManager, nameof(uiManager));

            ValidateConsumerArray(buyingAreas, nameof(buyingAreas));
            ValidateConsumerArray(moneyPickups, nameof(moneyPickups));
            ValidateConsumerArray(serveStations, nameof(serveStations));
            ValidateCoreReference(tableManager, nameof(tableManager));
        }

        private void InitializeComponents()
        {
            currencyService.Initialize(playerWallet);
            economyManager.Initialize(currencyService);
            playerMovement.Initialize(inputManager);
            uiManager.Initialize(playerWallet);

            foreach (var area in buyingAreas)
                area.Initialize(economyManager);

            foreach (var pickup in moneyPickups)
                pickup.Initialize(currencyService);

            foreach (var station in serveStations)
                station.Initialize(currencyService, tableManager);
        }

        private void ValidateCoreReference(UnityEngine.Object reference, string fieldName)
        {
            if (reference == null)
                throw new InvalidOperationException(
                    $"{nameof(MainSceneInstaller)}: '{fieldName}' is not assigned.");
        }

        private void ValidateConsumerArray<T>(T[] array, string fieldName) where T : class
        {
            if (array == null)
                throw new InvalidOperationException(
                    $"{nameof(MainSceneInstaller)}: '{fieldName}' array is not assigned.");

            var seen = new HashSet<T>();
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                    throw new InvalidOperationException(
                        $"{nameof(MainSceneInstaller)}: '{fieldName}[{i}]' is null.");

                if (!seen.Add(array[i]))
                    throw new InvalidOperationException(
                        $"{nameof(MainSceneInstaller)}: '{fieldName}[{i}]' duplicates an earlier entry.");
            }
        }
    }
}
