# 水弹幸存者 · 上手指南（给组长 / 美术 / 新加入的人）

> 这份文档回答三类问题：**图片放哪、怎么换**、**预制体在哪、怎么改**、**怎么和新东西互动**。
> 完全不需要看懂代码，照着做就行。程序细节在 `技术说明.md`。

---

## 一、先说清楚：之前为什么会"看不懂"

上一版是用**纯代码**把相机、玩家、敌人都"画"出来的：
运行时才生成对象，所以在编辑器的 Hierarchy 里看不见、Project 里也没有预制体和图片，
想改个颜色都得进代码。这个做法本身能跑，但对美术和策划确实很不友好 —— 已经改掉了。

现在改成了**正常的 Unity 做法**：

| | 上一版 | 现在 |
| --- | --- | --- |
| 角色外观 | 代码画的圆 | **图片**（`Assets/Resources/Art/`） |
| 有没有预制体 | 没有 | **有**（菜单一键生成 `Assets/Prefabs/`） |
| 场景里能不能看见对象 | 看不见 | **能**（菜单一键生成 `Assets/Scenes/WaterSurvivorTest.scene`） |
| 换图片 | 改代码 | **覆盖同名图片 / 在 Inspector 里拖** |
| 改速度、血量 | 改代码 | **在 Inspector 里改** |

按 Play 依然能直接玩（这条没变，方便快速测试）。

---

## 二、怎么把图片用起来（三步）

### 第 1 步：打开工程，确认图片进来了

工程里已经放好两张测试图：

```
Assets/Resources/Art/Player.jpg   ← 主角
Assets/Resources/Art/Enemy.jpg    ← 小怪
```

在 Project 窗口点一下它们，Inspector 里应该显示 **Texture Type = Sprite**、
**Read/Write 勾着**。这两项由工程里的自动导入脚本负责，**你不用管**，
以后往这个文件夹里拖新图，也会自动按这套设置导入。

### 第 2 步：跑一下看效果

打开任意场景（或 `Assets/Scenes/WaterSurvivorTest.scene`），按 **Play**。
角色就变成图片了。

### 第 3 步：以后怎么换成别的图

有两条路，选一条：

**路线 A（最简单）：覆盖同名文件**
把新图片改名成 `Player.jpg`（或 `Player.png`），丢进 `Assets/Resources/Art/` 覆盖旧的。
重新 Play 就是新图。**不用改任何代码、不用拖任何引用。**

**路线 B（正式做法）：改预制体的 Sprite**
1. 菜单 **工具 → 水弹幸存者 → 1. 生成预制体**（如果还没生成过）
2. Project 窗口进入 `Assets/Prefabs/`，**双击 `Player.prefab`**
3. 右侧 Inspector 找到 **Sprite Renderer** 组件 → 把新图拖到 **Sprite** 那一栏
4. `Ctrl+S` 保存预制体

> 关于图片的三个好消息：
> 1. **白底会被自动抠成透明**，所以你直接拿白底 jpg 就能用，不用先去做透明 png。
> 2. **图片四周的空白边会被自动裁掉**，角色多大就是多大，不会出现"图里一大片空白导致角色变小"。
> 3. 换图之后程序会**自动重新处理一遍**，你什么都不用点。
>
> 顺便说明一个目录：`Assets/Resources/Art/Processed/` 里放的是**程序自动生成的成品图**
> （已经抠好白底、裁好边、设好大小），预制体引用的就是它们。
> **不要手动改这个目录里的文件**，它是自动生成的；换图请覆盖 `Art/` 下的原图（`Player.jpg` / `Enemy.jpg`）。
> 这个目录要一起提交到 Git，否则别人拉下来预制体里的图片会变成空的。

---

## 三、预制体和场景在哪

打开工程后点两次菜单就有了（都在顶部菜单栏 **工具 → 水弹幸存者**）：

| 菜单项 | 做什么 | 生成的东西 |
| --- | --- | --- |
| **1. 生成预制体（Player / Enemy）** | 把玩家和敌人做成预制体 | `Assets/Prefabs/Player.prefab`、`Enemy.prefab` |
| **2. 生成测试场景（推荐）** | 新建一个摆好的场景并加入打包列表 | `Assets/Scenes/WaterSurvivorTest.scene` |
| 3. 在当前场景生成测试对象 | 往你当前打开的场景里摆一套 | 直接改当前场景 |

**推荐走第 2 条**：生成之后打开 `WaterSurvivorTest.scene`，
Hierarchy 里能直接看到 `Main Camera`、`Floor`、`Player`、`Enemy_0`~`Enemy_5`、`GameRoot`，
点谁改谁，改完 `Ctrl+S`。按 Play 就是改完的效果。

> 顺便解释一个之前容易懵的点：上一版你按 Play 才出现对象，是因为对象是运行时生成的。
> 现在场景里**真的躺着这些对象**，所以随时能看、能调。

### 在 Inspector 里能放心改什么

选中 `Player` 或敌人，右边能看到这些组件，都是给你调的：

