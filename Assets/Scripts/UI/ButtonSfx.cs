using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeonSwarm
{
    public class ButtonSfx : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (TryGetComponent<Selectable>(out var selectable) && !selectable.IsInteractable())
            {
                return;
            }
            AudioManager.Play(SfxId.Click);
        }
    }
}
