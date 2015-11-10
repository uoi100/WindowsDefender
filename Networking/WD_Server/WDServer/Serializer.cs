using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace WDServer
{
    class Serializer
    {
        /// <summary>
        /// Converts object to byte[]
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Serialize(Instruction obj)
        {
            return Encoding.Default.GetBytes(JsonConvert.SerializeObject(obj));
        }

        /// <summary>
        /// Concerts byte[] to object
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static Instruction DeSerialize(byte[] arr)
        {
            return JsonConvert.DeserializeObject<Instruction>(Encoding.Default.GetString(arr));
        }
    }
}

