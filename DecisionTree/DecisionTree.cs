using System;
using System.Collections.Generic;
using System.Linq;

namespace GrokTest
{
    /// <summary>
    /// A generic decision tree that supports flexible branching logic.
    /// T represents the type of the decision result or node value.
    /// </summary>
    /// <typeparam name="T">The type of the node value and final decision result.</typeparam>
    public class DecisionTree<T>
    {
        /// <summary>
        /// Represents a node in the decision tree.
        /// </summary>
        public class Node
        {
            /// <summary>
            /// The value stored in this node (used for leaf nodes as the decision result).
            /// </summary>
            public T? Value { get; set; }

            /// <summary>
            /// The child nodes for branching decisions.
            /// </summary>
            public List<Node> Children { get; set; } = new List<Node>();

            /// <summary>
            /// The decision function that determines which child to traverse to based on input.
            /// Returns the index of the child node to follow. If no valid child, returns -1.
            /// </summary>
            public Func<object, int> Decision { get; set; }

            /// <summary>
            /// Optional description for debugging or visualization.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// The original immutable input object that led to this node during traversal.
            /// </summary>
            public object? OriginalInput { get; set; }

            /// <summary>
            /// The results item containing the effects of running through the tree up to this node.
            /// </summary>
            public List<string> Effects { get; set; } = new List<string>();

            /// <summary>
            /// Creates a leaf node with a value.
            /// </summary>
            public Node(T value, string description = "")
            {
                Value = value;
                Description = description;
                Decision = _ => -1; // No children
            }

            /// <summary>
            /// Creates a decision node with children and a decision function.
            /// </summary>
            public Node(Func<object, int> decision, string description = "")
            {
                Decision = decision;
                Description = description;
            }
        }

        /// <summary>
        /// The root node of the decision tree.
        /// </summary>
        public Node? Root { get; set; }

        /// <summary>
        /// Traverses the decision tree based on the input and returns the final decision.
        /// Also populates OriginalInput and Effects on the nodes along the traversal path.
        /// </summary>
        /// <param name="input">The input data for making decisions.</param>
        /// <returns>The value from the leaf node reached.</returns>
        public T? Traverse(object input)
        {
            if (Root == null)
                throw new InvalidOperationException("Decision tree has no root node.");

            var current = Root;
            var effects = new List<string>();
            
            // Set root properties
            current.OriginalInput = input;
            current.Effects = new List<string>(effects);

            while (current.Children.Any())
            {
                // Add current node's description to effects before deciding
                if (!string.IsNullOrEmpty(current.Description))
                {
                    effects.Add(current.Description);
                    // Update current node's effects
                    current.Effects = new List<string>(effects);
                }

                int index = current.Decision(input);
                if (index < 0 || index >= current.Children.Count)
                    break; // Invalid decision, stop at current node
                
                current = current.Children[index];
                
                // Set properties on the new current node
                current.OriginalInput = input;
                current.Effects = new List<string>(effects);
            }
            
            return current.Value;
        }

        /// <summary>
        /// Adds a child node to the specified parent node.
        /// </summary>
        public void AddChild(Node parent, Node child)
        {
            if (parent != null)
                parent.Children.Add(child);
        }

        /// <summary>
        /// Builds a simple decision tree for demonstration.
        /// This creates a tree that decides vacation destinations based on preferences.
        /// </summary>
        public static DecisionTree<string> BuildVacationTree()
        {
            var tree = new DecisionTree<string>();

            // Root: Ask about budget
            tree.Root = new DecisionTree<string>.Node(input =>
            {
                var prefs = input as VacationPreferences;
                if (prefs == null) return 0;
                return prefs.Budget > 2000 ? 1 : 0; // High budget -> 1, Low -> 0
            }, "Budget check");

            // Low budget branch
            var lowBudget = new DecisionTree<string>.Node(input =>
            {
                var prefs = input as VacationPreferences;
                if (prefs == null) return 0;
                return prefs.PrefersBeach ? 1 : 0; // Beach -> 1, No beach -> 0
            }, "Low budget options");

            // High budget branch
            var highBudget = new DecisionTree<string>.Node(input =>
            {
                var prefs = input as VacationPreferences;
                if (prefs == null) return 0;
                return prefs.PrefersAdventure ? 0 : 1; // Adventure -> 0, Relax -> 1
            }, "High budget options");

            tree.AddChild(tree.Root, lowBudget);
            tree.AddChild(tree.Root, highBudget);

            // Low budget children
            var cityBreak = new DecisionTree<string>.Node("Paris", "Affordable city break");
            var beachResort = new DecisionTree<string>.Node("Thailand", "Budget beach destination");

            tree.AddChild(lowBudget, cityBreak);
            tree.AddChild(lowBudget, beachResort);

            // High budget children
            var adventureTrip = new DecisionTree<string>.Node("New Zealand", "Adventure paradise");
            var luxurySpa = new DecisionTree<string>.Node("Maldives", "Luxury relaxation");

            tree.AddChild(highBudget, adventureTrip);
            tree.AddChild(highBudget, luxurySpa);

            return tree;
        }
    }

    /// <summary>
    /// Sample input class for vacation preferences.
    /// </summary>
    public class VacationPreferences
    {
        public int Budget { get; set; }
        public bool PrefersBeach { get; set; }
        public bool PrefersAdventure { get; set; }
    }
}
