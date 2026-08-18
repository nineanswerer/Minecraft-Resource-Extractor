using System.Collections.Generic;
using System.Linq;

namespace mre.model
{
	public enum ResourceType
	{
		图片素材,
		音频素材,
		立体模型,
		方块配置,
		物品模型,
		语言文件,
		字体素材,
		粒子效果,
		GUI素材,
		实体纹理,
		护甲纹理,
		地图图标,
		着色器,
		动画配置,
		附魔闪光,
		工具纹理,
		松散图片,
	}

	public class ResourceTypeInfo
	{
		public ResourceType Type { get; set; }
		public string DisplayName { get; set; }
		/// <summary>用于匹配 jar 中条目的目录名（如 textures, blockstates, models 等）</summary>
		public List<string> MatchDirs { get; set; }
		/// <summary>用于提取的路径前缀（在 jar 中动态匹配后填充）</summary>
		public List<string> JarPaths { get; set; }
		/// <summary>可选：文件后缀过滤器（如 ".mcmeta"）。设置后只提取匹配该后缀的文件，而非整个目录</summary>
		public string FileSuffix { get; set; }
		/// <summary>松散文件：仅匹配前缀的直接子文件（无更深子目录），如命名空间根目录的 icon.png</summary>
		public bool DirectChildrenOnly { get; set; }
		public string Description { get; set; }

		public ResourceTypeInfo(ResourceType type, string displayName, string description, params string[] matchDirs)
		{
			Type = type;
			DisplayName = displayName;
			Description = description;
			MatchDirs = new List<string>(matchDirs);
			JarPaths = new List<string>();
		}
	}

	public static class ResourceTypes
	{
		public static readonly List<ResourceTypeInfo> AllTypes = new List<ResourceTypeInfo>
		{
			new ResourceTypeInfo(ResourceType.图片素材, "图片素材", "方块、物品、GUI、粒子等所有纹理", "textures"),
			new ResourceTypeInfo(ResourceType.音频素材, "音频素材", "音效、音乐", "sounds"),
			new ResourceTypeInfo(ResourceType.立体模型, "立体模型", "方块/物品的 3D 模型 JSON", "models"),
			new ResourceTypeInfo(ResourceType.方块配置, "方块配置", "方块状态定义", "blockstates"),
			new ResourceTypeInfo(ResourceType.物品模型, "物品模型", "物品模型 JSON", "models/item"),
			new ResourceTypeInfo(ResourceType.语言文件, "语言文件", "翻译文件", "lang"),
			new ResourceTypeInfo(ResourceType.字体素材, "字体素材", "字体纹理/配置", "font"),
			new ResourceTypeInfo(ResourceType.粒子效果, "粒子效果", "粒子纹理和定义", "particles", "textures/particle"),
			new ResourceTypeInfo(ResourceType.GUI素材, "GUI素材", "界面纹理", "textures/gui"),
			new ResourceTypeInfo(ResourceType.实体纹理, "实体纹理", "生物、物品等纹理", "textures/entity"),
			new ResourceTypeInfo(ResourceType.护甲纹理, "护甲纹理", "护甲层纹理", "textures/models/armor", "models/armor"),
			new ResourceTypeInfo(ResourceType.地图图标, "地图图标", "地图图标", "textures/map"),
			new ResourceTypeInfo(ResourceType.着色器, "着色器", "着色器程序", "shaders"),
			new ResourceTypeInfo(ResourceType.动画配置, "动画配置", "动画定义（.mcmeta 文件）", "textures") { FileSuffix = ".mcmeta" },
			new ResourceTypeInfo(ResourceType.附魔闪光, "附魔闪光", "附魔闪光等效果", "textures/glint", "glint"),
			new ResourceTypeInfo(ResourceType.工具纹理, "工具纹理", "工具纹理", "textures/tools", "textures/trident"),
			new ResourceTypeInfo(ResourceType.松散图片, "松散图片", "命名空间根目录的松散图片（icon.png 等）") { DirectChildrenOnly = true, FileSuffix = ".png" },
		};

