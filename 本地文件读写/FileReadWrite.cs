using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileReadWrite : MonoBehaviour
{
    static string Error = "错误！没有寻找到文件，建议查看文件名称是否输入错误？";

    static List<string> datas = new List<string>();
    /// <summary>
    /// 读取全部文件
    /// </summary>
    /// <param name="txtName">文件名</param>
    /// <returns>返回每行</returns>
    public static List<string> ReadData(string txtName)
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
    /// 写入一行文件
    /// </summary>
    /// <param name="txtName">文件名</param>
    /// <param name="data">写入数据</param>
    public static void WriteTxt(string txtName, string data)
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
    /// 删除文件
    /// </summary>
    /// <param name="txtName"></param>
    public static void DeleteTxt(string txtName)
    {
        FileInfo t = new FileInfo(Application.persistentDataPath + "\\" + txtName);//文件流 
        t.Delete();
    }

    /// <summary>
    /// 复制文件内容
    /// </summary>
    /// <param name="txtName"></param>
    /// <returns></returns>
    public static bool CopyTxt(string txtName)
    {
        bool IsCopyTxt = false;
        string CopyContent = "";
        ReadData(txtName);
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
}
