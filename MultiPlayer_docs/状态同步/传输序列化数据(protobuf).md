``` protobuf
// 三维向量3D向量（整数坐标）
message Vec3 {
  int32 x = 1;
  int32 y = 2;
  int32 z = 3;
}

// 实体信息
message NetEntity {
  int32 id = 1;               // 实体ID
  Vec3 position = 2;      // 位置坐标
  Vec3 direction = 3;     // 方向向量
  int32 speed=4;              //速度
}

// 实体状态枚举
enum EntityState {
  NONE = 0;   // 无状态
  IDLE = 1;   // 空闲状态
  MOVE = 2;   // 移动状态
  JUMP = 3;   // 跳跃状态
}
```

- 状态同步主要同步核心属性位置、旋转、速度、动画状态；
- 位置和旋转通过自定的Vector3进行网络数据传输，protobuf默认使用Varint编码，int32占用的字节实际少更小，但是float即使数值只有一位还是占用4个字节，所以使用int32减少带宽压力；
- 动画状态主要是同步状态的枚举，在动画控制脚本中通过有限状态机根据不同的状态切换不同的动画参数；