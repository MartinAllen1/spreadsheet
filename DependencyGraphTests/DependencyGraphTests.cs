namespace CS3500.DevelopmentTests;
using CS3500.DependencyGraph;
/// <summary>
/// This is a test class for DependencyGraphTest and is intended
/// to contain all DependencyGraphTest Unit Tests
/// </summary>
[TestClass]
public class DependencyGraphTests
{
    /// <summary>
    /// Tests the data processing performance of the DependencyGraph,
    /// depends on the data structures selected,
    /// the multiple four basic operations should be finished within 2 seconds.
    /// the result data should also match, making sure that operations run correctly.
    /// </summary>
    [TestMethod]
    [Timeout( 2000 )] // 2 second run time limit
    public void StressTest()
    {
        DependencyGraph dg = new();
        // A constant number of strings to use
        const int SIZE = 200;
        string[] letters = new string[SIZE];
        for ( int i = 0; i < SIZE; i++ )
        {
            letters[i] = string.Empty + ( (char) ( 'a' + i ) );
        }
        // HashSet to store local dependency for later comparison
        HashSet<string>[] dependents = new HashSet<string>[SIZE];
        HashSet<string>[] dependees = new HashSet<string>[SIZE];
        for ( int i = 0; i < SIZE; i++ )
        {
            dependents[i] = [];
            dependees[i] = [];
        }
        // Add a bunch of dependencies
        for ( int i = 0; i < SIZE; i++ )
        {
            for ( int j = i + 1; j < SIZE; j++ )
            {
                dg.AddDependency( letters[i], letters[j] );
                dependents[i].Add( letters[j] );
                dependees[j].Add( letters[i] );
            }
        }
        // Remove a bunch of dependencies
        for ( int i = 0; i < SIZE; i++ )
        {
            for ( int j = i + 4; j < SIZE; j += 4 )
            {
                dg.RemoveDependency( letters[i], letters[j] );
                dependents[i].Remove( letters[j] );
                dependees[j].Remove( letters[i] );
            }
        }
        // Add some dependencies back
        for ( int i = 0; i < SIZE; i++ )
        {
            for ( int j = i + 1; j < SIZE; j += 2 )
            {
                dg.AddDependency( letters[i], letters[j] );
                dependents[i].Add( letters[j] );
                dependees[j].Add( letters[i] );
            }
        }
        // Remove some dependencies
        for ( int i = 0; i < SIZE; i += 2 )
        {
            for ( int j = i + 3; j < SIZE; j += 3 )
            {
                dg.RemoveDependency( letters[i], letters[j] );
                dependents[i].Remove( letters[j] );
                dependees[j].Remove( letters[i] );
            }
        }
        // Compare dependency relationship stores in dependency graph with local dependency.
        for ( int i = 0; i < SIZE; i++ )
        {
            Assert.IsTrue( dependents[i].SetEquals( new HashSet<string>( dg.GetDependents( letters[i] ) ) ) );
            Assert.IsTrue( dependees[i].SetEquals( new HashSet<string>( dg.GetDependees( letters[i] ) ) ) );
        }
    }

    /// <summary>
    /// Tests the circle dependency for dependency graph
    /// </summary>
    [TestMethod]
    public void DependencyGraph_CheckCircleDependency_Match()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        dg.AddDependency("b", "a");

