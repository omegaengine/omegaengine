/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Xunit;

namespace OmegaEngine.Foundation.Collections;

/// <summary>
/// Contains test methods for <see cref="Pool{T}"/>.
/// </summary>
public class PoolTest
{
    /// <summary>
    /// A simple test class that implements IPoolable for testing purposes.
    /// </summary>
    private class TestPoolable(string name) : IPoolable<TestPoolable>
    {
        public string Name { get; } = name;
        public TestPoolable? NextElement { get; set; }
    }

    [Fact]
    public void TestAddAndCount()
    {
        var pool = new Pool<TestPoolable>();
        pool.Count.Should().Be(0);

        var item1 = new TestPoolable("Item1");
        pool.Add(item1);
        pool.Count.Should().Be(1);

        var item2 = new TestPoolable("Item2");
        pool.Add(item2);
        pool.Count.Should().Be(2);

        var item3 = new TestPoolable("Item3");
        pool.Add(item3);
        pool.Count.Should().Be(3);
    }

    [Fact]
    public void TestAddItemAlreadyInPool()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable("Item1");
        var item2 = new TestPoolable("Item2");
        
        // Add two items so item2.NextElement is not null
        pool.Add(item1);
        pool.Add(item2);

        // Trying to add item2 again should throw because its NextElement is not null
        pool.Invoking(p => p.Add(item2))
            .Should().Throw<ArgumentException>()
            .WithMessage("*already in a pool*");
    }

    [Fact]
    public void TestContains()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable("Item1");
        var item2 = new TestPoolable("Item2");
        var item3 = new TestPoolable("Item3");

        pool.Add(item1);
        pool.Add(item2);

        pool.Contains(item1).Should().BeTrue();
        pool.Contains(item2).Should().BeTrue();
        pool.Contains(item3).Should().BeFalse();
    }

    [Fact]
    public void TestRemove()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable("Item1");
        var item2 = new TestPoolable("Item2");
        var item3 = new TestPoolable("Item3");

        pool.Add(item1);
        pool.Add(item2);
        pool.Add(item3);
        pool.Count.Should().Be(3);

        // Remove middle item
        pool.Remove(item2).Should().BeTrue();
        pool.Count.Should().Be(2);
        pool.Contains(item2).Should().BeFalse();

        // Remove first item
        pool.Remove(item3).Should().BeTrue();
        pool.Count.Should().Be(1);
        pool.Contains(item3).Should().BeFalse();

        // Remove last item
        pool.Remove(item1).Should().BeTrue();
        pool.Count.Should().Be(0);
        pool.Contains(item1).Should().BeFalse();
    }

    [Fact]
    public void TestRemoveNonExistent()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable("Item1");
        var item2 = new TestPoolable("Item2");

        pool.Add(item1);
        pool.Remove(item2).Should().BeFalse();
        pool.Count.Should().Be(1);
    }

    [Fact]
    public void TestClear()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable("Item1");
        var item2 = new TestPoolable("Item2");
        var item3 = new TestPoolable("Item3");

        pool.Add(item1);
        pool.Add(item2);
        pool.Add(item3);
        pool.Count.Should().Be(3);

        pool.Clear();
        pool.Count.Should().Be(0);
        pool.Contains(item1).Should().BeFalse();
        pool.Contains(item2).Should().BeFalse();
        pool.Contains(item3).Should().BeFalse();
    }

    [Fact]
    public void TestForEach()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable("Item1");
        var item2 = new TestPoolable("Item2");
        var item3 = new TestPoolable("Item3");

        pool.Add(item1);
        pool.Add(item2);
        pool.Add(item3);

        var visited = new List<TestPoolable>();
        pool.ForEach(item => visited.Add(item));

        visited.Should().HaveCount(3);
        // Items are added at the beginning, so order is reversed
        visited.Should().ContainInOrder(item3, item2, item1);
    }

    [Fact]
    public void TestForEachEmptyPool()
    {
        var pool = new Pool<TestPoolable>();
        var visited = new List<TestPoolable>();
        
        pool.ForEach(item => visited.Add(item));
        visited.Should().BeEmpty();
    }

    [Fact]
    public void TestRemoveAll()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable("Item1");
        var item2 = new TestPoolable("Item2");
        var item3 = new TestPoolable("Item3");

        pool.Add(item1);
        pool.Add(item2);
        pool.Add(item3);

        var removed = new List<TestPoolable>();
        pool.RemoveAll(item => removed.Add(item));

        pool.Count.Should().Be(0);
        removed.Should().HaveCount(3);
        removed.Should().ContainInOrder(item3, item2, item1);
        
        // Verify that NextElement is cleared
        item1.NextElement.Should().BeNull();
        item2.NextElement.Should().BeNull();
        item3.NextElement.Should().BeNull();
    }

    [Fact]
    public void TestRemoveWhere()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable("Item1");
        var item2 = new TestPoolable("Item2");
        var item3 = new TestPoolable("Item3");
        var item4 = new TestPoolable("Item4");

        pool.Add(item1);
        pool.Add(item2);
        pool.Add(item3);
        pool.Add(item4);
        pool.Count.Should().Be(4);

        // Remove items with names ending in "2" or "4"
        pool.RemoveWhere(item => item.Name.EndsWith("2") || item.Name.EndsWith("4"));

        pool.Count.Should().Be(2);
        pool.Contains(item1).Should().BeTrue();
        pool.Contains(item2).Should().BeFalse();
        pool.Contains(item3).Should().BeTrue();
        pool.Contains(item4).Should().BeFalse();
        
        // Verify NextElement is cleared for removed items
        item2.NextElement.Should().BeNull();
        item4.NextElement.Should().BeNull();
    }

    [Fact]
    public void TestRemoveFirst()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable("Item1");
        var item2 = new TestPoolable("Item2");
        var item3 = new TestPoolable("Item3");
        var item4 = new TestPoolable("Item4");

        pool.Add(item1);
        pool.Add(item2);
        pool.Add(item3);
        pool.Add(item4);
        pool.Count.Should().Be(4);

        // Remove first item with name ending in "2"
        pool.RemoveFirst(item => item.Name.EndsWith("2"));

        pool.Count.Should().Be(3);
        pool.Contains(item1).Should().BeTrue();
        pool.Contains(item2).Should().BeFalse();
        pool.Contains(item3).Should().BeTrue();
        pool.Contains(item4).Should().BeTrue();
        
        item2.NextElement.Should().BeNull();
    }

    [Fact]
    public void TestRemoveFirstNoneMatch()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable("Item1");
        var item2 = new TestPoolable("Item2");

        pool.Add(item1);
        pool.Add(item2);

        // Try to remove item that doesn't match
        pool.RemoveFirst(item => item.Name == "Item3");
        pool.Count.Should().Be(2);
    }

    [Fact]
    public void TestAddAfterRemove()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable("Item1");
        var item2 = new TestPoolable("Item2");

        pool.Add(item1);
        pool.Add(item2);
        pool.Remove(item1);

        // Should be able to add item1 again after removing it
        pool.Add(item1);
        pool.Count.Should().Be(2);
        pool.Contains(item1).Should().BeTrue();
    }
}
