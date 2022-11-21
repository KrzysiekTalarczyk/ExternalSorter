using ExternalSorter.Model;

namespace ExternalSorter.Sorter
{
    internal class ElementSrtedList
    {
        private readonly List<Element> sortedList;

        public ElementSrtedList()
        {
            sortedList = new List<Element>();
        }

        public List<Element> SortedList { get { return sortedList; } }
        
        public void Insert(string? line)
        {
            if (line != null)
            {
                var element = Element.TryParse(line);
                if (element is not null)
                {
                    sortedList.Add(element);
                }
            }
        }
    }
}
