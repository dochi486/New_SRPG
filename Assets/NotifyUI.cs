using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NotifyUI : SingletonMonoBehavior<NotifyUI>
{
    Text contentText;
    CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        contentText = transform.Find("ContentText").GetComponent<Text>();

    }

    internal void Show(string text, float visibleTime = 3)
    {
        base.Show();

        canvasGroup.DOKill();

        canvasGroup.alpha = 1;

        contentText.text = text;

        canvasGroup.DOFade(0, 1).SetDelay(visibleTime).OnComplete(Close);
    }

}
