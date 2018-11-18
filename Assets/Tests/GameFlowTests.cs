using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class GameFlowTests
  {
    //[UnityTest]
    public IEnumerator GameFlowTestsWithEnumeratorPasses()
    {
      GameFlowTestable gameFlow = new GameFlowTestable();
      yield return gameFlow;

      Assert.AreEqual(gameFlow.tileHoverColor, Color.white, "Tile hover color");
      Assert.AreEqual(gameFlow.tileHoverIllegalMoveColor, Color.yellow, "Tile hover color");
      Assert.AreEqual(gameFlow.tileNormalColor, Color.grey, "Tile hover color");
      Assert.AreEqual(gameFlow.tileDimensions, 2.0f, "Tile hover color");
      Assert.AreEqual(gameFlow.tileDropRate, 4.0f, "Tile hover color");
      Assert.AreEqual(gameFlow.tileDestroyRate, 10.0f, "Tile hover color");
    }
  }

  public class GameFlowTestable : GameFlow, IMonoBehaviourTest
  {
    public struct State
    {
      public string behaviorName;
      public struct Edge
      {
        public string destinationState;
        public class Trigger
        {
          public enum TriggerType
          {
            Boolean,
            Trigger,
            None
          }
          public TriggerType triggerType;
          public string triggerName;
          public bool flag;
        }
        public Trigger[] triggers;
      }
      public Edge[] edges;
    }
    private State[] dag;

    private int currentState = Animator.StringToHash("");
    private List<int> closedStates = new List<int>();
    private List<State.Edge> visitedEdges = new List<State.Edge>();

    private Animator animator = null;

    public bool IsTestFinished { get; private set; }
    private void Start()
    {
      GameObject matchObject = new GameObject("Match");
      match = matchObject.AddComponent<MatchTestable>();
      tileHoverColor = Color.white;
      tileHoverIllegalMoveColor = Color.yellow;
      tileNormalColor = Color.grey;
      tileDimensions = 2.0f;
      tileDropRate = 4.0f;
      tileDestroyRate = 10.0f;

      match.RegisterSide(new Side() { name = "Side 1" });
      match.RegisterSide(new Side() { name = "Side 2" });

      animator = gameObject.AddComponent<Animator>();
      animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/GameFlow");


      dag = new State[]
      {
        new State() { behaviorName="Game Start", edges = new State.Edge[] {
          new State.Edge() { destinationState = "Title", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="Fade from Black Complete", flag=true }  }  }
        } },
        new State() { behaviorName="Title", edges = new State.Edge[] {
          new State.Edge() { destinationState = "Destroy Board", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Trigger, triggerName="Click Anywhere" },
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="Board Visible", flag=true }
          } },
          new State.Edge() { destinationState = "Main Menu", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Trigger, triggerName="Click Anywhere" },
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="Board Visible", flag=false }
          } }
        } },
        new State() { behaviorName="Main Menu", edges = new State.Edge[] {
          new State.Edge() { destinationState = "Create New Match - Invisible", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.None }
          } }
        } },
        new State() { behaviorName="Create New Match - Invisible", edges = new State.Edge[] {
          new State.Edge() { destinationState = "Create New Match - Visible", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="New Match Dialog Visible", flag=true }
          } }
        } },
        new State() { behaviorName="Create New Match - Visible", edges = new State.Edge[] {
          new State.Edge() { destinationState = "Match Play", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="New Match Dialog Visible", flag=false }
          } }
        } },
        new State() { behaviorName="Match Play", edges = new State.Edge[] {
          new State.Edge() { destinationState = "Show Match HUD", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.None }
          } }
        } },
        new State() { behaviorName="Show Match HUD", edges = new State.Edge[] {
          new State.Edge() { destinationState = "Create Match", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.None }
          } }
        } },
        new State() { behaviorName="Create Match", edges = new State.Edge[] {
          new State.Edge() { destinationState = "Create Board", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.None }
          } }
        } },
        new State() { behaviorName="Show Match HUD", edges = new State.Edge[] {
          new State.Edge() { destinationState = "Create Tiles", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="Board Visible", flag = true }
          } }
        } },
        new State() { behaviorName="Create Tiles", edges = new State.Edge[] {
          new State.Edge() { destinationState = "New Game", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Trigger, triggerName="Tiles Placed" }
          } }
        } },
        new State() { behaviorName="New Game", edges = new State.Edge[] {
          new State.Edge() { destinationState = "New Turn", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.None }
          } }
        } },
        new State() { behaviorName="New Turn", edges = new State.Edge[] {
          new State.Edge() { destinationState = "Wait for Player", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="Turn Instructions Visible", flag = true }
          } }
        } },
        new State() { behaviorName="Wait for Player", edges = new State.Edge[] {
          new State.Edge() { destinationState = "End Turn", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Trigger, triggerName="Turn Played" }
          } }
        } },
        new State() { behaviorName="End Turn", edges = new State.Edge[] {
          new State.Edge() { destinationState = "New Turn", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="Game Ended", flag=false },
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="Match Won", flag=false },
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="Turn Instructions Visible", flag=false }
          } },
          new State.Edge() { destinationState = "Clear Tiles", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="Game Ended", flag=true },
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="Match Won", flag=false },
          } },
          new State.Edge() { destinationState = "Title", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Boolean, triggerName="Match Won", flag=true },
          } }
        } },
        new State() { behaviorName="Clear Tiles", edges = new State.Edge[] {
          new State.Edge() { destinationState = "Update Match HUD", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.Trigger, triggerName="Cleared Tiles" }
          } }
        } },
        new State() { behaviorName="Update Match HUD", edges = new State.Edge[] {
          new State.Edge() { destinationState = "New Game", triggers = new State.Edge.Trigger[] {
            new State.Edge.Trigger() { triggerType=State.Edge.Trigger.TriggerType.None }
          } }
        } },
      };

    }

    private bool StepTowards(string targetBehaviorName, Stack<State> stateStack)
    {
      State state = stateStack.Peek();
      if (state.behaviorName == targetBehaviorName)
      {
        return true;
      }

      foreach (var edge in state.edges)
      {
        if (visitedEdges.IndexOf(edge) >= 0)
        {
          continue;
        }
        State? targetState = null;

        foreach (var s in dag)
        {
          if (s.behaviorName == edge.destinationState)
          {
            targetState = s;
            break;
          }
        }

        if (targetState == null)
        {
          continue;
        }

        stateStack.Push((State)targetState);
        bool result = StepTowards(targetBehaviorName, stateStack);
        if (result)
        {
          return true;
        }
        visitedEdges.Add(edge);
      }

      stateStack.Pop();
      return false;
    }

    private void Update()
    {
      var animatorState = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
      if (animatorState != currentState)
      {
        closedStates.Add(currentState);
        currentState = animatorState;
      }
      else
      {
        foreach (var state in dag)
        {
          if (Animator.StringToHash(state.behaviorName) == currentState)
          {
            foreach (var edge in state.edges)
            {

            }
          }
        }
      }
    }
  }

}
