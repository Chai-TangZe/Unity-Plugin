using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileReadWrite : MonoBehaviour
{
    static string Error = "错误！没有寻找到文件，建议查看文件名称是否输入错误？";

    static List<string> datas = new List<string>();

    /// <summary>
    /// 在本地读取文件全部数据
    /// </summary>
    /// <param name="txtName">文件名</param>
    /// <returns>返回每行</returns>
    public static List<string> ReadLocalData(string txtName)
    {
        datas.Clear();
        StreamReader sr = null;//读取
        if (new FileInfo(Application.persistentDataPath + "\\" + txtName).Exists)
            sr = File.OpenText(Application.persistentDataPath + "\\" + txtName);//读取文件
        else
        {
            datas.Add(Error);
            return datas;
        }
        
        //读取所有行
        string data = null;
        do
        {
            data = sr.ReadLine();
            if (data != null)
                datas.Add(data);
        } while (data != null);
        sr.Close();//关闭流
        sr.Dispose();//销毁流
        return datas;
    }

    /// <summary>
    /// 在本地写入一行数据
    /// </summary>
    /// <param name="txtName">文件名</param>
    /// <param name="data">写入数据</param>
    public static void WriteLocalTxt(string txtName, string data)
    {
        StreamWriter sw;//写入
        FileInfo t = new FileInfo(Application.persistentDataPath + "\\" + txtName);//文件流 
        if (!t.Exists)
        {
            sw = t.CreateText();//创建
        }
        else
        {
            sw = t.AppendText();//打开文件
        }
        sw.WriteLine(data);//写入数据      
        sw.Flush();//清除缓冲区
        sw.Close();//关闭流
        sw.Dispose();//销毁流
    }

    /// <summary>
    /// 删除本地文件
    /// </summary>
    /// <param name="txtName"></param>
    public static void DeleteLocalTxt(string txtName)
    {
        FileInfo t = new FileInfo(Application.persistentDataPath + "\\" + txtName);//文件流 
        t.Delete();
    }

    /// <summary>
    /// 复制本地文件的全部内容
    /// </summary>
    /// <param name="txtName"></param>
    /// <returns></returns>
    public static bool CopyLocalTxt(string txtName)
    {
        bool IsCopyTxt = false;
        string CopyContent = "";
        ReadLocalData(txtName);
        foreach (string data in datas)
        {
            CopyContent += data + "\n";
        }
        if (datas[0]== Error)
        {
            IsCopyTxt=false;
        }
        else
        {
            IsCopyTxt = true;
        }
        GUIUtility.systemCopyBuffer = CopyContent;
        return IsCopyTxt;
    }

    /// <summary>
    /// 读取StreamingAssets目录文件
    /// </summary>
    /// <param name="path">路径："/文件名加后缀"</param>
    /// <returns>返回字符串</returns>
    public static List<string> ReadStreamingAssetsData(string path)
    {
        string Path = GetStreamingAssetsPath();

        path = Path + path;

        //打开文件
        StreamReader tmpReader = new StreamReader(path);
        datas.Clear();
        string ReadRow = tmpReader.ReadLine();
        while (ReadRow != null && ReadRow != "")
        {
            datas.Add(ReadRow);
            ReadRow = tmpReader.ReadLine();
        }
        tmpReader.Close();
        return datas;
    }

    /// <summary>
    /// StreamingAssets目录写入一行数据
    /// </summary>
    /// <param name="path"></param>
    public static void WriteStreamingAssetsData(string path, string data)
    {
        ReadStreamingAssetsData(path);
        string Path = GetStreamingAssetsPath();
        path = Path + path;
        StreamWriter tmpWrite = new StreamWriter(path);
        foreach (string linedata in datas)
        {
            tmpWrite.WriteLine(linedata);
        }
        tmpWrite.WriteLine(data);
        tmpWrite.Close();
    }

    /// <summary>
    /// 得到StreamingAssets目录
    /// </summary>
    /// <returns></returns>
    static string GetStreamingAssetsPath()
    {
        string str = Application.streamingAssetsPath + "/";
        return str;
    }

}
