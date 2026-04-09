using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityGameFramework.Runtime;

namespace Mini3.DataTables
{
    public class DRCard : DataRowBase
    {
        private int m_Id;
        private string m_Name;
        private int m_Quality;

        public override int Id => m_Id;
        public string Name => m_Name;
        public int Quality => m_Quality;

        public override bool ParseDataRow(string dataRowString, object userData)
        {
            string[] columns = dataRowString.Split('\t');
            int index = 0;
            m_Id = ParseInt(columns[index++]);
            m_Name = ParseString(columns[index++]);
            m_Quality = ParseInt(columns[index++]);
            return true;
        }

        public override bool ParseDataRow(byte[] dataRowBytes, int startIndex, int length, object userData)
        {
            using (MemoryStream stream = new MemoryStream(dataRowBytes, startIndex, length, false))
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
            {
                m_Id = reader.ReadInt32();
                m_Name = reader.ReadString();
                m_Quality = reader.ReadInt32();

                return true;
            }
        }

        private static int ParseInt(string value) => int.Parse(value, CultureInfo.InvariantCulture);
        private static long ParseLong(string value) => long.Parse(value, CultureInfo.InvariantCulture);
        private static float ParseFloat(string value) => float.Parse(value, CultureInfo.InvariantCulture);
        private static double ParseDouble(string value) => double.Parse(value, CultureInfo.InvariantCulture);
        private static bool ParseBool(string value) => value == "1" || bool.Parse(value);
        private static string ParseString(string value) => value;

    }
}
