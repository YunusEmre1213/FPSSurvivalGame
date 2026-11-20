using System.Collections;
using UnityEngine;

namespace Project.UI
{

    public class IntroSequenceController : MonoBehaviour
    {
        [Header("Gruplar")]
        [SerializeField] private CanvasGroup overlayGroup;   
        [SerializeField] private CanvasGroup contentGroup;   
        [SerializeField] private CanvasGroup buttonsGroup;  

        [Header("Zamanlama")]
        [SerializeField] private float holdBlackDuration = 0.6f;
        [SerializeField] private float titleFadeInDuration = 1.2f;
        [SerializeField] private float holdTitleDuration = 1.6f;
        [SerializeField] private float overlayFadeOutDuration = 1.2f;

        private void Awake()
        {
            overlayGroup.alpha = 1f;
            overlayGroup.interactable = false;
            overlayGroup.blocksRaycasts = true;
            contentGroup.alpha = 0f;


            buttonsGroup.alpha = 0f;
            buttonsGroup.interactable = false;
            buttonsGroup.blocksRaycasts = false;
        }

        private void Start()
        {

            FPSCounterController.Instance?.SetSuppressed(true);

            StartCoroutine(PlayIntro());
        }

        private IEnumerator PlayIntro()
        {

            yield return new WaitForSeconds(holdBlackDuration);

            yield return Fade(contentGroup, 0f, 1f, titleFadeInDuration);

            yield return new WaitForSeconds(holdTitleDuration);


            yield return FadeOverlayAndRevealButtons(overlayFadeOutDuration);

            overlayGroup.gameObject.SetActive(false);
            buttonsGroup.interactable = true;
            buttonsGroup.blocksRaycasts = true;

            FPSCounterController.Instance?.SetSuppressed(false);
        }

        private IEnumerator FadeOverlayAndRevealButtons(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                overlayGroup.alpha = Mathf.Lerp(1f, 0f, t);
                buttonsGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            overlayGroup.alpha = 0f;
            buttonsGroup.alpha = 1f;
        }

        private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            group.alpha = to;
        }
    }
}