## 一 Unity环境配置

1.首先导入GoogleVRForUnity_1.200.1.unitypackage

2.点击File\BuildSettings,导入当前场景,选择安卓/IOS

3.点击PlayerSettings,在Player中找到XRSettings,点击Virtual Reality Supported,点击+选择Cardboard

4.同样在PlayerSettings这个界面找到OtherSettings,在GraphicsAPIs移除Vulkan

##### 至此,环境已经搭建完成了!

## 二 如何使用

1.场景中创建空物体,取名为:CameraRig,将摄像机作为CameraRig的子物体(重置位置)

2.在Assets中搜索:GvrEditorEmulator预制体拖入场景

##### 这样最基本的功能已经实现了,打包后就可以用手机VR盒子控制Camera了, 由于是VR模式,所以不能用UI(无法看清显得很奇怪)

## 三 如何加一个注视点

1.在Assets中搜索:GvrReticlePointer预制体作为Camera的子物体

2.在Assets中搜索:GvrEventSystem预制体拖入场景

3.选择Camera添加组件:GvrPointerPhysicsRaycaster

##### 这样再次运行屏幕中心就会出现一个小点,打包后就是在屏幕中间,也不会感到别扭

## 如何进行交互

1.创建一个Cube,添加组件EventTrigger,添加各种事件,比如PointerEnter,PointerExit,PointerDown...

2.编写代码:

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEvent : MonoBehaviour
{
    public void PointerEnter()
    {
        Debug.Log ("PointerEnter");
    }
    public void PointerExit()
    {
        Debug.Log ("PointerExit");
    }
    public void PointerDown()
    {
        Debug.Log ("PointerDown");
    }
}

```

3.将代码添加到任意物体,将物体分别添加到EventTrigger事件中选择对应的方法

##### 此时运行就可以对物体进行相应的操作了
