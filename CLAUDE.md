# CLAUDE.md

Minecraft 资源提取器（MRE）—— C# WinForms（.NET Framework 4.7.2），MVC 架构（controller / model / view）。从 JKerboeuf/Minecraft-Resource-Extractor 汉化并增强。

## 目录约定（重要）

```
C:\Users\steam\Desktop\123\                      ← 项目根（不在 git 里）
├── MRE-交接文档.md                                ← 交接文档（手工维护，写功能/踩坑/路线图）
├── test-output\                                   ← 测试提取输出（临时，可整体清空）
├── releases\                                      ← 发布包 MRE-vX.X.X\ + MRE-vX.X.X.zip
└── Minecraft-Resource-Extractor\                  ← git 仓库（源码在这里）
    └── MinecraftResourceExtractor\                ← 项目源码 + mre.csproj
        ├── controller\  model\  view\             ← 代码（只在这里改）
        ├── bin\Debug\                             ← Debug 构建产物（测试用）
        └── bin\Release\                           ← Release 构建产物（发布用）
```

- **源码只在** `MinecraftResourceExtractor\` 下改。
- **测试用 Debug** 构建，**发布用 Release** 构建。
- **测试提取输出** → `C:\Users\steam\Desktop\123\test-output\`（由 config.json 的 `outputPath` 控制，不在 git 里）。
- **发布包** → `C:\Users\steam\Desktop\123\releases\`（不进 git）。
- 程序运行配置在 `C:\Users\steam\AppData\Roaming\mre\config.json`（含 javaPath、outputPath 等）。

## 构建

MSBuild 路径（注意用 `-` 前缀，不是 `/`）：

```bash
cd "c:/Users/steam/Desktop/123/Minecraft-Resource-Extractor/MinecraftResourceExtractor"

# 测试构建（Debug）
"/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" mre.csproj -t:Build -p:Configuration=Debug -v:minimal

# 发布构建（Release）
"/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" mre.csproj -t:Build -p:Configuration=Release -v:minimal
```

改完代码后**两个配置都要编译验证**——用户日常跑的是 Debug，别只编译 Release 就以为改动生效了（曾踩过坑）。

## 测试约定

- 测试跑 `bin\Debug\MinecraftResourceExtractor.exe`（或 VS 里 F5）。
- 提取结果在 `C:\Users\steam\Desktop\123\test-output\`，验证后可直接清空。
- 测试用的 jar 由用户自己准备，任意位置即可。

## 发布约定

- **用户手动发布 Release**（不装 gh、不自动化）。Claude 只负责：更新版本号、构建、打包到 `releases\`、提交、打 tag、推送。
- 打包格式：`releases\MRE-vX.X.X\`（exe + dll + config + Resources\）+ `MRE-vX.X.X.zip`。
- 推送 GitHub 需要代理：`git -c http.proxy=http://127.0.0.1:10808 -c https.proxy=http://127.0.0.1:10808 push ...`（直连被墙）。
- 完整流程见 `MRE-交接文档.md` 的「发布流程」章节。

## 关键代码位置

- 资源类型匹配/提取：`model/ResourceType.cs`、`model/JarFile.cs`
- 批量提取：`model/ModBatchExtractor.cs`
- UI 主窗体：`view/FrmMre.cs`、进度弹窗 `view/FrmProgress.cs`
- 控制器（异步加载/提取）：`controller/Controller.cs`
