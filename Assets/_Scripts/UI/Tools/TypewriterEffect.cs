using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TypewriterEffect : MonoBehaviour
{
    public TMP_Text textDisplay;
    public ScrollRect scrollRect;
    public string fullText;
    public float delay = 0.07f;

    private void Start()
    {
        StartCoroutine(ShowText());
    }

    private IEnumerator ShowText()
    {
        textDisplay.text = "";
        foreach (char letter in fullText.ToCharArray())
        {
            textDisplay.text += letter;

            // Ép UI cập nhật lại kích thước ngay lập tức
            Canvas.ForceUpdateCanvases();

            // Đặt giá trị cuộn về 0 (nghĩa là đáy của danh sách)
            scrollRect.verticalNormalizedPosition = 0f;

            yield return new WaitForSeconds(delay);
        }
    }
}
