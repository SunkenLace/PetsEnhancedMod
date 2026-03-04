using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using System.Text.Json;
using System.IO.Compression;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Buffers;
using static Pets_Enhanced_Mod.ModEntry;
using Pets_Enhanced_Mod.Utilities.Custom_Classes;

namespace Pets_Enhanced_Mod.Utilities
{
    public static class CacheReciclerHelper
    {
        //------------------------------------------------------------------------------------------------------------ | Area for debugging | ------------------------------------------------------------------------------------------------------------


        private static readonly Stopwatch _stopwatch = new();
        private static readonly Queue<double> _stopwatchMeasurements = new();

        private static long AllocatedMemoryOld = 0;
        public static void GetAllocatedMemoryStart() => AllocatedMemoryOld = GC.GetAllocatedBytesForCurrentThread();
        public static long GetAllocatedMemoryEnd(bool _callWriteMonitor, bool _ignoreZero)
        {
            long allocatedMemoryTotal = GC.GetAllocatedBytesForCurrentThread() - AllocatedMemoryOld;
            if (_callWriteMonitor && ((_ignoreZero && allocatedMemoryTotal != 0) || !_ignoreZero))
            {
                ModEntry.WriteMonitor($"Memory allocated by code: {allocatedMemoryTotal:N0} bytes", LogLevel.Warn);
            }
            return allocatedMemoryTotal;
        }

        public static void GetMeasuredPerformanceStart()
        {
            AllocatedMemoryOld = GC.GetAllocatedBytesForCurrentThread(); _stopwatch.Restart();
        }
        public static long GetMeasuredPerformanceEnd(bool _callWriteMonitor, bool _ignoreZero)
        {
            _stopwatch.Stop();
            long allocatedMemoryTotal = GC.GetAllocatedBytesForCurrentThread() - AllocatedMemoryOld;
            double elapsedSeconds = (double)_stopwatch.ElapsedTicks / Stopwatch.Frequency;
            _stopwatchMeasurements.Enqueue(elapsedSeconds);
            if (_stopwatchMeasurements.Count > 70)
            {
                _stopwatchMeasurements.Dequeue();
            }
            if (_callWriteMonitor && ((_ignoreZero && allocatedMemoryTotal != 0 && elapsedSeconds != 0) || !_ignoreZero))
            {
                ModEntry.WriteMonitor($"Memory allocated by code: {allocatedMemoryTotal:N0} bytes | Code execution time: {_stopwatchMeasurements.Average():F6} seconds", LogLevel.Warn);
            }
            return allocatedMemoryTotal;
        }


        //------------------------------------------------------------------------------------------------------------ | List Pool for recycling | ------------------------------------------------------------------------------------------------------------

        private static readonly Queue<List<StardewValley.Characters.Pet>> ListPet_pool = new(2);
        private static readonly Queue<List<Point>> ListPoint_pool = new(2);
        private static readonly Queue<Stack<Point>> StackPoint_pool = new(2);
        private static readonly Queue<List<long>> ListLong_pool = new(2);
        private static readonly Queue<HashSet<(int,int)>> ListHashSetTuppleIntInt_pool = new(2);

        public static void InitializePool()
        {
            ListPet_pool.Enqueue(new(20));
            ListPoint_pool.Enqueue(new(100));
            StackPoint_pool.Enqueue(new(100));
            ListLong_pool.Enqueue(new(10));
            ListHashSetTuppleIntInt_pool.Enqueue(new(100));
        }
        public static void ClearPool()
        {
            ListPet_pool.Clear();
            ListPoint_pool.Clear();
            StackPoint_pool.Clear();
            ListLong_pool.Clear();
            ListHashSetTuppleIntInt_pool.Clear();
        }

        //------------------------------------------------------------------------------------------------------------ | Area for renting Lists | ------------------------------------------------------------------------------------------------------------

        public static List<StardewValley.Characters.Pet> RentReciclablePetList()
        {
            if (ListPet_pool.TryDequeue(out var _dic))
            {
                _dic.Clear();
                return _dic;
            }
            return new(20);
        }
        public static List<Point> RentListPoint()
        {
            if (ListPoint_pool.TryDequeue(out var _list))
            {
                _list.Clear();
                return _list;
            }
            return new(30);
        }
        public static Stack<Point> RentStackPoint()
        {
            if (StackPoint_pool.TryDequeue(out var _list))
            {
                _list.Clear();
                return _list;
            }
            return new(30);
        }
        public static List<long> RentLong()
        {
            if (ListLong_pool.TryDequeue(out var _list))
            {
                _list.Clear();
                return _list;
            }
            return new(30);
        }
        public static HashSet<(int, int)> RentHSTIntInt()
        {
            if (ListHashSetTuppleIntInt_pool.TryDequeue(out var _list))
            {
                _list.Clear();
                return _list;
            }
            return new(100);
        }


        //------------------------------------------------------------------------------------------------------------ | Area for returning Lists | ------------------------------------------------------------------------------------------------------------

        public static void Return(List<StardewValley.Characters.Pet> _list)
        {
            if (_list is null) { return; }
            _list.Clear();
            ListPet_pool.Enqueue(_list);
        }
        public static void Return(List<Point> _list)
        {
            if (_list == null) { return; }
            _list.Clear();
            ListPoint_pool.Enqueue(_list);
        }
        public static void Return(Stack<Point> _list)
        {
            if (_list == null) { return; }
            _list.Clear();
            StackPoint_pool.Enqueue(_list);
        }
        public static void Return(List<long> _list)
        {
            if (_list == null) { return; }
            _list.Clear();
            ListLong_pool.Enqueue(_list);
        }
        public static void Return(HashSet<(int, int)> _list)
        {
            if (_list == null) { return; }
            _list.Clear();
            ListHashSetTuppleIntInt_pool.Enqueue(_list);
        }

        //------------------------------------------------------------------------------------------------------------ | String caching area | ------------------------------------------------------------------------------------------------------------

        private static readonly Dictionary<(string, string), string> ConcatenatedStringRegister = new();

        public static string ConcatenateStringEfficient(string a, string b)
        {
            var key = (a, b);
            if (!ConcatenatedStringRegister.ContainsKey(key))
            {
                ConcatenatedStringRegister.TryAdd(key, a + b);
            }
            return ConcatenatedStringRegister[key];
        }
        public static void ClearConcatenatedStringRegister() => ConcatenatedStringRegister.Clear();


    }
}
