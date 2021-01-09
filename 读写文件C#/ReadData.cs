using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReadWriteData
{
    static List<string> DataList = new List<string>();

    /// <summary>
    /// 读取文件,读的每行中间连接没有换行
    /// </summary>
    /// <param name="path">路径："/文件名加后缀"</param>
    /// <returns>返回字符串</returns>
    public static List<string> GetData(string path)
    {
        string Path = 
#if UNITY_ANDROID

             Application.dataPath + "!assets"+ "/";
#else

        Application.streamingAssetsPath + "/";

#endif

        path = Path + path;
        
        //打开文件
        StreamReader tmpReader = new StreamReader(path);
        DataList.Clear();
        string ReadRow = tmpReader.ReadLine();
        while (ReadRow != null&& ReadRow!="")
        {
            DataList.Add(ReadRow);
            ReadRow = tmpReader.ReadLine();
        }
        tmpReader.Close();
        return DataList;
    }
    /// <summary>
    /// 写入一行数据
    /// </summary>
    /// <param name="path"></param>
    public static void SetData(string path,string data)
    {
        GetData(path);
        string Path =
#if UNITY_ANDROID

             Application.dataPath + "!assets"+ "/";
#else

        Application.streamingAssetsPath + "/";

#endif
        path = Path + path;
        StreamWriter tmpWrite = new StreamWriter(path);
        foreach (string linedata in DataList)
        {
            tmpWrite.WriteLine(linedata);
        }
        tmpWrite.WriteLine(data);
        tmpWrite.Close();
    }
    
}
