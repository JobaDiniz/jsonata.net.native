using System.Collections.Generic;
using Jsonata.Net.Native.Syntax;
using Xunit;

namespace Jsonata.Net.Native.Tests;

public class NodeEqualityShould
{
    [Fact]
    public void StringNode_ReturnTrue_WhenSameValue()
    {
        // Arrange
        var node1 = new StringNode("hello");
        var node2 = new StringNode("hello");

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void StringNode_ReturnFalse_WhenDifferentValue()
    {
        // Arrange
        var node1 = new StringNode("hello");
        var node2 = new StringNode("world");

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void StringNode_ReturnFalse_WhenNull()
    {
        // Arrange
        var node1 = new StringNode("hello");
        Node? node2 = null;

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NumberIntNode_ReturnTrue_WhenSameValue()
    {
        // Arrange
        var node1 = new NumberIntNode(42);
        var node2 = new NumberIntNode(42);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NumberIntNode_ReturnFalse_WhenDifferentValue()
    {
        // Arrange
        var node1 = new NumberIntNode(42);
        var node2 = new NumberIntNode(24);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NumberDoubleNode_ReturnTrue_WhenSameValue()
    {
        // Arrange
        var node1 = new NumberDoubleNode(3.14);
        var node2 = new NumberDoubleNode(3.14);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NumberDoubleNode_ReturnTrue_WhenValuesWithinEpsilon()
    {
        // Arrange
        var node1 = new NumberDoubleNode(1.0);
        var node2 = new NumberDoubleNode(1.0 + double.Epsilon);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void NumberDoubleNode_ReturnFalse_WhenValuesBeyondEpsilon()
    {
        // Arrange
        var node1 = new NumberDoubleNode(1.0);
        var node2 = new NumberDoubleNode(1.1);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BooleanNode_ReturnTrue_WhenSameValue()
    {
        // Arrange
        var node1 = new BooleanNode(true);
        var node2 = new BooleanNode(true);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BooleanNode_ReturnFalse_WhenDifferentValue()
    {
        // Arrange
        var node1 = new BooleanNode(true);
        var node2 = new BooleanNode(false);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void NullNode_ReturnTrue_WhenBothNullNodes()
    {
        // Arrange
        var node1 = new NullNode();
        var node2 = new NullNode();

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ArrayNode_ReturnTrue_WhenSameItems()
    {
        // Arrange
        var items1 = new List<Node> { new StringNode("a"), new NumberIntNode(1) };
        var items2 = new List<Node> { new StringNode("a"), new NumberIntNode(1) };
        var node1 = new ArrayNode(items1);
        var node2 = new ArrayNode(items2);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ArrayNode_ReturnFalse_WhenDifferentItems()
    {
        // Arrange
        var items1 = new List<Node> { new StringNode("a"), new NumberIntNode(1) };
        var items2 = new List<Node> { new StringNode("b"), new NumberIntNode(1) };
        var node1 = new ArrayNode(items1);
        var node2 = new ArrayNode(items2);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ArrayNode_ReturnFalse_WhenDifferentCount()
    {
        // Arrange
        var items1 = new List<Node> { new StringNode("a") };
        var items2 = new List<Node> { new StringNode("a"), new NumberIntNode(1) };
        var node1 = new ArrayNode(items1);
        var node2 = new ArrayNode(items2);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ArrayNode_ReturnTrue_WhenEmpty()
    {
        // Arrange
        var items1 = new List<Node>();
        var items2 = new List<Node>();
        var node1 = new ArrayNode(items1);
        var node2 = new ArrayNode(items2);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VariableNode_ReturnTrue_WhenSameName()
    {
        // Arrange
        var node1 = new VariableNode("varName");
        var node2 = new VariableNode("varName");

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VariableNode_ReturnFalse_WhenDifferentName()
    {
        // Arrange
        var node1 = new VariableNode("varName1");
        var node2 = new VariableNode("varName2");

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DifferentNodeTypes_ReturnFalse()
    {
        // Arrange
        var node1 = new StringNode("42");
        var node2 = new NumberIntNode(42);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PathNode_ReturnTrue_WhenSameStepsAndKeepArrays()
    {
        // Arrange
        var steps1 = new List<Node> { new FieldNameNode("field1"), new FieldNameNode("field2") };
        var steps2 = new List<Node> { new FieldNameNode("field1"), new FieldNameNode("field2") };
        var node1 = new PathNode(steps1, keepArrays: true);
        var node2 = new PathNode(steps2, keepArrays: true);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void PathNode_ReturnFalse_WhenDifferentKeepArrays()
    {
        // Arrange
        var steps1 = new List<Node> { new FieldNameNode("field1") };
        var steps2 = new List<Node> { new FieldNameNode("field1") };
        var node1 = new PathNode(steps1, keepArrays: true);
        var node2 = new PathNode(steps2, keepArrays: false);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PathNode_ReturnFalse_WhenDifferentSteps()
    {
        // Arrange
        var steps1 = new List<Node> { new FieldNameNode("field1") };
        var steps2 = new List<Node> { new FieldNameNode("field2") };
        var node1 = new PathNode(steps1, keepArrays: true);
        var node2 = new PathNode(steps2, keepArrays: true);

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RegexNode_ReturnTrue_WhenSamePattern()
    {
        // Arrange
        var node1 = new RegexNode("test.*");
        var node2 = new RegexNode("test.*");

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void RegexNode_ReturnFalse_WhenDifferentPattern()
    {
        // Arrange
        var node1 = new RegexNode("test.*");
        var node2 = new RegexNode("other.*");

        // Act
        var result = node1.Equals(node2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void PolymorphicEquals_WorksCorrectly()
    {
        // Arrange
        Node node1 = new StringNode("hello");
        Node node2 = new StringNode("hello");
        Node node3 = new StringNode("world");

        // Act
        var result1 = node1.Equals(node2);
        var result2 = node1.Equals(node3);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public void PolymorphicEquals_WorksCorrectlyWithComplexArrayNode()
    {
        // Arrange
        var items1 = new List<Node> 
        { 
            new StringNode("test"), 
            new NumberIntNode(42),
            new ArrayNode(new List<Node> { new BooleanNode(true), new NullNode() })
        };
        var items2 = new List<Node> 
        { 
            new StringNode("test"), 
            new NumberIntNode(42),
            new ArrayNode(new List<Node> { new BooleanNode(true), new NullNode() })
        };
        var items3 = new List<Node> 
        { 
            new StringNode("test"), 
            new NumberIntNode(43), // Different number
            new ArrayNode(new List<Node> { new BooleanNode(true), new NullNode() })
        };

        Node arrayNode1 = new ArrayNode(items1);
        Node arrayNode2 = new ArrayNode(items2);
        Node arrayNode3 = new ArrayNode(items3);

        // Act
        var result1 = arrayNode1.Equals(arrayNode2);
        var result2 = arrayNode1.Equals(arrayNode3);
        var result3 = arrayNode1.Equals(new StringNode("not an array"));

        // Assert
        Assert.True(result1);
        Assert.False(result2);
        Assert.False(result3);
    }
}