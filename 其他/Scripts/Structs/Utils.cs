using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils{

    public static bool CircleHitRects(Vector2 circlePivot, float circleRadius, List<Rect> rects){
        if (rects.Count <= 0) return false;
        for (var i = 0; i < rects.Count; i++){
            if (Utils.CircleHitRect(circlePivot, circleRadius, rects[i]) == true){
                return true;
            }
        }
        return false;
    }
    public static bool CircleHitRects(Vector2 circlePivot, float circleRadius, Rect[] rects){
        List<Rect> rl = new List<Rect>();
        for (var i = 0; i < rects.Length; i++){
            rl.Add(rects[i]);
        }
        return CircleHitRects(circlePivot, circleRadius, rl);
    }

    public static bool CircleHitRect(Vector2 circlePivot, float circleRadius, Rect rect){
        int xp = circlePivot.x < rect.x ? 0 : (circlePivot.x > rect.x + rect.width ? 2 : 1);
        int yp = circlePivot.y < rect.y ? 0 : (circlePivot.y > rect.y + rect.height ? 2 : 1);
        
        if (yp == 1 && xp == 1) return true;  //在中间，则一定命中
        
        if (yp != 1 && xp == 1){
            float halfRect = rect.height / 2;
            float toHeart = Mathf.Abs(circlePivot.y - (rect.y + halfRect));
            return (toHeart <= circleRadius + halfRect);
        }else
        if (yp == 1 && xp != 1){
            float halfRect = rect.width / 2;
            float toHeart = Mathf.Abs(circlePivot.x - (rect.x + halfRect));
            return (toHeart <= circleRadius + halfRect);
        }else{
            return InRange(
                circlePivot.x, circlePivot.y, 
                yp == 0 ? rect.x : (rect.x + rect.width), 
                xp == 0 ? rect.y : (rect.y + rect.height), 
                circleRadius
            );
        }
    }

    private static bool isOnUpperSideOfLine(Vector2 corner1, Vector2 oppositeCorner, Vector2 ballCenter)
    {
        return ((oppositeCorner.x - corner1.x) * (ballCenter.y - corner1.y) - (oppositeCorner.y - corner1.y) * (ballCenter.x - corner1.x)) > 0;
    }

    public static void GetCollisionNormal(Rect boxBeingIntersected, Vector2 ballCenter,ref Vector3 velocity)
    {

        Vector2 bl = new Vector2(boxBeingIntersected.x, boxBeingIntersected.y);
        Vector2 br = new Vector2(boxBeingIntersected.x + boxBeingIntersected.width, boxBeingIntersected.y);
        Vector2 tr = new Vector2(boxBeingIntersected.x + boxBeingIntersected.width, boxBeingIntersected.y + boxBeingIntersected.height);
        Vector2 tl = new Vector2(boxBeingIntersected.x, boxBeingIntersected.y + boxBeingIntersected.height);

        bool isAboveAC = isOnUpperSideOfLine(tr, bl, ballCenter);
        bool isAboveDB = isOnUpperSideOfLine(br, tl, ballCenter);

        if (isAboveAC)
        {
            if (isAboveDB)
            {
                //bottom edge has intersected
                velocity.z = -velocity.z;
            }
            else { 
                //right edge intersected
                velocity.x = -velocity.x;
            }
        }
        else
        {
            if (isAboveDB)
            {
                //left edge has intersected
                velocity.x = -velocity.x;
            }
            else {
                //top edge intersected
                velocity.z = -velocity.z;
            }
        }
    }
    ///<summary>
    ///AABB的矩形之间是否有碰撞
    ///<param name="a">一个rect</param>
    ///<param name="b">另一个rect</param>
    ///<return>是否碰撞到了，true代表碰到了</return>
    ///</summary>
    public static bool RectCollide(Rect a, Rect b){
        float ar = a.x + a.width;
        float br = b.x + b.width;
        float ab = a.y + a.height;
        float bb = b.y + b.height;
        return (
            (a.x >= b.x && a.x <= br) ||
            (b.x >= a.x && b.x <= ar)
        ) && (
            (a.y >= b.y && a.y <= bb) ||
            (b.y >= a.y && b.y <= ab)
        );
    }

   
    public static bool InRange(float x1, float y1, float x2, float y2, float range){
        return Mathf.Pow(x1 - x2, 2) + Mathf.Pow(y1 - y2, 2) <= Mathf.Pow(range,  2);
    }


    ///<summary>
    ///根据面向和移动方向得到一个资源名预订了规则的后缀名
    ///<param name="faceDegree">面向角度</param>
    ///<param name="moveDegree">移动角度</param>
    ///<return>约定好的关键字，比如"Forward","Back","Left","Right"，对应到角色动画的key</return>
    ///</summary>
    public static string GetTailStringByDegree(float faceDegree, float moveDegree){
        float fd = faceDegree;
        float md = moveDegree;
        while (fd < 180) fd += 360;
        while (md < 180) md += 360;
        fd = fd % 360;
        md = md % 360;
        float dd = md - fd;
        if (dd > 180){
            dd -= 360;
        }else if (dd < -180){
            dd += 360;
        }
        //Debug.Log("degree:"+fd + " / " + md + " / " + dd);
        if (dd >= -45 && dd <= 45){
            return "Forward";
        }else
        if (dd < -45 && dd >= -135){
            return "Left";
        }else
        if (dd > 45 && dd <= 135){
            return "Right";
        }else{
            return "Back";
        }
    }
    /// <summary>
    /// 绘制扇形可视区域
    /// https://www.codenong.com/cs105801592/
    /// </summary>
    private static float ScopeDistance = 2f;//扇形距离
    private static float ScopeJiaodu = 120;//扇形的角度
    public static bool ScopeIsAnimal(Transform casterTrans, Vector3 vector)
    {
        float distance = Vector3.Distance(casterTrans.position, vector);//距离
        Vector3 norVec = casterTrans.rotation * Vector3.forward * 5;//此处*5只是为了画线更清楚,可以不要
        Vector3 temVec = vector - casterTrans.position;
        //Debug.DrawLine(casterTrans.position, norVec, Color.red);//画出技能释放者面对的方向向量
        float jiajiao = Mathf.Acos(Vector3.Dot(norVec.normalized, temVec.normalized)) * Mathf.Rad2Deg;//计算两个向量间的夹角
        if (distance < ScopeDistance)
        {
            if (jiajiao <= ScopeJiaodu * 0.5f)
            {
                return true;
                //Debug.Log("在扇形范围内");
            }
        }
        return false;
    }
   
    

}