using UnityEngine;
using Unity​Engine.Rendering;
using System.Collections.Generic;
namespace AfterimageSample
{
    public class AfterImage
    {
        //RenderParams[] _params;
        Mesh[] _meshes;
        Matrix4x4[] _matrices;
        List<Material[]> materials;
        /// <summary>
        /// 描画された回数.
        /// </summary>
        public int FrameCount { get; private set; }

        /// <summary>
        /// コンストラクタ.
        /// </summary>
        /// <param name="meshCount">描画するメッシュの数.</param>
        public AfterImage(int meshCount)
        {
            //_params = new RenderParams[meshCount];
            _meshes = new Mesh[meshCount];
            _matrices = new Matrix4x4[meshCount];
            materials = new List<Material[]>();
            Reset();
        }

        /// <summary>
        /// 描画前もしくは後に実行する.
        /// </summary>
        public void Reset()
        {
            FrameCount = 0;
        }

        /// <summary>
        /// メッシュごとに使用するマテリアルを用意し、現在のメッシュの形状を記憶させる.
        /// </summary>
        /// <param name="material">使用するマテリアル. </param>
        /// <param name="layer">描画するレイヤー.</param>
        /// <param name="renderers">記憶させるSkinnedMeshRendereの配列.</param>
        public void Setup(Material material, int layer, SkinnedMeshRenderer[] renderers, Matrix4x4 prevmatrix,float t)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                // マテリアルにnullが渡されたらオブジェクトのマテリアルをそのまま使う.
                if (material == null)
                {
                    materials.Clear();

                    Material[] materials_now = new Material[renderers[i].materials.Length];

                    for (int j = 0; j < materials_now.Length; j++)
                    {
                        Material thismaterial = new Material(renderers[i].materials[j]);

                        /*thismaterial.SetFloat("_Surface", 1);

                        thismaterial.SetOverrideTag("RenderType", "Transparent");

                        thismaterial.renderQueue = (int)RenderQueue.Transparent;

                        thismaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);

                        thismaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);

                        thismaterial.SetInt("_ZWrite", 0);

                        thismaterial.DisableKeyword("_ALPHATEST_ON");

                        thismaterial.EnableKeyword("_ALPHABLEND_ON");

                        thismaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                        var color = thismaterial.color;

                        color.a = 0.1f;

                        thismaterial.color = color;*/

                        materials_now[j] = thismaterial;
                    }

                    materials.Add(materials_now);

                    //materials.Add(renderers[i].materials);
                }
                //if (_params[i].material != material)
                //{
                //    _params[i] = new RenderParams(material);
                //}
                // レイヤーを設定する.
                //if (_params[i].layer != layer)
                //{
                //    _params[i].layer = layer;
                //}
                // 現在のメッシュの状態を格納する.
                if (_meshes[i] == null)
                {
                    _meshes[i] = new Mesh();
                }
                renderers[i].BakeMesh(_meshes[i], true);

                Vector3 position_now = renderers[i].transform.localToWorldMatrix.GetPosition();
                Quaternion rotation_now = renderers[i].transform.localToWorldMatrix.rotation;
                Vector3 scale_now = renderers[i].transform.localToWorldMatrix.lossyScale;

                Vector3 prev_position = prevmatrix.GetPosition();
                Quaternion prev_rotation = prevmatrix.rotation;
                Vector3 prev_scale = prevmatrix.lossyScale;

                Vector3 position = Vector3.Lerp(prev_position, position_now, t);
                Quaternion rotation = Quaternion.Lerp(prev_rotation, rotation_now, t);
                Vector3 scale = Vector3.Lerp(prev_scale, scale_now, t);

                _matrices[i] = Matrix4x4.Translate(position)*Matrix4x4.Rotate(rotation) * Matrix4x4.Scale(scale);
            }
        }

        /// <summary>
        /// 記憶したメッシュを全て描画する.
        /// </summary>
        public void RenderMeshes(int ClipFrameBegin,Material material)
        {
            if (FrameCount >= ClipFrameBegin)
            {
                for (int i = 0; i < _meshes.Length; i++)
                {
                    for (int subMesh = 0; subMesh < _meshes[i].subMeshCount; subMesh++)
                    {
                        if (material == null)
                            Graphics.DrawMesh(_meshes[i], _matrices[i], materials[i][subMesh], 0, Camera.current, subMesh);
                        else
                            Graphics.DrawMesh(_meshes[i], _matrices[i], material, 0, Camera.current, subMesh);
                    }
                }
            }
        }

        public void Increment()
        {
            FrameCount++;
        }
    }
}
