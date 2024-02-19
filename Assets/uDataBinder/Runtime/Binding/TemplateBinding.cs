using System;
using System.Text.RegularExpressions;
using uDataBinder.Binder;
using UnityEngine;

namespace uDataBinder
{
    public static class TemplateBinding
    {
        public static bool IsSuccessfullyConverted(string template, string result)
        {
            var pattern = template.Replace("+", "\\+");
            for (var i = 0; i < 8; ++i)
            {
                var replacement = DataBinding.LeftDelimiter + "[^" + DataBinding.LeftDelimiter + DataBinding.RightDelimiter + "]+" + DataBinding.RightDelimiter;
                pattern = Regex.Replace(pattern, replacement, ".+").Replace(".+.+", ".+");
                var begin = pattern.IndexOf(DataBinding.LeftDelimiter.ToString(), StringComparison.Ordinal);
                if (begin >= 0)
                {
                    var end = pattern.IndexOf(DataBinding.RightDelimiter.ToString(), begin + 1, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        continue;
                    }
                }
                break;
            }

            return Regex.IsMatch(result, pattern);
        }

        public static string Parse(string template, DataBinder dataBinder = null)
        {
            var result = "";
            var key = "";

            var startPosition = -1;
            var recursive = false;
            for (var i = 0; i < template.Length; ++i)
            {
                var s = template[i];

                if (s == DataBinding.LeftDelimiter)
                {
                    if (startPosition >= 0)
                    {
                        result += DataBinding.LeftDelimiter + key;
                        key = "";
                        recursive = true;
                    }

                    startPosition = i;
                }
                else if (s == DataBinding.RightDelimiter)
                {
                    if (startPosition >= 0)
                    {
                        GameObject baseObject = null;
                        if (dataBinder != null)
                        {
                            baseObject = dataBinder.gameObject;
                        }

                        var value = DataBinding.GetValue(key, baseObject);
                        if (value != null)
                        {
                            result += value.ToString();
                        }

                        if (dataBinder != null)
                        {
                            DataBinding.Register(key, dataBinder, baseObject);
                        }

                        startPosition = -1;
                        key = "";
                    }
                    else
                    {
                        result += s;
                    }
                }
                else
                {
                    if (startPosition < 0)
                    {
                        result += s;
                    }
                    else
                    {
                        key += s;
                    }
                }
            }

            if (startPosition >= 0)
            {
                result += DataBinding.LeftDelimiter + key;
            }
            else if (recursive)
            {
                result = Parse(result, dataBinder);
            }

            return result;
        }
    }
}