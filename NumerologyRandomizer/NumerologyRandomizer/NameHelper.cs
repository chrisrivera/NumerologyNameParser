using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NumerologyRandomizer
{
    public class NameHelper
    {
        private readonly string _lastName;
        private readonly string _lastNameNumbers_P;
        private readonly List<int> _lastNameNumbersList_P;
        private readonly Dictionary<char, int> _p;

        public NameHelper(string lastName)
        {

            //_p = GetNumerologicalValuesDictionary_Pythagorean();
            _p = GetNumerologicalValuesDictionary_ChaldeanAndIndian();
            _lastName = lastName;
            _lastNameNumbers_P = GenerateNumberString_P(_lastName);
            _lastNameNumbersList_P = GenerateDistinctNumbers_P(_lastName);

        }
                
        public IEnumerable<string> OpenFile(string path)
        {
            return File.ReadLines(path);
        }

        public int DetermineNumber(char input)
        {
            return _p.Single(kvp => kvp.Key == char.ToUpper(input)).Value;
        }
        
        public ResultObject CalculateNumbersForFirstAndMiddleNames(string p, string p_2)
        {
            ResultObject obj = new ResultObject();
            obj.FirstName = p;
            obj.FirstNameNumbers_P = GenerateNumberString_P(p);
            obj.MiddleName = p_2;
            obj.MiddleNameNumbers_P = GenerateNumberString_P(p_2);
            obj.LastName = _lastName;
            obj.LastNameNumbers_P = _lastNameNumbers_P;

            var nums = GenerateDistinctNumbers_P(string.Concat(p, p_2, _lastName));
            obj.AllNumbersUsed_P = AllNumbersAreUsed(nums);

            obj.SoulNumber = DetermineSoulNumber_P(string.Concat(p, p_2, _lastName));
            obj.PathOfDestinyNumber = DetermineDestinyNumber_P(string.Concat(p, p_2, _lastName));
            obj.OuterPersonalityNumber = DeterminePersonalityNumber_P(string.Concat(p, p_2, _lastName));

            obj.SoulNumber_2 = DetermineSoulNumber_P_2(string.Concat(p, p_2, _lastName));
            obj.PathOfDestinyNumber_2 = DetermineDestinyNumber_P_2(string.Concat(p, p_2, _lastName));
            obj.OuterPersonalityNumber_2 = DeterminePersonalityNumber_P_2(string.Concat(p, p_2, _lastName));

            return obj;
        }

        public ResultObject CalculateNumbersForFirstNameOnly(string p)
        {
            ResultObject obj = new ResultObject();
            obj.FirstName = p;
            obj.FirstNameNumbers_P = GenerateNumberString_P(p);
            obj.LastName = _lastName;
            obj.LastNameNumbers_P = _lastNameNumbers_P;

            var nums = GenerateDistinctNumbers_P(string.Concat(p, _lastName));
            obj.AllNumbersUsed_P = AllNumbersAreUsed(nums);

            obj.SoulNumber = DetermineSoulNumber_P(string.Concat(p, _lastName));
            obj.PathOfDestinyNumber = DetermineDestinyNumber_P(string.Concat(p, _lastName));
            obj.OuterPersonalityNumber = DeterminePersonalityNumber_P(string.Concat(p, _lastName));

            obj.SoulNumber_2 = DetermineSoulNumber_P_2(string.Concat(p, _lastName));
            obj.PathOfDestinyNumber_2 = DetermineDestinyNumber_P_2(string.Concat(p, _lastName));
            obj.OuterPersonalityNumber_2 = DeterminePersonalityNumber_P_2(string.Concat(p, _lastName));

            return obj;
        }

        public string GenerateNumberString_P(string p)
        {
            return string.Join(", ", GetNameNumberList_P(p));
        }
        
        public List<int> GenerateDistinctNumbers_P(string p)
        {
            return GetNameNumberList_P(p).Distinct().ToList();
        }

        private int DetermineSoulNumber_P(string p)
        {
            int total = 0;
            //vowels only
            foreach (char c in p.ToCharArray())
            {
                if (IsVowel(c))
                {
                    total = total + DetermineNumber(c);
                }
            }
            return BreakDownNumberToSingleDigit(total);
        }

        private int DeterminePersonalityNumber_P(string p)
        {
            int total = 0;
            //vowels only
            foreach (char c in p.ToCharArray())
            {
                if (IsConsonant(c))
                {
                    total = total + DetermineNumber(c);
                }
            }
            return BreakDownNumberToSingleDigit(total);
        }

        private int DetermineDestinyNumber_P(string p)
        {
            int total = 0;
            //vowels only
            foreach (char c in p.ToCharArray())
            {
                total = total + DetermineNumber(c);
            }            
            return BreakDownNumberToSingleDigit(total);
        }

        private int DetermineSoulNumber_P_2(string p)
        {
            int total = 0;
            //vowels only
            foreach (char c in p.ToCharArray())
            {
                if (IsVowel(c))
                {
                    total = total + DetermineNumber(c);
                }
            }
            return BreakDownNumberToDoubleDigit(total);
        }

        private int DeterminePersonalityNumber_P_2(string p)
        {
            int total = 0;
            //vowels only
            foreach (char c in p.ToCharArray())
            {
                if (IsConsonant(c))
                {
                    total = total + DetermineNumber(c);
                }
            }
            return BreakDownNumberToDoubleDigit(total);
        }

        private int DetermineDestinyNumber_P_2(string p)
        {
            int total = 0;
            //vowels only
            foreach (char c in p.ToCharArray())
            {
                total = total + DetermineNumber(c);
            }
            return BreakDownNumberToDoubleDigit(total);
        }

        private bool IsVowel(char c)
        {
            char comparer = char.ToUpper(c);
            return (comparer == 'A' || comparer == 'E' || comparer == 'I' || comparer == 'O' || comparer == 'U'); //|| comparer == 'Y');
        }

        private bool IsConsonant(char c)
        {
            char comparer = char.ToUpper(c);
            return (comparer != 'A' && comparer != 'E' && comparer != 'I' && comparer != 'O' && comparer != 'U');
        }

        private int BreakDownNumberToSingleDigit(int num)
        {
            if (num <= 9)
            {
                return num;
            }
            else
            {
                string numbersStr = num.ToString();
                char[] nums = numbersStr.ToCharArray();
                int newTotal = 0;
                foreach (char cNum in nums)
                {
                    newTotal = int.Parse(cNum.ToString()) + newTotal;
                }
                return BreakDownNumberToSingleDigit(newTotal);
            }
        }

        private int BreakDownNumberToDoubleDigit(int num)
        {
            if (num <= 99)
            {
                return num;
            }
            else
            {
                string numbersStr = num.ToString();
                char[] nums = numbersStr.ToCharArray();
                int newTotal = 0;
                foreach (char cNum in nums)
                {
                    newTotal = int.Parse(cNum.ToString()) + newTotal;
                }
                return BreakDownNumberToDoubleDigit(newTotal);
            }
        }

        private string GenerateNumberString(List<int> numbers)
        {
            return string.Join(", ", numbers.OrderBy(p => p));
        }

        private bool AllNumbersAreUsed(List<int> distinctNums)
        {
            return distinctNums.Count == 9;
        }

        private List<int> GetNameNumberList_P(string p)
        {
            List<int> numbers = new List<int>();
            foreach (char c in p)
            {
                numbers.Add(DetermineNumber(c));
            }
            return numbers;
        }

        private Dictionary<char, int> GetNumerologicalValuesDictionary_ChaldeanAndIndian()
        {
            Dictionary<char, int> values = new Dictionary<char, int>();
            values.Add(' ', 0);
            values.Add('A', 1);
            values.Add('B', 2);
            values.Add('C', 3);
            values.Add('D', 4);
            values.Add('E', 5);
            values.Add('F', 8);
            values.Add('G', 3);
            values.Add('H', 5);
            values.Add('I', 1);
            values.Add('J', 1);
            values.Add('K', 2);
            values.Add('L', 3);
            values.Add('M', 4);
            values.Add('N', 5);
            values.Add('O', 7);
            values.Add('P', 8);
            values.Add('Q', 1);
            values.Add('R', 2);
            values.Add('S', 3);
            values.Add('T', 4);
            values.Add('U', 6);
            values.Add('V', 6);
            values.Add('W', 6);
            values.Add('X', 5);
            values.Add('Y', 1);
            values.Add('Z', 7);
            return values;
        }

        private Dictionary<char, int> GetNumerologicalValuesDictionary_Pythagorean()
        {
            Dictionary<char, int> values = new Dictionary<char, int>();
            values.Add(' ', 0); 
            values.Add('A', 1);
            values.Add('B', 2);
            values.Add('C', 3);
            values.Add('D', 4);
            values.Add('E', 5);
            values.Add('F', 6);
            values.Add('G', 7);
            values.Add('H', 8);
            values.Add('I', 9);
            values.Add('J', 1);
            values.Add('K', 2);
            values.Add('L', 3);
            values.Add('M', 4);
            values.Add('N', 5);
            values.Add('O', 6);
            values.Add('P', 7);
            values.Add('Q', 8);
            values.Add('R', 9);
            values.Add('S', 1);
            values.Add('T', 2);
            values.Add('U', 3);
            values.Add('V', 4);
            values.Add('W', 5);
            values.Add('X', 6);
            values.Add('Y', 7);
            values.Add('Z', 8);
            return values;
        }
        
    }
}
