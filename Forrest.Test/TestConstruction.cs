namespace Forrest.Test;

public class TestConstruction
{
    private List<NodeWithParent> _singleRootParent = new();
    private List<NodeWithChildren> _singleRootChildren = new();
    private List<NodeWithParent> _multiRootParent = new();
    private List<NodeWithChildren> _multiRootChildren = new();

    [OneTimeSetUp]
    public void Setup()
    {
        _singleRootParent.Add(new NodeWithParent { Value = 1 });
        _singleRootParent.Add(new NodeWithParent { Value = 2, Parent = _singleRootParent[0] });
        _singleRootParent.Add(new NodeWithParent { Value = 3, Parent = _singleRootParent[0] });
        _singleRootParent.Add(new NodeWithParent { Value = 4, Parent = _singleRootParent[1] });
        _singleRootParent.Add(new NodeWithParent { Value = 5, Parent = _singleRootParent[1] });
        _singleRootParent.Add(new NodeWithParent { Value = 6, Parent = _singleRootParent[2] });
        _singleRootParent.Add(new NodeWithParent { Value = 7, Parent = _singleRootParent[2] });

        _singleRootChildren.Add(new NodeWithChildren { Value = 1 });
        _singleRootChildren.Add(new NodeWithChildren { Value = 2 });
        _singleRootChildren.Add(new NodeWithChildren { Value = 3 });
        _singleRootChildren.Add(new NodeWithChildren { Value = 4 });
        _singleRootChildren.Add(new NodeWithChildren { Value = 5 });
        _singleRootChildren.Add(new NodeWithChildren { Value = 6 });

        _singleRootChildren[0].Children.Add(_singleRootChildren[1]);
        _singleRootChildren[0].Children.Add(_singleRootChildren[2]);
        _singleRootChildren[1].Children.Add(_singleRootChildren[3]);
        _singleRootChildren[1].Children.Add(_singleRootChildren[4]);
        _singleRootChildren[2].Children.Add(_singleRootChildren[5]);

        _multiRootParent.Add(new NodeWithParent { Value = 1 });
        _multiRootParent.Add(new NodeWithParent { Value = 2 });
        _multiRootParent.Add(new NodeWithParent { Value = 3 });
        _multiRootParent.Add(new NodeWithParent { Value = 4 });
        _multiRootParent.Add(new NodeWithParent { Value = 5 });
        _multiRootParent.Add(new NodeWithParent { Value = 6 });
        _multiRootParent.Add(new NodeWithParent { Value = 7 });
        _multiRootParent.Add(new NodeWithParent { Value = 8 });

        _multiRootParent[1].Parent = _multiRootParent[0];
        _multiRootParent[2].Parent = _multiRootParent[0];
        _multiRootParent[3].Parent = _multiRootParent[1];
        _multiRootParent[4].Parent = _multiRootParent[1];
        _multiRootParent[5].Parent = _multiRootParent[2];
        // No parent for _multiRootParent[6]
        _multiRootParent[7].Parent = _multiRootParent[6];

        _multiRootChildren.Add(new NodeWithChildren { Value = 1 });
        _multiRootChildren.Add(new NodeWithChildren { Value = 2 });
        _multiRootChildren.Add(new NodeWithChildren { Value = 3 });
        _multiRootChildren.Add(new NodeWithChildren { Value = 4 });
        _multiRootChildren.Add(new NodeWithChildren { Value = 5 });
        _multiRootChildren.Add(new NodeWithChildren { Value = 6 });
        _multiRootChildren.Add(new NodeWithChildren { Value = 7 });
        _multiRootChildren.Add(new NodeWithChildren { Value = 8 });

        _multiRootChildren[0].Children.Add(_multiRootChildren[1]);
        _multiRootChildren[0].Children.Add(_multiRootChildren[2]);
        _multiRootChildren[1].Children.Add(_multiRootChildren[3]);
        _multiRootChildren[1].Children.Add(_multiRootChildren[4]);
        _multiRootChildren[2].Children.Add(_multiRootChildren[5]);
        // _multiRootChildren[6] is not in any children list
        _multiRootChildren[7].Children.Add(_multiRootChildren[6]);
    }

    [Test]
    public void TestParentSingle()
    {
        var parentTree = _singleRootParent.ToTree(n => n.Parent);
        Assert.Multiple(() =>
        {
            Assert.That(parentTree.RootCount, Is.EqualTo(1));
            Assert.That(parentTree.NodeCount, Is.EqualTo(7));
            Assert.That(parentTree.Depth, Is.EqualTo(3));
            Assert.That(parentTree.IsEmpty, Is.False);
            Assert.That(parentTree.Contains(_singleRootParent[0]), Is.True);
            Assert.That(parentTree.Contains(new NodeWithParent { Value = 8 }), Is.False);
        });
    }

    [Test]
    public void TestParentMulti()
    {
        var parentTree = _multiRootParent.ToTree(n => n.Parent);
        Assert.Multiple(() =>
        {
            Assert.That(parentTree.RootCount, Is.EqualTo(2));
            Assert.That(parentTree.NodeCount, Is.EqualTo(8));
            Assert.That(parentTree.Depth, Is.EqualTo(3));
            Assert.That(parentTree.IsEmpty, Is.False);
            Assert.That(parentTree.Contains(_multiRootParent[0]), Is.True);
            Assert.That(parentTree.Contains(new NodeWithParent { Value = 9 }), Is.False);
        });
    }


