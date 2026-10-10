using System.Collections;
using UnityEngine;
using Project.Core;
using Project.UI;

namespace Project.Gameplay.Resources
{
    public class TreeInteractable : MonoBehaviour
    {
        [Header("Agac Ayarlari")]
        [Tooltip("Agacin devrilmesi icin gereken toplam vurus sayisi")]
        [SerializeField] private int hitsRequired = 5;

        [Header("Devrilme Animasyonu")]
        [SerializeField] private float fallDuration = 1.2f;
        [Tooltip("Agac devrildikten sonra en AZ ne kadar sure sahnede kalsin (saniye). Ses klibi daha uzunsa, ses bitene kadar bekler.")]
        [SerializeField] private float lingerAfterFall = 2f;

        [Header("Odun Dususu")]
        [SerializeField] private GameObject woodPickupPrefab;
        [SerializeField] private int woodAmountPerTree = 3;
        [SerializeField] private Transform woodDropPoint;
        [Tooltip("Odun prefablari bu yukseklikten dusup gercek fizikle yere otursun")]
        [SerializeField] private float woodDropHeight = 1.5f;

        [Header("Ilerleme Cubugu (Dunya Uzayinda)")]
        [SerializeField] private GameObject chopBarPrefab;
        [SerializeField] private Transform barAnchor;

        [Header("Efektler")]
        [SerializeField] private GameObject fallEffectPrefab;
        [SerializeField] private AudioClip fallSound;

        private int _currentHits;
        private bool _isChopped;
        private WorldChopBar _chopBar;

        private void Awake()
        {
            if (chopBarPrefab != null)
            {
                Vector3 spawnPos = barAnchor != null ? barAnchor.position : transform.position + Vector3.up * 2f;
                GameObject barInstance = Instantiate(chopBarPrefab, spawnPos, Quaternion.identity);
                _chopBar = barInstance.GetComponent<WorldChopBar>();
            }
        }

        public void ReceiveChop(int amount)
        {
            if (_isChopped) return;

            _currentHits += amount;
            if (_currentHits > hitsRequired) _currentHits = hitsRequired;

            EventBus.Publish(new TreeHitEvent(_currentHits, hitsRequired));

            if (_chopBar != null)
            {
                _chopBar.SetProgress((float)_currentHits / hitsRequired);
            }

            if (_currentHits >= hitsRequired)
            {
                _isChopped = true;
                StartCoroutine(FallAndChop());
            }
        }

        private IEnumerator FallAndChop()
        {
            if (_chopBar != null)
            {
                _chopBar.Hide();
                Destroy(_chopBar.gameObject, 0.2f);
            }

            Vector3 fallAxis = new Vector3(Random.Range(-0.3f, 0.3f), 0f, 1f).normalized;
            if (Random.value > 0.5f) fallAxis = -fallAxis;

            Quaternion startRot = transform.rotation;
            Quaternion endRot = startRot * Quaternion.AngleAxis(85f, fallAxis);

            float elapsed = 0f;
            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fallDuration);
                float eased = t * t;
                transform.rotation = Quaternion.Slerp(startRot, endRot, eased);
                yield return null;
            }

            transform.rotation = endRot;

            if (fallSound != null)
            {
                AudioSource.PlayClipAtPoint(fallSound, transform.position);
            }

            if (fallEffectPrefab != null)
            {
                Instantiate(fallEffectPrefab, transform.position, Quaternion.identity);
            }

            DropWood();

            EventBus.Publish(new TreeChoppedEvent(transform.position));

            float waitTime = lingerAfterFall;
            if (fallSound != null) waitTime = Mathf.Max(waitTime, fallSound.length);
            yield return new WaitForSeconds(waitTime);

            gameObject.SetActive(false);
            Destroy(gameObject, 0.1f);
        }

        private void DropWood()
        {
            if (woodPickupPrefab == null) return;

            Vector3 dropCenter = woodDropPoint != null ? woodDropPoint.position : transform.position;

            for (int i = 0; i < woodAmountPerTree; i++)
            {
                Vector3 horizontalOffset = new Vector3(Random.Range(-0.6f, 0.6f), 0f, Random.Range(-0.6f, 0.6f));
                Vector3 spawnPos = dropCenter + horizontalOffset + Vector3.up * woodDropHeight;

                GameObject wood = Instantiate(woodPickupPrefab, spawnPos, Random.rotation);

                Rigidbody rb = wood.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;

                    rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
                    rb.AddForce(Random.insideUnitSphere * 0.3f, ForceMode.Impulse);
                }
            }
        }
    }
}