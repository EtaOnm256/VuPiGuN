using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HorizontalStretcher : MonoBehaviour
{
    private RectTransform _rectTransform, _parentRectTransform;
    private VerticalLayoutGroup verticalLayoutGroup;

    void OnEnable()
    {
        UpdateWidth();
    }

    void OnRectTransformDimensionsChange()
    {
        UpdateWidth();
    }


    private void UpdateWidth()
    {
        if (verticalLayoutGroup == null || _rectTransform == null || _parentRectTransform == null)
        {
            verticalLayoutGroup = GetComponentInParent<VerticalLayoutGroup>();
            if (verticalLayoutGroup != null)
            {
                _parentRectTransform = verticalLayoutGroup.GetComponent<RectTransform>();
                _rectTransform = GetComponent<RectTransform>();
                _rectTransform.pivot = new Vector2(0, 1);
                if (_parentRectTransform.rect.size.x - (verticalLayoutGroup.padding.left + verticalLayoutGroup.padding.right) > 0)
                    _rectTransform.sizeDelta = new Vector2(_parentRectTransform.rect.size.x - (verticalLayoutGroup.padding.left + verticalLayoutGroup.padding.right), _rectTransform.sizeDelta.y);
            }
        }
        else
        {
            if(_parentRectTransform.rect.size.x - (verticalLayoutGroup.padding.left + verticalLayoutGroup.padding.right) > 0)
                _rectTransform.sizeDelta = new Vector2(_parentRectTransform.rect.size.x - (verticalLayoutGroup.padding.left + verticalLayoutGroup.padding.right), _rectTransform.sizeDelta.y);
        }
    }
}