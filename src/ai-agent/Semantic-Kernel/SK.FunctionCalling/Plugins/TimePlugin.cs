namespace SK.FunctionCalling.Plugins
{
    using Microsoft.SemanticKernel;
    using System;
    using System.ComponentModel;

    public class TimePlugin
    {
        [KernelFunction, Description("获取当前城市的时间")]
        public string GetCurrentTime(string city)
        {
            // 获取当前时间并格式化为字符串
            return city + "时间为：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
