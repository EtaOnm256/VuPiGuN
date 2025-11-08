using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;
[ExecuteInEditMode]
public class WorldMapNode : MonoBehaviour
{


    private Scene m_Scene;
    private Shape m_Path;
    private VectorUtils.TessellationOptions m_Options;
    private Mesh m_Mesh;
    //public Transform[] controlPoints;

    public Vector2 Pos;

    public float Radius = 10.0f;
    public float LineThickness = 2;
    public Color InnerColor;
    public Color OuterColor;

    // Start is called before the first frame update
    void Start()
    {
        // Prepare the vector path, add it to the vector scene.
         m_Path = new Shape()
         {
             Contours = new BezierContour[] { new BezierContour() },
             PathProps = new PathProperties()
             {
                 Stroke = new Stroke() { Color = OuterColor, HalfThickness = LineThickness }
             },
             Fill = new SolidFill() { Color = InnerColor }
         };

        m_Scene = new Scene()
        {
            Root = new SceneNode() { Shapes = new List<Shape> { m_Path } }
        };

        m_Options = new VectorUtils.TessellationOptions()
        {
            StepDistance = 1000.0f,
            MaxCordDeviation = 0.05f,
            MaxTanAngleDeviation = 0.05f,
            SamplingStepSize = 0.01f
        };

        // Instantiate a new mesh, it will be filled with data in Update()
        m_Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_Mesh;
    }

    // Update is called once per frame
    void Update()
    {
        // Update the control points of the spline.
        /*m_Path.Contours[0].Segments[0].P0 = (Vector2)controlPoints[0].localPosition;
        m_Path.Contours[0].Segments[0].P1 = (Vector2)controlPoints[1].localPosition;
        m_Path.Contours[0].Segments[0].P2 = (Vector2)controlPoints[2].localPosition;
        m_Path.Contours[0].Segments[1].P0 = (Vector2)controlPoints[3].localPosition;
        */


        VectorUtils.MakeCircleShape(m_Path, Pos, Radius);

        m_Path.Fill = new SolidFill() { Color = InnerColor };
        m_Path.PathProps = new PathProperties()
        {
            Stroke = new Stroke() { Color = OuterColor, HalfThickness = LineThickness }
        };
        m_Path.Contours[0].Closed = true;
        // Tessellate the vector scene, and fill the mesh with the resulting geometry.
        var geoms = VectorUtils.TessellateScene(m_Scene, m_Options);
        VectorUtils.FillMesh(m_Mesh, geoms, 1.0f);
    }
}
