public class Constant
{
    //是否使用xxtea加密
    public const string XXTKey = "34fb7a3d270b27be5a574";
    public const bool UseXXTEA = true;

    public const bool IsShowLogView = false;
}

//碰撞分组
public  enum MyEnum
 {
    DEFAULT= 1 << 0,//
    PLAYER=1 << 1, //玩家
    COLLIDER_ITEM= 1 << 2, //碰撞器
    MONSTER=1 << 3,//小怪
    REWARD= 1 << 4,//奖品
    MONSTER_SKILL_COLLIDER= 1 << 5,//怪物技能
    OBSTACLE= 1 << 6,//障碍
}