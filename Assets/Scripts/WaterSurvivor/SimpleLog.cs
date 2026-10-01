using System;
using System.IO;
using UnityEngine;

namespace WaterSurvivor
{
    /// <summary>
    /// 双通道日志：既进 Unity 控制台（Debug.Log），又追加进磁盘上的 debug.log 文件。
    ///
    /// debug.log 在哪：
    ///   编辑器里      →  &lt;工程根目录&gt;\debug.log （和 Assets、ProjectSettings 同级）
    ///   打包出来的游戏 →  Application.persistentDataPath\debug.log
    ///
    /// 两个细节：
    ///   1. AutoFlush = true，游戏还在跑的时候就能用记事本打开看最新内容；
    ///   2. 用 FileShare.Read 打开，运行时允许别的程序读，不会把文件锁死。
    /// </summary>
    public static class SimpleLog
    {
        static readonly object _lock = new object();
        static StreamWriter _writer;
        static bool _openTried;      // 只尝试打开一次，避免每帧都去碰磁盘
        static bool _openFailed;

        /// <summary>算出 debug.log 的完整路径（纯计算，不会真的创建/打开文件）。</summary>
        public static string ResolveFilePath()
        {
#if UNITY_EDITOR
            // 编辑器里 Application.dataPath 是 <工程>/Assets，上一级就是工程根目录
            DirectoryInfo root = Directory.GetParent(Application.dataPath);
            if (root != null) return Path.Combine(root.FullName, "debug.log");
#endif
            return Path.Combine(Application.persistentDataPath, "debug.log");
        }

        static void OpenOnce()
        {
            if (_openTried) return;
            _openTried = true;

            if (!GameConfig.WriteLogFile) return;

            try
            {
                string path = ResolveFilePath();
                _writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read));
                _writer.AutoFlush = true;
                _writer.WriteLine();
                _writer.WriteLine("========== 新会话 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ==========");
                Debug.Log("[SimpleLog] 日志文件：" + path);
            }
            catch (Exception e)
            {
                _openFailed = true;
                _writer = null;
                Debug.LogWarning("[SimpleLog] 打不开 debug.log，本次只输出到控制台：" + e.Message);
            }
        }

        public static void Log(string message) { Write(0, message); }
        public static void Warn(string message) { Write(1, message); }
        public static void Error(string message) { Write(2, message); }

        static void Write(int level, string message)
        {
            // ---- 通道 1：Unity 控制台 ----
            if (level == 1) Debug.LogWarning(message);
            else if (level == 2) Debug.LogError(message);
            else Debug.Log(message);

            // ---- 通道 2：debug.log 文件 ----
            if (!GameConfig.WriteLogFile || _openFailed) return;

            string levelName = level == 1 ? "WARN " : (level == 2 ? "ERROR" : "INFO ");
            string line = DateTime.Now.ToString("HH:mm:ss.fff") + " [" + levelName + "] " + message;

#if UNITY_EDITOR
            // 编辑器里【非 Play 状态】时不要长期占着文件句柄：
            // 否则编辑器菜单里打过一次日志后，句柄会一直留着，
            // 进 Play 模式时新句柄打不开文件，日志就写不进 debug.log 了。
            // 所以这里改成"写一行、关一次"。
            if (!Application.isPlaying)
            {
                try { File.AppendAllText(ResolveFilePath(), line + Environment.NewLine); }
                catch { }
                return;
            }
#endif

            OpenOnce();
            if (_writer == null) return;

            lock (_lock)
            {
                try { _writer.WriteLine(line); }
                catch { /* 磁盘满 / 文件被独占之类，忽略，绝不让写日志把游戏搞崩 */ }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterQuitHook()
        {
            Application.quitting += Close;
        }

        static void Close()
        {
            lock (_lock)
            {
                if (_writer == null) return;
                try
                {
                    _writer.WriteLine("========== 会话结束 " + DateTime.Now.ToString("HH:mm:ss") + " ==========");
                    _writer.Flush();
                    _writer.Dispose();
                }
                catch { }
                _writer = null;
            }
        }
    }
}
