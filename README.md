# Minecraft 资源提取器

**Minecraft Resource Extractor (MRE)** 是一款面向数据包和资源包创作者的小工具。

使用此工具，你可以轻松地从任意 minecraft.jar 版本或模组中提取纹理、音效和数据文件（成就、合成配方、战利品表等）。

## 系统要求

### Windows

- **[Java JDK](https://www.oracle.com/java/technologies/downloads/)** — 用于解压 .jar 文件内容
- .NET Framework 4.7.2 或更高版本

### Linux

- **Java JDK** (*openjdk*) — 用于解压 .jar 文件内容
- **jq** — 命令行 JSON 解析工具

安装命令：

```Shell
sudo apt-get update && sudo apt-get install openjdk-11-jdk-headless jq
```

## Windows 使用方法

- 确保已安装 Java JDK（程序会自动检测 `JAVA_HOME` 和 `PATH` 环境变量）
- 从 **[Releases](https://github.com/JKerboeuf/Minecraft-Resource-Extractor/releases/latest)** 页面下载最新版本，在 "Assets" 下找到 **"mre-for-windows.zip"**
- 解压并运行 `MinecraftResourceExtractor.exe`
- 按照界面指引操作：
  1. **步骤1** — 选择"提取 Minecraft 资源"或"从 jar 文件提取"
  2. **步骤2** — 选择要提取的游戏版本
  3. **步骤3** — 选择提取类型（jar 文件 或 assets 资源文件）
  4. **步骤4** — 按资源类型或文件夹选择要提取的内容，指定输出目录
- 提取的文件默认保存在程序同目录下的 **"mre-output"** 文件夹中

## Linux 使用方法

- 确保已安装上述依赖
- 从 **[Releases](https://github.com/JKerboeuf/Minecraft-Resource-Extractor/releases/latest)** 页面下载最新版本，在 "Assets" 下找到 **"mre-for-linux.zip"**
- 解压并运行 `mre.sh`，可附带 `.minecraft` 路径或 `.jar` 文件路径，也可额外指定 Java 的 `jar` 二进制文件路径
- 提取的文件默认保存在脚本同目录下的 **"mre-output"** 文件夹中

### 示例

```Shell
./mre.sh "/mnt/c/users/你的用户名/AppData/Roaming/.minecraft"
```

```Shell
./mre.sh "/mnt/c/Users/你的用户名/AppData/Roaming/.minecraft/mods/某个模组.jar"
```

```Shell
./mre.sh "/mnt/c/users/你的用户名/AppData/Roaming/.minecraft" "/some/path/to/jar"
```

## 可以提取什么？

本工具支持提取三类文件：

### version.jar → assets

适用于**资源包**创作者，包含所有**纹理**和**模型**文件。

### version.jar → data

适用于**数据包**创作者，包含所有数据文件（`json` 格式），涵盖战利品表、合成配方、进度、世界生成等。

### Assets 资源文件

适用于**资源包**创作者，这些文件名称经过哈希编码，手动处理较为繁琐。主要包括**音效**、**音乐**和**语言**文件，也包含 **Programmer Art 资源包**及其他默认文件。

## 资源类型分类

| 类型 | 匹配目录 | 说明 |
|------|---------|------|
| 图片素材 | `textures/` | 方块、物品、GUI、粒子等所有纹理 |
| 音频素材 | `sounds/` | 音效、音乐 |
| 立体模型 | `models/` | 方块/物品的 3D 模型 JSON |
| 方块配置 | `blockstates/` | 方块状态定义 |
| 物品模型 | `models/item/` | 物品模型 JSON |
| 语言文件 | `lang/` | 翻译文件 |
| 字体素材 | `font/` | 字体纹理/配置 |
| 粒子效果 | `particles/`, `textures/particle/` | 粒子纹理和定义 |
| GUI 素材 | `textures/gui/` | 界面纹理 |
| 实体纹理 | `textures/entity/` | 生物、物品展示框等纹理 |
| 护甲纹理 | `textures/models/armor/`, `models/armor/` | 护甲层纹理 |
| 地图图标 | `textures/map/` | 地图图标 |
| 着色器 | `shaders/` | 着色器程序 |
| 动画配置 | `textures/` (含 .mcmeta) | 动画元数据 |
| 附魔闪光 | `textures/glint/` | 附魔闪光效果 |
| 工具纹理 | `textures/tools/` | 工具纹理 |

> 程序会根据 jar 包中的实际内容动态过滤，只显示当前 jar 中包含的资源类型，支持原版和模组 jar 包。

## 截图

![MRE 截图](https://i.imgur.com/1pqQNQH.png)

## 致谢

- 原作者：[Julien Kerboeuf](https://github.com/JKerboeuf)
- 本项目基于 [Minecraft-Resource-Extractor](https://github.com/JKerboeuf/Minecraft-Resource-Extractor) 进行汉化和功能增强
