using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupComparison : MonoBehaviour
{
    public static bool Comparison<T>( List<List<T>> Groups1, List<List<T>> Groups2 )
    {
        if (Groups1.Count != Groups2.Count || Groups2 == null)
        {
            Debug.Log ("组数量不正确");
            return false;
        }
        //判断有相同的端子的组
        int trueCount = 0;
        foreach (var group2 in Groups2)
        {
            foreach (var group1 in Groups1)
            {
                foreach (var item in group1)
                {
                    //拿出这个组里的其中一个与正确的相比较,只要有一个相同就可能是同一个组
                    if (Equals (group2[0], item))
                    {
                        //Count一样就判断两组数据是否一样,如果一样就记录,重新循环
                        if (group2.Count == group1.Count)
                        {
                            if (IsSameTerminal (group2, group1))
                            {
                                trueCount++;
                                goto Pos;
                            }
                        }
                    }
                }
            }
        Pos:;
        }
        if (trueCount == Groups2.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    static bool IsSameTerminal<T>( List<T> mGroup1, List<T> mGroup2 )
    {
        int trueCount = 0;
        foreach (var m1 in mGroup1)
        {
            foreach (var m2 in mGroup2)
            {
                if (Equals (m1, m2)) // Solution 2.
                {
                    trueCount++;
                    break;
                }
            }
        }
        if (trueCount == mGroup1.Count)
        {
            return true;
        }
        return false;
    }
}
