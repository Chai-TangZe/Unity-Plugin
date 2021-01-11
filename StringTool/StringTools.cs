using System.Collections.Generic;

public class StringTools
{
    /// <summary>
    /// 替换字符
    /// </summary>
    /// <param name="str">需要修改的字符串</param>
    /// <param name="Target">需要替换的字符串</param>
    /// <param name="ModifyTarget">替换为ModifyTarget</param>
    /// <returns></returns>
    public static string StringReplace(string str,string Target,string ModifyTarget)
    {
        string ModifyStr="";
        ModifyStr = System.Text.RegularExpressions.Regex.Replace(str, Target, ModifyTarget);
        return ModifyStr;
    }
    /// <summary>
    /// 替换字符
    /// </summary>
    /// <param name="str">需要修改的字符串</param>
    /// <param name="Target1">需要替换的其中一个字符串</param>
    /// <param name="Target2">需要替换的其中一个字符串</param>
    /// <param name="ModifyTarget1">Target1替换为ModifyTarget1</param>
    /// <param name="ModifyTarget2">Target2替换为ModifyTarget2</param>
    /// <returns></returns>
    public static string StringReplace(string str, string Target1, string Target2, string ModifyTarget1, string ModifyTarget2)
    {
        string ModifyStr = "";
        ModifyStr = System.Text.RegularExpressions.Regex.Replace(str, Target1, ModifyTarget1);
        return System.Text.RegularExpressions.Regex.Replace(ModifyStr, Target2, ModifyTarget2);
    }

    /// <summary>
    /// 字符串截取
    /// </summary>
    /// <param name="str">需要截取的字符串</param>
    /// <param name="LocationStart">从这个地方开始截取</param>
    /// <param name="LocationEnd">截取到这个地方</param>
    /// <returns></returns>
    public static string StringSubstring(string str, char LocationStart, char LocationEnd)
    {
        if (str.Length<3)
            return "字符串少于3，无法截取";
        string ModifyStr = "";
        int index0 = 0;
        int index1 = 0;
        for (int j = 0; j < str.Length; j++)
        {
            if (str[j] == LocationStart)
            {
                index0 = j;
            }
            if (str[j] == LocationEnd)
            {
                index1 = j;
            }
        }
        ModifyStr = str.Substring(index0+1, index1- index0-1);
        return ModifyStr;
    }

    /// <summary>
    /// 字符串截取
    /// </summary>
    /// <param name="str">需要截取的字符串</param>
    /// <param name="LocationStart">从这个地方开始截取</param>
    /// <param name="LocationEnd">截取到这个地方</param>
    /// <returns></returns>
    public static List<string> StringSubstrings(string str, char LocationStart, char LocationEnd)
    {
        List<string> ModifyStrs = new List<string>();
        if (str.Length < 3)
            return null;

        int index0 = 0;
        int index1 = 0;
        for (int j = 0; j < str.Length; j++)
        {
            if (str[j] == LocationStart)
            {
                index0 = j;
            }
            if (str[j] == LocationEnd)
            {
                index1 = j;
                ModifyStrs.Add(str.Substring(index0 + 1, index1 - index0 - 1));
            }
        }
        return ModifyStrs;
    }
    public static List<string> StringSubstrings(string str, char LocationStart1, char LocationEnd1, char LocationStart2, char LocationEnd2)
    {
        List<string> ModifyStrs = new List<string>();
        if (str.Length < 3)
            return null;

        int index0 = -1;
        int index1 = -1;
        int index2 = -1;
        int index3 = -1;
        for (int j = 0; j < str.Length; j++)
        {
            if (str[j] == LocationStart1)
                index0 = j;
            if (str[j] == LocationStart2)
                index2 = j;
            if (str[j] == LocationEnd1 && index0 != -1)
            {
                index1 = j;
                ModifyStrs.Add(str.Substring(index0 + 1, index1 - index0 - 1));
            }
            if (str[j] == LocationEnd2 && index2 != -1)
            {
                index3 = j;
                ModifyStrs.Add(str.Substring(index2 + 1, index3 - index2 - 1));
            }
        }
        return ModifyStrs;
    }
}
