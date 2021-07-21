using UnityEngine;
using UnityEngine.UI;

public class CharacterStateUI : SingletonMonoBehavior<CharacterStateUI>
{
    Text status;
    Text nickName;

    RectTransform mpGauge;
    RectTransform mpBg;
    RectTransform hpGauge;
    RectTransform hpBg;
    Image mpGaugeImage;
    Image hpGaugeImage;

    internal void Show(Character character)
    {
        base.Show();

        status = transform.Find("Status").GetComponent<Text>();
        nickName = transform.Find("Name").GetComponent<Text>();


        mpGauge = transform.Find("MPBar/MpGauge").GetComponent<RectTransform>();
        mpBg = transform.Find("MPBar/MpBg").GetComponent<RectTransform>();
        hpGauge = transform.Find("HPBar/HpGauge").GetComponent<RectTransform>();
        hpBg = transform.Find("HPBar/HpBg").GetComponent<RectTransform>();
        mpGaugeImage = mpGauge.GetComponent<Image>();
        hpGaugeImage = hpGauge.GetComponent<Image>();

        var size = mpGauge.sizeDelta;
        size.x = character.maxMp;
        mpGauge.sizeDelta = size;
        mpBg.sizeDelta = size;

        size = hpGauge.sizeDelta;
        size.x = character.maxHp;
        hpGauge.sizeDelta = size;
        hpBg.sizeDelta = size;


        mpGaugeImage.fillAmount = character.mp / character.maxMp;
        hpGaugeImage.fillAmount = character.hp / character.maxHp;

        nickName.text = character.nickName;
        status.text = character.status.ToString();
    }
}
