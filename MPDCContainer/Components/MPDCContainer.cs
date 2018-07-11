using System;
using System.Collections.Generic;
using System.Text;

namespace MPDC.Container
{
    /// <summary>
    /// Singleton container
    /// </summary>
    public class MPDCContainer
    {
        private static volatile MPDCContainer _instance;
        private static object _syncRoot = new Object();

        private volatile Dictionary<Type, Func<object>> _funcs;
        private volatile Dictionary<Type, object> _objects;

        /// <summary>
        /// Threadsafe instance of MPDCCOntainer type
        /// </summary>
        public static MPDCContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new MPDCContainer();
                        }
                    }
                }

                return _instance;
            }
        }

        private MPDCContainer()
        {
            _funcs = new Dictionary<Type, Func<object>>();
            _objects = new Dictionary<Type, object>();
        }
        /// <summary>
        /// Register type without initializing instantly. Instance will be created after first time of accessing it and will stay same instance for other retrievals 
        /// </summary>
        /// <typeparam name="T">Type to be stored as abstraction parent</typeparam>
        /// <typeparam name="O">Implementator type</typeparam>
        /// <param name="createInstanceFunc">A delegate that returns instance of the implementator type</param>
        public void Register<T, O>(Func<O> createInstanceFunc, RegisterAction action = RegisterAction.None) where O : class, T
        {
            var type = typeof(T);

            switch (action)
            {
                case RegisterAction.None:
                    if (_funcs.ContainsKey(type))
                        throw new Exception($"{type.FullName} is already registered");
                    break;
                case RegisterAction.Replace:
                    if (!_funcs.ContainsKey(type))
                        throw new Exception($"{type.FullName} is not yet registered, so it can't be replaced");
                    break;
                case RegisterAction.SkipIfRegistered:
                    if (_funcs.ContainsKey(type))
                        return;
                    break;
                case RegisterAction.AddOrReplace:
                    break;
            }
            _funcs[type] = createInstanceFunc;
        }

        /// <summary>
        /// Register type by direct instance. This instance will be returned whenever registered T type is requested
        /// </summary>
        /// <typeparam name="T">Type to be stored as abstraction parent</typeparam>
        /// <typeparam name="O">Implementator type</typeparam>
        /// <param name="instance">the instance of the implementator type</param>
        public void Register<T, O>(O instance, RegisterAction action = RegisterAction.None) where O : T
        {
            var type = typeof(T);

            switch (action)
            {
                case RegisterAction.None:
                    if (_objects.ContainsKey(type))
                        throw new Exception($"{type.FullName} is already registered");
                    break;
                case RegisterAction.Replace:
                    if (!_objects.ContainsKey(type))
                        throw new Exception($"{type.FullName} is not yet registered, so it can't be replaced");
                    break;
                case RegisterAction.SkipIfRegistered:
                    if (_objects.ContainsKey(type))
                        return;
                    break;
                case RegisterAction.AddOrReplace:
                    break;
            }
            _objects[type] = instance;
        }

        /// <summary>
        /// Retrieve registered instance by corresponding abstraction
        /// </summary>
        /// <typeparam name="T">The type of abstration the instance was registered by</typeparam>
        /// <returns>Returns the registered instance that implements given abstaction</returns>
        public T Get<T>()
        {
            var type = typeof(T);
            if (!_objects.ContainsKey(type))
            {
                lock (_syncRoot)
                {
                    if (!_objects.ContainsKey(type))
                    {
                        if (_funcs.ContainsKey(type))
                        {
                            _objects.Add(type, _funcs[type].Invoke());
                            _funcs.Remove(type);
                        }
                        else
                            throw new Exception($"{type.FullName} is not registered");
                    }
                }
            }
            return (T)_objects[type];
        }

        public enum RegisterAction {None, Replace, SkipIfRegistered, AddOrReplace }
    }
}
