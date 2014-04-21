using General.Service.BSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace General.Service
{
    public class SubscriberGroupThreadPool <T> where T:PrincipalIdentifier,new()
    {
        private IPrincipalManagement _bss;
        //private ThreadPriority _priority;
        Dictionary<T, GroupMembership[]> _result;
        private List<AutoResetEvent> handlerStack = new List<AutoResetEvent>();
        public SubscriberGroupThreadPool(IPrincipalManagement bss):this(bss,5)
        {
            this._bss = bss;
            _result = new Dictionary<T, GroupMembership[]>();
        }
        public SubscriberGroupThreadPool(IPrincipalManagement bss, int threads)
        {
            this._bss = bss;
            ThreadPool.SetMaxThreads(threads, threads);
            
        }
        private void Add(T key, GroupMembership[] sg) 
        {
            lock (_result)
            {
                _result.Add(key, sg);
            }
        }
        public void Execute(T key) 
        {
            var item = new WorkInfoItem
            {
                AutoResetEvent=new AutoResetEvent(false),
                Id=key
            };
            handlerStack.Add(item.AutoResetEvent);
            ThreadPool.QueueUserWorkItem(new WaitCallback(p =>
            {
                var workitem = p as WorkInfoItem;
                if (workitem.Id is AccountPrincipalExternalId)
                {
                    T _account = workitem.Id as T;
                    this.Add(_account, this._bss.GetGroupMemberships(_account));
                }
                if (workitem.Id is DevicePrincipalExternalId)
                {
                    T _device = workitem.Id as T;
                    this.Add(_device, this._bss.GetGroupMemberships(_device));
                }
                workitem.AutoResetEvent.Set();
            }), item);
        }
        public void WaitAllDone()
        {
            foreach (var item in handlerStack)
            {
                item.WaitOne();
            }
        }
        public Dictionary<string, GroupMembership[]> Result
        {
            get
            {
                if (typeof(T) == typeof(AccountPrincipalExternalId))
                {
                    return _result.ToDictionary(
                        p => {
                            return ((AccountPrincipalExternalId)Convert.ChangeType(p.Key, typeof(AccountPrincipalExternalId))).Id;
                        },
                        p => p.Value
                    );
                }
                if (typeof(T) == typeof(DevicePrincipalExternalId))
                {
                    return _result.ToDictionary(
                       p =>
                       {
                           return ((DevicePrincipalExternalId)Convert.ChangeType(p.Key, typeof(DevicePrincipalExternalId))).Id;
                       },
                       p => p.Value
                   );
                }
                return null;
            }
        }
        public Dictionary<T, GroupMembership[]> Raw
        {
            get
            {
                return _result;
            }
        }
     
    }
    public class WorkInfoItem
    {
        public AutoResetEvent AutoResetEvent { get; set; }
        public PrincipalIdentifier Id { get; set; }
    }
}
