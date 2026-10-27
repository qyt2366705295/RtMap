# 每台机器的配置

程序从 EXE 所在目录开始，逐级向上查找最近的 `AppSettings.json`，与当前工作目录无关。
推荐布局：

```text
D:\git\
├─ AppSettings.json          本机配置，不进入 Git
└─ RtMap\                   仓库
   ├─ config\AppSettings.example.json
   └─ bin\Debug\RtMap.exe
```

## 本次迁移

1. 本次修改已为原 `config\AppSettings.json` 补齐新增字段，并保留原连接信息。
2. 由你将该文件移动到仓库同级目录，例如 `D:\git\AppSettings.json`。
3. 检查每台机器的安装路径和存储路径，尤其是 `PythonExe`、`Gdal2TilesPy`。
4. 如果要保留旧任务记录，将原 EXE 目录下的 `active_tasks.json` 复制到配置的 `Paths.StateDirectory`。
5. 重新编译并启动。以后更新代码时不需要移动或覆盖本机配置。

新机器可复制 `AppSettings.example.json` 到仓库同级目录，重命名为 `AppSettings.json`，填写连接信息和路径。
示例中的地址、用户名和密码是占位值，不可直接用于连接。

## 加载规则

- 缺少文件、JSON 格式错误、配置节缺失或必填值为空时，显示错误并退出，不回退到源码默认配置，也不自动创建真实配置。
- 所有配置节和字段都需要填写；服务器密码和 UOS 路径前缀允许为空字符串。
- `Paths` 中的本机路径和两个本地监控路径支持 `%APPDATA%`、`%SystemRoot%` 等环境变量。
- 相对本机路径以 `AppSettings.json` 所在目录为基准。例如 `Paths.StateDirectory` 为 `RtMapState` 时，在本布局下对应 `D:\git\RtMapState`。
- 路径配置不加额外的命令行引号；JSON 中反斜杠应写成 `\\`。
- 启动时验证字段，不要求所有外部工具和共享盘立即可用；使用相应功能时仍需其实际存在且具备访问权限。
- 修改配置后重启程序。不要在更近的上级目录遗留另一份 `AppSettings.json`，否则会优先加载它。

## 配置项

原有的 `Server`、`Uos`、`Monitor` 字段继续使用。其中二维本地监控现在也读取 `Monitor.DefaultLocalInfoDir`。

| Paths 字段 | 用途 |
| --- | --- |
| VideoSamplerExe | 视频抽帧程序，所有手动和自动入口共用 |
| OtFRunnerExe | 实时 Sfm 引擎 |
| PythonExe / Gdal2TilesPy | GDAL 使用的 Python 与切片脚本；二者应属于同一可用环境 |
| RtScenExe / Obj2TilesExe | 三维处理与 3D Tiles 转换程序 |
| ViewerExe / SfmUiExe | 查看器、Sfm 图形界面 |
| Mip2DExe / Mip3DExe | 二维/三维 Mip 程序 |
| Mip2DConfig / Mip3DConfig | 两个 Mip 程序的配置文件 |
| LasModuleDirectoryA / LasModuleDirectoryB | 两个 Las 实例目录，各包含 `setting.ini` 和 `三维重建.exe` |
| WslExe | Windows 的 WSL 启动程序，示例使用 `%SystemRoot%\System32\wsl.exe` |
| CameraParametersRoot | 相机参数库，包含型号子目录下的 `camera.xml` 和 Slm 的 `config` 目录 |
| CameraMapFile | 独立的相机匹配 JSON 文件 |
| ImageRoot | 任务影像根目录，手动下载默认位于其 `Manual_Download` 子目录 |
| SfmWorkspaceRoot | Sfm / Slm 工程与成果根目录 |
| TileCacheRoot | 手动及任务工具的切片缓存根目录 |
| PointCloudRoot | 三维任务点云根目录，其下按任务 ID 创建目录 |
| Workspace3DRoot | 三维工程根目录，其下按任务 ID 创建目录 |
| StateDirectory | 本机 `active_tasks.json` 的保存目录，建议放在仓库外 |
| LocalInputRoot | 手动本地数据来源的默认目录 |
| SharedOutputRoot | 正射成果共享目录，程序在其下生成 `Sfm_<日期>\<任务ID>` |

`Wsl.Distribution` 指定发行版；`CodeDirectory` 指定 Linux 算法目录；`PythonCommand` 指定 Linux Python 命令或程序路径；`MountRoot` 指定 Windows 磁盘挂载根路径，标准环境通常为 `/mnt`。
现有 WSL 路径转换仅支持带盘符的 Windows 绝对路径，不支持 UNC 路径。

`OutputOrtho`、`gtiff`、`3DTiles`、数据库文件等保持固定内部结构，从配置根目录派生。
`TaskInfo.image_path`、`result_path`、`dom_path` 继续由任务下发；`dom_path` 必须能被执行三维任务的机器直接访问。
程序工作目录在原先需要指定的位置由 EXE 所在目录或任务工作空间派生。

实际配置已加入 `.gitignore`。取消当前跟踪不会抹除旧 Git 历史中的配置内容。

## 验证配置代码

在项目根目录运行 `./tests/Run-ConfigChecks.ps1`。脚本使用 Visual Studio MSBuild 构建 Release，
并在 `obj` 下创建隔离配置，验证逐级查找、最近文件优先、与启动工作目录无关、路径解析、
无效配置报错、保存重载以及任务记录的配置目录。不会使用真实凭据、连接服务器或启动算法。
若无法自动找到构建工具，可传入 `-MSBuildPath` 指定 Visual Studio 的 `MSBuild.exe`。
