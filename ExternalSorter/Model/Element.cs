namespace ExternalSorter.Model
{
    public class Element : IComparable<Element>
    {
        public int Number { get; set; }
        public string Text { get; set; }

        public Element(string line)
        {
            var parts = line.Split(".", 2);
            Number = int.Parse(parts[0]);
            Text = parts[1];
        }

        public Element(int number, string text)
        {
            Number = number;
            Text = text;    
        }

        public int CompareTo(Element? second)
        {
            var compareResult = string.Compare(Text, second.Text, StringComparison.InvariantCulture);
            if (compareResult == 0)
            {
                return Number.CompareTo(second.Number);
            }
            return compareResult;
        }

        /// <summary>
        /// Parse Line To Element. When some exception return null.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Element? TryParse(string line)
        {
            try
            {
                return new Element(line);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public override string ToString()
        {
            return $"{Number}.{Text}";
        }
    }
}
