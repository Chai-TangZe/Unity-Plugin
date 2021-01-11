using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileReadWrite : MonoBehaviour
{
    static string Error = "����û��Ѱ�ҵ��ļ�������鿴�ļ������Ƿ��������";

    static List<string> datas = new List<string>();
    /// <summary>
    /// ��ȡȫ���ļ�
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
            datas.Add(Error);
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
    /// д��һ���ļ�
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

    /// <summary>
    /// ɾ���ļ�
    /// </summary>
    /// <param name="txtName"></param>
    public static void DeleteTxt(string txtName)
    {
        FileInfo t = new FileInfo(Application.persistentDataPath + "\\" + txtName);//�ļ��� 
        t.Delete();
    }

    /// <summary>
    /// �����ļ�����
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
