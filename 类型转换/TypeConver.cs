using System;
using System.IO;
using UnityEngine;
//using UnityEngine.UI;

public class TypeConver
{
    //public Image image;
    //// Start is called before the first frame update
    //void Start()
    //{
    //    image.sprite = Base64ToImage(ImageToBase64(Application.dataPath + "/test.png"));
    //}
    
    /// <summary> 
    /// ͼƬת����base64�����ı� 
    /// Application.dataPath /AssetĿ¼
    /// Application.persistentDataPath /����Ŀ¼
    /// Application.streamingAssetsPath /StreamingAssetsĿ¼
    /// </summary> 
    public static string ImageToBase64(string path)
    {
        try
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int)fs.Length);
            string base64String = Convert.ToBase64String(buffer);
            Debug.Log("ת���ɹ�");
            return  base64String;
        }
        catch (Exception e)
        {
            Debug.Log("ImgToBase64String ת��ʧ��:" + e.Message);
            return null;
        }
    }

    /// <summary>
    /// base64�����ı�ת����Image
    /// </summary>
    public static Sprite Base64ToImage(string recordBase64String)
    {
        string base64 = recordBase64String;
        byte[] bytes = Convert.FromBase64String(base64);
        Texture2D tex2D = new Texture2D(100, 100);
        tex2D.LoadImage(bytes);
        Sprite s = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f));
        Resources.UnloadUnusedAssets();
        return s;
    }
}