		/// <summary>
		/// 根据 jar 条目动态匹配每个资源类型的实际提取路径
		/// 适配所有命名空间（assets/minecraft/, assets/create/, assets/forge/ 等）
		/// </summary>
		public static void UpdateJarPaths(List<string> allEntries)
		{
			foreach (var info in AllTypes)
				info.JarPaths.Clear();

			foreach (var info in AllTypes)
			{
				if (info.DirectChildrenOnly)
				{
					// 松散文件类型：匹配 assets/<namespace>/ 下的直接图片文件（无更深子目录）
					foreach (var entry in allEntries)
					{
						string normalized = entry.Replace('\\', '/');
						if (!normalized.StartsWith("assets/"))
							continue;
						int nsEnd = normalized.IndexOf('/', "assets/".Length);
						if (nsEnd < 0)
							continue;
						if (normalized.IndexOf('/', nsEnd + 1) >= 0)
							continue; // 命名空间下还有更深子目录，非松散文件
						if (string.IsNullOrEmpty(info.FileSuffix) || normalized.EndsWith(info.FileSuffix, System.StringComparison.OrdinalIgnoreCase))
						{
							string nsRoot = normalized.Substring(0, nsEnd + 1); // assets/<namespace>/
							if (!info.JarPaths.Contains(nsRoot))
								info.JarPaths.Add(nsRoot);
						}
					}
					continue;
				}

				foreach (string matchDir in info.MatchDirs)
				{
					foreach (var entry in allEntries)
					{
						// 查找 /matchDir/ 在条目中的位置，提取到此目录的完整前缀
						string pattern = "/" + matchDir + "/";
						int idx = entry.IndexOf(pattern);
						if (idx >= 0)
						{
							// 提取路径到 matchDir/ 为止，如 assets/create/textures/
							string path = entry.Substring(0, idx + matchDir.Length + 2);
							if (!info.JarPaths.Contains(path))
								info.JarPaths.Add(path);
						}
						else if (entry.EndsWith("/" + matchDir))
						{
							// 条目以 /matchDir 结尾（是一个目录条目）
							string path = entry + "/";
							if (!info.JarPaths.Contains(path))
								info.JarPaths.Add(path);
						}
						else if (entry.StartsWith(matchDir + "/"))
						{
							// 顶层目录匹配（无 assets/ 前缀的情况）
							string path = matchDir + "/";
							if (!info.JarPaths.Contains(path))
								info.JarPaths.Add(path);
						}
					}
				}
			}
		}

		public static ResourceTypeInfo GetInfo(ResourceType type)
		{
			foreach (var info in AllTypes)
			{
				if (info.Type == type)
					return info;
			}
			return null;
		}

		public static List<string> GetJarPathsForType(ResourceType type)
		{
			foreach (var info in AllTypes)
			{
				if (info.Type == type)
					return info.JarPaths;
			}
			return new List<string>();
		}

		public static string GetDisplayName(ResourceType type)
		{
			foreach (var info in AllTypes)
			{
				if (info.Type == type)
					return info.DisplayName;
			}
			return type.ToString();
		}

		/// <summary>
		/// 把一个资源类型展开成「(前缀, 后缀, 子目录)」提取请求列表，供单 jar 与批量提取共用。
		/// 松散文件类型（DirectChildrenOnly）始终单独输出到以其类型名为名的子目录，避免与命名空间下其他资产混在一起。
		/// </summary>
		public static List<ExtractRequest> BuildExtractRequests(ResourceType type, bool groupByType)
		{
			var info = GetInfo(type);
			string suffix = info != null ? info.FileSuffix : null;
			bool directOnly = info != null && info.DirectChildrenOnly;
			string subDir = (groupByType || directOnly) ? GetDisplayName(type) : null;
			var requests = new List<ExtractRequest>();
			foreach (string jarPath in GetJarPathsForType(type))
				requests.Add(new ExtractRequest { Prefix = jarPath, FileSuffix = suffix, SubDirPrefix = subDir, DirectChildrenOnly = directOnly });
			return requests;
		}

		/// <summary>
		/// 构建「语言包提纯」提取请求：只提取每个命名空间 lang/ 目录下指定语言的 .json。
		/// 复用「语言文件」类型已动态匹配好的 assets/<ns>/lang/ 前缀，文件名走白名单精确过滤，
		/// 输出到「语言包/<jar名>/assets/<ns>/lang/<lang>.json」目录，供机翻 / 合成汉化资源包复用。
		/// </summary>
		public static List<ExtractRequest> BuildLangPackRequests(IList<string> languages)
		{
			var whitelist = new List<string>();
			if (languages != null)
			{
				foreach (string lang in languages)
					if (!string.IsNullOrEmpty(lang) && !whitelist.Contains(lang + ".json"))
						whitelist.Add(lang + ".json");
			}
			if (whitelist.Count == 0)
				whitelist.Add("en_us.json"); // 兜底：未指定语言时至少提取基准英文

			var requests = new List<ExtractRequest>();
			foreach (string jarPath in GetJarPathsForType(ResourceType.语言文件))
				requests.Add(new ExtractRequest { Prefix = jarPath, FileNameWhitelist = whitelist, SubDirPrefix = "语言包" });
			return requests;
		}
	}
}
