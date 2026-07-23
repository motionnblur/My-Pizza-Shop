using Engineering.ScriptableObjects;
using UnityEngine;

namespace Engineering.Scripts.Mono.Managers
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        
        [SerializeField] private SSound sSound;
        [SerializeField] private SVoidEventChannel groundMoneyCollectedEvent;
        [SerializeField] private SVoidEventChannel buyingAreaPurchasedEvent;
        [SerializeField] private SVoidEventChannel pizzaServedEvent;
        [SerializeField] private SVoidEventChannel pizzaTrashedEvent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            groundMoneyCollectedEvent?.RegisterListener(OnGroundMoneyCollected);
            buyingAreaPurchasedEvent?.RegisterListener(OnBuyingAreaPurchased);
            pizzaServedEvent?.RegisterListener(OnPizzaServed);
            pizzaTrashedEvent?.RegisterListener(OnPizzaTrashed);
        }

        private void OnDisable()
        {
            groundMoneyCollectedEvent?.UnregisterListener(OnGroundMoneyCollected);
            buyingAreaPurchasedEvent?.UnregisterListener(OnBuyingAreaPurchased);
            pizzaServedEvent?.UnregisterListener(OnPizzaServed);
            pizzaTrashedEvent?.UnregisterListener(OnPizzaTrashed);
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip != null)
                sfxSource.PlayOneShot(clip);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }

        private void OnGroundMoneyCollected()
        {
            if (sSound != null)
                PlaySFX(sSound.moneyCollectEffect);
        }

        private void OnBuyingAreaPurchased()
        {
            if (sSound != null)
                PlaySFX(sSound.buyEffect);
        }

        private void OnPizzaServed()
        {
            if (sSound != null)
                PlaySFX(sSound.pizzaServeEffect);
        }

        private void OnPizzaTrashed()
        {
            if (sSound != null)
                PlaySFX(sSound.pizzaTrashEffect);
        }
    }
}
