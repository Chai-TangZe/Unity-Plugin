using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileReadWrite : MonoBehaviour
{
    static List<string> datas = new List<string>();
    /// <summary>
    /// ���ļ�
    /// </summary>
    /// <param name="txtName">�ļ���</param>
    /// <returns>����ÿ��</returns>
    public static List<string> ReadData(string txtName)
    {
        datas.Clear();
        StreamReader sr = null;//��ȡ
        if (new FileInfo(Application.persistentDataPath + "\\" + txtName).Exists)
            sr = File.OpenText(Application.persistentDataPath + "\\" + txtName);//��ȡ�ļ�
        else
        {
            datas.Add("û��Ѱ�ҵ��ļ���");
            return datas;
        }
        
        //��ȡ������
        string data = null;
        do
        {
            data = sr.ReadLine();
            if (data != null)
                datas.Add(data);
        } while (data != null);
        sr.Close();//�ر���
        sr.Dispose();//������
        return datas;
    }

    /// <summary>
    /// д�ļ�
    /// </summary>
    /// <param name="txtName">�ļ���</param>
    /// <param name="data">д������</param>
    public static void WriteTxt(string txtName, string data)
    {
        StreamWriter sw;//д��
        FileInfo t = new FileInfo(Application.persistentDataPath + "\\" + txtName);//�ļ��� 
        if (!t.Exists)
        {
            sw = t.CreateText();//����
        }
        else
        {
            sw = t.AppendText();//���ļ�
        }
        sw.WriteLine(data);//д������      
        sw.Flush();//���������
        sw.Close();//�ر���
        sw.Dispose();//������
    }
    public static void DeleteTxt(string txtName)
    {
        FileInfo t = new FileInfo(Application.persistentDataPath + "\\" + txtName);//�ļ��� 
        t.Delete();
    }
}
