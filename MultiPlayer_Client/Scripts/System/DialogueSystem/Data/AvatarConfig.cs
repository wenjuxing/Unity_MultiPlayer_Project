using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 对话头像配置文件
/// </summary>
[CreateAssetMenu(fileName = "AvatarConfig",menuName = "Dialogue/AvatarConfig")]
public class AvatarConfig :ScriptableObject
{
    public string avatarPrefix = "Assets/Sprites/Avatars/";
}
