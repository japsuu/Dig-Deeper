using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TMPLinkHandler : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            TMP_Text pTextMeshPro = GetComponent<TMP_Text>();
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, eventData.position, null);
            if (linkIndex == -1)
                return;
            
            TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}