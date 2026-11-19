using System.Collections;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Ana menu acilis sekansi: once tam siyah ekran, sonra motif+baslik yavasca
    /// belirir, bir sure ekranda kalir, en sonunda tum overlay (siyah zemin dahil)
    /// yavasca solup altindaki canli menu sahnesini (kamp atesi + butonlar) acar.
    /// </summary>
    public class IntroSequenceController : MonoBehaviour
    {
        [Header("Gruplar")]
        [SerializeField] private CanvasGroup overlayGroup;   // IntroOverlay (siyah zemin + her sey) - son solma
        [SerializeField] private CanvasGroup contentGroup;   // IntroContent (motif + baslik) - ilk beliris
        [SerializeField] private CanvasGroup buttonsGroup;   // MenuButtonsRoot - kamp acilirken onunla birlikte belirir

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

            // Butonlar baslangicta tamamen gorunmez ve tiklanamaz - kamp acilirken
            // onlarla birlikte yavasca belirecekler.
            buttonsGroup.alpha = 0f;
            buttonsGroup.interactable = false;
            buttonsGroup.blocksRaycasts = false;
        }

        private void Start()
        {
            StartCoroutine(PlayIntro());
        }

        private IEnumerator PlayIntro()
        {
            // 1) Tam siyah, hic bir sey gorunmuyor
            yield return new WaitForSeconds(holdBlackDuration);

            // 2) Motif + baslik yavasca beliriyor (siyah zemin hala tam opak)
            yield return Fade(contentGroup, 0f, 1f, titleFadeInDuration);

            // 3) Baslik bir sure okunur halde ekranda kalsin
            yield return new WaitForSeconds(holdTitleDuration);

            // 4) Overlay (siyah zemin + motif + baslik) soluyor, AYNI ANDA
            // butonlar da kamp sahnesiyle birlikte belirmeye basliyor.
            yield return FadeOverlayAndRevealButtons(overlayFadeOutDuration);

            overlayGroup.gameObject.SetActive(false);
            buttonsGroup.interactable = true;
            buttonsGroup.blocksRaycasts = true;
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