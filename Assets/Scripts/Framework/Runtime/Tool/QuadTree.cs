using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// 四叉树
/// </summary>
public class QuadTree<DataType>
{
    public int NodeCount { get; private set; }

    public int DataCount { get => dataMap.Count; }

    public int DeathQueueCount { get => DeathNodeQueue.Count; }

    public int MaxBufferCount { get; set; } = 512;

    public QuadTreeNode Root { get; protected set; }
    public Float2 MinimumRange { get; protected set; }

    private Queue<QuadTreeNode> DeathNodeQueue = new Queue<QuadTreeNode>();

    private Dictionary<DataType, QuadTreeNode> dataMap;//简单的映射方案.以后再改
    private List<QuadTreeNode> tempNodeList = new List<QuadTreeNode>();
    public bool GetData(Float2 pos, float radius, ref HashSet<DataType> outList)
    {
        lock (this)
        {
            tempNodeList.Clear();
            if (FindNodePathByCircle(pos, radius, ref tempNodeList))
            {
                if (outList == null) outList = new HashSet<DataType>();

                for (int i = 0, j = tempNodeList.Count; i < j; ++i)
                {
                    var node = tempNodeList[i];
                    var datas = node.Datas;

                    //在这里可以添加 圆包含矩形 判断 来优化速度 

                    foreach (var kvp in datas)
                    {
                        var dis = Float2.Distance(pos, kvp.Value);
                        if (dis < radius)
                            outList.Add(kvp.Key);
                    }
                }
                return true;
            }
            return false;
        }
    }

