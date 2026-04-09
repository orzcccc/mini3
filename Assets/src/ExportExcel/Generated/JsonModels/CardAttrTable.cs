using System;
using System.Collections.Generic;

namespace Mini3.ExcelJson
{
    [Serializable]
    public class CardAttrTable
    {
        public string tableName;
        public List<CardAttrRow> rows = new List<CardAttrRow>();
    }
}
