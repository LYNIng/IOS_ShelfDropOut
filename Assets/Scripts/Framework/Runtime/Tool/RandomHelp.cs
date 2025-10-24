using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct RangeInt
{
    public int min;
    public int max;

    public RangeInt(int min, int max)
    {
        this.min = min;
        this.max = max;

    }
}

[Serializable]
public struct RangeFloat
{

    public float min;

    public float max;

    public RangeFloat(float vMin, float vMax)
    {
        min = vMin;
        max = vMax;
    }
    public bool InRange(float value)
    {
        return min < value && value < max;
    }
}

public class RandomHelp
{
    private static readonly System.Random _random = new System.Random();


    public static float ValueZeroToOne { get => RandomRange(0f, 1f); }

    public static int RandomRange(RangeInt range)
    {
        return RandomRange(range.min, range.max);
    }
    public static float RandomRange(RangeFloat range)
    {
        return RandomRange(range.min, range.max);
    }
    public static float RandomRange(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);/* (float)GetRandom().NextDouble() * (max - min) + min;*/
    }
    public static int RandomRange(int min, int max)
    {
        return Mathf.Clamp(UnityEngine.Random.Range(min, max), min, max - 1);
    }
    public static int nDValue(int N, int value)
    {
        int result = 0;
        for (int i = 0; i < N; ++i)
        {
            var rand = RandomRange(1, value + 1);
            result += rand;
        }
        return result;
    }
    /// <summary>
    /// 随机选择一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T RandomSelectCollectionsElement<T>(List<T> list)
    {
        RandomSelectCollectionsElement(list, out int outVal, out T outRes);
        return outRes;
    }

    public static bool RandomSelectCollectionsElement<T>(List<T> list, out int outVal, out T outRes)
    {
        if (list == null || list.Count == 0)
        {
            outVal = 0;
            outRes = default(T);
            return false;
        }

        outVal = RandomRange(0, list.Count);
        outRes = list[outVal];
        return true;
    }

    /// <summary>
    /// 随机选择一个元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <returns></returns>
    public static T RandomSelectCollectionsElement<T>(T[] arr)
    {
        if (arr == null || arr.Length == 0) return default(T);

        int rand = RandomRange(0, arr.Length);
        return arr[rand];
    }
    /// <summary>
    /// 随机调用Action
    /// </summary>
    /// <param name="actions"></param>
    public static void CallRandomAction(params UnityAction[] actions)
    {
        if (actions.Length == 0)
        {
            return;
        }
        int rand = RandomRange(0, 10000);
        rand = rand % actions.Length;
        actions[rand].Invoke();
    }

    /// <summary>
    /// 检测一次 概率为 0 - 100 的判定
    /// </summary>
    /// <param name="value"></param>
    public static bool RandomTesting(int value)
    {
        bool b = false;
        int rand = RandomRange(0, 100);

        if (rand <= value)
            b = true;

        return b;
    }
    /// <summary>
    /// 随机一个百分比
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool RandomTesting(float value)
    {
        bool b = false;
        float rand = RandomRange(0f, 100f);

        if (rand <= value)
            b = true;

        return b;
    }
    //随机在一个圆中取一个点
    public static Vector2 GetRandomPointInCircle(float radius)
    {
        // 随机角度 (0到2π)
        float angle = RandomRange(0f, Mathf.PI * 2f);
        // 随机半径 (0到radius)，使用平方根确保均匀分布
        float r = Mathf.Sqrt(RandomRange(0f, 1f)) * radius;

        // 转换为笛卡尔坐标
        float x = r * Mathf.Cos(angle);
        float y = r * Mathf.Sin(angle);

        return new Vector2(x, y);
    }

    public static T GetRandomEnumValue<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T))
                  .Cast<T>()
                  .OrderBy(x => _random.Next())
                  .First();
    }

    // 泛型方法，可以排除指定的枚举值
    public static T GetRandomEnumValue<T>(params T[] excludedValues) where T : Enum
    {
        var allValues = Enum.GetValues(typeof(T)).Cast<T>();
        var availableValues = allValues.Except(excludedValues).ToArray();

        if (availableValues.Length == 0)
            throw new InvalidOperationException("No available enum values after exclusion.");

        return availableValues[_random.Next(availableValues.Length)];
    }

    public static (int x, int y) GetRandomXY(int minX, int maxX, int minY, int maxY)
    {
        int randomX = _random.Next(minX, maxX + 1);
        int randomY = _random.Next(minY, maxY + 1);
        return (randomX, randomY);
    }

    public class RandomList<T>
    {
        public RandomList()
        {
            _randomList = new List<T>();
        }
        public RandomList(List<T> list)
        {
            _randomList = new List<T>(list);
        }
        public RandomList(T[] array)
        {
            _randomList = new List<T>(array);
        }
        public void Add(T item)
        {
            if (!_randomList.Contains(item))
                _randomList.Add(item);
        }
        public void Add(List<T> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item != null && !_randomList.Contains(item))
                {
                    _randomList.Add(item);
                }
            }
        }
        public void Add(T[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item != null && !_randomList.Contains(item))
                {
                    _randomList.Add(item);
                }
            }
        }
        public void Remove(T item)
        {
            if (_randomList.Contains(item))
                _randomList.Remove(item);
        }
        /// <summary>
        /// 随机一个Item
        /// </summary>
        /// <param name="popItem">随机出Item后,是否从列表中弹出</param>
        /// <returns></returns>
        public T RandomSelectItem(bool popItem)
        {
            if (List.Count == 0)
                return default(T);
            //int v = (int)(System.Random.value * 100);
            //int index = v % _randomList.Count;
            var item = RandomSelectCollectionsElement(_randomList);
            if (popItem)
                _randomList.Remove(item);
            return item;
        }

        public void Clear()
        {
            _randomList.Clear();
        }

        public void RandomAddToQueue(Queue<T> queue, bool popItem)
        {
            if (popItem)
            {
                while (Count > 0)
                {
                    var item = RandomSelectItem(true);
                    queue.Enqueue(item);
                }
            }
            else
            {
                var ls = new List<T>(_randomList);
                while (Count > 0)
                {
                    var item = RandomSelectItem(true);
                    queue.Enqueue(item);
                }
                _randomList.AddRange(ls);
            }
        }

        private List<T> _randomList;
        public List<T> List { get { return _randomList; } }
        public int Count { get { return _randomList.Count; } }
    }

    public class RandomRange_Weight<T> : IEnumerable<T>
    {
        public int Count { get { return NodeList.Count; } }
        /// <summary>
        /// 添加一个权重目标
        /// </summary>
        /// <param name="weight">权重值 , 如果该值为小于等于0,这个调用将无效</param>
        /// <param name="srcTarget"></param>
        public void AddWeightTarget(int weight, T srcTarget)
        {
            if (weight <= 0) return;
            var node = new RangeNode<T>(_maxWeight, _maxWeight + weight - 1, srcTarget);
            NodeList.Add(node);
            _maxWeight += weight;
        }

        public void Clear()
        {
            NodeList.Clear();
            _maxWeight = 1;
        }

        public T RandomSelectorItem(bool popItem = false)
        {
            if (NodeList.Count == 0) return default(T);
            if (NodeList.Count == 1) return NodeList[0].Target;

            var rand = RandomRange(1, _maxWeight);
            RangeNode<T> resultItem = null;
            foreach (var it in NodeList)
            {
                if (it.CheckRange(rand))
                {
                    resultItem = it;
                    break;
                }
            }

            if (resultItem == null)
            {
                Debug.Log("异常权重设置.找不到返回值");
                return default(T);
            }

            if (popItem)
            {
                NodeList.Remove(resultItem);
                Refresh();
            }
            return resultItem.Target;

        }

        public bool RemoveItem(T srcTarget)
        {
            if (NodeList.Count == 0) return false;
            for (int i = 0; i < NodeList.Count; ++i)
            {
                var node = NodeList[i];
                if (node.Target.Equals(srcTarget))
                {
                    NodeList.RemoveAt(i);
                    Refresh();
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 刷新将会刷新权重分配
        /// </summary>
        public void Refresh()
        {
            _maxWeight = 1;
            for (int i = 0; i < NodeList.Count; ++i)
            {
                var node = NodeList[i];
                int weight = node.Max - node.Min;
                node.Min = _maxWeight;
                node.Max = _maxWeight + weight - 1;
                _maxWeight += weight;
            }
        }

        public bool TryGetWeightByItem(T item, out int weight)
        {
            for (int i = 0; i < NodeList.Count; ++i)
            {
                RangeNode<T> resultItem = NodeList[i];
                if (resultItem.Target.Equals(item))
                {
                    weight = resultItem.Min;
                    return true;
                }
            }
            weight = 0;
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < NodeList.Count; ++i)
            {
                var node = NodeList[i];
                yield return node.Target;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private List<RangeNode<T>> _nodeList;
        private List<RangeNode<T>> NodeList { get { if (_nodeList == null) _nodeList = new List<RangeNode<T>>(); return _nodeList; } }

        private int _maxWeight = 1;

        private class RangeNode<T1>
        {
            private int _min;
            public int Min { get { return _min; } set { _min = value; } }
            private int _max;
            public int Max { get { return _max; } set { _max = value; } }
            private T1 _target;
            public T1 Target { get { return _target; } }
            public RangeNode(int min, int max, T1 target)
            {
                _min = min;
                _max = max;
                _target = target;
            }
            public bool CheckRange(int value)
            {
                if (value >= _min && value <= _max)
                    return true;
                return false;
            }
        }
    }
    /// <summary>
    /// 在一个范围内随机取点
    /// </summary>
    public class RandomPointInRange
    {
        private int[] _trianglesIndexArr;
        private Vector3[] _points;

        public RandomPointInRange(Collider2D collider2D)
        {
            SetRange(collider2D);
        }
        public RandomPointInRange(Mesh mesh)
        {
            SetRange(mesh);
        }
        public RandomPointInRange(int[] triangles, Vector3[] points)
        {
            SetRange(triangles, points);
        }

        public void SetRange(Collider2D collider2d)
        {
            var mesh = collider2d.CreateMesh(true, true);
            SetRange(mesh);

            GameObject.Destroy(mesh);
        }

        public void SetRange(Mesh mesh)
        {
            SetRange(mesh.triangles, mesh.vertices);
        }

        public void SetRange(int[] triangles, Vector3[] points)
        {
            _trianglesIndexArr = triangles;
            _points = points;
        }

        public Vector3 RandomPoint()
        {
            if (_trianglesIndexArr == null || _trianglesIndexArr.Length == 0 || _points == null || _points.Length == 0) return Vector3.zero;

            int rand = RandomRange(0, _trianglesIndexArr.Length / 3) * 3;

            Vector3 point = Vector3.Lerp(_points[_trianglesIndexArr[rand]], _points[_trianglesIndexArr[rand + 1]], RandomHelp.ValueZeroToOne);
            Vector3.Lerp(point, _points[_trianglesIndexArr[rand + 2]], RandomHelp.ValueZeroToOne);
            return point;
        }

    }

    /// <summary>
    /// 随机取点.可以避免重复
    /// 这个算法预生成了所有可能出现的点
    /// </summary>
    public class RandomXY
    {
        private readonly List<(int X, int Y)> _allPossiblePoints;
        private int _currentIndex = 0;
        public RandomXY(int minX, int maxX, int minY, int maxY)
        {
            // 预生成所有可能的点
            _allPossiblePoints = new List<(int X, int Y)>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    _allPossiblePoints.Add((x, y));
                }
            }

            // Fisher-Yates 洗牌算法打乱顺序
            Shuffle();
        }

        private void Shuffle()
        {
            int n = _allPossiblePoints.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                (_allPossiblePoints[k], _allPossiblePoints[n]) =
                    (_allPossiblePoints[n], _allPossiblePoints[k]);
            }
            _currentIndex = 0;
        }

        public (int X, int Y)? GetNextUniquePoint()
        {
            if (_currentIndex >= _allPossiblePoints.Count)
            {
                return null; // 所有点已用完
            }

            return _allPossiblePoints[_currentIndex++];
        }

        public void Reset()
        {
            Shuffle();
        }
    }

    public class RandomCell_Weight
    {
        private class Cell
        {
            public int weight;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        private Cell[] _cells;

        public RandomCell_Weight(int width, int height, int defaultWeight = 1)
        {
            Width = width;
            Height = height;

            _cells = new Cell[width * height];

        }


        private int ToIndex(int x, int y)
        {
            return CommonUtil.ToIndex(x, y, Width);
        }
    }
}


public static class RandomUtil
{
    public static int RandomPick(this RangeInt rangeInt)
    {
        return RandomHelp.RandomRange(rangeInt);
    }
    public static int RandomPickOne(this ref RangeInt rangeInt)
    {
        return RandomHelp.RandomRange(rangeInt);
    }
    public static float RandomPick(this RangeFloat rangeFloat)
    {
        return RandomHelp.RandomRange(rangeFloat);
    }
    public static float RandomPickOne(this ref RangeFloat rangeFloat)
    {
        return RandomHelp.RandomRange(rangeFloat);
    }
    public static int RandomPickOne(this int[] intArr)
    {
        return RandomHelp.RandomSelectCollectionsElement(intArr);
    }
    public static T RandomPickOne<T>(this T[] tArr)
    {
        return RandomHelp.RandomSelectCollectionsElement(tArr);
    }
}

