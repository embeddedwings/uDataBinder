using System.Collections.Generic;
using UnityEngine;

namespace uDataBinder.Demo
{
    public class DataBindingScene : MonoBehaviour
    {
        public class Obj
        {
            public int i;
            public float f;
            public string s;
        }

        public string member = "member ok!";
        public string property => "property ok!";

        public string[] ary => new string[] { "ary1", "ary2", "ary3", };
        public List<int> list => new() { 10, 20, 30, 40, 50 };
        public List<int> hugeList = new();
        public Dictionary<string, string> dic => new() { { "a", "100" }, { "b", "200" }, { "c", "300" } };
        public List<Obj> objs => new() {
            new Obj { i = 3, f = 5.5f, s = "abc" },
            new Obj { i = 5, f = 1.1f, s = "efg" },
            new Obj { i = 1, f = 3.3f, s = "bcd" },
            new Obj { i = 4, f = 2.2f, s = "def" },
            new Obj { i = 2, f = 4.4f, s = "cde" },
        };

        private int _count = 0;
        public int Count => _count;
        public string count => "count: " + _count;
        public string countNotify => "count (notify): " + _count;

        private int _count2 = 0;
        public int Count2 => _count2;

        private float _prevTime;

        protected void Awake()
        {
            for (var i = 0; i < 1000; ++i)
            {
                hugeList.Add(i);
            }
        }

        protected void OnEnable()
        {
            DataBinding.Set("Test", this);
        }

        protected void OnDisable()
        {
            DataBinding.Unset("Test");
        }

        protected void Update()
        {
            if (Time.time - _prevTime > 1.0f)
            {
                ++_count;
                DataBinding.Notify("Test.countNotify", null, true);
                DataBinding.Notify("Test.Count", null, true);
                _prevTime = Time.time;
            }
        }

        public void AddCount2()
        {
            _count2 += 100;
            DataBinding.Notify("Test.Count2", null, true);
        }

        public void SubCount2()
        {
            _count2 -= 100;
            DataBinding.Notify("Test.Count2", null, true);
        }

        public void ResetCount2()
        {
            _count2 = 0;
            DataBinding.Notify("Test.Count2", null, true);
        }

        protected async void LateUpdate()
        {
            await DataBinding.Execute();
        }
    }
}