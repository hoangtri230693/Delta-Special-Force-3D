using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AutoScrollText : MonoBehaviour
{
    public ScrollRect scrollRect;

    private float scrollSpeed = 0.0068f;
    private float currentPos = 1f;

    private void OnEnable()
    {
        StartCoroutine(ScrollRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        AudioManager.instance.StopAudio();
    }

    private IEnumerator ScrollRoutine()
    {
        currentPos = 1f;
        scrollRect.verticalNormalizedPosition = 1f;

        yield return new WaitForSeconds(1f);
        AudioManager.instance.PlayLore();

        while (currentPos > 0f)
        {
            currentPos -= scrollSpeed * Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(currentPos);

            yield return null;
        }
    }
}