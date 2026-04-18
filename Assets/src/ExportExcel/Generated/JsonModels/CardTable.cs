using System;
using System.Collections.Generic;

[Serializable]
public class CardTable
{
    public string tableName;
    public List<CardRow> rows = new List<CardRow>();
}
