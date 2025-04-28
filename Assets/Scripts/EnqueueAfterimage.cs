using UnityEngine;

namespace AfterimageSample
{
    public class EnqueueAfterimage : MonoBehaviour
    {
        [SerializeField] AfterimageRenderer _afterimageRenderer;
        [SerializeField] int _intervalFrames;
        [SerializeField] public int _clipFrameBegin;
        Vector3 _oldPosition;
        int _count = 0;

        void OnEnable()
        {
            _oldPosition = _afterimageRenderer.transform.position;
            _afterimageRenderer._clipFrameBegin = _clipFrameBegin;
            _count = 0;
        }

        void FixedUpdate()
        {
            if (_afterimageRenderer.transform.position == _oldPosition)
            {
                _count = 0;
                return;
            }
            if (_count >= _intervalFrames)
            {
                _count = 0;
                _afterimageRenderer.Enqueue();
            }
            _oldPosition = _afterimageRenderer.transform.position;
            _count++;
        }
    }
}

