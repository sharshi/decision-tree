using GrokTest;
using Xunit;
using System;

namespace DecisionTree.Test
{
    public class DecisionTreeTests
    {
        #region Node Tests

        [Fact]
        public void Node_LeafNode_CreatesWithValueAndDescription()
        {
            // Arrange & Act
            var node = new DecisionTree<string>.Node("test value", "test description");

            // Assert
            Assert.Equal("test value", node.Value);
            Assert.Equal("test description", node.Description);
            Assert.Empty(node.Children);
            Assert.NotNull(node.Decision);
        }

        [Fact]
        public void Node_LeafNode_DefaultDecisionReturnsNegativeOne()
        {
            // Arrange
            var node = new DecisionTree<string>.Node("test");

            // Act
            int result = node.Decision(new object());

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void Node_DecisionNode_CreatesWithDecisionFunction()
        {
            // Arrange
            Func<object, int> decision = input => 0;

            // Act
            var node = new DecisionTree<string>.Node(decision, "decision node");

            // Assert
            Assert.Null(node.Value);
            Assert.Equal("decision node", node.Description);
            Assert.Equal(0, node.Decision(new object()));
        }

        #endregion

        #region DecisionTree Basic Tests

        [Fact]
        public void DecisionTree_Traverse_NullRoot_ThrowsException()
        {
            // Arrange
            var tree = new DecisionTree<string>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => tree.Traverse("input"));
        }

        [Fact]
        public void DecisionTree_Traverse_SingleLeafNode_ReturnsValue()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            tree.Root = new DecisionTree<string>.Node("result");

            // Act
            var result = tree.Traverse("any input");

            // Assert
            Assert.Equal("result", result);
        }

        [Fact]
        public void DecisionTree_AddChild_AddsChildToParent()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            var parent = new DecisionTree<string>.Node(input => 0);
            var child = new DecisionTree<string>.Node("child value");

            // Act
            tree.AddChild(parent, child);

