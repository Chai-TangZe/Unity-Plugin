using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityTestCite
{
    public class TestClass
    {
        GameObject cCube = null;
        public void test(GameObject cube,Text text )
        {
            cCube = cube;
            text.text = cCube.name+"0";
        }
        public void test2()
        {
            cCube.transform.Rotate (0, 50f, 0);
        }
    }
}
