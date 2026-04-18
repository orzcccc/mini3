using System;
using System.Collections.Generic;

[Serializable]
public class CardAttrTable
{
    public string tableName;
    public List<CardAttrRow> rows = new List<CardAttrRow>();
}
