using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameCard : MonoBehaviour
{
    [SerializeField]
    private Image ChildIcon;

    public Sprite HiddenSprite;
    public Sprite iconSprite;

    public bool isSelected;
    public bool isMatched = false;

    public GameController gameController;
  

    public void SetIconSprite(Sprite sp)
    {
        iconSprite = sp;
    }

    public void OnCardClicked()
    {
        if (isMatched) return;
        gameController.SelectedCard(this);
    }

    public void ShowCard()
    {

        transform.DORotate(new Vector3(0f, 180f, 0f), 0.2f);    // Rotates to 180 degrees

        DOVirtual.DelayedCall(0.1f, () => ChildIcon.sprite = iconSprite);

        isSelected = true;
    }

    public void HideCard()
    {
        transform.DORotate(new Vector3(0f, 0f, 0f), 0.2f);    // Rotates back to 0 degrees 

        DOVirtual.DelayedCall(0.1f, () => ChildIcon.sprite = HiddenSprite);

        isSelected = false;
    }
}