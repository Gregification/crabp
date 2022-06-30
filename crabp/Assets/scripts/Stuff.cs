using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum math_func_indic
{
    add,//0
    subtract,//1
    multiply,//2
    greatestOf,//3
    leastOf,//4
    difference,//5
    mean,//6
    combineModiPerc,//7
    removeModiPerc,//8
}


public static class Stuff
{
    public static math_func_indic[] math_func_indic_nonreverseables = new math_func_indic[]
    {
        math_func_indic.difference,
        math_func_indic.greatestOf,
        math_func_indic.leastOf,
        math_func_indic.mean
    };

    public static float[] combineArr(math_func_indic function, float[] aa, float[] b)
    {
        float[] a = aa;
        int sl = a.Length > b.Length ? b.Length : a.Length;
        if (sl == 0) return a;
        switch(function)
        {
            case math_func_indic.add:
                for (int i = 0; i < sl; i++)
                    a[i] += b[i];
                break;

            case math_func_indic.subtract:
                for (int i = 0; i < sl; i++)
                    a[i] -= b[i];
                break;

            case math_func_indic.multiply:
                for (int i = 0; i < sl; i++)
                    a[i] *= b[i];
                break;

            case math_func_indic.greatestOf:
                for (int i = 0; i < sl; i++)
                    a[i] = a[i] > b[i] ? a[i] : b[i];
                break;

            case math_func_indic.leastOf:
                for (int i = 0; i < sl; i++)
                    a[i] = a[i] > b[i] ? b[i] : a[i];
                break;

            case math_func_indic.difference:
                for (int i = 0; i < sl; i++)
                    a[i] = Mathf.Abs(Mathf.Abs(a[i]) - Mathf.Abs(b[i]));
                break;

            case math_func_indic.mean:
                for (int i = 0; i < sl; i++)
                    a[i] = (a[i] + b[i]) / 2;
                break;

            case math_func_indic.combineModiPerc:
                for (int i = 0; i < sl; i++)
                    a[i] = combineModiPercent(a[i], b[i]);
                break;

            case math_func_indic.removeModiPerc:
                for (int i = 0; i < sl; i++)
                    a[i] = removeModiPercent(a[i], b[i]);
                break;

            default:
                Debug.Log("combineArr in Stuff.cs went wrong. probley missing function");
                break;
        }

        return a;
    }

    public static float[] combineArr(math_func_indic function, float[] a, float b)
    {
        float[] ba = new float[a.Length];
        for (int i = 0; i < ba.Length; i++)
            ba[i] = b;

        return combineArr(function, a, ba);
    }

    //returns null if the function is unreverseable, otherwise does what its supposed to
    public static float[] reverseCombineArr(math_func_indic ogFunction, float[] alterdArr, float[] b)
    {
        if (Array.Exists(math_func_indic_nonreverseables, v => v == ogFunction))
            return null;

        float[] a = alterdArr;
        int sl = alterdArr.Length > b.Length ? b.Length : alterdArr.Length;
        if (sl == 0) return alterdArr;
        switch (ogFunction)
        {
            case math_func_indic.add:
                for (int i = 0; i < sl; i++)
                    a[i] -= b[i];
                break;

            case math_func_indic.subtract:
                for (int i = 0; i < sl; i++)
                    a[i] += b[i];
                break;

            case math_func_indic.multiply:
                for (int i = 0; i < sl; i++)
                    a[i] /= b[i];
                break;

            case math_func_indic.combineModiPerc:
                for (int i = 0; i < sl; i++)
                    a[i] = removeModiPercent(a[i], b[i]);
                break;

            case math_func_indic.removeModiPerc:
                for (int i = 0; i < sl; i++)
                    a[i] = combineModiPercent(a[i], b[i]);
                break;

            default:
                Debug.Log("reverseCombineArr in Stuff.cs went wrong. probley missing function or something not listed on 'math_func_indic_nonreverseables'");
                break;
        }
        return a;
    }
    public static float[] reversCombineArr(math_func_indic ogFunciton, float[] alteredArr, float b)
    {
        float[] nb = new float[alteredArr.Length];
        for (int i = 0; i < nb.Length; i++)
            nb[i] = b;

        return reverseCombineArr(ogFunciton, alteredArr, nb);
    }

    //on normal scale
    public static float combineModiPercent(float origional, float modi)
    {
        return origional + (1 - origional) * modi;
    }
    public static float removeModiPercent(float altered, float modi)
    {
        return (altered - modi) / (1 - modi);
    }
}

/*
 * m_YourThirdButton.onClick.AddListener(() => ButtonClicked(42));
 * case var e when e is math_func_indic.add:
*/