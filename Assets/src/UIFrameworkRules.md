# Mini3 UI 框架规则

## 1. 总体目标

当前 UI 框架基于 `GameFramework/UnityGameFramework` 封装，目标是统一：

- UI 打开与关闭流程
- `Resources` 资源命名与加载规则
- `View` / `Item` 的代码组织方式
- `UIWidget` 扫描、绑定、脚本生成流程

核心运行时入口：

- `UIMgr`：统一管理 UI 打开、关闭、查询
- `ResMgr`：统一通过资源名加载预设、图片
- `BaseUI`：所有界面基类
- `BaseItem`：所有可复用 UI 组件基类
- `UIWidget`：挂在预设根节点上的编辑器驱动组件

## 2. 命名规则

### 2.1 界面命名

界面类、界面预设、界面根节点必须以 `View` 结尾。

示例：

- `BattleMainView`
- `LoginView`
- `BagView`

对应生成脚本：

- `BattleMainView : BaseUI`

### 2.2 复用组件命名

可复用组件类、组件根节点必须以 `Item` 结尾。

示例：

- `RewardItem`
- `CardItem`
- `BagSlotItem`

对应生成脚本：

- `RewardItem : BaseItem`

### 2.3 节点后缀规则

`UIWidget` 会扫描根节点下所有子节点，并按后缀识别绑定类型：

- `xxxTxt` -> `Text`
- `xxxImg` -> `Image`
- `xxxRawImg` -> `RawImage`
- `xxxBtn` -> `Button`
- `xxxGo` -> `GameObject`
- `xxxTrans` -> `Transform`
- `xxxItem` -> 可复用组件节点

说明：

- `xxxItem` 节点本身也需要挂 `UIWidget`
- `xxxItem` 节点对应的组件根名称也应是一个以 `Item` 结尾的合法组件名

## 3. 目录规则

### 3.1 运行时代码目录

- `Assets/src/Script/Manager/UIMgr.cs`
- `Assets/src/Script/Manager/ResMgr.cs`
- `Assets/src/Script/UI/Base/BaseUI.cs`
- `Assets/src/Script/UI/Base/BaseItem.cs`
- `Assets/src/Script/UI/Binding/UIWidget.cs`
- `Assets/src/Script/UI/Binding/UIBindData.cs`
- `Assets/src/Script/UI/Config/UIGroupName.cs`
- `Assets/src/Script/UI/Config/UIFormDefine.cs`
- `Assets/src/Script/UI/Config/UIPathRegistry.cs`

### 3.2 编辑器代码目录

- `Assets/src/Editor/UI/UIWidgetEditor.cs`
- `Assets/src/Editor/UI/UIBindingScanner.cs`
- `Assets/src/Editor/UI/UICodeGenerator.cs`
- `Assets/src/Editor/UI/UIResourceNameValidator.cs`

### 3.3 生成脚本目录

生成的 UI 脚本统一输出到：

- `Assets/src/Script/UI/<Module>/<UIName>/<UIName>.cs`

示例：

- `Assets/src/Script/UI/Battle/BattleMainView/BattleMainView.cs`
- `Assets/src/Script/UI/Common/RewardItem/RewardItem.cs`

### 3.4 Resources 目录规则

Prefab 与图片资源统一放在 `Assets/Resources` 下。

推荐目录：

- `Assets/Resources/UI/<Module>/<ViewName>.prefab`
- `Assets/Resources/Prefab/<Category>/<PrefabName>.prefab`
- `Assets/Resources/Image/<Module>/<SpriteName>.png`

示例：

- `Assets/Resources/UI/Battle/BattleMainView.prefab`
- `Assets/Resources/Prefab/Card/WarriorCard.prefab`
- `Assets/Resources/Image/Common/CommonCloseImg.png`

## 4. 资源规则

### 4.1 唯一命名

所有 prefab 名、图片名必须全局唯一。

原因：

- `ResMgr` 通过资源名直接查路径
- `UIPathRegistry` 通过资源名生成静态映射
- 名称重复会导致编辑器扫描时报错

示例：

- 合法：`BattleMainView`
- 合法：`WarriorCard`
- 合法：`CommonCloseImg`
- 不合法：两个不同目录下同时存在 `BattleMainView.prefab`

### 4.2 加载方式

运行时通过 `ResMgr` 按资源名加载：

- `ResMgr.inst.LoadPrefab("BattleMainView")`
- `ResMgr.inst.LoadPrefab("WarriorCard")`
- `ResMgr.inst.LoadSprite("CommonCloseImg")`

不直接在业务中手写 `Resources` 路径。

### 4.3 注册表生成规则

资源注册表由编辑器工具自动生成，不手写维护。

当前生成规则：

- 统一扫描 `Assets/Resources` 下所有 prefab
- 扫描 `Assets/Resources/Image` 下所有图片资源
- 自动生成 `资源名 -> Resources路径` 映射

对应入口：

- `Tools/UI/Generate Resource Registry`

## 5. 运行时职责

### 5.1 `UIMgr`

职责：

- 打开 UI
- 关闭 UI
- 查询 UI
- 维护 UI 名称与实例映射
- 初始化时自动加载 `UICanvas`
- 缓存 `UIRoot` 与五层节点
- 统一接入 `UIComponent`

常用方式：

```csharp
UIMgr.inst.Open("BattleMainView");
UIMgr.inst.Close("BattleMainView");
```

### 5.2 `BaseUI`

`BaseUI` 继承自 `UIFormLogic`，是所有 `View` 的基类。

