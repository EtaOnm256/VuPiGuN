using System.Collections.Generic;
using UnityEngine;

namespace AfterimageSample
{
    public class AfterimageRenderer : MonoBehaviour
    {
        [SerializeField] Material _material;
        [SerializeField] int _duration = 150;
        [SerializeField] int _layer = 6;

        SkinnedMeshRenderer[] _renderers;
        Stack<AfterImage> _pool = new Stack<AfterImage>();
        Queue<AfterImage> _renderQueue = new Queue<AfterImage>();

        [System.NonSerialized] public int _clipFrameBegin;

        void Awake()
        {
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        void Update()
        {
            Render();
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < _renderQueue.Count;)
            {
                var afterimage = _renderQueue.Dequeue();

                afterimage.Increment();

                // 描画回数が限度を超えるまで繰り返しキューに入れる.
                // 限度を超えたらプールに返す.
                if (afterimage.FrameCount < _duration)
                {
                    _renderQueue.Enqueue(afterimage);
                    i++;
                }
                else
                {
                    afterimage.Reset();
                    _pool.Push(afterimage);
                }
            }
        }

        /// <summary>
        /// キューに入っているAfterImageのメッシュを描画する.
        /// </summary>
        public void Render()
        {
            for (int i = 0; i < _renderQueue.Count;)
            {
                var afterimage = _renderQueue.Dequeue();

                afterimage.RenderMeshes(_clipFrameBegin, _material);
 
                _renderQueue.Enqueue(afterimage);
                i++;
            }
        }

        /// <summary>
        /// 描画待ちのキューにAfterimageオブジェクトを入れる.
        /// </summary>
        public void Enqueue()
        {
            AfterImage afterimage;
            if (_pool.Count > 0)
            {
                afterimage = _pool.Pop();
            }
            else
            {
                afterimage = new AfterImage(_renderers.Length);
            }
            afterimage.Setup(_material, _layer, _renderers);
            _renderQueue.Enqueue(afterimage);
        }        

        public void Clear()
        {
            for (int i = 0; i < _renderQueue.Count;)
            {
                var afterimage = _renderQueue.Dequeue();
                afterimage.Reset();
                _pool.Push(afterimage);
            }
        }
    }
}

