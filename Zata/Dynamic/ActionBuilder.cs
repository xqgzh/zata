using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Zata.Dynamic
{
    public class ActionBuilder
    {
        public ActionBuilder()
        {
            ActionTypeList.Add(typeof(CachedMethodAttribute));
            ActionTypeList.Add(typeof(ArgumentConvertAction));
        }

        public ActionBuilder(Type actionType) 
            : this()
        {
            ActionTypeList.Add(actionType);
        }

        public ActionBuilder(params Type[] actionTypeList)
            : this()
        {
            ActionTypeList.AddRange(actionTypeList);
        }

        Dictionary<string, IAction> ActionDict = new Dictionary<string, IAction>();

        protected List<Type> ActionTypeList = new List<Type>();

        private string[] KeyNameFormater = {"{0}.{1}", "{0}/{1}"};

        #region 注册类型

        /// <summary>
        /// 注册类型
        /// </summary>yanfei01
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ActionBuilder RegistType<T>() where T : new()
        {
            return RegistType<T>(true);
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="IsCreateIfRequest">是否每次请求都创建新实例</param>
        /// <returns></returns>
        public ActionBuilder RegistType<T>(bool IsCreateIfRequest) where T : new()
        {
            Type t = typeof(T);
            T TypeInstance = IsCreateIfRequest ? default(T) : new T();

            string ClassName = t.Name.ToLower();

            //只获取当前类定义的公用类型, 包括静态和实例方法
            MethodInfo[] methods = t.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly);



            object[] typeAttributes = t.GetCustomAttributes(false);

            foreach (var m in methods)
            {
                object[] methodAttributes = m.GetCustomAttributes(false);

                IAction action = CreateAction(typeAttributes, IsCreateIfRequest, () => { return new T(); }, TypeInstance, m, methodAttributes);

                AddActionFlowToDictionary(ClassName, m, action);
            }

            return this;
        }

        /// <summary>
        /// 根据上下文信息, 将业务方法流程加入流程字典中
        /// </summary>
        /// <param name="ClassName"></param>
        /// <param name="m"></param>
        /// <param name="action"></param>
        private void AddActionFlowToDictionary(string ClassName, MethodInfo m, IAction action)
        {
            foreach (string s in KeyNameFormater)
            {
                string key = string.Format(s, ClassName, m.Name);

                ActionDict.Add(key.ToLower(), action);
            }
        }

        #endregion

        #region 创建方法

        /// <summary>
        /// 创建方法
        /// </summary>
        /// <param name="typeAtrributes"></param>
        /// <param name="isCreateIfRequest"></param>
        /// <param name="creater"></param>
        /// <param name="globaInstance"></param>
        /// <param name="methodInfo"></param>
        /// <param name="methodAttributes"></param>
        /// <returns></returns>
        public virtual IAction CreateAction(
            object[] typeAtrributes, bool isCreateIfRequest,  Func<object> creater, object globaInstance, 
            MethodInfo methodInfo, object[] methodAttributes)
        {
            MethodWrapper methodWrapper = new MethodWrapper(typeAtrributes, isCreateIfRequest, creater, globaInstance, methodInfo, methodAttributes);

            List<IAction> actionList = new List<IAction>();

            //根据全局Action列表初始化当前流程的Action列表
            foreach (Type t in ActionTypeList)
            {
                IAction action = CreateAction(t, typeAtrributes, methodAttributes);

                if (action != null)
                {
                    actionList.Add(action);
                }
            }

            //按照倒序, 对每一个Action进行初始化配置, 根据配置结果, 加入类型
            IAction NextAction = null;

            for (int i = actionList.Count - 1; i >= 0; i--)
            {
                NextAction = actionList[i].Init(methodWrapper, NextAction);
            }

            return NextAction;
        }

        #endregion

        #region 根据方法上下文中创建的IAction

        /// <summary>
        /// 在方法上下文中寻找对应的IAction
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="typeAtrributes"></param>
        /// <param name="methodAttributes"></param>
        /// <returns></returns>
        IAction CreateAction(Type actionType, object[] typeAtrributes, object[] methodAttributes)
        {
            IAction action = null;

            //在当前的上下文属性中寻找匹配的IAction
            foreach (object o in typeAtrributes)
            {
                if (actionType.IsInstanceOfType(o))
                {
                    action = o as IAction;
                    if (action != null)
                        break;
                }
            }

            if (action == null)
            {
                foreach (object o in methodAttributes)
                {
                    if (actionType.IsInstanceOfType(o))
                    {
                        action = o as IAction;
                        if (action != null)
                            break;
                    }
                }
            }

            if (action == null)
            {
                //没有找到匹配的IAction, 直接创建IAction
                action = Activator.CreateInstance(actionType) as IAction;
            }

            return action;

        }

        #endregion

        #region 查询已注册的方法

        /// <summary>
        /// 查询已注册的方法
        /// </summary>
        /// <param name="methodKey"></param>
        /// <returns></returns>
        public virtual IAction FindAction(string methodKey)
        {
            if (string.IsNullOrEmpty(methodKey) == false && ActionDict.ContainsKey(methodKey.ToLower()))
                return ActionDict[methodKey.ToLower()];

            return null;
        }

        #endregion
    }
}