负责：

- 统一生命周期
- 组件绑定
- 事件注册与反注册
- 刷新界面
- 关闭自身
- 声明默认 UI 层级

常用重写方法：

- `BindComponents()`
- `BindEvents()`
- `UnbindEvents()`
- `OnOpenUI(object userData)`
- `OnCloseUI(bool isShutdown, object userData)`
- `RefreshView()`
- `LayerName`

### 5.3 `BaseItem`

`BaseItem` 是所有复用组件的基类。

负责：

- 接收根节点 `GameObject`
- 绑定内部子节点
- 注册和反注册事件
- 提供 `Dispose()` 释放逻辑

常用重写方法：

- `BindComponents()`
- `BindEvents()`
- `UnbindEvents()`
- `RefreshView()`

## 6. View 与 Item 的生成规则

### 6.1 View 生成规则

如果 `UIWidget.UIName` 以 `View` 结尾，则生成：

- 类名：`XXXView`
- 基类：`BaseUI`

示例：

```csharp
public sealed class BattleMainView : BaseUI
{
}
```

### 6.2 Item 生成规则

如果 `UIWidget.UIName` 以 `Item` 结尾，则生成：

- 类名：`XXXItem`
- 基类：`BaseItem`

示例：

```csharp
public sealed class RewardItem : BaseItem
{
    public RewardItem(GameObject root) : base(root)
    {
    }
}
```

## 7. Item 子节点绑定规则

当某个 `View` 或 `Item` 内存在一个 `xxxItem` 子节点时：

1. 该节点应挂 `UIWidget`
2. 该节点会被识别为一个可复用组件节点
3. 生成代码时不会绑定成 `UIWidget`
4. 而是生成两组字段：

- `xxxItemGo : GameObject`
- `xxxItem : XxxItem`

并在 `BindComponents()` 中自动生成初始化代码：

```csharp
m_rewardItemGo = FindGameObject("RewardItem");
m_rewardItem = m_rewardItemGo != null ? new RewardItem(m_rewardItemGo) : null;
```

在 `UnbindEvents()` 中会自动释放：

```csharp
if (m_rewardItem != null)
{
    m_rewardItem.Dispose();
    m_rewardItem = null;
}
```

## 8. UIWidget 使用流程

### 8.1 创建 View

1. 在 `Assets/Resources/UI/<Module>/` 下创建预设
2. 预设名必须以 `View` 结尾
3. 根节点挂：
   - `UIForm`
   - 对应 `View` 脚本
   - `UIWidget`
4. 设置 `UIWidget.ModuleName`
5. 设置 `UIWidget.UIName`

### 8.2 创建 Item

1. 创建一个以 `Item` 结尾的组件根节点
2. 根节点挂 `UIWidget`
3. 设置 `UIWidget.ModuleName`
4. 设置 `UIWidget.UIName`
5. 生成后脚本继承 `BaseItem`

### 8.3 执行编辑器工具

可用入口：

- `Tools/UI/Generate Resource Registry`
- `UIWidget` 右键 `Rescan Bindings`
- `UIWidget` 右键 `Validate Resource Names`
- `UIWidget` 右键 `Generate UI Script`
- `UIWidget` 右键 `Open Script Folder`

推荐顺序：

1. 先命名好节点
2. 执行 `Rescan Bindings`
3. 执行 `Generate UI Script`
4. 执行 `Generate Resource Registry`

## 9. 当前生成器行为说明

当前生成器会自动生成：

- 字段声明
- `BindComponents()`
- `BindEvents()`
- `UnbindEvents()`
- `RefreshView()`
- 按钮点击回调空方法

注意：

- 当前是单类生成模式
- 生成的脚本默认由工具接管
- 如果手动修改同名生成脚本，后续再次生成可能被覆盖

## 10. 当前建议

### 10.1 View 命名建议

- 所有界面统一用 `View`
- 不再使用 `Panel`、`Window`、`Form` 作为业务命名后缀

### 10.2 Item 使用建议

- 有复用意义的节点再抽成 `Item`
- 单纯只用一次的普通子节点不必强行做成 `Item`

### 10.3 资源建议

- UI 预设名与脚本类名保持完全一致
- 图片资源名保持全局唯一
- 尽量不要在业务代码中直接写 `Resources.Load`

## 11. 一个完整示例

### 11.1 预设结构

`BattleMainView.prefab`

- `TitleTxt`
- `CloseBtn`
- `RewardItem`

其中：

- `BattleMainView` 根节点挂 `UIWidget`
- `RewardItem` 节点也挂 `UIWidget`

### 11.2 生成后的 View 代码风格

```csharp
private Text m_titleTxt;
private Button m_closeBtn;
private GameObject m_rewardItemGo;
private RewardItem m_rewardItem;

protected override void BindComponents()
{
    base.BindComponents();
    m_titleTxt = FindComponent<Text>("TitleTxt");
    m_closeBtn = FindComponent<Button>("CloseBtn");
    m_rewardItemGo = FindGameObject("RewardItem");
    m_rewardItem = m_rewardItemGo != null ? new RewardItem(m_rewardItemGo) : null;
}
```

## 12. 后续可扩展方向

后续如有需要，可以继续扩展：

- `TMP_Text` 支持
- 自定义 Inspector 按钮
- `View` / `Item` 双文件生成
- `Item` 自动生成 `SetData(...)` 模板
- UI 分层对应的 `Canvas.sortingOrder` 控制
