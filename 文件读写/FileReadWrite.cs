using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileReadWrite : MonoBehaviour
{
    static string Error = "����û��Ѱ�ҵ��ļ�������鿴�ļ������Ƿ��������";

    static List<string> datas = new List<string>();

    /// <summary>
    /// �ڱ��ض�ȡ�ļ�ȫ������
    /// </summary>
    /// <param name="txtName">�ļ���</param>
    /// <returns>����ÿ��</returns>
    public static List<string> ReadLocalData(string txtName)
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
    /// �ڱ���д��һ������
    /// </summary>
    /// <param name="txtName">�ļ���</param>
    /// <param name="data">д������</param>
    public static void WriteLocalTxt(string txtName, string data)
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
    /// ɾ�������ļ�
    /// </summary>
    /// <param name="txtName"></param>
    public static void DeleteLocalTxt(string txtName)
    {
        FileInfo t = new FileInfo(Application.persistentDataPath + "\\" + txtName);//�ļ��� 
        t.Delete();
    }

    /// <summary>
    /// ���Ʊ����ļ���ȫ������
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
    /// ��ȡStreamingAssetsĿ¼�ļ�
    /// </summary>
    /// <param name="path">·����"/�ļ����Ӻ�׺"</param>
    /// <returns>�����ַ���</returns>
    public static List<string> ReadStreamingAssetsData(string path)
    {
        string Path = GetStreamingAssetsPath();

        path = Path + path;

        //���ļ�
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
    /// StreamingAssetsĿ¼д��һ������
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
    /// �õ�StreamingAssetsĿ¼
    /// </summary>
    /// <returns></returns>
    static string GetStreamingAssetsPath()
    {
        string str = Application.streamingAssetsPath + "/";
        return str;
    }

}
