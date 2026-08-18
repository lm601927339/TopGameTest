using UnityEngine;

public class BulletBezier
{
    public Vector2 startPos;
    public Vector2 midPos;
    public Vector2 endPos;
    public float percentFly { get; set; }

    public BulletBezier(Vector2 startPos, Vector2 endPos)
    {
        this.startPos = startPos;
        this.endPos = endPos;
        percentFly = 0;
    }

    public void SetEndPos(Vector2 endPos) {
        this.endPos = endPos;
    }


    /// <summary>
    /// bezier 曲线
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    public  Vector2 Bezier()
    {
        var ab = Vector2.Lerp(startPos, midPos, percentFly);
        var bc = Vector2.Lerp(midPos, endPos, percentFly);
        return Vector2.Lerp(ab, bc, percentFly);
    }
    /// <summary>
    /// 根据目标朝向获取 中间点
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public  void GetBezierMidPos(Transform tf, Vector2 a, Vector2 b)
    {
        float mR = tf.rotation.eulerAngles.y * Mathf.PI / 180;
        float rd = Random.Range(10f, 20f); 
        this.midPos = new Vector2(Mathf.Sin(mR) * rd, Mathf.Cos(mR) * rd);
        //Vector2 m = Vector2.Lerp(a, b, 0.4f);
        //Vector2 normal = Vector2.Perpendicular(a - b).normalized; //a b 垂线的方向

        //float rd = Random.Range(-20f, 20f);
        //float currRatio = 0.3f;
        //return m + (a - b).magnitude * currRatio * rd * normal;

        //this.midPos = a + new Vector2(Random.Range(-20f, 20f), Random.Range(-20f, 20f));
    }

}
