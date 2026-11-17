---
name: unit-converter
description: 用于执行单位转换，通过 value 和 factor 计算结果
---

## 使用方法

当用户请求单位转换时：

1. 首先查看 `references/conversion-table.md`，找到对应的换算系数  
2. 运行 `scripts/convert.py` 脚本，并传入参数 `--value <数值> --factor <系数>`  
   （例如：`--value 26.2 --factor 1.60934`）  
3. 将转换结果清晰地展示出来，并同时标明原单位和目标单位