    [Test]
    public void TestChildSingle()
    {
        var childrenTree = _singleRootChildren.ToTree(n => n.Children);
        Assert.Multiple(() =>
        {
            Assert.That(childrenTree.RootCount, Is.EqualTo(1));
            Assert.That(childrenTree.NodeCount, Is.EqualTo(6));
            Assert.That(childrenTree.Depth, Is.EqualTo(3));
            Assert.That(childrenTree.IsEmpty, Is.False);
            Assert.That(childrenTree.Contains(_singleRootChildren[0]), Is.True);
            Assert.That(childrenTree.Contains(new NodeWithChildren { Value = 7 }), Is.False);
        });
    }

    [Test]
    public void TestChildMulti()
    {
        var childrenTree = _multiRootChildren.ToTree(n => n.Children);
        Assert.Multiple(() =>
        {
            Assert.That(childrenTree.RootCount, Is.EqualTo(2));
            Assert.That(childrenTree.NodeCount, Is.EqualTo(8));
            Assert.That(childrenTree.Depth, Is.EqualTo(3));
            Assert.That(childrenTree.IsEmpty, Is.False);
            Assert.That(childrenTree.Contains(_multiRootChildren[0]), Is.True);
            Assert.That(childrenTree.Contains(new NodeWithChildren { Value = 9 }), Is.False);
        });
    }

    [Test]
    public void TestParentSingleAuto()
    {
        var parentTree = _singleRootParent.ToTree();
        Assert.Multiple(() =>
        {
            Assert.That(parentTree.RootCount, Is.EqualTo(1));
            Assert.That(parentTree.NodeCount, Is.EqualTo(7));
            Assert.That(parentTree.Depth, Is.EqualTo(3));
            Assert.That(parentTree.IsEmpty, Is.False);
            Assert.That(parentTree.Contains(_singleRootParent[0]), Is.True);
            Assert.That(parentTree.Contains(new NodeWithParent { Value = 8 }), Is.False);
        });
    }

    [Test]
    public void TestParentMultiAuto()
    {
        var parentTree = _multiRootParent.ToTree();
        Assert.Multiple(() =>
        {
            Assert.That(parentTree.RootCount, Is.EqualTo(2));
            Assert.That(parentTree.NodeCount, Is.EqualTo(8));
            Assert.That(parentTree.Depth, Is.EqualTo(3));
            Assert.That(parentTree.IsEmpty, Is.False);
            Assert.That(parentTree.Contains(_multiRootParent[0]), Is.True);
            Assert.That(parentTree.Contains(new NodeWithParent { Value = 9 }), Is.False);
        });
    }

    [Test]
    public void TestChildSingleAuto()
    {
        var childrenTree = _singleRootChildren.ToTree();
        Assert.Multiple(() =>
        {
            Assert.That(childrenTree.RootCount, Is.EqualTo(1));
            Assert.That(childrenTree.NodeCount, Is.EqualTo(6));
            Assert.That(childrenTree.Depth, Is.EqualTo(3));
            Assert.That(childrenTree.IsEmpty, Is.False);
            Assert.That(childrenTree.Contains(_singleRootChildren[0]), Is.True);
            Assert.That(childrenTree.Contains(new NodeWithChildren { Value = 7 }), Is.False);
        });
    }

    [Test]
    public void TestChildMultiAuto()
    {
        var childrenTree = _multiRootChildren.ToTree();
        Assert.Multiple(() =>
        {
            Assert.That(childrenTree.RootCount, Is.EqualTo(2));
            Assert.That(childrenTree.NodeCount, Is.EqualTo(8));
            Assert.That(childrenTree.Depth, Is.EqualTo(3));
            Assert.That(childrenTree.IsEmpty, Is.False);
            Assert.That(childrenTree.Contains(_multiRootChildren[0]), Is.True);
            Assert.That(childrenTree.Contains(new NodeWithChildren { Value = 9 }), Is.False);
        });
    }

    [Test]
    public void TestCreateAndFlattenEquality()
    {
        var tree = _multiRootParent.ToTree();
        var flattened = tree.Flatten();

        var fromFlat = Tree.Create(flattened, n => n.Parent);
        // Check if the tree is the same
        Assert.Multiple(() =>
        {
            Assert.That(fromFlat.RootCount, Is.EqualTo(tree.RootCount));
            Assert.That(fromFlat.NodeCount, Is.EqualTo(tree.NodeCount));
            Assert.That(fromFlat.Depth, Is.EqualTo(tree.Depth));
            Assert.That(fromFlat.IsEmpty, Is.EqualTo(tree.IsEmpty));
            Assert.That(fromFlat.Contains(_multiRootParent[0]), Is.True);
            Assert.That(fromFlat.Contains(new NodeWithParent { Value = 9 }), Is.False);
        });

        // Check if the original list contains all the nodes from the flattened tree
        foreach (var node in _multiRootParent)
        {
            Assert.That(flattened.Contains(node), Is.True);
        }
    }
}

public class NodeWithParent
{
    public NodeWithParent? Parent { get; set; }
    public int Value { get; set; }
}

public class NodeWithChildren
{
    public List<NodeWithChildren> Children { get; set; } = new();
    public int Value { get; set; }
}