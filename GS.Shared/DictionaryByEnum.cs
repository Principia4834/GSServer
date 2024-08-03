using ASCOM.DeviceInterface;
using GS.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GS.Shared
{
    /// <summary>A dictionary keyed by an Enum</summary>
    /// <typeparam name="T">Type stored in array</typeparam>
    /// <typeparam name="TU">Indexer Enum type</typeparam>
    public class DictionaryByEnum<T, TU> : Dictionary<TU, T> where TU : Enum // requires C# 7.3 or later
    {
    }
}