        const int expected = 2;
        Assert.AreEqual(expected, dg.Size);
        Assert.IsTrue(dg.GetDependents("a").SequenceEqual(dg.GetDependees("a")));
        Assert.IsTrue(dg.GetDependents("b").SequenceEqual(dg.GetDependees("b")));
    }

    /// <summary>
    /// Tests the size for an empty graph
    /// </summary>
    [TestMethod]
    public void Size_EmptyDependencyGraph_ZeroSize()
    {
        DependencyGraph dg = new ();
        const int expected = 0;
        Assert.AreEqual(expected, dg.Size);
    }

    /// <summary>
    /// Tests the size for graph with some dependency relationships.
    /// </summary>
    [TestMethod]
    public void Size_DependencyGraphWithSomeElements_CorrectSize()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "c");
        dg.AddDependency("b", "d");

        const int expected = 3;
        Assert.AreEqual(expected, dg.Size);
    }

    /// <summary>
    /// Tests the size after replacing dependents with some new dependents for a given node
    /// </summary>
    [TestMethod]
    public void Size_CheckGraphSizeAfterReplacingDependents_SizeMatch()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "f");

        dg.ReplaceDependents("a", ["c", "d", "e"]);

        const int expected = 3;
        Assert.AreEqual(expected, dg.Size);
    }

    /// <summary>
    /// Tests the size after replacing dependents with empty set for a given node
    /// </summary>
    [TestMethod]
    public void Size_CheckGraphSizeAfterReplacingEmptyDependent_ZeroSize()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "f");

        dg.ReplaceDependents("a", []);

        const int expected = 0;
        Assert.AreEqual(expected, dg.Size);
    }

    /// <summary>
    /// Tests size after replacing dependee with empty set for a given node
    /// </summary>
    [TestMethod]
    public void Size_CheckGraphSizeAfterReplacingEmptyDependee_ZeroSize()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "c");
        dg.AddDependency("b", "c");

        dg.ReplaceDependees("c", []);

        const int expected = 0;
        Assert.AreEqual(expected, dg.Size);
    }

    /// <summary>
    /// Tests size after replacing dependees with some new dependees for a given node
    /// </summary>
    [TestMethod]
    public void Size_CheckGraphSizeAfterReplacingDependees_SizeMatch()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "c");
        dg.AddDependency("b", "c");

        dg.ReplaceDependees("c", ["a", "b", "e"]);

        const int expected = 3;
        Assert.AreEqual(expected, dg.Size);
    }

    /// <summary>
    /// Tests size for repeat dependency
    /// </summary>
    [TestMethod]
    public void Size_CheckRepeatElement_SizeMatch()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "c");
        dg.AddDependency("a", "c");

        const int expected = 1;
        Assert.AreEqual(expected, dg.Size);
    }

    /// <summary>
    /// Tests size for remove nonexistent element from empty graph
    /// </summary>
    [TestMethod]
    public void Size_CheckRemoveEmptyGraph_SizeMatch()
    {
        DependencyGraph dg = new ();
        dg.RemoveDependency("a", "c");

        const int expected = 0;
        Assert.AreEqual(expected, dg.Size);
    }

    /// <summary>
    /// Tests GetDependents for an empty graph
    /// </summary>
    [TestMethod]
    public void GetDependents_EmptyGraph_EmptySetNoElement()
    {
        DependencyGraph dg = new ();
        Assert.IsFalse(dg.GetDependents("a").Any());
    }

    /// <summary>
    /// Tests GetDependees for an empty graph
    /// </summary>
    [TestMethod]
    public void GetDependees_EmptyGraph_EmptySetNoElement()
    {
        DependencyGraph dg = new ();
        Assert.IsFalse(dg.GetDependees("a").Any());
    }

    /// <summary>
    /// Tests HasDependents for an empty graph
    /// </summary>
    [TestMethod]
    public void HasDependents_EmptyGraphNoDependent_False()
    {
        DependencyGraph dg = new ();
        Assert.IsFalse( dg.HasDependents("d") );
    }

    /// <summary>
    /// Tests HasDependents for node with no dependent
    /// </summary>
    [TestMethod]
    public void HasDependents_NoDependent_False()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "c");
        dg.AddDependency("b", "d");

        Assert.IsFalse( dg.HasDependents("d") );
    }

    /// <summary>
    /// Tests HasDependents for node with some dependents
    /// </summary>
    [TestMethod]
    public void HasDependents_SomeDependents_True()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "c");
        dg.AddDependency("b", "d");

        Assert.IsTrue( dg.HasDependents("a") );
    }

    /// <summary>
    /// Tests HasDependees for an empty graph
    /// </summary>
    [TestMethod]
    public void HasDependees_EmptyGraphNoDependee_False()
    {
        DependencyGraph dg = new ();
        Assert.IsFalse( dg.HasDependees("d") );
    }

    /// <summary>
    /// Tests HasDependees for node with no dependee
    /// </summary>
    [TestMethod]
    public void HasDependees_NoDependee_False()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "c");
        dg.AddDependency("b", "d");

        Assert.IsFalse( dg.HasDependees("a") );
    }

    /// <summary>
    /// Tests HasDependees for node with some dependees
    /// </summary>
    [TestMethod]
    public void HasDependees_SomeDependees_True()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "c");
        dg.AddDependency("b", "c");

        Assert.IsTrue( dg.HasDependees("c") );
    }

    /// <summary>
    /// Tests ReplaceDependents for empty graph with new dependents for a given node
    /// </summary>
    [TestMethod]
    public void ReplaceDependents_EmptyGraphToNewDependents_Match()
    {
        DependencyGraph dg = new ();
        HashSet<string> newDependents = ["c", "d", "e"];

        dg.ReplaceDependents("a", newDependents);

        Assert.IsTrue(newDependents.SetEquals(dg.GetDependents("a")));
    }

    /// <summary>
    /// Tests ReplaceDependents for node with no dependent with some dependents
    /// </summary>
    [TestMethod]
    public void ReplaceDependents_NoDependentToSomeDependents_Match()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        HashSet<string> newDependents = ["c", "d", "e"];

        dg.ReplaceDependents("b", newDependents);

        Assert.IsTrue(newDependents.SetEquals(dg.GetDependents("b")));
    }

    /// <summary>
    /// Tests ReplaceDependents for node with some dependents with new dependents
    /// </summary>
    [TestMethod]
    public void ReplaceDependents_SomeDependentsToNewDependents_Match()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "f");
        HashSet<string> newDependents = ["c", "d", "e"];

        dg.ReplaceDependents("a", newDependents);

        Assert.IsTrue(newDependents.SetEquals(dg.GetDependents("a")));
    }

    /// <summary>
    /// Tests ReplaceDependents for node with some dependents to an empty set
    /// </summary>
    [TestMethod]
    public void ReplaceDependents_SomeDependentsToNoDependent_HasDependentsFalse()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        dg.AddDependency("a", "f");

        dg.ReplaceDependents("a", []);

        Assert.IsFalse(dg.HasDependents("a"));
    }

    /// <summary>
    /// Tests ReplaceDependents for the corresponding dependee relationship
    /// </summary>
    [TestMethod]
    public void ReplaceDependents_CheckCorrespondingDependees_Match()
    {
        DependencyGraph dg = new ();

        dg.ReplaceDependents("a", ["d"]);
        dg.ReplaceDependents("b", ["d"]);
        dg.ReplaceDependents("c", ["d"]);
        HashSet<string> newDependees = ["a", "b", "c"];

        Assert.IsTrue(newDependees.SetEquals(dg.GetDependees("d")));
    }

    /// <summary>
    /// Tests ReplaceDependees for an empty graph
    /// </summary>
    [TestMethod]
    public void ReplaceDependees_EmptyGraphToNewDependees_Match()
    {
        DependencyGraph dg = new ();
        HashSet<string> newDependees = ["c", "d", "e"];

        dg.ReplaceDependees("a", newDependees);

        Assert.IsTrue(newDependees.SetEquals(dg.GetDependees("a")));
    }

    /// <summary>
    /// Tests ReplaceDependees for node with no dependee to some dependees
    /// </summary>
    [TestMethod]
    public void ReplaceDependees_NoDependeeToSomeDependees_Match()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "b");
        HashSet<string> newDependees = ["c", "d", "e"];

        dg.ReplaceDependees("a", newDependees);

        Assert.IsTrue(newDependees.SetEquals(dg.GetDependees("a")));
    }

    /// <summary>
    /// Tests ReplaceDependees for node with some dependees to new dependees
    /// </summary>
    [TestMethod]
    public void ReplaceDependees_SomeDependeesToNewDependees_Match()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "c");
        dg.AddDependency("b", "c");
        HashSet<string> newDependees = ["c", "d", "e"];

        dg.ReplaceDependees("c", newDependees);

        Assert.IsTrue(newDependees.SetEquals(dg.GetDependees("c")));
    }

    /// <summary>
    /// Tests ReplaceDependees for node with some dependees to an empty set
    /// </summary>
    [TestMethod]
    public void ReplaceDependees_SomeDependeesToEmptySet_HasDependeesFalse()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("a", "c");
        dg.AddDependency("b", "c");

        dg.ReplaceDependees("c", []);

        Assert.IsFalse(dg.HasDependees("c"));
    }

    /// <summary>
    /// Tests ReplaceDependees for the corresponding dependent relationship
    /// </summary>
    [TestMethod]
    public void ReplaceDependees_CheckCorrespondingDependents_Match()
    {
        DependencyGraph dg = new ();
        dg.AddDependency("d", "e");

        dg.ReplaceDependees("a", ["d"]);
        dg.ReplaceDependees("b", ["d"]);
        dg.ReplaceDependees("c", ["d"]);

        HashSet<string> newDependents = ["a", "b", "c", "e"];
        Assert.IsTrue(newDependents.SetEquals(dg.GetDependents("d")));
    }
}