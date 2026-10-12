/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.ComponentModel;
using System.Numerics;
using AwesomeAssertions;
using Xunit;

namespace OmegaEngine.Foundation.Design;

public class NumericsConvertersTest
{
    public NumericsConvertersTest()
    {
        NumericsConverters.Register();
    }

    [Fact]
    public void ConvertsVector3ToAndFromString()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Vector3));

        converter.ConvertToInvariantString(new Vector3(1, 2.5f, -3)).Should().Be("1, 2.5, -3");
        converter.ConvertFromInvariantString("1, 2.5, -3").Should().Be(new Vector3(1, 2.5f, -3));
    }

    [Fact]
    public void ParsesDefaultValues()
        => new DefaultValueAttribute(typeof(Vector3), "0,0,0").Value.Should().Be(Vector3.Zero);

    [Fact]
    public void ExposesFieldsAsProperties()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Vector2));

        var properties = converter.GetProperties(new Vector2(1, 2))!;
        properties.Count.Should().Be(2);
        properties["Y"]!.GetValue(new Vector2(1, 2)).Should().Be(2f);

        converter.CreateInstance(new Hashtable {["X"] = 3f, ["Y"] = 4f}).Should().Be(new Vector2(3, 4));
    }

    [Fact]
    public void ConvertsQuaternions()
        => TypeDescriptor.GetConverter(typeof(Quaternion)).ConvertFromInvariantString("0, 0, 0, 1").Should().Be(Quaternion.Identity);
}
