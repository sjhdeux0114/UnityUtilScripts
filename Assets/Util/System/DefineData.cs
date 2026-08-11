using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public struct I_POINT
{
    public int min;
    public int max;
    public readonly int Get()
    {
        return UnityEngine.Random.Range(min, max + 1);
    }
    public I_POINT(int m1, int m2)
    {
        min = m1;
        max = m2;
    }
}
[System.Serializable]
public struct F_POINT
{
    public float min;
    public float max;
    public readonly float Get()
    {
        return UnityEngine.Random.Range(min, max);
    }
    public F_POINT(float m1, float m2)
    {
        min = m1;
        max = m2;
    }
}




// C# 객체를 복사해주는 클래스
public class ObjectCopy
{
    // 1. Deep Clone 구현
    public static T DeepClone<T>(T obj)
    {
        if (obj == null)
            throw new ArgumentNullException("Object cannot be null.");


        return (T)Process(obj, new Dictionary<object, object>() { });
    }



    private static object Process(object obj, Dictionary<object, object> circular)
    {
        if (obj == null)
            return null;



        Type type = obj.GetType();



        if (type.IsValueType || type == typeof(string))
        {
            return obj;
        }



        if (type.IsArray)
        {
            if (circular.ContainsKey(obj))
                return circular[obj];



            string typeNoArray = type.FullName.Replace("[]", string.Empty);
            Type elementType = Type.GetType(typeNoArray + ", " + type.Assembly.FullName);
            var array = obj as Array;
            Array arrCopied = Array.CreateInstance(elementType, array.Length);

            circular[obj] = arrCopied;



            for (int i = 0; i < array.Length; i++)
            {
                object element = array.GetValue(i);
                object objCopy = null;



                if (element != null && circular.ContainsKey(element))
                    objCopy = circular[element];
                else
                    objCopy = Process(element, circular);



                arrCopied.SetValue(objCopy, i);
            }



            return Convert.ChangeType(arrCopied, obj.GetType());
        }



        if (type.IsClass)
        {
            if (circular.ContainsKey(obj))
                return circular[obj];



            object objValue = Activator.CreateInstance(obj.GetType());
            circular[obj] = objValue;
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);



            foreach (FieldInfo field in fields)
            {
                object fieldValue = field.GetValue(obj);

                if (fieldValue == null)
                    continue;



                object objCopy = circular.ContainsKey(fieldValue) ? circular[fieldValue] : Process(fieldValue, circular);
                field.SetValue(objValue, objCopy);
            }

            return objValue;
        }
        else
            throw new ArgumentException("Unknown type");
    }

    public static T DeepCopy<T>(T obj)
    {
        using (var stream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Position = 0;

            return (T)formatter.Deserialize(stream);
        }
    }




}





public class _DEF_UTIL
{

    public const int _DEF_COIN = 5;

    public const int SAVE_CODE = 1251;

