using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DeputyScripts.EvenManager;

namespace DeputyScripts
{
    public class EvenManager
    {

        #region object事件分发
        //执行事件，参数为对象列表
        public delegate void EventDelegate(object[] args);

        //使用字典格式（因为会有多个执行事件）存储执行事件，并用字符串存储ID
        private Dictionary<string, Dictionary<int, EventDelegate>> eventListeners = new Dictionary<string, Dictionary<int, EventDelegate>>();
        //public delegate void EventDelegateClass<T>(T args) where T : class;
        //private Dictionary<string, Dictionary<int, EventDelegateClass<EventDelegateClass2>>> EventListenerClass = new Dictionary<string, Dictionary<int, EventDelegateClass<EventDelegateClass2>>>();

        public static EvenManager Instance { get { return Singleton<EvenManager>.Instance; } }

        /// <summary>
        /// 订阅事件，即存储一组执行事件
        /// </summary>
        /// <param name="eventName">一个触发事件对应的所有执行事件的ID</param>
        /// <param name="handler">执行事件</param>
        public void AddEvent(string eventName, EventDelegate handler)
        {
            //如果要存储执行事件为空，不处理
            if (handler == null)
            {
                return;
            }

            //如果事件字典中不包含该eventName这个键，则创建一个键值对（即一个新的触发事件）
            if (!eventListeners.ContainsKey(eventName))
            {
                eventListeners.Add(eventName, new Dictionary<int, EventDelegate>());


            }

            //获取eventName键对应的值,也同样是一个字典类型
            var handlerDic = eventListeners[eventName];
            //获取输入的委托事件的哈希值(字典索引的一种方式)
            var handlerHash = handler.GetHashCode();

            //如果输入事件对应的字典中包含执行事件，则从中移除
            if (handlerDic.ContainsKey(handlerHash))
            {
                handlerDic.Remove(handlerHash);
            }

            //将eventName键对应的值（输入的执行事件）的字典类型（用handler的哈希值作为键）加入到事件字典中
            eventListeners[eventName].Add(handler.GetHashCode(), handler);
        }
        /// <summary>
        /// 订阅事件，即存储一组执行事件
        /// </summary>
        /// <param name="eventNameList"></param>
        public void AddEvents(Dictionary<string, EvenManager.EventDelegate> eventNameList)
        {
            foreach (var val in eventNameList)
            {
                if (val.Key.Contains(" "))
                {
                   // Log.PrintWarning(val.Key + " 监听字符串存在空格");
                    continue;
                }
                AddEvent(val.Key, val.Value);
            }
        }
        /// <summary>
        /// 注销事件，即删除一组执行事件
        /// </summary>
        /// <param name="eventNameList"></param>
        public void RemoveEvents(Dictionary<string, EvenManager.EventDelegate> eventNameList)
        {
            foreach (var val in eventNameList)
            {
                RemoveEvent(val.Key, val.Value);
            }
        }
        /// <summary>
        /// 注销事件：在触发事件对应的多个执行事件中删除不再需要的执行事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handLer"></param>
        public void RemoveEvent(string eventName, EventDelegate handLer)
        {
            //同样，如果执行事件为空，不进行后续处理
            if (handLer == null)
            {
                return;
            }

            //如果事件字典中包含输eventName这个ID,则移除该键中对应的字典中的输入的执行事件
            if (eventListeners.ContainsKey(eventName))
            {
                eventListeners[eventName].Remove(handLer.GetHashCode());
                //在移除后，如果eventName对应的字典为空，或者不存在，则删除该键值对 
                if (eventListeners[eventName] == null || eventListeners[eventName].Count == 0)
                {
                    eventListeners.Remove(eventName);
                }


            }
        }

        /// <summary>
        /// 触发事件：根据ID来找到触发事件对应的所有执行事件并运行
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="objs"></param>
        public void SendEvent(string eventName, params object[] objs)
        {
            // 如果包含eventName这个ID
            if (eventListeners.ContainsKey(eventName))
            {
                //获取eventName键对应的所有执行事件
                var handlerDic = eventListeners[eventName];
                if (handlerDic != null && handlerDic.Count > 0)
                {
                    var dic = new Dictionary<int, EventDelegate>(handlerDic);
                    //通过对eventName键对应的所有执行进行遍历，然后运行
                    foreach (var f in dic.Values)
                    {
                        //f(objs);
                        try
                        {
                            //执行所有的委托事件，即EventDelegate(object[] args)
                            f(objs);
                        }
                        catch (System.Exception ex)
                        {
                           // Log.PrintError($"{ex.Message} \n {ex.Data["StackTrace"]}");
                            return;
                        }
                    }
                }

            }
        }
        #endregion

        #region byte[]事件分发
        public delegate void EventDelegateBytes(byte[] args);
        private Dictionary<string, Dictionary<int, EventDelegateBytes>> eventListenerBytes = new Dictionary<string, Dictionary<int, EventDelegateBytes>>();
        //byte[] 重载
        public void AddEvent(string eventName, EventDelegateBytes handler)
        {
            if (handler == null)
            {
                return;
            }

            if (!eventListenerBytes.ContainsKey(eventName))
            {
                eventListenerBytes.Add(eventName, new Dictionary<int, EventDelegateBytes>());
            }

            var handlerDic = eventListenerBytes[eventName];
            var handlerHash = handler.GetHashCode();
            if (handlerDic.ContainsKey(handlerHash))
            {
                handlerDic.Remove(handlerHash);
            }
            eventListenerBytes[eventName].Add(handler.GetHashCode(), handler);
        }

        public void AddEvents(Dictionary<string, EventDelegateBytes> eventNameList)
        {
            foreach (var val in eventNameList)
            {
                AddEvent(val.Key, val.Value);
            }
        }
        public void RemoveEvents(Dictionary<string, EventDelegateBytes> eventNameList)
        {
            foreach (var val in eventNameList)
            {
                RemoveEvent(val.Key, val.Value);
            }
        }

        public void RemoveEvent(string eventName, EventDelegateBytes handLer)
        {
            if (handLer == null)
            {
                return;
            }
            if (eventListenerBytes.ContainsKey(eventName))
            {
                eventListenerBytes[eventName].Remove(handLer.GetHashCode());
                if (eventListenerBytes[eventName] == null || eventListenerBytes[eventName].Count == 0)
                {
                    eventListenerBytes.Remove(eventName);
                }
            }
        }

        public void SendEvent(string eventName, byte[] objs)
        {
            if (eventListenerBytes.ContainsKey(eventName))
            {
                var handlerDic = eventListenerBytes[eventName];
                if (handlerDic != null && handlerDic.Count > 0)
                {
                    var dic = new Dictionary<int, EventDelegateBytes>(handlerDic);
                    foreach (var f in dic.Values)
                    {
                        try
                        {
                            f(objs);
                        }
                        catch (System.Exception ex)
                        {
                            Log.PrintError($"{ex.Message} \n {ex.Data["StackTrace"]}");
                            return;
                        }
                    }
                }

            }
        }
        #endregion
        /// <summary>
        /// 删除事件：与注销事件不同，删除事件直接删除触发事件所对应的所有执行事件，同时删除这个触发事件
        /// </summary>
        /// <param name="eventName"></param>
        public void ClearEvents(string eventName)
        {
            if (eventListeners.ContainsKey(eventName))
            {
                eventListeners.Remove(eventName);
            }

            if (eventListenerBytes.ContainsKey(eventName))
            {
                eventListenerBytes.Remove(eventName);
            }
        }
    }
}


