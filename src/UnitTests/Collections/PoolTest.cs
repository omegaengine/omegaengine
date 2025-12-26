/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace OmegaEngine.Collections;

/// <summary>
/// Contains test methods for <see cref="Pool{T}"/>.
/// </summary>
public class PoolTest
{
    private class TestPoolable : IPoolable<TestPoolable>
    {
        public string Value { get; set; } = "";
        public TestPoolable NextElement { get; set; } = null!;
    }

    [Fact]
    public void TestAddAndCount()
    {
        var pool = new Pool<TestPoolable>();
        pool.Count.Should().Be(0);

        var item1 = new TestPoolable { Value = "A" };
        pool.Add(item1);
        pool.Count.Should().Be(1);

        var item2 = new TestPoolable { Value = "B" };
        pool.Add(item2);
        pool.Count.Should().Be(2);
    }

    [Fact]
    public void TestAddNull()
    {
        var pool = new Pool<TestPoolable>();
        Assert.Throws<ArgumentNullException>(() => pool.Add(null!));
    }

    [Fact]
    public void TestAddItemAlreadyInPool()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable { Value = "A" };
        var item2 = new TestPoolable { Value = "B" };
        
        pool.Add(item1);
        pool.Add(item2); // Now item2.NextElement is item1, not null
        
        Assert.Throws<ArgumentException>(() => pool.Add(item2));
    }

    [Fact]
    public void TestForEach()
    {
        var pool = new Pool<TestPoolable>();
        pool.Add(new TestPoolable { Value = "A" });
        pool.Add(new TestPoolable { Value = "B" });
        pool.Add(new TestPoolable { Value = "C" });

        var values = new List<string>();
        pool.ForEach(item => values.Add(item.Value));
        
        values.Should().Equal("C", "B", "A"); // LIFO order
    }

    [Fact]
    public void TestContains()
    {
        var pool = new Pool<TestPoolable>();
        var item1 = new TestPoolable { Value = "A" };
        var item2 = new TestPoolable { Value = "B" };
        var item3 = new TestPoolable { Value = "C" };

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
        var item1 = new TestPoolable { Value = "A" };
        var item2 = new TestPoolable { Value = "B" };
        var item3 = new TestPoolable { Value = "C" };

        pool.Add(item1);
        pool.Add(item2);
        pool.Add(item3);

        pool.Remove(item2).Should().BeTrue();
        pool.Count.Should().Be(2);
        pool.Contains(item2).Should().BeFalse();
        
        pool.Remove(item2).Should().BeFalse(); // Already removed
    }

    [Fact]
    public void TestClear()
    {
        var pool = new Pool<TestPoolable>();
        pool.Add(new TestPoolable { Value = "A" });
        pool.Add(new TestPoolable { Value = "B" });

        pool.Count.Should().Be(2);
        pool.Clear();
        pool.Count.Should().Be(0);
    }

    [Fact]
    public void TestRemoveAll()
    {
        var pool = new Pool<TestPoolable>();
        pool.Add(new TestPoolable { Value = "A" });
        pool.Add(new TestPoolable { Value = "B" });
        pool.Add(new TestPoolable { Value = "C" });

        var values = new List<string>();
        pool.RemoveAll(item => values.Add(item.Value));
        
        values.Should().Equal("C", "B", "A");
        pool.Count.Should().Be(0);
    }

    [Fact]
    public void TestRemoveWhere()
    {
        var pool = new Pool<TestPoolable>();
        pool.Add(new TestPoolable { Value = "A" });
        pool.Add(new TestPoolable { Value = "B" });
        pool.Add(new TestPoolable { Value = "C" });
        pool.Add(new TestPoolable { Value = "B" });

        pool.RemoveWhere(item => item.Value == "B");
        pool.Count.Should().Be(2);

        var values = new List<string>();
        pool.ForEach(item => values.Add(item.Value));
        values.Should().NotContain("B");
    }

    [Fact]
    public void TestRemoveFirstWithPredicate()
    {
        var pool = new Pool<TestPoolable>();
        pool.Add(new TestPoolable { Value = "A" });
        pool.Add(new TestPoolable { Value = "B" });
        pool.Add(new TestPoolable { Value = "C" });
        pool.Add(new TestPoolable { Value = "B" });

        pool.RemoveFirst(item => item.Value == "B");
        pool.Count.Should().Be(3);

        var values = new List<string>();
        pool.ForEach(item => values.Add(item.Value));
        values.Count(v => v == "B").Should().Be(1); // Only one B removed
    }

    [Fact]
    public void TestRemoveFirstNotFound()
    {
        var pool = new Pool<TestPoolable>();
        pool.Add(new TestPoolable { Value = "A" });

        pool.RemoveFirst(item => item.Value == "Z");
        pool.Count.Should().Be(1); // No change
    }
}
