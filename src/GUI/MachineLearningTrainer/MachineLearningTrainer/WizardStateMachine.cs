using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MachineLearningTrainer
{
    public enum ProcessState
    {
        WelcomePage,
        DataDecision,
        TabularDataDecision,
        CNNDataDecision,
       
    }

    public enum Command
    {
        Previous,
        Next,
        Left,
        Right
    }

    public class WizardStateMachine
    {
        class StateTransition
        {
            readonly ProcessState CurrentState;
            readonly Command Command;

            public StateTransition(ProcessState currentState, Command command)
            {
                CurrentState = currentState;
                Command = command;
            }

            public override int GetHashCode()
            {
                return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                StateTransition other = obj as StateTransition;
                return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
            }
        }

        Dictionary<StateTransition, ProcessState> transitions;
        public ProcessState CurrentState { get; private set; }

        public WizardStateMachine()
        {
            CurrentState = ProcessState.WelcomePage;
            transitions = new Dictionary<StateTransition, ProcessState>
            {
                { new StateTransition(ProcessState.WelcomePage, Command.Next), ProcessState.DataDecision },
                { new StateTransition(ProcessState.WelcomePage, Command.Previous), ProcessState.WelcomePage },

                { new StateTransition(ProcessState.DataDecision, Command.Previous), ProcessState.WelcomePage },
                { new StateTransition(ProcessState.DataDecision, Command.Left), ProcessState.TabularDataDecision },
                { new StateTransition(ProcessState.DataDecision, Command.Right), ProcessState.CNNDataDecision },

            };
        }

        public ProcessState GetNext(Command command)
        {
            StateTransition transition = new StateTransition(CurrentState, command);
            ProcessState nextState;
            if (!transitions.TryGetValue(transition, out nextState))
                throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
            return nextState;
        }

        public ProcessState MoveNext(Command command)
        {
            CurrentState = GetNext(command);
            return CurrentState;
        }


    }
}
