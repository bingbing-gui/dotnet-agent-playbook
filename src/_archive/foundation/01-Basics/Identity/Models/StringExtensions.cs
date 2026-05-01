using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Identity.Models
{
    /// <summary>
    /// 用于字符串的扩展方法
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 转话为蛇形命名法
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToSnakeCase(this string input)
        {
            //if (string.IsNullOrEmpty(input)) { return input; }

            //var startUnderscores = Regex.Match(input, @"^_+");
            //return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").Replace("([A-Z]+)([A-Z][a-z])", "$1_$2").ToLower();

            //if (string.IsNullOrEmpty(input)) { return input; }

            // 保留开头的下划线
            var startUnderscores = Regex.Match(input, @"^_+").Value;

            // 使用正则表达式处理：
            // 1. ([a-z0-9])([A-Z])：小写字母或数字后接大写字母，插入下划线
            // 2. ([A-Z]+)([A-Z][a-z])：连续大写字母和紧随的小写字母之间插入下划线
            string snakeCase = Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2");
            snakeCase = Regex.Replace(snakeCase, @"([A-Z]+)([A-Z][a-z])", "$1_$2");

            // 转换为小写并返回结果
            return startUnderscores + snakeCase.ToLower();
        }
    }
}
