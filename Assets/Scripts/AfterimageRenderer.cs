using System.Collections.Generic;
using UnityEngine;

namespace AfterimageSample
{
    public class AfterimageRenderer : MonoBehaviour
    {
        [SerializeField] Material _material;
        [SerializeField] int _duration = 150;
        [SerializeField] int _layer = 6;
        [SerializeField] int _interpolate = 1;

        [SerializeField] SkinnedMeshRenderer[] _renderers;
        Stack<AfterImage> _pool = new Stack<AfterImage>();
        Queue<AfterImage> _renderQueue = new Queue<AfterImage>();

        [System.NonSerialized] public int _clipFrameBegin;

        bool prevmatrix_valid = false;
        Matrix4x4 prevmatrix = new Matrix4x4();

        void Awake()
        {
            //_renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
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
            for (int idx_inter = 0; idx_inter < _interpolate; idx_inter++)
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

                float t = (_interpolate - idx_inter) / (float)_interpolate;

                if (prevmatrix_valid)
                    afterimage.Setup(_material, _layer, _renderers, prevmatrix, t);
                else
                    afterimage.Setup(_material, _layer, _renderers, _renderers[0].localToWorldMatrix, t);

                _renderQueue.Enqueue(afterimage);
            }

            prevmatrix = _renderers[0].localToWorldMatrix;
            prevmatrix_valid = true;
            
        }        

        public void Clear()
        {
            for (int i = 0; i < _renderQueue.Count;)
            {
                var afterimage = _renderQueue.Dequeue();
                afterimage.Reset();
                _pool.Push(afterimage);
            }
            prevmatrix_valid = false;
        }
    }
}

