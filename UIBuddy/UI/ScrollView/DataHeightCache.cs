using System;
using System.Collections.Generic;
using System.Linq;

namespace UIBuddy.UI.ScrollView;

/// <summary>
/// Used to handle the underlying height data for a scroll pool, tracking which data values are at which position and how far they span.<br/><br/>
/// 
/// A DataHeightCache is created and managed automatically by a ScrollPool, you do not need to use this class yourself.
/// </summary>
public class DataHeightCache<T> where T : ICell
{
    private ScrollPool<T> ScrollPool { get; }

    public DataHeightCache(ScrollPool<T> scrollPool)
    {
        ScrollPool = scrollPool;
    }

    private readonly List<DataViewInfo> _heightCache = new();

    public DataViewInfo this[int index]
    {
        get => _heightCache[index];
        set => SetIndex(index, value);
    }

    public int Count => _heightCache.Count;

    public float TotalHeight => _totalHeight;
    private float _totalHeight;

    private float DefaultHeight => _mDefaultHeight ?? (float)(_mDefaultHeight = ScrollPool.PrototypeHeight);
    private float? _mDefaultHeight;

    /// <summary>
    /// Lookup table for "which data index first appears at this position"<br/>
    /// Index: DefaultHeight * index from top of data<br/>
    /// Value: the first data index at this position<br/>
    /// </summary>
    private readonly List<int> _rangeCache = new();

    /// <summary>Same as GetRangeIndexOfPosition, except this rounds up to the next division if there was remainder from the previous cell.</summary>
    private int GetRangeCeilingOfPosition(float position) => (int)Math.Ceiling((decimal)position / (decimal)DefaultHeight);

    /// <summary>Get the first range (division of DefaultHeight) which the position appears in.</summary>
    private int GetRangeFloorOfPosition(float position) => (int)Math.Floor((decimal)position / (decimal)DefaultHeight);

    public int GetFirstDataIndexAtPosition(float desiredHeight)
    {
        if (!_heightCache.Any())
            return 0;

        int rangeIndex = GetRangeFloorOfPosition(desiredHeight);

        // probably shouldnt happen but just in case
        if (rangeIndex < 0)
            return 0;
        if (rangeIndex >= _rangeCache.Count)
        {
            int idx = ScrollPool.DataSource.ItemCount - 1;
            return idx;
        }

        int dataIndex = _rangeCache[rangeIndex];
        DataViewInfo cache = _heightCache[dataIndex];

        // if the DataViewInfo is outdated, need to rebuild
        int expectedMin = GetRangeCeilingOfPosition(cache.startPosition);
        int expectedMax = expectedMin + cache.normalizedSpread - 1;
        if (rangeIndex < expectedMin || rangeIndex > expectedMax)
        {
            RecalculateStartPositions(ScrollPool.DataSource.ItemCount - 1);

            rangeIndex = GetRangeFloorOfPosition(desiredHeight);
            dataIndex = _rangeCache[rangeIndex];
        }

        return dataIndex;
    }

    /// <summary>
    /// Get the spread of the height, starting from the start position.<br/><br/>
    /// The "spread" begins at the start of the next interval of the DefaultHeight, then increases for
    /// every interval beyond that.
    /// </summary>
    private int GetRangeSpread(float startPosition, float height)
    {
        // get the remainder of the start position divided by min height
        float rem = startPosition % DefaultHeight;

        // if there is a remainder, this means the previous cell started in  our first cell and
        // they take priority, so reduce our height by (minHeight - remainder) to account for that.
        // We need to fill that gap and reach the next cell before we take priority.
        if (rem != 0.0f)
            height -= DefaultHeight - rem;

        return (int)Math.Ceiling((decimal)height / (decimal)DefaultHeight);
    }

    /// <summary>Append a data index to the cache with the provided height value.</summary>
    public void Add(float value)
    {
        value = Math.Max(DefaultHeight, value);

        int spread = GetRangeSpread(_totalHeight, value);

        _heightCache.Add(new DataViewInfo(_heightCache.Count, value, _totalHeight, spread));

        int dataIdx = _heightCache.Count - 1;
        for (int i = 0; i < spread; i++)
            _rangeCache.Add(dataIdx);

        _totalHeight += value;
    }

