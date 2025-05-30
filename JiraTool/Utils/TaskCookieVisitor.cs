using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CefSharp;

namespace JiraTool.Utils
{
    /// <summary>
    /// 异步Cookie访问器
    /// </summary>
    public class TaskCookieVisitor : ICookieVisitor
    {
        private readonly TaskCompletionSource<List<Cookie>> _taskCompletionSource;
        private readonly List<Cookie> _cookies;

        public Task<List<Cookie>> Task => _taskCompletionSource.Task;

        public TaskCookieVisitor()
        {
            _taskCompletionSource = new TaskCompletionSource<List<Cookie>>();
            _cookies = new List<Cookie>();
        }

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            _cookies.Add(cookie);
            
            if (count == total - 1)
            {
                _taskCompletionSource.TrySetResult(_cookies);
            }
            
            return true;
        }

        public void Dispose()
        {
            // 确保任务完成
            _taskCompletionSource.TrySetResult(_cookies);
        }
    }
}
