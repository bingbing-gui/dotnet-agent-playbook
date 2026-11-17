# 单位转换脚本
# 使用乘法系数进行数值转换：result = value × factor
#
# 用法：
#   python scripts/convert.py --value 26.2 --factor 1.60934
#   python scripts/convert.py --value 75 --factor 2.20462

import argparse
import json


def main() -> None:
    parser = argparse.ArgumentParser(
        description="使用乘法系数对数值进行转换。",
        epilog="示例：\n"
        "  python scripts/convert.py --value 26.2 --factor 1.60934\n"
        "  python scripts/convert.py --value 75 --factor 2.20462",
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    parser.add_argument("--value", type=float, required=True, help="需要转换的数值。")
    parser.add_argument("--factor", type=float, required=True, help="来自换算表的转换系数。")
    args = parser.parse_args()

    if args.value is None or args.factor is None:
        print(json.dumps({"error": "missing parameters"}))
        return
    result = round(args.value * args.factor, 4)
    print(json.dumps({"value": args.value, "factor": args.factor, "result": result}))


if __name__ == "__main__":
    main()