    /// <summary>Remove the last (highest count) index from the height cache.</summary>
    public void RemoveLast()
    {
        if (!_heightCache.Any())
            return;

        _totalHeight -= _heightCache[_heightCache.Count - 1];
        _heightCache.RemoveAt(_heightCache.Count - 1);

        int idx = _heightCache.Count;
        while (_rangeCache.Count > 0 && _rangeCache[_rangeCache.Count - 1] == idx)
            _rangeCache.RemoveAt(_rangeCache.Count - 1);
    }

    /// <summary>Set a given data index with the specified value.</summary>
    public void SetIndex(int dataIndex, float height)
    {
        height = (float)Math.Floor(height);
        height = Math.Max(DefaultHeight, height);

        // If the index being set is beyond the DataSource item count, prune and return.
        if (dataIndex >= ScrollPool.DataSource.ItemCount)
        {
            while (_heightCache.Count > dataIndex)
                RemoveLast();
            return;
        }

        // If the data index exceeds our cache count, fill the gap.
        // This is done by the ScrollPool when the DataSource sets its initial count, or the count increases.
        if (dataIndex >= _heightCache.Count)
        {
            while (dataIndex > _heightCache.Count)
                Add(DefaultHeight);
            Add(height);
            return;
        }

        // We are actually updating an index. First, update the height and the totalHeight.
        DataViewInfo cache = _heightCache[dataIndex];
        if (cache.height != height)
        {
            float diff = height - cache.height;
            _totalHeight += diff;
            cache.height = height;
        }

        // update our start position using the previous cell (if it exists)
        if (dataIndex > 0)
        {
            DataViewInfo prev = _heightCache[dataIndex - 1];
            cache.startPosition = prev.startPosition + prev.height;
        }

        // Get the normalized range index (actually ceiling) and spread based on our start position and height
        int rangeIndex = GetRangeCeilingOfPosition(cache.startPosition);
        int spread = GetRangeSpread(cache.startPosition, height);

        // If the previous item in the range cache is not the previous data index, there is a gap.
        if (_rangeCache[rangeIndex] != dataIndex)
        {
            // Recalculate start positions up to this index. The gap could be anywhere before here.
            RecalculateStartPositions(ScrollPool.DataSource.ItemCount - 1);
            // Get the range index and spread again after rebuilding
            rangeIndex = GetRangeCeilingOfPosition(cache.startPosition);
            spread = GetRangeSpread(cache.startPosition, height);
        }

        if (_rangeCache[rangeIndex] != dataIndex)
            throw new IndexOutOfRangeException($"Trying to set dataIndex {dataIndex} at rangeIndex {rangeIndex}, but cache is corrupt or invalid!");

        if (spread != cache.normalizedSpread)
        {
            int spreadDiff = spread - cache.normalizedSpread;
            cache.normalizedSpread = spread;

            UpdateSpread(dataIndex, rangeIndex, spreadDiff);
        }

        // set the struct back to the array
        _heightCache[dataIndex] = cache;
    }

    private void UpdateSpread(int dataIndex, int rangeIndex, int spreadDiff)
    {
        if (spreadDiff > 0)
        {
            while (_rangeCache[rangeIndex] == dataIndex && spreadDiff > 0)
            {
                _rangeCache.Insert(rangeIndex, dataIndex);
                spreadDiff--;
            }
        }
        else
        {
            while (_rangeCache[rangeIndex] == dataIndex && spreadDiff < 0)
            {
                _rangeCache.RemoveAt(rangeIndex);
                spreadDiff++;
            }
        }
    }

    private void RecalculateStartPositions(int toIndex)
    {
        if (_heightCache.Count <= 1)
            return;

        _rangeCache.Clear();

        DataViewInfo cache;
        DataViewInfo prev = DataViewInfo.None;
        for (int idx = 0; idx <= toIndex && idx < _heightCache.Count; idx++)
        {
            cache = _heightCache[idx];

            if (!prev.Equals(DataViewInfo.None))
                cache.startPosition = prev.startPosition + prev.height;
            else
                cache.startPosition = 0;

            cache.normalizedSpread = GetRangeSpread(cache.startPosition, cache.height);
            for (int i = 0; i < cache.normalizedSpread; i++)
                _rangeCache.Add(cache.dataIndex);

            _heightCache[idx] = cache;

            prev = cache;
        }
    }
}