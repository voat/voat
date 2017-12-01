using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voat.Tests.Infrastructure
{
    public static class TestExtensions
    {
        public static string Repeat(this string s, int n)
        {
            return new String(Enumerable.Range(0, n).SelectMany(x => s).ToArray());
        }
        public static string RepeatUntil(this string s, int n)
        {
            var times = n / s.Length + 1;
            var result = new String(Enumerable.Range(0, times).SelectMany(x => s).ToArray());
            return result.Substring(0, n);
        }
        public static string GetUnitTestUserPassword(this string userName)
        {
            var pwd = userName;
            while (pwd.Length < 6)
            {
                pwd += userName;
            }
            return pwd;
        }
    }
}
