using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Card
{
    public string CardId;
    public string Title;

    [TextArea(3, 6)]
    public string RulesText;

    public CardType CardType;
    public List<CardOutcome> Outcomes;

    public Card()
    {
        CardId = Guid.NewGuid().ToString("N");
        Title = "";
        RulesText = "";
        CardType = CardType.Go;
        Outcomes = new List<CardOutcome>();
    }

    public Card(string title, string rulesText, CardType cardType, List<CardOutcome> outcomes)
        : this()
    {
        Title = title;
        RulesText = rulesText;
        CardType = cardType;
        Outcomes = outcomes ?? new List<CardOutcome>();
    }

    public CardResolution Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogWarning("Tried to apply a card to a null player.");
            return new CardResolution();
        }

        CardOutcome fallbackOutcome = null;

        for (int outcomeIndex = 0; outcomeIndex < Outcomes.Count; outcomeIndex++)
        {
            CardOutcome currentOutcome = Outcomes[outcomeIndex];

            if (currentOutcome.IsFallbackOutcome)
            {
                if (fallbackOutcome == null)
                {
                    fallbackOutcome = currentOutcome;
                }

                continue;
            }

            if (currentOutcome.ConditionsAreMet(player))
            {
                return currentOutcome.Apply(player, this);
            }
        }

        if (fallbackOutcome != null)
        {
            return fallbackOutcome.Apply(player, this);
        }

        if (Outcomes.Count == 1)
        {
            return Outcomes[0].Apply(player, this);
        }

        return new CardResolution(
            CardId,
            Title,
            RulesText,
            CardType,
            new MovementInstruction(),
            0,
            0,
            new List<PlayerFlagChange>());
    }
}

[Serializable]
public class CardOutcome
{
    [TextArea(2, 5)]
    public string OutcomeText;

    public bool IsFallbackOutcome;
    public ConditionMatchMode ConditionMatchMode;
    public List<PlayerCondition> Conditions;
    public MovementInstruction Movement;
    public int WaitTurns;
    public CurrencyChange CurrencyChange;
    public List<PlayerFlagChange> FlagChanges;

    public CardOutcome()
    {
        OutcomeText = "";
        IsFallbackOutcome = false;
        ConditionMatchMode = ConditionMatchMode.All;
        Conditions = new List<PlayerCondition>();
        Movement = new MovementInstruction();
        WaitTurns = 0;
        CurrencyChange = new CurrencyChange();
        FlagChanges = new List<PlayerFlagChange>();
    }

    public CardOutcome(
        string outcomeText,
        List<PlayerCondition> conditions,
        MovementInstruction movement,
        int waitTurns,
        CurrencyChange currencyChange,
        List<PlayerFlagChange> flagChanges,
        bool isFallbackOutcome = false,
        ConditionMatchMode conditionMatchMode = ConditionMatchMode.All)
        : this()
    {
        OutcomeText = outcomeText;
        Conditions = conditions ?? new List<PlayerCondition>();
        Movement = movement ?? new MovementInstruction();
        WaitTurns = waitTurns;
        CurrencyChange = currencyChange ?? new CurrencyChange();
        FlagChanges = flagChanges ?? new List<PlayerFlagChange>();
        IsFallbackOutcome = isFallbackOutcome;
        ConditionMatchMode = conditionMatchMode;
    }

    public bool ConditionsAreMet(Player player)
    {
        if (player == null)
        {
            return false;
        }

        if (Conditions == null || Conditions.Count == 0)
        {
            return true;
        }

        if (ConditionMatchMode == ConditionMatchMode.Any)
        {
            for (int conditionIndex = 0; conditionIndex < Conditions.Count; conditionIndex++)
            {
                if (Conditions[conditionIndex].IsMet(player))
                {
                    return true;
                }
            }

            return false;
        }

        for (int conditionIndex = 0; conditionIndex < Conditions.Count; conditionIndex++)
        {
            if (!Conditions[conditionIndex].IsMet(player))
            {
                return false;
            }
        }

        return true;
    }

    public CardResolution Apply(Player player, Card sourceCard)
    {
        int cashBeforeResolution = player.CashAmount;

        for (int flagIndex = 0; flagIndex < FlagChanges.Count; flagIndex++)
        {
            PlayerFlagChange flagChange = FlagChanges[flagIndex];
            player.SetFlag(flagChange.FlagType, flagChange.NewValue);
        }

        if (CurrencyChange != null)
        {
            CurrencyChange.Apply(player);
        }

        if (WaitTurns > 0)
        {
            player.AddWaitTurns(WaitTurns);
        }

        if (Movement != null &&
            Movement.MovementTiming == MovementTiming.AfterWaiting &&
            WaitTurns > 0)
        {
            player.QueueDelayedMovement(Movement);
        }

        int cashAfterResolution = player.CashAmount;
        int appliedCashChange = cashAfterResolution - cashBeforeResolution;

        string resolutionText = OutcomeText;

        if (string.IsNullOrWhiteSpace(resolutionText))
        {
            resolutionText = sourceCard.RulesText;
        }

        return new CardResolution(
            sourceCard.CardId,
            sourceCard.Title,
            resolutionText,
            sourceCard.CardType,
            Movement != null ? Movement.Clone() : new MovementInstruction(),
            WaitTurns,
            appliedCashChange,
            new List<PlayerFlagChange>(FlagChanges));
    }
}

