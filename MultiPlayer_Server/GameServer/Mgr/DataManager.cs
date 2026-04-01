using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Summer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class DataManager : Singleton<DataManager>
{

    public Dictionary<int, SpaceDefine> Spaces;
    public Dictionary<int, UnitDefine> Units;
    public Dictionary<int, SpawnDefine> Spawns;
    public Dictionary<int, SkillDefine> Skills;
    public Dictionary<int, ItemDefine> Items;
    public Dictionary<int, TaskDefine> Tasks;
    public Dictionary<int, ShopDefine> GoodsItem;

    //对话数据
    public Dictionary<int, DialogueMain> dialogueMains;
    public Dictionary<int, DialogueGroup> dialogueGroups;
    public Dictionary<int, DialogueData> dialogueDatas;
    public Dictionary<int, DialogueOption> dialogueOptions;
    public void Init()
    {
        Spaces = Load<SpaceDefine>("Data/SpaceDefine.json");
        Units = Load<UnitDefine>("Data/UnitDefine.json");
        Spawns = Load<SpawnDefine>("Data/SpawnDefine.json");
        Skills = Load<SkillDefine>("Data/SkillDefine.json");
        Items = Load<ItemDefine>("Data/ItemDefine.json");
        Tasks = Loadplus<TaskDefine>("Data/TaskDefine.json");
        GoodsItem = Load<ShopDefine>("Data/ShopDefine.json");

        //对话数据
        dialogueMains = Load<DialogueMain>("Data/DialogueMain.json");
        dialogueGroups = Load<DialogueGroup>("Data/DialogueGroup.json");
        dialogueDatas = Load<DialogueData>("Data/DialogueData.json");
        dialogueOptions = Load<DialogueOption>("Data/DialogueOption.json");

    }

    /// <summary>
    /// 此方法用于把对象结构的JSON文件直接转字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public Dictionary<int, T> Load<T>(string filePath)
    {
        // 获取exe文件所在目录的绝对路径
        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string exeDirectory = Path.GetDirectoryName(exePath);
        // 构建1.txt文件的完整路径
        string txtFilePath = Path.Combine(exeDirectory, filePath);
        // 读取1.txt文件的内容
        string content = File.ReadAllText(txtFilePath);
        // 打印1.txt文件的内容
        //Console.WriteLine(content);
        return JsonConvert.DeserializeObject<Dictionary<int, T>>(content, settings);
    }
    /// <summary>
    /// 此方法用于把数组结构的JSON文件先转列表再转字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public  Dictionary<int, T> Loadplus<T>(string filePath)
    {
        // 1. 构建文件完整路径（和你原始逻辑一致）
        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string exeDirectory = Path.GetDirectoryName(exePath);
        string txtFilePath = Path.Combine(exeDirectory, filePath);

        // 2. 检查文件是否存在
        if (!File.Exists(txtFilePath))
        {
            throw new FileNotFoundException("配置文件不存在", txtFilePath);
        }

        // 3. 读取JSON内容（指定UTF-8编码，避免中文乱码）
        string content = File.ReadAllText(txtFilePath, System.Text.Encoding.UTF8);

        // 4. 第一步：反序列化JSON数组→List<T>（核心修改）
        List<T> configList = JsonConvert.DeserializeObject<List<T>>(content, settings);
        if (configList == null || configList.Count == 0)
        {
            Console.WriteLine($"警告：配置文件 {filePath} 为空或格式错误，返回空字典");
            return new Dictionary<int, T>();
        }

        // 5. 第二步：List<T>→Dictionary<int, T>（用TaskId作为Key）
        // 反射获取T的TaskId属性（确保T有公开的int类型TaskId）
        var taskIdProperty = typeof(T).GetProperty("TaskId");
        if (taskIdProperty == null || taskIdProperty.PropertyType != typeof(int) || !taskIdProperty.CanRead)
        {
            throw new ArgumentException($"类型 {typeof(T).Name} 必须包含公开的int类型 TaskId 属性");
        }

        // 遍历List，构建字典
        Dictionary<int, T> configDict = new Dictionary<int, T>();
        foreach (var item in configList)
        {
            int taskId = (int)taskIdProperty.GetValue(item); // 获取当前项的TaskId
            if (configDict.ContainsKey(taskId))
            {
                Console.WriteLine($"警告：配置文件中存在重复的TaskId={taskId}，已忽略后续重复项");
                continue;
            }
            configDict.Add(taskId, item);
        }

        Console.WriteLine($"成功加载配置：{filePath}，共 {configDict.Count} 条数据");
        return configDict;
    }
    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        Converters = new JsonConverter[] {
            new FloatArrayConverter(),
            new IntArrayConverter(),
        }
    };

    //float[]
    public class FloatArrayConverter : JsonConverter<float[]>
    {
        public override float[] ReadJson(JsonReader reader, Type objectType, float[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.String)
            {
                string[] values = token.ToString().Replace("[", "").Replace("]", "").Split(',');
                float[] result = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    float.TryParse(values[i], out result[i]);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, float[] value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    //int[]
    public class IntArrayConverter : JsonConverter<int[]>
    {
        public override int[] ReadJson(JsonReader reader, Type objectType, int[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.String)
            {
                string[] values = token.ToString().Replace("[", "").Replace("]", "").Split(',');
                int[] result = new int[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    int.TryParse(values[i], out result[i]);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, int[] value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }


}
