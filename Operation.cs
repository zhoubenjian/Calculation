using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculation
{
    /// <summary>
    /// 运算符详情（优先级，操作个数，结合规则，运算符类型, 运算）
    /// </summary>
    public record OperatorInfo(int priority, int countOfNum, Associative associative, OperatorType ot, 
        Func<decimal[], decimal> func, string desc);


    public class Operation
    {
        private Stack<string> tempStack;                // 逆波兰表达式
        private Stack<string> optStack;                 // 操作符栈
        private Dictionary<string, OperatorInfo> dic;

        public Operation()
        {
            tempStack = new Stack<string>();
            optStack = new Stack<string>();

            dic = new Dictionary<string, OperatorInfo>()
            {
                { "#", new OperatorInfo(0, 0, Associative.LEFT, OperatorType.OPERTOR, param => param[0], "#") },
                { "+", new OperatorInfo(1, 2, Associative.LEFT, OperatorType.OPERTOR, param => param[0] + param[1], "加") },
                { "-", new OperatorInfo(1, 2, Associative.LEFT, OperatorType.OPERTOR, param => param[0] - param[1], "减") },
                { "*", new OperatorInfo(10, 2, Associative.LEFT, OperatorType.OPERTOR, param => param[0] * param[1], "加") },
                { "/", new OperatorInfo(10, 2, Associative.LEFT, OperatorType.OPERTOR, param => param[0] / param[1], "除") }
            };

            // optStack首先压入#
            optStack.Push("#");
        }


        /// <summary>
        /// 计算逻辑
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public decimal CalculationResult(string str)
        {
            bool isNum = false;
            bool isOpt = false;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {           
                if ('('.Equals(str[i]))
                {
                    // 操作sb
                    StringBuilderOperation(sb);

                    // 直接置于optStack栈顶
                    optStack.Push(str[i].ToString());
                    continue;
                }
                else if (')'.Equals(str[i]))
                {
                    // 操作sb
                    StringBuilderOperation(sb);

                    while (true)
                    {
                        if (!"(".Equals(optStack.Peek()))

                            // 将栈顶至"("之间的所有操作符一次压入optStack
                            tempStack.Push(optStack.Pop());

                        else 
                        {
                            // 移除"("操作符
                            optStack.Pop();
                            break;
                        }

                        // 终止操作符
                        if ("#".Equals(optStack.Peek()))
                        {
                            break;
                        }
                    }
                    continue;
                } 
                else if (CharIsNum(str[i]))
                {
                    isNum = true;
                    isOpt = false;
                }
                else if (!CharIsNum(str[i]))
                {
                    isOpt = true;
                    isNum = false;
                }

                if (sb.Length == 0)
                {
                    sb.Append(str[i]);
                    continue;
                }

                // 操作数
                if (isNum == true && isOpt == false)
                {
                    if (CharIsNum(LastIndexOfStringBuilder(sb)))

                        // StringBuilder最后一位是操作数
                        sb.Append(str[i]);   
                    
                    else
                    {
                        // 操作符压栈
                        PushOperator(sb.ToString());

                        // 清空
                        sb.Clear();

                        // 保存操作符
                        sb.Append(str[i]);
                    }
                }          

                // 操作符
                if (isOpt == true && isNum == false)
                {                
                    if (!CharIsNum(LastIndexOfStringBuilder(sb)))
                  
                        // StringBuilder最后一位是操作符
                        sb.Append(str[i]);
                    
                    else
                    {
                        // 操作数压栈
                        tempStack.Push(sb.ToString());

                        // 清空
                        sb.Clear();

                        // 保存操作符
                        sb.Append(str[i]);
                    }
                }
            }

            // 操作sb
            StringBuilderOperation(sb);

            // optStack剩余符号，依次弹出压入tempStack
            while (!"#".Equals(optStack.Peek()))
            {
                tempStack.Push(optStack.Pop());
            }

            // 最终表达式(逆波兰表达式的倒置)
            Stack<string> finalStack = new Stack<string>();
            // 逆波兰表达式 转置 最终结果
            while (tempStack.Count > 0)
            {
                finalStack.Push(tempStack.Pop());
            }

            ///*** 遍历结果(仅测试打印) ***/
            //while (finalStack.Count > 0)
            //{
            //    Console.Write($"{finalStack.Pop()} ");
            //}
            ///*** 遍历结果(仅测试打印) ***/

            return Result(finalStack);
        }



        public void StringBuilderOperation(StringBuilder sb)
        {
            if (sb.Length != 0)
            {
                decimal value = decimal.Zero;
                if (StringIsNum(sb.ToString(), out value))
                {
                    tempStack.Push(sb.ToString());
                }
                else
                {
                    PushOperator(sb.ToString());
                }

                // 清空StringBuilder
                sb.Clear();
            }
        }

        /// <summary>
        /// 计算结果
        /// </summary>
        /// <param name="finalStack"></param>
        /// <returns></returns>
        public decimal Result(Stack<string> finalStack)
        {
            List<decimal> tempList = new List<decimal>();
            while (finalStack.Count > 0)
            {
                decimal value = decimal.Zero;
                if (StringIsNum(finalStack.Peek(), out value))
                {
                    // 保存操作数
                    tempList.Add(value);                      
                }
                else
                {
                    int countOfNum = dic[finalStack.Peek()].countOfNum;
                    Func<decimal[], decimal> func = dic[finalStack.Peek()].func;
                    Associative associative = dic[finalStack.Peek()].associative;

                    // 运算
                    decimal tempValue = func(tempList.GetRange(tempList.Count - countOfNum, countOfNum).ToArray());
                    
                    // 移除tempList中已计算的操作数
                    tempList.RemoveRange(tempList.Count - countOfNum, countOfNum);

                    // 添加临时结果tempValue
                    tempList.Add(tempValue);
                }

                // 弹出栈顶操作数
                finalStack.Pop();
            }

            return tempList[0];
        }

        /// <summary>
        /// 操作符压栈
        /// </summary>
        /// <param name="optStr"></param>
        public void PushOperator(string optStr)
        {
            if ("(".Equals(optStack.Peek()) || dic[optStack.Peek()].priority < dic[optStr].priority)
            {
                // 操作符压栈optStack
                optStack.Push(optStr);

            }
            else
            {
                while (true)
                {
                    if (dic[optStack.Peek()].priority >= dic[optStr].priority)
                    {
                        // optStack栈顶符号弹出，压入tempStack
                        tempStack.Push(optStack.Pop());
                    }
                    else
                    {
                        // 操作符压栈optStack
                        optStack.Push(optStr);

                        break;
                    }

                    if ("#".Equals(dic[optStack.Peek()]))
                    {
                        // 操作符压栈optStack
                        optStack.Push(optStr);

                        break;
                    }
                }
            }

            //if ("#".Equals(dic[optStack.Peek()]))
            //{
            //    // 操作符压栈optStack
            //    optStack.Push(optStr);
            //}
            //else
            //{
            //    if (dic[optStack.Peek()].priority < dic[optStr].priority)
            //    {
            //        // 操作符压栈optStack
            //        optStack.Push(optStr);
            //    }
            //    else
            //    {
            //        while (true)
            //        {
            //            if (dic[optStack.Peek()].priority >= dic[optStr].priority)
            //            {
            //                // optStack栈顶符号弹出，压入tempStack
            //                tempStack.Push(optStack.Pop());
            //            }
            //            else
            //            {
            //                // 操作符压栈optStack
            //                optStack.Push(optStr);

            //                break;
            //            }

            //            if ("#".Equals(dic[optStack.Peek()]))
            //            {
            //                // 操作符压栈optStack
            //                optStack.Push(optStr);

            //                break;
            //            }
            //        }
            //    }
            //}
            
        }

        /// <summary>
        /// 判断数字字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool CharIsNum(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断数字字符串
        /// </summary>
        /// <param name="s"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool StringIsNum(string s, out decimal value)
        {
            return decimal.TryParse(s, out value);
        }

        /// <summary>
        /// StringBuilder最后一位
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        public char LastIndexOfStringBuilder(StringBuilder sb)
        {
            char c = sb[sb.Length - 1];
            return c;
        }
    }
}
