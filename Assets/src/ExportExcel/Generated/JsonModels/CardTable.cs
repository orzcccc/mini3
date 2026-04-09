using System;
using System.Collections.Generic;

namespace Mini3.ExcelJson
{
    [Serializable]
    public class CardTable
    {
        public string tableName;
        public List<CardRow> rows = new List<CardRow>();
    }
}