    public bool TryGetDataByFilter(RectFloat rect, Func<DataType, bool> filter, out DataType data)
    {
        lock (this)
        {
            data = default;

            tempNodeList.Clear();
            if (FindNodePathByRect(rect, ref tempNodeList))
            {

                for (int i = 0, j = tempNodeList.Count; i < j; ++i)
                {
                    var node = tempNodeList[i];
                    var datas = node.Datas;
                    if (rect.Contains(node.RangeRect))
                    {
                        var keys = node.Datas.Keys;
                        foreach (var key in keys)
                        {
                            if (filter.Invoke(key))
                            {
                                data = key;
                                return true;
                            }
                        }
                    }
                    else
                    {
                        foreach (var kvp in datas)
                        {
                            if (rect.Contains(kvp.Value))
                            {
                                if (filter.Invoke(kvp.Key))
                                {
                                    data = kvp.Key;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
    /// <summary>
    /// 通过矩形获取数据 矩形锚点在左下角
    /// 如果要创建锚点在中心的矩形
    /// 使用 RectFloat.Create
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="outList"></param>
    /// <returns></returns>
    public bool GetData<T>(RectFloat rect, ref T outList) where T : ICollection<DataType>, new()
    {
        lock (this)
        {
            if (outList == null)
                outList = new T();

            tempNodeList.Clear();
            if (FindNodePathByRect(rect, ref tempNodeList))
            {

                for (int i = 0, j = tempNodeList.Count; i < j; ++i)
                {
                    var node = tempNodeList[i];
                    var datas = node.Datas;
                    if (rect.Contains(node.RangeRect))
                    {
                        var keys = node.Datas.Keys;
                        foreach (var key in keys)
                            outList.Add(key);
                    }
                    else
                    {
                        foreach (var kvp in datas)
                        {
                            if (rect.Contains(kvp.Value))
                            {
                                outList.Add(kvp.Key);
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

    }
    /// <summary>
    /// 通过圆获取数据
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="radius"></param>
    /// <param name="outList"></param>
    /// <returns></returns>
    public bool GetData<T>(Float2 pos, float radius, ref T outList) where T : ICollection<DataType>, new()
    {
        lock (this)
        {
            tempNodeList.Clear();
            //List<QuadTreeNode> nodeList = new List<QuadTreeNode>();
            if (FindNodePathByCircle(pos, radius, ref tempNodeList))
            {
                if (outList == null) outList = new T();

                for (int i = 0, j = tempNodeList.Count; i < j; ++i)
                {
                    var node = tempNodeList[i];
                    var datas = node.Datas;

                    //在这里可以添加 圆包含矩形 判断 来优化速度 

                    foreach (var kvp in datas)
                    {
                        var dis = Float2.Distance(pos, kvp.Value);
                        if (dis < radius)
                            outList.Add(kvp.Key);
                    }
                }
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 通过一个点来获取节点路径
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="outList"></param>
    /// <returns></returns>
    public bool FindNodePath(Float2 pos, ref List<QuadTreeNode> outList)
    {
        lock (this)
        {
            if (Root == null) return false;
            if (FindNodePathByPos(pos, ref outList))
            {
                //return true;
            }
            return true;
        }
    }
    /// <summary>
    /// 通过一个圆获取节点路径
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="radius"></param>
    /// <param name="outList"></param>
    /// <returns></returns>
    public bool FindNodePath(Float2 pos, float radius, ref List<QuadTreeNode> outList)
    {
        lock (this)
        {
            if (Root == null) return false;

            return FindNodePathByCircle(pos, radius, ref outList);
        }
    }
    /// <summary>
    /// 通过一个矩形获取节点路径
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="outList"></param>
    /// <returns></returns>
    public bool FindNodePath(RectFloat rect, ref List<QuadTreeNode> outList)
    {
        lock (this)
        {
            if (Root == null) return false;

            return FindNodePathByRect(rect, ref outList);
        }

    }
    /// <summary>
    /// 深度遍历
    /// </summary>
    /// <param name="action"></param>
    public void ForEach_Depth(Action<DataType, Float2> action)
    {
        lock (this)
        {
            if (Root == null) return;
            Root.ForEach_Depth(action);
        }

    }
    /// <summary>
    /// 深度遍历
    /// </summary>
    /// <param name="action"></param>
    public void ForEach_Depth(Action<QuadTreeNode> action)
    {
        lock (this)
        {
            if (Root == null) return;
            Root.ForEach_Depth(action);
        }
    }
    /// <summary>
    /// 广度遍历
    /// </summary>
    /// <param name="action"></param>
    public void ForEach_Breadth(Action<DataType, Float2> action)
    {
        lock (this)
        {
            if (Root == null) return;
            Root.ForEach_Breadth(action);

        }

    }
    /// <summary>
    /// 广度遍历
    /// </summary>
    /// <param name="action"></param>
    public void ForEach_Breadth(Action<QuadTreeNode> action)
    {
        lock (this)
        {
            if (Root == null) return;
            Root.ForEach_Breadth(action);
        }

    }
    /// <summary>
    /// 添加数据
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="data"></param>
    public void Add(Float2 pos, DataType data)
    {

        lock (this)
        {
            QuadTreeNode tempNode = null;
            if (Root == null)
            {
                tempNode = Root = CreateNode(new RectFloat(pos, MinimumRange));
                var fNode = tempNode.AddData(pos, data);
                dataMap.Add(data, fNode);
                return;
            }
            tempNode = Root;
            if (dataMap.ContainsKey(data))
            {
                dataMap.Remove(data);
            }

            if (tempNode.IsInRange(pos))
            {
                var fNode = tempNode.AddData(pos, data);
                dataMap.Add(data, fNode);
            }
            else
            {
                var exNode = ExtendNode(tempNode, pos);
                Root = exNode;
                var fNode = exNode.AddData(pos, data);
                dataMap.Add(data, fNode);
            }
        }
    }
    /// <summary>
    /// 添加一个空位置 这个方法只会添加Node 不会添加数据
    /// </summary>
    /// <param name="pos"></param>
    public void AddEmpty(Float2 pos)
    {
        lock (this)
        {
            if (Root == null)
            {
                Root = CreateNode(new RectFloat(pos, MinimumRange));
                return;
            }
            var tempNode = Root;
            if (tempNode.IsInRange(pos))
            {
                tempNode.FindLeafNodeByChildren(pos);
            }
            else
            {
                var exNode = ExtendNode(tempNode, pos);
                Root = exNode;
                Root.FindLeafNodeByChildren(pos);
            }
        }
    }
    public bool RemoveData(DataType data)
    {
        lock (this)
        {
            if (dataMap.ContainsKey(data))
            {
                var node = dataMap[data];
                node.RemoveData(data);
                dataMap.Remove(data);
                CheckDeath(node);
                return true;
            }
            return false;
        }
    }
    public bool RemoveData(Float2 pos, DataType data)
    {
        lock (this)
        {
            var tempLs = Root.FindNodePath(pos);
            if (tempLs != null && tempLs.Count > 0)
            {
                var temp = tempLs[tempLs.Count - 1];

                if (temp.Datas.ContainsKey(data))
                {
                    temp.Datas.Remove(data);
                    dataMap.Remove(data);
                    CheckDeath(temp);
                }
                return true;
            }
            return false;
        }

    }
    public bool RemoveData(RectFloat rect)
    {
        lock (this)
        {
            List<DataType> datas = new List<DataType>();
            if (GetData(rect, ref datas))
            {
                for (int i = 0; i < datas.Count; ++i)
                {
                    var item = datas[i];
                    RemoveData(item);
                }
                datas.Clear();
                return true;
            }
            return false;
        }
    }
    public void Clear()
    {
        Root = null;
        DeathNodeQueue.Clear();
        dataMap.Clear();
    }
    /// <summary>
    /// 移动数据 如果没有这个Data会直接添加进去
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="data"></param>
    public void MoveData(Float2 pos, DataType data)
    {
        lock (this)
        {
            if (!dataMap.ContainsKey(data))
            {
                Add(pos, data);
                return;
            }
            var node = dataMap[data];
            if (node.IsInRange(pos))
            {
                node.Datas[data] = pos;
                return;
            }

            if (Root.IsInRange(pos))
            {
                var tempNode = node.Parent;
                while (tempNode != null)
                {
                    if (tempNode.IsInRange(pos))
                    {
                        node.RemoveData(data);
                        var fNode = tempNode.AddData(pos, data);
                        dataMap[data] = fNode;
                        CheckDeath(node);
                        break;
                    }
                    else
                        tempNode = tempNode.Parent;
                }
            }
            else
            {
                node.RemoveData(data);
                Add(pos, data);
                CheckDeath(node);
            }
        }
    }
    public void DeathNodeEnqueue(QuadTreeNode node)
    {
        lock (this)
        {
            NodeCount--;
            node.Parent = null;
            node.param = null;
            node.RootTree = null;
            node.RangeRect = RectFloat.zero;
            for (int i = 0, j = node.Children.Length; i < j; i++)
                node.Children[i] = null;
            if (DeathNodeQueue.Count < MaxBufferCount && !DeathNodeQueue.Contains(node))
                DeathNodeQueue.Enqueue(node);
        }
    }
    public void CheckDeath(QuadTreeNode node)
    {
        lock (this)
        {
            if (node.Parent != null && node.IsEmpty)
            {
                node.Parent.RemoveChildNode(node);
            }
        }
    }
    public QuadTreeNode CreateNode(RectFloat rect)
    {
        NodeCount++;
        if (DeathNodeQueue.Count > 0)
        {
            var node = DeathNodeQueue.Dequeue();
            node.Set(rect, this);
            return node;
        }

        return new QuadTreeNode(rect, this);
    }
    private QuadTreeNode ExtendNode(QuadTreeNode node, Float2 pos)
    {
        if (node.IsInRange(pos)) return node;

        var edir = node.GetDirectionEnum(pos);
        var partentPos = Float2.zero;
        if (edir == DirectionEnmu.LeftTop)
        {
            partentPos = new Float2(node.RangeRect.xMin - node.RangeRect.size.x, node.RangeRect.yMin);
        }
        else if (edir == DirectionEnmu.RightTop)
        {
            partentPos = new Float2(node.RangeRect.xMin, node.RangeRect.yMin);
        }
        else if (edir == DirectionEnmu.LeftBottom)
        {
            partentPos = new Float2(node.RangeRect.xMin - node.RangeRect.size.x, node.RangeRect.yMin - node.RangeRect.size.y);
        }
        else
        {
            partentPos = new Float2(node.RangeRect.xMin, node.RangeRect.yMin - node.RangeRect.size.y);
        }


        var partentSize = node.RangeRect.size * 2;
        RectFloat partentRect = new RectFloat(partentPos, partentSize);
        var partentNode = CreateNode(partentRect);


        partentNode.SetChildNode(node);

        //UnityEngine.Debug.Log($"扩展 Parent {partentNode.RangeRect}");
        return ExtendNode(partentNode, pos);
    }

    /// <summary>
    /// 动态的四叉树, 初始需要给单个最小节点的Size
    /// 输入 最小宽高来初始化
    /// </summary>
    /// <param name="minimumRange"></param>
    public QuadTree(float minimumRange)
    {
        MinimumRange = new Float2(minimumRange, minimumRange);
        dataMap = new Dictionary<DataType, QuadTreeNode>();
    }

    /// <summary>
    /// 通过Pos获取到这个节点的叶子坐标路径
    /// 这个方法不会添加节点
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool FindNodePathByPos(Float2 pos, ref List<QuadTreeNode> outList)
    {
        if (outList == null) outList = new List<QuadTreeNode>();
        if (!Root.IsInRange(pos)) return false;
        outList.AddRange(Root.FindNodePath(pos));

        return true;
    }
    private bool FindNodePathByCircle(Float2 pos, float radius, ref List<QuadTreeNode> outList)
    {
        if (outList == null) outList = new List<QuadTreeNode>();
        if (Root == null) return false;
        Root.FindNodePath(pos, radius, ref outList);
        return true;
    }
    private bool FindNodePathByRect(RectFloat rect, ref List<QuadTreeNode> outList)
    {
        if (outList == null) outList = new List<QuadTreeNode>();

        if (Root == null) return false;
        Root.FindNodePath(rect, ref outList);
        return true;
    }

    private List<DataType> FindNodeDataByPos(Float2 pos, QuadTreeNode.Condition condition)
    {
        var ls = new List<DataType>();
        Root.FindNodePathData(pos, ref ls, true, condition);

        return ls;
    }



    public class QuadTreeNode
    {
        public delegate bool Condition(DataType data, Float2 pos);

        public QuadTree<DataType> RootTree;

        public QuadTreeNode Parent;

        public RectFloat RangeRect;

        public Dictionary<DataType, Float2> Datas;

        public QuadTreeNode[] Children;

        public object param;

        public bool IsMinSize => RangeRect.size == RootTree.MinimumRange;

        /// <summary>
        /// 有数据 或 子节点 不为空
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (Datas.Count > 0) return false;
                for (int i = 0, j = Children.Length; i < j; ++i)
                {
                    var child = Children[i];
                    if (child != null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private Stack<Queue<QuadTreeNode>> quadTreeNodesStack = new Stack<Queue<QuadTreeNode>>();

        /// <summary>
        /// 清除 所有子节点
        /// </summary>
        public void ClearAll()
        {
            ClearChild();
            ClearData();
        }
        public void ClearChild()
        {
            for (int i = 0, j = Children.Length; i < j; ++i)
            {
                var child = Children[i];
                if (child != null)
                {
                    child.ClearAll();

                    Children[i] = null;
                }

            }
        }
        public void ClearData()
        {
            Datas.Clear();
        }

        public void Set(RectFloat rect, QuadTree<DataType> treeRoot)
        {
            RangeRect = rect;
            RootTree = treeRoot;
        }
        public void ForEach_Depth(Action<DataType, Float2> action)
        {

            var datas = Datas;
            if (datas != null)
            {
                foreach (var kvp in Datas)
                {
                    action?.Invoke(kvp.Key, kvp.Value);
                }
            }

            if (Children == null) return;
            for (int i = 0, j = Children.Length; i < j; ++i)
            {
                var child = Children[i];
                if (child == null) continue;
                child.ForEach_Depth(action);
            }
        }

        public void ForEach_Depth(Action<QuadTreeNode> action)
        {
            action?.Invoke(this);
            if (Children == null) return;
            for (int i = 0, j = Children.Length; i < j; ++i)
            {
                var child = Children[i];
                if (child == null) continue;
                child.ForEach_Depth(action);
            }
        }

        public void ForEach_Breadth(Action<DataType, Float2> action)
        {
            Queue<KeyValuePair<DataType, Float2>> queue = new Queue<KeyValuePair<DataType, Float2>>();
            if (Datas != null && Datas.Count != 0)
            {
                foreach (var kvp in Datas)
                    queue.Enqueue(kvp);
            }

            _ForEach_Breadth(this, ref queue);

            while (queue.Count > 0)
            {
                var temp = queue.Dequeue();
                action?.Invoke(temp.Key, temp.Value);
            }
        }

        private void _ForEach_Breadth(QuadTreeNode node, ref Queue<KeyValuePair<DataType, Float2>> queue)
        {
            if (node.Children == null) return;
            //Queue<QuadTreeNode> childQueue = PopQuadTreeNodeQueue();//  new Queue<QuadTreeNode>();
            int childCount = node.Children.Length;
            for (int i = 0; i < childCount; ++i)
            {
                var child = node.Children[i];
                if (child == null || child.Datas == null || child.Datas.Count == 0) continue;
                var datas = child.Datas;
                foreach (var data in datas)
                    queue.Enqueue(data);
            }
            for (int i = 0; i < childCount; ++i)
            {
                var child = node.Children[i];
                if (child == null || child.Datas == null || child.Datas.Count == 0) continue;

                _ForEach_Breadth(child, ref queue);
            }
        }

        private void _ForEach_Breadth(QuadTreeNode node, ref Queue<QuadTreeNode> queue)
        {
            if (node.Children == null) return;
            int childCount = node.Children.Length;
            for (int i = 0; i < childCount; ++i)
            {
                var child = node.Children[i];
                if (child == null) continue;
                queue.Enqueue(child);
            }
            for (int i = 0; i < childCount; ++i)
            {
                var child = node.Children[i];
                if (child == null) continue;

                _ForEach_Breadth(child, ref queue);
            }
        }
        public void ForEach_Breadth(Action<QuadTreeNode> action)
        {
            Queue<QuadTreeNode> queue = PopQuadTreeNodeQueue();
            queue.Enqueue(this);


            _ForEach_Breadth(this, ref queue);

            while (queue.Count > 0)
            {
                var temp = queue.Dequeue();
                action?.Invoke(temp);
            }
            BackQuadTreeNodeQueue(queue);
        }

        public QuadTreeNode AddData(Float2 pos, DataType data)
        {
            if (IsMinSize)
            {
                SetData(pos, data);
                return this;
            }
            else
            {
                var temp = FindLeafNodeByChildren(pos);
                temp.SetData(pos, data);
                return temp;
            }
        }

        public void SetData(Float2 pos, DataType data)
        {
            if (!Datas.ContainsKey(data))
            {
                Datas.Add(data, pos);
            }
            else
                Datas[data] = pos;
        }

        public bool RemoveData(DataType data)
        {
            if (Datas.ContainsKey(data)) return Datas.Remove(data);
            return false;
        }

        public void RemoveChildNode(QuadTreeNode childNode)
        {
            for (int i = 0, j = Children.Length; i < j; ++i)
            {
                var child = Children[i];
                if (childNode == child)
                {
                    Children[i] = null;
                    childNode.ClearAll();
                    RootTree.DeathNodeEnqueue(childNode);
                    break;
                }
            }
            RootTree.CheckDeath(this);
        }

        /// <summary>
        /// 通过Pos获取叶节点 这个方法会填充空的节点 直到节点达到最小尺寸
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public QuadTreeNode FindLeafNodeByChildren(Float2 pos)
        {
            if (IsMinSize) return this;
            var eDir = GetDirectionEnum(pos);
            var temp = Children[(int)eDir];
            if (temp == null)
            {
                //补充叶节点
                var childPos = Float2.zero;
                if (eDir == DirectionEnmu.LeftTop)
                {
                    childPos = new Float2(RangeRect.xMin, RangeRect.center.y);
                }
                else if (eDir == DirectionEnmu.RightTop)
                {
                    childPos = new Float2(RangeRect.center.x, RangeRect.center.y);
                }
                else if (eDir == DirectionEnmu.LeftBottom)
                {
                    childPos = new Float2(RangeRect.xMin, RangeRect.yMin);
                }
                else
                {
                    childPos = new Float2(RangeRect.center.x, RangeRect.yMin);
                }
                var childSize = RangeRect.size / 2;
                RectFloat childRect = new RectFloat(childPos, childSize);
                var childNode = RootTree.CreateNode(childRect);
                SetChildNode(childNode);
                temp = childNode;
                //UnityEngine.Debug.Log($"扩展 Child {childNode.RangeRect}");
            }
            return temp.FindLeafNodeByChildren(pos);
        }
        /// <summary>
        /// 获取节点的路径
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="containsSelf"></param>
        /// <returns></returns>
        public List<QuadTreeNode> FindNodePath(Float2 pos, bool containsSelf = true)
        {
            var ls = new List<QuadTreeNode>();
            _FindNodePath(this, pos, ref ls, containsSelf);
            return ls;
        }
        public bool FindNodePath(Float2 pos, float radius, ref List<QuadTreeNode> outList, bool containsSelf = true)
        {
            if (outList == null) outList = new List<QuadTreeNode>();

            if (CheckCircleIntersectsRectangle(RangeRect, pos, radius))
            {
                Queue<QuadTreeNode> queue = PopQuadTreeNodeQueue();
                if (containsSelf)
                    queue.Enqueue(this);

                _FindNodePath(this, pos, radius, ref queue);

                while (queue.Count > 0)
                {
                    outList.Add(queue.Dequeue());
                }
                BackQuadTreeNodeQueue(queue);
            }

            return outList.Count > 0;
        }

        public bool FindNodePath(RectFloat rect, ref List<QuadTreeNode> outList, bool contaninsSelf = true)
        {
            if (outList == null) outList = new List<QuadTreeNode>();
            //var ls = new List<QuadTreeNode>();
            if (RangeRect.Overlaps(rect))
            {
                Queue<QuadTreeNode> queue = new Queue<QuadTreeNode>();
                if (contaninsSelf)
                    queue.Enqueue(this);

                _FindNodePath(this, rect, ref queue);

                while (queue.Count > 0)
                {
                    outList.Add(queue.Dequeue());
                }
            }

            return outList.Count > 0;
        }
        private void _FindNodePath(QuadTreeNode node, RectFloat rect, ref Queue<QuadTreeNode> queue)
        {
            int childCount = node.Children.Length;
            Queue<QuadTreeNode> tempQue = PopQuadTreeNodeQueue();
            for (int i = 0; i < childCount; ++i)
            {
                var child = node.Children[i];
                if (child != null && child.RangeRect.Overlaps(rect))
                {
                    queue.Enqueue(child);
                    tempQue.Enqueue(child);
                }
            }

            while (tempQue.Count > 0)
            {
                var cNode = tempQue.Dequeue();
                _FindNodePath(cNode, rect, ref queue);
            }
            BackQuadTreeNodeQueue(tempQue);
        }
        private void _FindNodePath(QuadTreeNode node, Float2 pos, float radius, ref Queue<QuadTreeNode> queue)
        {

            int childCount = node.Children.Length;
            Queue<QuadTreeNode> tempQue = PopQuadTreeNodeQueue();
            for (int i = 0; i < childCount; ++i)
            {
                var child = node.Children[i];
                if (child != null && CheckCircleIntersectsRectangle(child.RangeRect, pos, radius))
                {
                    queue.Enqueue(child);
                    tempQue.Enqueue(child);
                }
            }

            while (tempQue.Count > 0)
            {
                var cNode = tempQue.Dequeue();
                _FindNodePath(cNode, pos, radius, ref queue);
            }
            BackQuadTreeNodeQueue(tempQue);
        }
        /// <summary>
        /// 获取节点路径 这个方法不会填充空节点
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="outList"></param>
        /// <param name="containsSelf"></param>
        private void _FindNodePath(QuadTreeNode node, Float2 pos, ref List<QuadTreeNode> outList, bool containsSelf = true)
        {
            if (outList == null) outList = new List<QuadTreeNode>();
            if (containsSelf)
                outList.Add(this);

            var dirNode = node[node.GetDirectionEnum(pos)];
            if (dirNode == null) return;
            else
                outList.Add(dirNode);

            _FindNodePath(dirNode, pos, ref outList, false);
        }

        public void FindNodePathData(Float2 pos, ref List<DataType> outList, bool containsSelf = true, Condition condition = null)
        {
            if (outList == null) outList = new List<DataType>();

            if (containsSelf)
                _CheckConditionToList(ref outList, Datas.Keys, condition);

            var childNode = this[GetDirectionEnum(pos)];
            if (childNode == null) return;
            else
                _CheckConditionToList(ref outList, childNode.Datas.Keys, condition);

            FindNodePathData(pos, ref outList, false);
        }

        private void _CheckConditionToList(ref List<DataType> inLs, ICollection<DataType> datas, Condition condition)
        {
            if (datas == null || datas.Count == 0) return;
            if (condition == null)
                inLs.AddRange(datas);
            else
            {
                foreach (var data in datas)
                {
                    if (condition.Invoke(data, Datas[data]))
                        inLs.Add(data);
                }
            }
        }
        private bool CheckCircleIntersectsRectangle(RectFloat rectFloat, Float2 cPos, float cRadius)
        {
            return CheckCircleIntersectsRectangle(rectFloat.xMin, rectFloat.yMin, rectFloat.xMax, rectFloat.yMax, cPos.x, cPos.y, cRadius);
        }
        private bool CheckCircleIntersectsRectangle(float rectMinX, float rectMinY, float rectMaxX, float rectMaxY, float cX, float cY, float cRadius)
        {
            float mX = Math.Min(Math.Abs(rectMinX - cX), Math.Abs(rectMaxX - cX));
            float mY = Math.Min(Math.Abs(rectMinY - cY), Math.Abs(rectMaxY - cY));

            if (mX * mX + mY * mY < cRadius * cRadius) return true;

            float x0 = (rectMinX + rectMaxX) / 2;
            float y0 = (rectMinY + rectMaxY) / 2;
            if ((Math.Abs(x0 - cX) < Math.Abs(rectMaxX - rectMinX) / 2 + cRadius)
                && Math.Abs(cY - y0) < Math.Abs(rectMaxY - rectMinY) / 2)
                return true;
            if (Math.Abs(y0 - cY) < Math.Abs(rectMaxY - rectMinY) / 2 + cRadius
                && Math.Abs(cX - x0) < Math.Abs(rectMaxX - rectMinX) / 2)
                return true;

            return false;
        }
        public void SetChildNode(QuadTreeNode node)
        {
            var dir = GetDirectionEnum(node.RangeRect.center);
            Children[(int)dir] = node;
            node.Parent = this;
        }
        private Queue<QuadTreeNode> PopQuadTreeNodeQueue()
        {
            if (quadTreeNodesStack.Count > 0)
                return quadTreeNodesStack.Pop();
            else
            {
                return new Queue<QuadTreeNode>();
            }
        }
        private void BackQuadTreeNodeQueue(Queue<QuadTreeNode> node)
        {
            node.Clear();
            quadTreeNodesStack.Push(node);
        }

        public DirectionEnmu GetDirectionEnum(Float2 pos)
        {
            if (pos.x <= RangeRect.center.x)
            {
                if (pos.y < RangeRect.center.y)
                {
                    return DirectionEnmu.LeftBottom;
                }
                else
                {
                    return DirectionEnmu.LeftTop;
                }
            }
            else
            {
                if (pos.y < RangeRect.center.y)
                {
                    return DirectionEnmu.RightBottom;
                }
                else
                {
                    return DirectionEnmu.RightTop;
                }
            }

        }

        /// <summary>
        /// 一个这个点是否在这个范围内
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool IsInRange(Float2 pos)
        {
            return RangeRect.Contains(pos);
        }
        /// <summary>
        ///  是否完全包含输入矩形
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool IsInRange(RectFloat rect)
        {
            //return RangeRect.Contains(new Float2(rect.xMin, rect.yMin))
            //    && RangeRect.Contains(new Float2(rect.xMin, rect.yMax))
            //    && RangeRect.Contains(new Float2(rect.xMax, rect.yMin))
            //    && RangeRect.Contains(new Float2(rect.xMax, rect.yMax));
            return RangeRect.Contains(rect);
        }
        /// <summary>
        ///  是否与输入矩形相交
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool IsIntersect(RectFloat rect)
        {
            //return RangeRect.Contains(new Float2(rect.xMin, rect.yMin))
            //    || RangeRect.Contains(new Float2(rect.xMin, rect.yMax))
            //    || RangeRect.Contains(new Float2(rect.xMax, rect.yMin))
            //    || RangeRect.Contains(new Float2(rect.xMax, rect.yMax));
            return RangeRect.Overlaps(rect);
        }
        public override string ToString()
        {
            return base.ToString();
        }

        public QuadTreeNode this[DirectionEnmu dir] => Children[(int)dir];

        public QuadTreeNode(RectFloat range, QuadTree<DataType> rootTree)
        {
            this.RootTree = rootTree;
            RangeRect = range;
            Datas = new Dictionary<DataType, Float2>();
            Children = new QuadTreeNode[4];

        }
    }

    public enum DirectionEnmu
    {
        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom,
    }


}



public struct CircleFloat
{
    private float m_x;
    private float m_y;
    private float m_radius;

    public float x
    {
        get
        {
            return m_x;
        }
        set
        {
            m_x = value;
        }
    }

    public float y
    {
        get
        {
            return m_y;
        }
        set
        {
            m_y = value;
        }
    }

    public Float2 centerPoints
    {
        get
        {
            return new Float2(x, y);
        }
    }

    public float radius
    {
        get
        {
            return m_radius;
        }
        set
        {
            m_radius = value;
        }
    }

    public CircleFloat(float x, float y, float radius)
    {
        m_x = x;
        m_y = y;
        m_radius = radius;
    }
    public CircleFloat(Float2 pos, float radius)
    {
        m_x = pos.x;
        m_y = pos.y;
        m_radius = radius;
    }

    public bool Overlaps(CircleFloat circleFloat)
    {
        return (centerPoints - circleFloat.centerPoints).sqrMagnitude < (radius + circleFloat.radius) * (radius + circleFloat.radius);

    }

    public bool Contains(Float2 points)
    {
        float disSqua = (points - centerPoints).sqrMagnitude;
        float radiusSqua = radius * radius;
        return disSqua <= radiusSqua;
    }

    public static CircleFloat Create(float x, float y, float radius)
    {
        return new CircleFloat(x, y, radius);
    }
    public static CircleFloat Create(Float2 pos, float radius)
    {
        return new CircleFloat(pos, radius);
    }
}

public struct RectFloat : IEquatable<RectFloat>, IFormattable
{
    private float m_XMin;
    private float m_YMin;
    private float m_Width;
    private float m_Height;

    public float x
    {
        get
        {
            return m_XMin;
        }
        set
        {
            m_XMin = value;
        }
    }

    //
    // 摘要:
    //     The Y coordinate of the rectangle.
    public float y
    {
        get
        {
            return m_YMin;
        }
        set
        {
            m_YMin = value;
        }
    }

    //
    // 摘要:
    //     The X and Y position of the rectangle.
    public Float2 position
    {
        get
        {
            return new Float2(m_XMin, m_YMin);
        }
        set
        {
            m_XMin = value.x;
            m_YMin = value.y;
        }
    }

    //
    // 摘要:
    //     The position of the center of the rectangle.
    public Float2 center
    {
        get
        {
            return new Float2(x + m_Width / 2f, y + m_Height / 2f);
        }
        set
        {
            m_XMin = value.x - m_Width / 2f;
            m_YMin = value.y - m_Height / 2f;
        }
    }

    //
    // 摘要:
    //     The position of the minimum corner of the rectangle.
    public Float2 min
    {
        get
        {
            return new Float2(xMin, yMin);
        }
        set
        {
            xMin = value.x;
            yMin = value.y;
        }
    }

    //
    // 摘要:
    //     The position of the maximum corner of the rectangle.
    public Float2 max
    {
        get
        {
            return new Float2(xMax, yMax);
        }
        set
        {
            xMax = value.x;
            yMax = value.y;
        }
    }

    //
    // 摘要:
    //     The width of the rectangle, measured from the X position.
    public float width
    {
        get
        {
            return m_Width;
        }
        set
        {
            m_Width = value;
        }
    }

    //
    // 摘要:
    //     The height of the rectangle, measured from the Y position.
    public float height
    {
        get
        {
            return m_Height;
        }
        set
        {
            m_Height = value;
        }
    }

    //
    // 摘要:
    //     The width and height of the rectangle.
    public Float2 size
    {
        get
        {
            return new Float2(m_Width, m_Height);
        }
        set
        {
            m_Width = value.x;
            m_Height = value.y;
        }
    }

    //
    // 摘要:
    //     The minimum X coordinate of the rectangle.
    public float xMin
    {
        get
        {
            return m_XMin;
        }
        set
        {
            float xMax = this.xMax;
            m_XMin = value;
            m_Width = xMax - m_XMin;
        }
    }

    //
    // 摘要:
    //     The minimum Y coordinate of the rectangle.
    public float yMin
    {
        get
        {
            return m_YMin;
        }
        set
        {
            float yMax = this.yMax;
            m_YMin = value;
            m_Height = yMax - m_YMin;
        }
    }

    //
    // 摘要:
    //     The maximum X coordinate of the rectangle.
    public float xMax
    {
        get
        {
            return m_Width + m_XMin;
        }
        set
        {
            m_Width = value - m_XMin;
        }
    }

    //
    // 摘要:
    //     The maximum Y coordinate of the rectangle.
    public float yMax
    {
        get
        {
            return m_Height + m_YMin;
        }
        set
        {
            m_Height = value - m_YMin;
        }
    }



    public RectFloat(Float2 pos, Float2 size)
    {
        m_XMin = pos.x;
        m_YMin = pos.y;
        m_Width = size.x;
        m_Height = size.y;
    }
    public RectFloat(float x, float y, float width, float height)
    {
        m_XMin = x;
        m_YMin = y;
        m_Width = width;
        m_Height = height;
    }

    public RectFloat(RectFloat source)
    {
        m_XMin = source.m_XMin;
        m_YMin = source.m_YMin;
        m_Width = source.m_Width;
        m_Height = source.m_Height;
    }

    public bool Contains(Float2 point)
    {
        return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
    }
    /// <summary>
    /// 只判断X 与 Y
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool Contains(Float3 point)
    {
        return point.x >= xMin && point.x < xMax && point.y >= yMin && point.y < yMax;
    }

    public bool Contains(RectFloat rect)
    {
        return Contains(new Float2(rect.xMin, rect.yMin))
            && Contains(new Float2(rect.xMin, rect.yMax))
            && Contains(new Float2(rect.xMax, rect.yMin))
            && Contains(new Float2(rect.xMax, rect.yMax));
    }

    public bool Overlaps(RectFloat other)
    {
        return other.xMax > xMin && other.xMin < xMax && other.yMax > yMin && other.yMin < yMax;
    }

    public bool Overlaps(RectFloat other, bool allowInverse)
    {
        RectFloat rect = this;
        if (allowInverse)
        {
            rect = OrderMinMax(rect);
            other = OrderMinMax(other);
        }

        return rect.Overlaps(other);
    }
    public bool Overlaps(Float2 pos, float radius)
    {
        return CheckCircleIntersectsRectangle(this, pos, radius);
    }

    private static RectFloat OrderMinMax(RectFloat rect)
    {
        if (rect.xMin > rect.xMax)
        {
            float xMin = rect.xMin;
            rect.xMin = rect.xMax;
            rect.xMax = xMin;
        }

        if (rect.yMin > rect.yMax)
        {
            float yMin = rect.yMin;
            rect.yMin = rect.yMax;
            rect.yMax = yMin;
        }

        return rect;
    }

    public bool Equals(RectFloat other)
    {
        return x.Equals(other.x) && y.Equals(other.y) && width.Equals(other.width) && height.Equals(other.height);
    }
    public override bool Equals(object other)
    {
        if (!(other is RectFloat))
        {
            return false;
        }

        return Equals((RectFloat)other);
    }
    public override string ToString()
    {
        return ToString(null);
    }
    public string ToString(string format)
    {
        return ToString(format, CultureInfo.InvariantCulture.NumberFormat);
    }
    public string ToString(string format, IFormatProvider formatProvider)
    {
        if (string.IsNullOrEmpty(format))
        {
            format = "F2";
        }

        return String.Format("(x:{0}, y:{1}, width:{2}, height:{3})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), width.ToString(format, formatProvider), height.ToString(format, formatProvider));
    }
    public override int GetHashCode()
    {
        return x.GetHashCode() ^ (width.GetHashCode() << 2) ^ (y.GetHashCode() >> 2) ^ (height.GetHashCode() >> 1);
    }
    public static RectFloat zero = new RectFloat(0f, 0f, 0f, 0f);

    private bool CheckCircleIntersectsRectangle(RectFloat rectFloat, Float2 cPos, float cRadius)
    {
        return CheckCircleIntersectsRectangle(rectFloat.xMin, rectFloat.yMin, rectFloat.xMax, rectFloat.yMax, cPos.x, cPos.y, cRadius);
    }
    private bool CheckCircleIntersectsRectangle(float rectMinX, float rectMinY, float rectMaxX, float rectMaxY, float cX, float cY, float cRadius)
    {
        float mX = Math.Min(Math.Abs(rectMinX - cX), Math.Abs(rectMaxX - cX));
        float mY = Math.Min(Math.Abs(rectMinY - cY), Math.Abs(rectMaxY - cY));

        if (mX * mX + mY * mY < cRadius * cRadius) return true;

        float x0 = (rectMinX + rectMaxX) / 2;
        float y0 = (rectMinY + rectMaxY) / 2;
        if ((Math.Abs(x0 - cX) < Math.Abs(rectMaxX - rectMinX) / 2 + cRadius)
            && Math.Abs(cY - y0) < Math.Abs(rectMaxY - rectMinY) / 2)
            return true;
        if (Math.Abs(y0 - cY) < Math.Abs(rectMaxY - rectMinY) / 2 + cRadius
            && Math.Abs(cX - x0) < Math.Abs(rectMaxX - rectMinX) / 2)
            return true;

        return false;
    }
    public static bool operator !=(RectFloat lhs, RectFloat rhs)
    {
        return !(lhs == rhs);
    }
    public static bool operator ==(RectFloat lhs, RectFloat rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width && lhs.height == rhs.height;
    }

    public static RectFloat CreateRectFromCenter(Float2 centerPos, Float2 size)
    {
        RectFloat result = new RectFloat(centerPos.x - size.x / 2, centerPos.y - size.y / 2, size.x, size.y);
        return result;
    }
}

public struct Float2 : IEquatable<Float2>, IFormattable
{
    public float x;
    public float y;

    public Float2 normalized
    {
        get
        {
            Float2 result = new Float2(x, y);
            result.Normalize();
            return result;
        }
    }
    public float sqrMagnitude => x * x + y * y;
    public float magnitude => (float)Math.Sqrt(x * x + y * y);
    private static readonly Float2 zeroFloat2 = new Float2(0f, 0f);
    private static readonly Float2 upFloat2 = new Float2(0f, 1f);
    private static readonly Float2 downFloat2 = new Float2(0f, -1f);
    private static readonly Float2 leftFloat2 = new Float2(-1f, 0f);
    private static readonly Float2 rightFloat2 = new Float2(-1f, 0f);
    private static readonly Float2 oneFloat2 = new Float2(1f, 1f);
    private static readonly Float2 twoFloat2 = new Float2(2f, 2f);
    public static Float2 zero => zeroFloat2;
    public static Float2 up => upFloat2;
    public static Float2 down => downFloat2;
    public static Float2 left => leftFloat2;
    public static Float2 right => rightFloat2;
    public static Float2 one => oneFloat2;
    public static Float2 two => twoFloat2;
    public bool Equals(Float2 other)
    {
        return x == other.x && y == other.y;
    }
    public string ToString(string format)
    {
        return ToString(format, CultureInfo.InvariantCulture.NumberFormat);
    }
    public string ToString(string format, IFormatProvider formatProvider)
    {
        return $"Float2 : ({x}),({y})";
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Float2 lhs, Float2 rhs)
    {
        float num = lhs.x - rhs.x;
        float num2 = lhs.y - rhs.y;
        return num * num + num2 * num2 < 9.99999944E-11f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Float2 lhs, Float2 rhs)
    {
        return !(lhs == rhs);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(float newX, float newY)
    {
        x = newX;
        y = newY;
    }

    public static float Distance(Float2 a, Float2 b)
    {
        float num = a.x - b.x;
        float num2 = a.y - b.y;
        return (float)Math.Sqrt(num * num + num2 * num2);
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ (y.GetHashCode() << 2);
    }

    public override bool Equals(object other)
    {
        if (!(other is Float2))
        {
            return false;
        }

        return Equals((Float2)other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator +(Float2 a, Float2 b)
    {
        return new Float2(a.x + b.x, a.y + b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator -(Float2 a, Float2 b)
    {
        return new Float2(a.x - b.x, a.y - b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator *(Float2 a, Float2 b)
    {
        return new Float2(a.x * b.x, a.y * b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator /(Float2 a, Float2 b)
    {
        return new Float2(a.x / b.x, a.y / b.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator -(Float2 a)
    {
        return new Float2(0f - a.x, 0f - a.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator *(Float2 a, float d)
    {
        return new Float2(a.x * d, a.y * d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator *(float d, Float2 a)
    {
        return new Float2(a.x * d, a.y * d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float2 operator /(Float2 a, float d)
    {
        return new Float2(a.x / d, a.y / d);
    }

    public void Normalize()
    {
        float magnitude = this.magnitude;
        if (magnitude > 1E-05f)
        {
            this /= magnitude;
        }
        else
        {
            this = zero;
        }
    }

    public Float2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}


public struct Float3
{
    public float x;
    public float y;
    public float z;

    public UnityEngine.Vector3 ToVector3()
    {
        return new UnityEngine.Vector3(x, y, z);
    }

    public Float3(UnityEngine.Vector3 vec3)
    {
        this.x = vec3.x;
        this.y = vec3.y;
        this.z = vec3.z;
    }
    public Float3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public Float3(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Float3 zero = new Float3(0, 0, 0);
}
public struct Int3
{
    public int x;
    public int y;
    public int z;

    public Int3(float x, float y, float z)
    {
        this.x = (int)x;
        this.y = (int)y;
        this.z = (int)z;
    }
    public Int3(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public Int3(Vector3 vec3)
    {
        this.x = (int)vec3.x;
        this.y = (int)vec3.y;
        this.z = (int)vec3.z;
    }
    public Int3(Vector3Int vec3)
    {
        this.x = vec3.x;
        this.y = vec3.y;
        this.z = vec3.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector3 ToVector3Int()
    {
        return new Vector3Int(x, y, z);
    }
}

public struct Int2
{
    public int x;
    public int y;

    public Int2(float x, float y)
    {
        this.x = (int)x;
        this.y = (int)y;
    }
    public Int2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public Int2(Vector2 vec2)
    {
        this.x = (int)vec2.x;
        this.y = (int)vec2.y;
    }
    public Int2(Vector2Int vec2)
    {
        this.x = vec2.x;
        this.y = vec2.y;
    }

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }

    public Vector2 ToVector2Int()
    {
        return new Vector2Int(x, y);
    }
}