    static public int seed = 0;
    static public int[] _RND_CODE = new int[1024] {
        668,591,915,452,135,406,222,59,85,973,338,47,728,94,565,73,
        919,987,490,81,182,784,198,21,74,794,865,165,293,177,892,254,
        530,378,235,576,383,743,489,841,286,343,489,19,147,519,645,897,
        553,522,379,165,1020,951,497,907,393,893,377,364,565,56,789,932,
        81,84,540,349,883,1012,714,175,497,672,819,863,295,178,897,579,
        148,992,636,795,822,727,257,603,904,351,183,513,366,124,276,229,
        577,580,962,62,1003,296,13,565,832,203,691,347,131,658,612,215,
        1019,822,215,596,444,1018,110,378,850,961,468,532,828,200,833,372,
        396,589,285,738,239,835,599,876,978,70,987,618,35,934,471,246,
        762,992,517,568,933,703,809,582,470,583,236,41,851,505,761,631,
        393,318,974,565,373,898,999,184,222,363,642,373,31,515,343,940,
        490,222,793,859,706,265,463,717,804,726,267,680,599,558,24,472,
        333,355,224,740,984,929,896,888,1017,333,346,569,60,435,551,978,
        240,319,575,629,736,925,949,128,878,256,784,670,630,318,609,687,
        338,169,40,700,290,626,980,584,349,929,865,373,943,917,657,369,
        175,312,440,279,1006,419,760,431,430,96,885,727,144,225,257,698,
        747,682,231,915,395,875,688,637,363,1015,279,830,59,575,209,985,
        623,415,495,317,902,330,704,942,432,632,270,194,764,252,439,123,
        671,166,649,260,686,765,997,324,648,826,939,1015,139,954,371,878,
        910,924,103,753,754,963,820,417,1001,312,482,787,604,493,897,110,
        576,155,158,917,949,972,299,881,442,171,693,75,414,713,803,502,
        260,415,307,863,148,710,393,732,369,232,283,373,283,210,743,154,
        953,828,439,1007,795,478,952,962,625,246,310,582,270,745,2,288,
        533,1003,527,189,186,33,986,889,786,450,547,717,689,949,597,232,
        452,538,145,609,273,819,731,805,487,369,238,926,399,834,461,648,
        229,1019,863,304,706,554,992,919,866,573,221,344,955,235,272,699,
        672,78,130,756,599,416,14,737,272,745,725,840,819,59,490,975,
        1013,922,487,693,85,293,549,922,569,250,767,724,598,386,874,252,
        96,562,253,321,704,729,49,837,1005,126,31,985,852,152,100,806,
        83,269,355,32,771,603,614,378,552,231,186,349,673,314,139,909,
        895,473,743,257,829,168,271,283,873,674,712,983,261,588,70,417,
        149,984,640,329,31,841,504,517,971,899,793,473,542,723,163,691,
        1010,146,89,379,432,866,1000,938,1022,71,183,97,914,654,116,988,
        161,792,199,712,697,834,878,54,992,226,309,954,399,822,764,149,
        510,21,997,115,498,714,1009,307,1008,726,331,983,657,407,626,317,
        782,828,311,774,524,475,1017,176,292,35,410,205,557,46,538,199,
        331,34,610,893,681,1,489,470,806,916,728,994,503,918,421,585,
        624,1016,172,594,510,1014,648,35,27,820,847,463,304,804,774,962,
        187,131,652,623,821,757,660,781,339,865,991,632,770,23,697,397,
        637,849,605,564,897,968,713,340,409,574,865,884,178,25,166,2,
        142,790,807,371,266,821,788,890,356,414,254,669,23,16,288,4,
        123,449,849,479,198,209,64,243,441,311,213,246,915,799,603,314,
        699,988,490,1004,239,325,130,840,61,738,208,483,713,43,317,393,
        285,861,761,752,199,711,828,957,726,563,953,979,607,541,999,580,
        835,964,994,58,550,308,788,822,318,9,417,484,0,605,554,397,
        1017,844,984,54,38,874,114,1012,160,557,278,699,395,175,232,543,
        763,651,671,772,73,258,866,1001,72,963,433,189,293,343,956,121,
        9,555,985,787,581,944,542,984,849,1006,922,1017,30,88,426,143,
        801,898,744,372,957,805,324,758,1002,1006,1005,99,441,949,718,840,
        624,156,690,863,214,469,985,842,109,710,947,91,360,873,971,1008,
        536,290,412,418,623,584,187,228,231,226,809,599,963,443,543,924,
        287,81,392,897,422,586,952,332,936,926,263,246,881,232,398,128,
        35,1021,586,150,537,299,217,40,993,461,631,1011,683,386,903,690,
        166,23,828,278,480,73,585,973,361,79,506,720,321,964,370,1023,
        852,680,563,965,880,392,386,1011,888,352,928,985,978,965,672,550,
        23,596,80,858,428,119,671,985,436,636,813,457,817,520,743,28,
        90,632,159,3,846,961,833,798,660,841,414,128,169,624,1018,420,
        430,979,697,748,884,536,595,732,8,601,529,244,603,953,193,367,
        633,899,895,346,805,669,727,85,787,305,198,611,729,543,816,835,
        641,393,26,861,847,1018,167,577,37,450,1018,244,432,589,194,731,
        647,46,518,293,753,672,638,785,252,611,985,578,338,1007,3,489,
        809,717,626,209,926,355,544,638,731,317,308,354,674,482,677,197,
        111,491,987,682,24,522,82,993,778,265,609,235,33,613,239,53,
        446,529,459,738,987,1008,828,292,525,904,585,435,364,283,111,794};

    static public void SJH_random_Seed(int n)
    {
        seed = n;
    }
    static public int SJH_random(int n)
    {
        seed += SJH_Get_Rnd_Code(seed);

        return seed % n;
    }
    static public int SJH_Get_Rnd_Code(int code)
    {
        int n = code % _RND_CODE.Length;
        return _RND_CODE[n];
    }


    static public int random(int n)
    {
        return UnityEngine.Random.Range(0, n);
    }
    static public float random_f(float n)
    {
        return UnityEngine.Random.Range(0, n);
    }
    static public int randomRange(int n, int max)
    {
        return UnityEngine.Random.Range(n, max);
    }

    static public int RandomGet(int[] array)
    {
        if (array.Length <= 1)
            return 0;
        int total = 0;
        for (int i = 0; i < array.Length; i++)
        {
            total += array[i];
        }

        int rnd = UnityEngine.Random.Range(0, total);
        int end = 0;
        for (int i = 0; i < array.Length; i++)
        {
            end += array[i];
            if (rnd < end)
            {
                return i;
            }
        }

        return 0;

    }

    static public void Make_Rnd_Array(ref int[] data, int code, int seed)
    {
        data[0] = seed;

        SJH_random_Seed(code + seed);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = seed + i;
            if (data[i] > 0)
                data[i] = data[i] % data.Length;

        }
        for (int i = 1; i < data.Length; i++)
        {
            int rnd = SJH_random(data.Length - 1) + 1;
            int tmp = data[i];

            data[i] = data[rnd];
            data[rnd] = tmp;


        }
    }
    static public void Make_Suncha_Array(ref int[] data, int seed)
    {
        data[0] = seed;

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = seed + i;
            if (data[i] >= data.Length)
                data[i] -= data.Length;

        }

    }

    static public void Swap(ref int a, ref int b)
    {
        int tmp = a;
        a = b;
        b = tmp;
    }
}
