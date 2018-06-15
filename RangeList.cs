using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Production_Validator
{
    public class RangeList : IEnumerable<Range>
    {
        private readonly List<Range> ranges = new List<Range>();

        public IEnumerator<Range> GetEnumerator()
        {
            return ranges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Range rangeToAdd)
        {
            var mergableRange = new List<Range>();
            foreach (var range in ranges)
            {
                if (rangeToAdd.Start == range.Start && rangeToAdd.End == range.End)
                    return; // already exists

                if (mergableRange.Any())
                {
                    if (rangeToAdd.End >= range.Start - 1) mergableRange.Add(range);
                }
                else
                {
                    if (rangeToAdd.Start >= range.Start - 1
                        && rangeToAdd.Start <= range.End + 1)
                    {
                        mergableRange.Add(range);
                        continue;
                    }

                    if (range.Start >= rangeToAdd.Start
                        && range.End <= rangeToAdd.End)
                        mergableRange.Add(range);
                }
            }

            if (!mergableRange.Any()) //Standalone range
            {
                ranges.Add(rangeToAdd);
            }
            else //merge overlapping ranges
            {
                mergableRange.Add(rangeToAdd);
                var min = mergableRange.Min(x => x.Start);
                var max = mergableRange.Max(x => x.End);
                foreach (var range in mergableRange) ranges.Remove(range);
                ranges.Add(new Range(min, max));
            }

            SortAndMerge();
        }

        public void Add(long value)
        {
            // is it within or contiguous to an existing range
            foreach (var range in ranges)
            {
                if (value >= range.Start && value <= range.End)
                    return; // already in a range
                if (value == range.Start - 1)
                {
                    range.Update(value, range.End);
                    SortAndMerge();
                    return;
                }

                if (value == range.End + 1)
                {
                    range.Update(range.Start, value);
                    SortAndMerge();
                    return;
                }
            }

            // not in any ranges
            ranges.Add(value);
            SortAndMerge();
        }

        private void SortAndMerge()
        {
            if (ranges.Count > 1)
            {
                ranges.Sort((a, b) => a.Start.CompareTo(b.Start));
                var i = ranges.Count - 1;
                do
                {
                    var start = ranges[i].Start;
                    var end = ranges[i - 1].End;
                    if (end == start - 1)
                    {
                        // merge and remove
                        ranges[i - 1].Update(ranges[i - 1].Start, ranges[i].End);
                        ranges.RemoveAt(i);
                    }
                } while (i-- > 1);
            }
        }
    }

    public class Range
    {
        public Range(long startEnd) : this(startEnd, startEnd)
        {
        }

        public Range(long start, long end)
        {
            if (end >= start)
            {
                Start = start;
                End = end;
            }
            else
            {
                Start = end;
                End = start;
            }
        }

        public long Start { get; private set; }
        public long End { get; private set; }

        public void Update(long newStart, long newEnd)
        {
            Start = newStart;
            End = newEnd;
        }

        public static implicit operator Range(long i)
        {
            return new Range(i);
        }

        public override string ToString()
        {
            if (Start == End)
                return Start.ToString();
            return string.Format("{0}-{1}", Start, End);
        }
    }
}