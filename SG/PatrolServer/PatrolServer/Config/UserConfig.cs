using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PatrolServer.Config
{
    public class UserConfig
    {
        ///用户配置接口
        private HashSet<Object> _Sets = null;

        public HashSet<Object> Sets
        {
            get
            {
                if (_Sets == null)
                {
                    _Sets = new HashSet<object>();
                }
            }
        }

        public int Init()
        {
            int count = 0;

            ///从数据源中加载基础配置信息到缓存中

            return count;
        }
    }
}
