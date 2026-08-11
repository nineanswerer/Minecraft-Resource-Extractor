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
	}

	public class ResourceTypeInfo
	{
		public ResourceType Type { get; set; }
		public string DisplayName { get; set; }
		/// <summary>用于匹配 jar 中条目的目录名（如 textures, blockstates, models 等）</summary>
		public List<string> MatchDirs { get; set; }
		/// <summary>用于提取的路径前缀（在 jar 中动态匹配后填充）</summary>
		public List<string> JarPaths { get; set; }
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
			new ResourceTypeInfo(ResourceType.动画配置, "动画配置", "动画定义（.mcmeta 文件）", "textures"),
			new ResourceTypeInfo(ResourceType.附魔闪光, "附魔闪光", "附魔闪光等效果", "textures/glint", "glint"),
			new ResourceTypeInfo(ResourceType.工具纹理, "工具纹理", "工具纹理", "textures/tools", "textures/trident"),
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
	}
}