[Serializable]
public class PlayerCondition
{
    public PlayerCheckType CheckType;
    public bool ExpectedValue;

    public PlayerCondition()
    {
        CheckType = PlayerCheckType.HasId;
        ExpectedValue = true;
    }

    public PlayerCondition(PlayerCheckType checkType, bool expectedValue = true)
    {
        CheckType = checkType;
        ExpectedValue = expectedValue;
    }

    public bool IsMet(Player player)
    {
        if (player == null)
        {
            return false;
        }

        return player.MeetsCheck(CheckType) == ExpectedValue;
    }
}

[Serializable]
public class PlayerFlagChange
{
    public PlayerFlagType FlagType;
    public bool NewValue;

    public PlayerFlagChange()
    {
        FlagType = PlayerFlagType.HasId;
        NewValue = true;
    }

    public PlayerFlagChange(PlayerFlagType flagType, bool newValue)
    {
        FlagType = flagType;
        NewValue = newValue;
    }
}

[Serializable]
public class CurrencyChange
{
    public CurrencyChangeMode ChangeMode;
    public int Amount;

    public CurrencyChange()
    {
        ChangeMode = CurrencyChangeMode.None;
        Amount = 0;
    }

    public CurrencyChange(CurrencyChangeMode changeMode, int amount)
    {
        ChangeMode = changeMode;
        Amount = amount;
    }

    public void Apply(Player player)
    {
        if (player == null)
        {
            return;
        }

        switch (ChangeMode)
        {
            case CurrencyChangeMode.None:
                return;

            case CurrencyChangeMode.Add:
                player.ChangeCash(Amount);
                return;

            case CurrencyChangeMode.Set:
                player.SetCash(Amount);
                return;
        }
    }
}

[Serializable]
public class MovementInstruction
{
    public MovementMode MovementMode;
    public MovementTiming MovementTiming;
    public int SpaceCount;
    public BoardSpaceType TargetSpaceType;

    public MovementInstruction()
    {
        MovementMode = MovementMode.None;
        MovementTiming = MovementTiming.Immediate;
        SpaceCount = 0;
        TargetSpaceType = BoardSpaceType.Start;
    }

    public MovementInstruction(MovementMode movementMode, int spaceCount)
        : this()
    {
        MovementMode = movementMode;
        SpaceCount = spaceCount;
    }

    public MovementInstruction(
        MovementMode movementMode,
        BoardSpaceType targetSpaceType,
        MovementTiming movementTiming = MovementTiming.Immediate)
        : this()
    {
        MovementMode = movementMode;
        TargetSpaceType = targetSpaceType;
        MovementTiming = movementTiming;
    }

    public MovementInstruction(
        MovementMode movementMode,
        int spaceCount,
        BoardSpaceType targetSpaceType,
        MovementTiming movementTiming)
    {
        MovementMode = movementMode;
        SpaceCount = spaceCount;
        TargetSpaceType = targetSpaceType;
        MovementTiming = movementTiming;
    }

    public MovementInstruction Clone()
    {
        return new MovementInstruction(
            MovementMode,
            SpaceCount,
            TargetSpaceType,
            MovementTiming);
    }
}

[Serializable]
public class CardResolution
{
    public string CardId;
    public string CardTitle;
    public string ResolutionText;
    public CardType CardType;
    public MovementInstruction Movement;
    public int WaitTurnsAdded;
    public int CashChangeAmount;
    public List<PlayerFlagChange> AppliedFlagChanges;

    public CardResolution()
    {
        CardId = "";
        CardTitle = "";
        ResolutionText = "";
        CardType = CardType.Go;
        Movement = new MovementInstruction();
        WaitTurnsAdded = 0;
        CashChangeAmount = 0;
        AppliedFlagChanges = new List<PlayerFlagChange>();
    }

    public CardResolution(
        string cardId,
        string cardTitle,
        string resolutionText,
        CardType cardType,
        MovementInstruction movement,
        int waitTurnsAdded,
        int cashChangeAmount,
        List<PlayerFlagChange> appliedFlagChanges)
    {
        CardId = cardId;
        CardTitle = cardTitle;
        ResolutionText = resolutionText;
        CardType = cardType;
        Movement = movement ?? new MovementInstruction();
        WaitTurnsAdded = waitTurnsAdded;
        CashChangeAmount = cashChangeAmount;
        AppliedFlagChanges = appliedFlagChanges ?? new List<PlayerFlagChange>();
    }
}

public enum CardType
{
    Go,
    Stop,
    Question,
    Community
}

public enum BoardSpaceType
{
    Start,
    Go,
    Stop,
    Question,
    Community,
    Jump,
    Home
}

public enum MovementMode
{
    None,
    RelativeSpaces,
    NextSpaceOfType,
    PreviousSpaceOfType,
    NearestSpaceOfType
}

public enum MovementTiming
{
    Immediate,
    AfterWaiting
}

public enum ConditionMatchMode
{
    All,
    Any
}

public enum CurrencyChangeMode
{
    None,
    Add,
    Set
}