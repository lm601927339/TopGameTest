using System;
using UnityEngine.Networking;

class Main
{

    public static PoolManager poolMgr = PoolManager.Instance;
    public static void InitApp() {
        //初始化数据

        //playerData.instance.loadGlobalCache();加上用户数据
        //playerData.instance.loadFromCache();加载本地存储数据
        //加载CSV相关配置
        //localConfig.instance.loadConfig(() => {
        //    this._loadFinish(); //加载第一个界面
        //})
    }


}