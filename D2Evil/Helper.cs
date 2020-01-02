using System.Collections.Generic;
using System.Linq;

namespace D2Evil
{
    public static class Helper
    {
        public static string ReadStringZeroTrim(this L2BinaryReader br)
        {
            var pos = br.BaseStream.Position;
            var length = 0;
            while (br.ReadByte() > 0)
            {
                length++;
            }

            br.BaseStream.Position = pos;
            var str = br.Encoding.GetString(br.ReadBytes(length));
            br.ReadByte(); //skip \0 - fail if end without \0
            return str;
        }

        public static void WriteStringZeroTrim(this L2BinaryWriter bw, string str)
        {
            //bw.Write(str.ToCharArray());
            bw.Write(bw.Encoding.GetBytes(str));
            bw.Write((byte) 0);
        }

        /// <summary>
        /// If 2 lists have same elements (don't care order)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        /// <returns></returns>
        public static bool SameAs<T>(this IList<T> listA, IList<T> listB)
        {
            return listB.Intersect(listA).Count() == listB.Count;
        }

        /// <summary>
        /// Does a list contain all values of another list?
        /// </summary>
        /// <remarks>Needs .NET 3.5 or greater.  Source:  https://stackoverflow.com/a/1520664/1037948 </remarks>
        /// <typeparam name="T">list value type</typeparam>
        /// <param name="containingList">the larger list we're checking in</param>
        /// <param name="lookupList">the list to look for in the containing list</param>
        /// <returns>true if it has everything</returns>
        public static bool ContainsAll<T>(this IEnumerable<T> containingList, IEnumerable<T> lookupList)
        {
            return !lookupList.Except(containingList).Any();
        }

        /// <summary>
        /// Does a list contain any values of another list?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="haystack"></param>
        /// <param name="needle"></param>
        /// <returns></returns>
        public static bool ContainsAny<T>(this IEnumerable<T> haystack, IEnumerable<T> needle)
        {
            return haystack.Intersect(needle).Any();
        }
    }
}