| 组件 | 里面的字段 | 意思 |
| --- | --- | --- |
| **Sprite Renderer** | Sprite / Color / Order in Layer | 换图片、染色、调前后遮挡 |
| **Transform** | Position / Scale | 位置和大小（改了不影响碰撞体） |
| **Circle Collider 2D** | Radius | **挨打范围**（和图片大小无关，可以单独调） |
| **Rigidbody 2D** | Gravity Scale | 2D 俯视角是 0，别动 |
| **Player Move** | Move Speed | 移动速度 |
| **Player Shooter** | Fire Interval / Bullet Speed / Bullet Lifetime / Muzzle Offset | 射速、子弹速度、子弹存活时间、枪口位置 |
| **Dummy Enemy**（敌人身上） | Move Speed / Max Hp / Target | 追击速度、要打几下才死、追谁 |

**改坏了怎么办**：用 GitHub Desktop 把这个文件 **Discard changes** 就回来了；
或者删掉 `Assets/Prefabs` 和场景，重新点一次菜单生成。

---

## 四、怎么让它和"新东西"互动

这一节是给要加新玩法的人看的。整个游戏的互动只有 **3 条约定**，记住就够了：

### 约定 1：水弹靠 **Tag（标签）** 判断打中了谁

敌人的 Tag 必须是 **`Enemy`**。
在 Hierarchy 里选中敌人 → Inspector 最上方 **Tag** 下拉 → 选 `Enemy`。
（新建敌人时忘了选，水弹就会直接穿过去，不报错也不掉血 —— 这是最容易踩的坑。）

所以要做一个"新敌人"，步骤是：复制 `Enemy.prefab` → 改名 → 换图片 → **确认 Tag 还是 Enemy** → 改速度血量。

### 约定 2：想被通知"我被打中了"，就写一个 `OnHit` 方法

敌人身上只要有这个方法的脚本，被打中时就会被自动调用：

```csharp
public void OnHit()
{
    // 扣血、闪白、飘字、掉模块……都写这里
}
```

不写也不会报错（只是没有受击反应）。
现成的例子就是 `Assets/Scripts/WaterSurvivor/DummyEnemy.cs` 里的 `OnHit`。

### 约定 3：要引用别的对象，用 **public 字段** 在 Inspector 里拖

例如敌人要追玩家，就是 `DummyEnemy` 上的 **Target** 字段，把 `Player` 拖进去即可。
`ArenaBootstrap` 在生成敌人时会自动填这个字段；
你自己在场景里摆的敌人，手动拖一下 `Player` 就行。

**新建东西的通用套路**：做一个预制体 → Tag 设 `Enemy` → 写脚本挂上去 → 需要引用谁就加 public 字段 + 拖。

---

## 五、常见问题

**Q：按 Play 出现了两套角色，翻倍了？**
场景里已经摆好对象，同时又让 `GameRoot` 上的 `ArenaBootstrap` 自动生成了。
选中 `GameRoot`，把 **Build On Start** 取消勾选即可（菜单生成的场景已经帮你关掉了）。

**Q：图片换成新的了，游戏里还是旧的？**
1）确认新图覆盖的是 `Assets/Resources/Art/` 下的同名文件；
2）停止 Play 再进一次（图片是进游戏时读的）。

**Q：图片显示成一片空白/粉红？**
在 Project 窗口点一下那张图，确认 Inspector 里 **Texture Type = Sprite**。
（丢进 `Art` 文件夹的图会自动设好，如果不对，多半是丢到别的文件夹去了。）

**Q：敌人不动 / 水弹打不中？**
先看 Tag 是不是 `Enemy`；再看敌人的 **Target** 字段有没有指向 `Player`。

**Q：角色太大了/太小了？**
调 `Player.prefab` 的 **Transform → Scale**，或者改 `Assets/Scripts/WaterSurvivor/GameConfig.cs` 里的
`PlayerArtSize` / `EnemyArtSize`（数值 = 图片最长边占几个世界单位）。
注意：**挨打范围由 Circle Collider 2D 的 Radius 决定，和图片大小是分开的**，别只顾着改图。

**Q：日志在哪？**
游戏里每次发射/命中/子弹超时都会记一条，两个地方都能看：
- Unity 的 **Console** 窗口；
- 工程根目录的 **`debug.log`** 文件（菜单 工具 → 水弹幸存者 → 打开 debug.log 所在文件夹）。

---

## 六、这批做了什么（对照最初的四条任务）

| 任务 | 状态 | 在哪 |
| --- | --- | --- |
| 1. 玩家 WASD 移动 | ✅ | `Player.prefab` 上的 `Player Move`（速度可调） |
| 2. 按住鼠标左键连发水弹，朝鼠标方向 | ✅ | `Player.prefab` 上的 `Player Shooter` |
| 3. 子弹命中敌人 / 飞太久 → 销毁并写 debug.log | ✅ | `Assets/Scripts/WaterSurvivor/WaterBullet.cs` |
| 4. 命中判定用标签，敌人是 `Enemy` | ✅ | Tag = `Enemy`（见约定 1） |
| 额外：把两张测试图接进游戏 | ✅ | `Assets/Resources/Art/Player.jpg`、`Enemy.jpg` |
| 额外：预制体 + 可视场景 + 这份文档 | ✅ | `Assets/Prefabs/`、`Assets/Scenes/`、本文件 |

设计文档里的气压、模块、热力值、水域、水泵、10 天流程这些还没做，属于下一批。
