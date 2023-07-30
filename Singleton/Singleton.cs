﻿using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CPPUtility
{
    // Singleton Helper
    internal static class SingletonHelper<T> where T : SingletonBase, new()
    {
        static T instance;
        public static bool IsInitalized { get; private set; } = false;

        public static T Instance
        {
            get
            {
                if (IsInitalized)
                {
                    return instance;
                }

                instance = new T();
                IsInitalized = true;

                return instance;
            }
        }
    }

    // Instanceメンバーを持たず、Singleton Helperから取得してほしいときにここから継承する
    internal abstract class SingletonBase
    {
        private static readonly Dictionary<Type, SingletonBase> instances = new Dictionary<Type, SingletonBase>();

        public SingletonBase()
        {
            var type = GetType();
            if (instances.ContainsKey(type))
            {
                throw new InvalidOperationException($"Duplicate Create Singleton class :{type}.");
            }
            instances.Add(type, this);


            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                try
                {
                    await InializeAsync();
                }
                catch (Exception exception)
                {
                    await exception.LogAsync();
                }
            }).FireAndForget();

        }
        protected virtual Task InializeAsync() { return Task.CompletedTask; }
    }

    // 基本こっちから継承する
    internal abstract class Singleton<T> : SingletonBase where T : Singleton<T>, new()
    {
        public static T Instance => SingletonHelper<T>.Instance;
    }
}

