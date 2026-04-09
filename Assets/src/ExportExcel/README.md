# Excel 导表说明

本目录用于存放策划原始 Excel 和导出产物。
当前规则为：一个 Excel 文件中的每个 Sheet 都会导出成一张独立的表。

## 目录约定

- `Excels/`：策划维护的 `.xlsx` 表，一个文件可包含多张 Sheet
- `Generated/DataTables/`：自动生成的 `DRxxx.cs`
- `Generated/JsonModels/`：自动生成的 JSON DTO
- `Assets/Resources/DataTables/Bytes/`：导出的 `.bytes`
- `Assets/Resources/DataTables/Json/`：导出的 `.json`

## 表头规范

每张表固定使用前 3 行作为表头：

1. 第 1 行：字段名
2. 第 2 行：字段类型
3. 第 3 行：字段说明
4. 第 4 行开始：数据

示例：

| id | name | quality | rewards | tags |
| --- | --- | --- | --- | --- |
| int | string | int | list<int> | list<string> |
| 主键 | 名称 | 品质 | 奖励列表 | 标签 |
| 1001 | 木剑 | 1 | 2001,2002 | weapon,starter |

## 当前支持类型

- `int`
- `long`
- `float`
- `double`
- `bool`
- `string`
- `list<int>`
- `list<long>`
- `list<float>`
- `list<double>`
- `list<bool>`
- `list<string>`

## 规则说明

- 第一列必须是 `id`
- 第一列类型必须是 `int`
- 每个 Sheet 都会被导出成一张表
- 表名优先取 Sheet 名字，取不到时回退到 Excel 文件名
- 字段名必须是合法 C# 标识符
- 以 `#` 开头的列会被忽略
- `list<T>` 使用英文逗号分隔
- 空白数据行会被跳过

## 使用方式

1. 把 `.xlsx` 放到 `Excels/`
   每个 Sheet 都会分别生成对应的代码和导出文件
2. 在 Unity 中执行 `Tools/Excel/Create Settings Asset`
3. 再执行 `Tools/Excel/Export All`
4. 默认会导出到 `Assets/Resources/DataTables/Bytes` 和 `Assets/Resources/DataTables/Json`
