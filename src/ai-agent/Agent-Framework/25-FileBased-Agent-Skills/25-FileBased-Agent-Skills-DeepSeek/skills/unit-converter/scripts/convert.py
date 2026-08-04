# 单位转换脚本
# 使用乘法系数进行数值转换：result = value × factor

import argparse
import json


def main() -> None:
	parser = argparse.ArgumentParser(description="使用乘法系数对数值进行转换。")
	parser.add_argument("--value", type=float, required=True, help="需要转换的数值。")
	parser.add_argument("--factor", type=float, required=True, help="来自换算表的转换系数。")
	args = parser.parse_args()

	result = round(args.value * args.factor, 4)
	print(json.dumps({"value": args.value, "factor": args.factor, "result": result}))


if __name__ == "__main__":
	main()
