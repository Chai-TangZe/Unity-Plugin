using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class ExternalCite : MonoBehaviour
{
    GameObject Cube;
    Text text;
    Type type = null;
    object objType;
    // Start is called before the first frame update
    void Start()
    {
        Cube = GameObject.Find ("Cube");
        text = GameObject.Find ("Text").GetComponent<Text>();
        //载入dll文件
        Assembly assembly = Assembly.LoadFrom ("F:/Program Files/项目文件/TestDynamicLoading/UnityTestCite/UnityTestCite/bin/Debug/UnityTestCite.dll");
        //得到里面的类
        try
        {
            type = assembly.GetType ("UnityTestCite.TestClass");
        }
        catch (Exception e)
        {
            Debug.Log (e);
            throw;
        }

        objType = Activator.CreateInstance (type);//创建实例化

        MethodInfo mi = type.GetMethod ("test");//得到方法
        mi.Invoke (objType, new object[] { Cube , text });//调用方法,传入参数
    }

    // Update is called once per frame
    void Update()
    {
        MethodInfo mi = type.GetMethod ("test2");
        mi.Invoke (objType, null);

    }
}