            // Assert
            Assert.Single(parent.Children);
            Assert.Equal("child value", parent.Children[0].Value);
        }

        #endregion

        #region Traversal Tests

        [Fact]
        public void DecisionTree_Traverse_SimpleBinaryTree_ReturnsCorrectPath()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            tree.Root = new DecisionTree<string>.Node(input =>
            {
                int value = (int)input;
                return value > 5 ? 1 : 0;
            });

            var lowPath = new DecisionTree<string>.Node("low");
            var highPath = new DecisionTree<string>.Node("high");

            tree.AddChild(tree.Root, lowPath);
            tree.AddChild(tree.Root, highPath);

            // Act & Assert
            Assert.Equal("low", tree.Traverse(3));
            Assert.Equal("high", tree.Traverse(7));
        }

        [Fact]
        public void DecisionTree_Traverse_InvalidDecisionIndex_StopsAtCurrentNode()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            tree.Root = new DecisionTree<string>.Node(input => -1); // Invalid index
            tree.Root.Value = "fallback";

            var child = new DecisionTree<string>.Node("should not reach");
            tree.AddChild(tree.Root, child);

            // Act
            var result = tree.Traverse("input");

            // Assert
            Assert.Equal("fallback", result);
        }

        [Fact]
        public void DecisionTree_Traverse_OutOfBoundsDecisionIndex_StopsAtCurrentNode()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            tree.Root = new DecisionTree<string>.Node(input => 5); // Out of bounds
            tree.Root.Value = "fallback";

            var child = new DecisionTree<string>.Node("should not reach");
            tree.AddChild(tree.Root, child);

            // Act
            var result = tree.Traverse("input");

            // Assert
            Assert.Equal("fallback", result);
        }

        [Fact]
        public void DecisionTree_Traverse_MultiLevelTree_TraversesCorrectly()
        {
            // Arrange
            var tree = new DecisionTree<int>();
            tree.Root = new DecisionTree<int>.Node(input =>
            {
                string str = (string)input;
                return str.Length > 3 ? 1 : 0;
            });

            var shortPath = new DecisionTree<int>.Node(input =>
            {
                string str = (string)input;
                return str.Contains("a") ? 1 : 0;
            });

            var longPath = new DecisionTree<int>.Node(input =>
            {
                string str = (string)input;
                return str.Contains("z") ? 1 : 0; // Contains "z" -> 1, No "z" -> 0
            });

            tree.AddChild(tree.Root, shortPath);
            tree.AddChild(tree.Root, longPath);

            // Short path leaves
            var shortNoA = new DecisionTree<int>.Node(1);
            var shortWithA = new DecisionTree<int>.Node(2);
            tree.AddChild(shortPath, shortNoA);
            tree.AddChild(shortPath, shortWithA);

            // Long path leaves
            var longNoZ = new DecisionTree<int>.Node(3);
            var longWithZ = new DecisionTree<int>.Node(4);
            tree.AddChild(longPath, longNoZ);
            tree.AddChild(longPath, longWithZ);

            // Act & Assert
            Assert.Equal(1, tree.Traverse("hi"));      // short, no 'a'
            Assert.Equal(2, tree.Traverse("hat"));     // short, with 'a'
            Assert.Equal(3, tree.Traverse("hello"));   // long, no 'z'
            Assert.Equal(4, tree.Traverse("pizza"));   // long, with 'z'
        }

        #endregion

        #region Generic Type Tests

        [Fact]
        public void DecisionTree_GenericTypes_WorksWithDifferentTypes()
        {
            // Test with int
            var intTree = new DecisionTree<int>();
            intTree.Root = new DecisionTree<int>.Node(42);
            Assert.Equal(42, intTree.Traverse("anything"));

            // Test with bool
            var boolTree = new DecisionTree<bool>();
            boolTree.Root = new DecisionTree<bool>.Node(true);
            Assert.True(boolTree.Traverse(new object()));

            // Test with custom class
            var customTree = new DecisionTree<DateTime>();
            customTree.Root = new DecisionTree<DateTime>.Node(DateTime.Now);
            Assert.IsType<DateTime>(customTree.Traverse("input"));
        }

        #endregion

        #region Vacation Tree Tests

        [Fact]
        public void BuildVacationTree_CreatesValidTree()
        {
            // Arrange & Act
            var tree = DecisionTree<string>.BuildVacationTree();

            // Assert
            Assert.NotNull(tree.Root);
            Assert.Equal(2, tree.Root.Children.Count);
            Assert.Equal("Budget check", tree.Root.Description);
        }

        [Fact]
        public void BuildVacationTree_LowBudgetBeach_ReturnsThailand()
        {
            // Arrange
            var tree = DecisionTree<string>.BuildVacationTree();
            var prefs = new VacationPreferences
            {
                Budget = 1000,
                PrefersBeach = true,
                PrefersAdventure = false
            };

            // Act
            var result = tree.Traverse(prefs);

            // Assert
            Assert.Equal("Thailand", result);
        }

        [Fact]
        public void BuildVacationTree_LowBudgetNoBeach_ReturnsParis()
        {
            // Arrange
            var tree = DecisionTree<string>.BuildVacationTree();
            var prefs = new VacationPreferences
            {
                Budget = 1500,
                PrefersBeach = false,
                PrefersAdventure = false
            };

            // Act
            var result = tree.Traverse(prefs);

            // Assert
            Assert.Equal("Paris", result);
        }

        [Fact]
        public void BuildVacationTree_HighBudgetAdventure_ReturnsNewZealand()
        {
            // Arrange
            var tree = DecisionTree<string>.BuildVacationTree();
            var prefs = new VacationPreferences
            {
                Budget = 3000,
                PrefersBeach = false,
                PrefersAdventure = true
            };

            // Act
            var result = tree.Traverse(prefs);

            // Assert
            Assert.Equal("New Zealand", result);
        }

        [Fact]
        public void BuildVacationTree_HighBudgetRelaxation_ReturnsMaldives()
        {
            // Arrange
            var tree = DecisionTree<string>.BuildVacationTree();
            var prefs = new VacationPreferences
            {
                Budget = 2500,
                PrefersBeach = true,
                PrefersAdventure = false
            };

            // Act
            var result = tree.Traverse(prefs);

            // Assert
            Assert.Equal("Maldives", result);
        }

        [Fact]
        public void BuildVacationTree_NullInput_UsesDefaultPaths()
        {
            // Arrange
            var tree = DecisionTree<string>.BuildVacationTree();

            // Act
            var result = tree.Traverse(new object());

            // Assert
            Assert.Equal("Paris", result); // Default low budget, no beach
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public void DecisionTree_Traverse_EmptyChildrenList_ReturnsCurrentValue()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            var node = new DecisionTree<string>.Node("value");
            node.Children.Clear(); // Ensure empty
            tree.Root = node;

            // Act
            var result = tree.Traverse("input");

            // Assert
            Assert.Equal("value", result);
        }

        [Fact]
        public void DecisionTree_AddChild_NullParent_DoesNothing()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            var child = new DecisionTree<string>.Node("child");

            // Act (should not throw)
            tree.AddChild(null!, child);

            // Assert (no exception thrown)
        }

        [Fact]
        public void DecisionTree_AddChild_NullChild_AddsNullToList()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            var parent = new DecisionTree<string>.Node(input => 0);

            // Act (should not throw)
            tree.AddChild(parent, null!);

            // Assert - null is actually added to the list
            Assert.Single(parent.Children);
            Assert.Null(parent.Children[0]);
        }

        [Fact]
        public void Node_ChildrenList_IsInitialized()
        {
            // Arrange & Act
            var node = new DecisionTree<string>.Node("test");

            // Assert
            Assert.NotNull(node.Children);
            Assert.Empty(node.Children);
        }

        #endregion

        #region Complex Decision Functions

        [Fact]
        public void DecisionTree_Traverse_ComplexDecisionFunction_WorksCorrectly()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            tree.Root = new DecisionTree<string>.Node(input =>
            {
                if (input is string str)
                {
                    if (str.Contains("error")) return 0;
                    if (str.Contains("warning")) return 1;
                    return 2;
                }
                return -1;
            });

            var errorNode = new DecisionTree<string>.Node("Handle Error");
            var warningNode = new DecisionTree<string>.Node("Handle Warning");
            var normalNode = new DecisionTree<string>.Node("Normal Processing");

            tree.AddChild(tree.Root, errorNode);
            tree.AddChild(tree.Root, warningNode);
            tree.AddChild(tree.Root, normalNode);

            // Act & Assert
            Assert.Equal("Handle Error", tree.Traverse("system error occurred"));
            Assert.Equal("Handle Warning", tree.Traverse("warning message"));
            Assert.Equal("Normal Processing", tree.Traverse("normal input"));
        }

        #endregion

        #region Node Properties Tests

        [Fact]
        public void Node_OriginalInputAndEffects_AreInitialized()
        {
            // Arrange & Act
            var node = new DecisionTree<string>.Node("test value", "test description");

            // Assert
            Assert.Null(node.OriginalInput);
            Assert.NotNull(node.Effects);
            Assert.Empty(node.Effects);
        }

        [Fact]
        public void DecisionTree_Traverse_SetsOriginalInputOnPath()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            tree.Root = new DecisionTree<string>.Node(input => 0, "root decision");
            var child = new DecisionTree<string>.Node("result", "leaf");
            tree.AddChild(tree.Root, child);

            var input = new object();

            // Act
            tree.Traverse(input);

            // Assert
            Assert.Equal(input, tree.Root.OriginalInput);
            Assert.Equal(input, child.OriginalInput);
        }

        [Fact]
        public void DecisionTree_Traverse_AccumulatesEffectsOnPath()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            tree.Root = new DecisionTree<string>.Node(input => 0, "root decision");
            var child = new DecisionTree<string>.Node("result", "leaf");
            tree.AddChild(tree.Root, child);

            // Act
            tree.Traverse("test input");

            // Assert
            Assert.Equal(new List<string> { "root decision" }, tree.Root.Effects);
            Assert.Equal(new List<string> { "root decision" }, child.Effects);
        }

        [Fact]
        public void DecisionTree_Traverse_MultiLevelTree_AccumulatesEffectsCorrectly()
        {
            // Arrange
            var tree = new DecisionTree<string>();
            tree.Root = new DecisionTree<string>.Node(input => 0, "level 1");
            var level2 = new DecisionTree<string>.Node(input => 0, "level 2");
            var leaf = new DecisionTree<string>.Node("final", "leaf");

            tree.AddChild(tree.Root, level2);
            tree.AddChild(level2, leaf);

            // Act
            tree.Traverse("input");

            // Assert
            Assert.Equal(new List<string> { "level 1" }, tree.Root.Effects);
            Assert.Equal(new List<string> { "level 1", "level 2" }, level2.Effects);
            Assert.Equal(new List<string> { "level 1", "level 2" }, leaf.Effects);
        }

        [Fact]
        public void DecisionTree_Traverse_VacationTree_SetsPropertiesCorrectly()
        {
            // Arrange
            var tree = DecisionTree<string>.BuildVacationTree();
            var prefs = new VacationPreferences
            {
                Budget = 1000,
                PrefersBeach = true,
                PrefersAdventure = false
            };

            // Act
            var result = tree.Traverse(prefs);

            // Assert
            Assert.Equal("Thailand", result);
            Assert.NotNull(tree.Root);
            Assert.Equal(prefs, tree.Root.OriginalInput);
            Assert.Equal(new List<string> { "Budget check" }, tree.Root.Effects);
            
            // Find the leaf node that was reached
            var current = tree.Root;
            while (current.Children.Any())
            {
                int index = current.Decision(prefs);
                if (index < 0 || index >= current.Children.Count) break;
                current = current.Children[index];
            }
            
            Assert.Equal(prefs, current.OriginalInput);
            Assert.Contains("Budget check", current.Effects);
            Assert.Contains("Low budget options", current.Effects);
        }

        #endregion
    }
}
