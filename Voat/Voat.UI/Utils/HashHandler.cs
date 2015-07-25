﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Voat.Utils
{
    public class HashHandler
    {

        public static bool CommentExists(string content) {

            if (!String.IsNullOrEmpty(content) && content.Length > 0) { 
            

            }

            return false;
        }

        private static string Compute(params string[] parts) {

            string checkBlock = String.Concat(parts);
            var alg = SHA1Managed.Create();
            return Convert.ToBase64String(alg.ComputeHash(System.Text.Encoding.Unicode.GetBytes(checkBlock)));

        }
    }
}