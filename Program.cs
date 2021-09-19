using System;
using System.Text;
using System.Collections.Generic;

namespace Calculation
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("请输入表达式(现支持+，-，*，/以及小括号):");
                Operation operation = new Operation();
                string str = Console.ReadLine();            // "4+(1+2)*3";
                decimal result = operation.CalculationResult(str);
                Console.WriteLine($"{str} = {result}");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }
    }
}
