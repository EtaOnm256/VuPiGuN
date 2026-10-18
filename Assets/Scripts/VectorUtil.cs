using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorUtil
{
    public static Vector2 rotate(Vector2 v, float deltaInRadian)
    {
        return new Vector2(
            v.x * Mathf.Cos(deltaInRadian) - v.y * Mathf.Sin(deltaInRadian),
            v.x * Mathf.Sin(deltaInRadian) + v.y * Mathf.Cos(deltaInRadian)
        );
    }

    public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        //lines are not parallel
        if (d != 0.0f)
        {

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }

        else
        {
            return false;
        }
    }
    static public Vector3 LinePrediction(Vector3 shotPosition, Vector3 targetPosition, Vector3 targetPrePosition, float bulletSpeed)
    {
        //Unityの物理はm/sなのでm/flameにする
        bulletSpeed = bulletSpeed * Time.fixedDeltaTime;
        //目標の1フレームの移動速度
        Vector3 v3_Mv = targetPosition - targetPrePosition;
        //射撃する位置から見た目標位置
        Vector3 v3_Pos = targetPosition - shotPosition;

        //ピタゴラスの定理から２つのベクトルの長さが等しい場合の式を作り
        //二次方程式の解の公式を使って弾が当たる予測時間を計算する
        float A = (v3_Mv.x * v3_Mv.x + v3_Mv.y * v3_Mv.y + v3_Mv.z * v3_Mv.z) - bulletSpeed * bulletSpeed;
        float B = 2 * (v3_Pos.x * v3_Mv.x + v3_Pos.y * v3_Mv.y + v3_Pos.z * v3_Mv.z);
        float C = (v3_Pos.x * v3_Pos.x + v3_Pos.y * v3_Pos.y + v3_Pos.z * v3_Pos.z);

        //0割り禁止処理
        if (A == 0)
        {
            if (B == 0)
            {
                return targetPosition;
            }
            else
            {
                return targetPosition + v3_Mv * (-C / B);
            }
        }

        //弾が当たる時間のフレームを計算する
        float flame1, flame2;
        //二次方程式の解の公式の判別式で分類
        float D = B * B - 4 * A * C;
        if (D > 0)
        {
            float E = Mathf.Sqrt(D);
            flame1 = (-B - E) / (2 * A);
            flame2 = (-B + E) / (2 * A);
            //解は2つなので正の数の最小値を使う
            flame1 = PlusMin(flame1, flame2);
        }
        else
        {
            //虚数解
            //当たらないので今の位置を狙う
            flame1 = 0;
        }

        //予想位置を返す
        return targetPosition + v3_Mv * flame1;
    }

    //プラスの最小値を返す(両方マイナスなら0)
    static public float PlusMin(float a, float b)
    {
        if (a < 0 && b < 0) return 0;
        if (a < 0) return b;
        if (b < 0) return a;
        return a < b ? a : b;
    }
}
