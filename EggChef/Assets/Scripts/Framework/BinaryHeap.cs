using System;
using System.Collections.Generic;

namespace Framework
{
    /// <summary>
    /// 二叉最小堆（binary min-heap）：O(log n) 里拿到当前最小的那个。
    ///
    /// 内部就是两个 List 并排，下标一一对应：
    ///   keys  : 优先级
    ///   items : 数据
    /// 并且满足堆序（heap property）：每个节点的 key 都不大于它两个孩子的 key。
    /// 父子下标关系：i 的孩子是 2i+1 和 2i+2，i 的父是 (i-1)/2。
    ///
    /// 为什么不直接 List.Sort 再取第一个？
    /// 排序是 O(n log n)，而且每进一个新数据就得重排；
    /// 堆只在「插进去」和「拿出来」时各做一次 O(log n) 的调整，边找边插的场景才划算（比如 A* 的 open 表）。
    ///
    /// 这里只做最小堆：Push 进来的 key 越小，越先 Pop 出去。
    /// </summary>
    public class BinaryHeap<T>
    {
        private readonly List<float> keys = new List<float>();
        private readonly List<T> items = new List<T>();

        public int Count => items.Count;

        public void Push(float key, T item)
        {
            keys.Add(key);
            items.Add(item);

            //新元素先扔到末尾，然后一直跟父节点比，比父小就换上去（上浮 sift-up）
            int i = items.Count - 1;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (keys[parent] <= keys[i]) 
                    break;
                Swap(parent, i);
                i = parent;
            }
        }

        public T Pop()
        {
            if (items.Count == 0)
                throw new InvalidOperationException("BinaryHeap 已经空了，不能再 Pop()");

            T top = items[0];

            //把末尾那个元素搬到堆顶，然后一路跟较小的孩子比，比孩子大就换下去（下沉 sift-down）
            int last = items.Count - 1;
            keys[0] = keys[last];
            items[0] = items[last];
            keys.RemoveAt(last);
            items.RemoveAt(last);

            int i = 0;
            while (true)
            {
                int left = i * 2 + 1;
                int right = left + 1;
                int small = i;

                if (left < items.Count && keys[left] < keys[small]) small = left;
                if (right < items.Count && keys[right] < keys[small]) small = right;
                if (small == i) 
                    break;

                Swap(small, i);
                i = small;
            }

            return top;
        }

        private void Swap(int a, int b)
        {
            (keys[a], keys[b]) = (keys[b], keys[a]);

            (items[a], items[b]) = (items[b], items[a]);
        }
    }
}
