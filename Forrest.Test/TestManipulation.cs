namespace Forrest.Test;

public class TestManipulation
{
    private List<TestNode> _nodesWithParent = new();
    
    [SetUp]
    public void Setup()
    {
        _nodesWithParent = new List<TestNode>();
        
    }
}

public class TestNode
{
    public TestNode? Parent { get; set; }
    public string Text { get; set; }
}