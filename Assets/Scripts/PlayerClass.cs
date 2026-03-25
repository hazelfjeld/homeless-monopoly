using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Player
{
    [Header("Character Info")]
    public string CharacterName;
    public int Age;

    [TextArea(4, 8)]
    public string Backstory;

    [Header("Board State")]
    public int BoardIndex;
    public int TurnsToWait;
    public int CashAmount;
    public MovementInstruction PendingMovement;

    [Header("Core Life Conditions")]
    public bool HasCar;
    public bool HasId;
    public bool HasJob;
    public bool HasShelter;
    public bool HasAddiction;
    public bool HasDriversLicense;
    public bool HasHealthInsurance;
    public bool HasCriminalRecord;
    public bool HasBeenEvictedInThePast;
    public bool HasCleanClothes;
    public bool IsLiterate;
    public bool HasGedOrDiploma;
    public bool HasPhone;
    public bool HasBackpack;
    public bool IsInRelationship;
    public bool IsVeteran;

    [Header("Game-Specific Resources")]
    public bool HasBirthCertificate;
    public bool HasSocialSecurityCard;
    public bool HasCaseworker;
    public bool HasWorkingPhone;
    public bool HasPlaceToSleep;
    public bool HasSafePlaceDuringDay;
    public bool IsHealthy;
    public bool HasBeenToJail;
    public bool IsInShelter;
    public bool NeedsMedication;
    public bool HasMedication;

    public Player()
    {
        CharacterName = "";
        Age = 0;
        Backstory = "";
        BoardIndex = 0;
        TurnsToWait = 0;
        CashAmount = 0;
        PendingMovement = new MovementInstruction();
    }

    public Player(string characterName, int age, string backstory)
        : this()
    {
        CharacterName = characterName;
        Age = age;
        Backstory = backstory;
    }

    public Player(
        string characterName,
        int age,
        string backstory,
        int startingCashAmount,
        List<PlayerFlagType> startingFlags)
        : this(characterName, age, backstory)
    {
        SetCash(startingCashAmount);

        if (startingFlags == null)
        {
            return;
        }

        for (int flagIndex = 0; flagIndex < startingFlags.Count; flagIndex++)
        {
            GiveFlag(startingFlags[flagIndex]);
        }
    }
    public Player Clone()
    {
        return JsonUtility.FromJson<Player>(JsonUtility.ToJson(this));
    }

    public void ResetForNewRun(int startingBoardIndex)
    {
        BoardIndex = startingBoardIndex;
        TurnsToWait = 0;
        PendingMovement = new MovementInstruction();
    }
    public CardResolution ApplyCard(Card card)
    {
        if (card == null)
        {
            Debug.LogWarning("Tried to apply a null card.");
            return new CardResolution();
        }

        return card.Apply(this);
    }

    public void GiveFlag(PlayerFlagType flagType)
    {
        SetFlag(flagType, true);
    }

    public void RemoveFlag(PlayerFlagType flagType)
    {
        SetFlag(flagType, false);
    }

    public bool GetFlag(PlayerFlagType flagType)
    {
        switch (flagType)
        {
            case PlayerFlagType.HasCar:
                return HasCar;

            case PlayerFlagType.HasId:
                return HasId;

            case PlayerFlagType.HasJob:
                return HasJob;

            case PlayerFlagType.HasShelter:
                return HasShelter;

            case PlayerFlagType.HasAddiction:
                return HasAddiction;

            case PlayerFlagType.HasDriversLicense:
                return HasDriversLicense;

            case PlayerFlagType.HasHealthInsurance:
                return HasHealthInsurance;

            case PlayerFlagType.HasCriminalRecord:
                return HasCriminalRecord;

            case PlayerFlagType.HasBeenEvictedInThePast:
                return HasBeenEvictedInThePast;

            case PlayerFlagType.HasCleanClothes:
                return HasCleanClothes;

            case PlayerFlagType.IsLiterate:
                return IsLiterate;

            case PlayerFlagType.HasGedOrDiploma:
                return HasGedOrDiploma;

            case PlayerFlagType.HasPhone:
                return HasPhone;

            case PlayerFlagType.HasBackpack:
                return HasBackpack;

            case PlayerFlagType.IsInRelationship:
                return IsInRelationship;

            case PlayerFlagType.IsVeteran:
                return IsVeteran;

            case PlayerFlagType.HasBirthCertificate:
                return HasBirthCertificate;

            case PlayerFlagType.HasSocialSecurityCard:
                return HasSocialSecurityCard;

            case PlayerFlagType.HasCaseworker:
                return HasCaseworker;

            case PlayerFlagType.HasWorkingPhone:
                return HasWorkingPhone;

            case PlayerFlagType.HasPlaceToSleep:
                return HasPlaceToSleep;

            case PlayerFlagType.HasSafePlaceDuringDay:
                return HasSafePlaceDuringDay;

            case PlayerFlagType.IsHealthy:
                return IsHealthy;

            case PlayerFlagType.HasBeenToJail:
                return HasBeenToJail;

            case PlayerFlagType.IsInShelter:
                return IsInShelter;

            case PlayerFlagType.NeedsMedication:
                return NeedsMedication;

            case PlayerFlagType.HasMedication:
                return HasMedication;

            default:
                Debug.LogWarning("Unknown PlayerFlagType: " + flagType);
                return false;
        }
    }

    public void SetFlag(PlayerFlagType flagType, bool value)
    {
        switch (flagType)
        {
            case PlayerFlagType.HasCar:
                HasCar = value;
                break;

            case PlayerFlagType.HasId:
                HasId = value;
                break;

            case PlayerFlagType.HasJob:
                HasJob = value;
                break;

            case PlayerFlagType.HasShelter:
                HasShelter = value;
                break;

            case PlayerFlagType.HasAddiction:
                HasAddiction = value;
                break;

            case PlayerFlagType.HasDriversLicense:
                HasDriversLicense = value;
                break;

            case PlayerFlagType.HasHealthInsurance:
                HasHealthInsurance = value;
                break;

            case PlayerFlagType.HasCriminalRecord:
                HasCriminalRecord = value;
                break;

            case PlayerFlagType.HasBeenEvictedInThePast:
                HasBeenEvictedInThePast = value;
                break;

            case PlayerFlagType.HasCleanClothes:
                HasCleanClothes = value;
                break;

            case PlayerFlagType.IsLiterate:
                IsLiterate = value;
                break;

            case PlayerFlagType.HasGedOrDiploma:
                HasGedOrDiploma = value;
                break;

            case PlayerFlagType.HasPhone:
                HasPhone = value;
                break;

            case PlayerFlagType.HasBackpack:
                HasBackpack = value;
                break;

            case PlayerFlagType.IsInRelationship:
                IsInRelationship = value;
                break;

            case PlayerFlagType.IsVeteran:
                IsVeteran = value;
                break;

            case PlayerFlagType.HasBirthCertificate:
                HasBirthCertificate = value;
                break;

            case PlayerFlagType.HasSocialSecurityCard:
                HasSocialSecurityCard = value;
                break;

            case PlayerFlagType.HasCaseworker:
                HasCaseworker = value;
                break;

            case PlayerFlagType.HasWorkingPhone:
                HasWorkingPhone = value;
                break;

            case PlayerFlagType.HasPlaceToSleep:
                HasPlaceToSleep = value;
                break;

            case PlayerFlagType.HasSafePlaceDuringDay:
                HasSafePlaceDuringDay = value;
                break;

            case PlayerFlagType.IsHealthy:
                IsHealthy = value;
                break;

            case PlayerFlagType.HasBeenToJail:
                HasBeenToJail = value;
                break;

            case PlayerFlagType.IsInShelter:
                IsInShelter = value;
                break;

            case PlayerFlagType.NeedsMedication:
                NeedsMedication = value;
                break;

            case PlayerFlagType.HasMedication:
                HasMedication = value;
                break;

            default:
                Debug.LogWarning("Unknown PlayerFlagType: " + flagType);
                break;
        }

        ApplyDependentFlagRules(flagType, value);
    }

    public bool MeetsCheck(PlayerCheckType checkType)
    {
        PlayerFlagType matchingFlagType;
        bool checkMatchesARealFlag = Enum.TryParse(checkType.ToString(), out matchingFlagType);

        if (checkMatchesARealFlag)
        {
            return GetFlag(matchingFlagType);
        }

        switch (checkType)
        {
            case PlayerCheckType.HasMoney:
                return CashAmount > 0;

            case PlayerCheckType.Under25:
                return Age < 25;

            case PlayerCheckType.NoAddictions:
                return !HasAddiction;

            case PlayerCheckType.NeverBeenEvicted:
                return !HasBeenEvictedInThePast;

            case PlayerCheckType.NeverBeenToJail:
                return !HasBeenToJail;

            default:
                Debug.LogWarning("Unknown PlayerCheckType: " + checkType);
                return false;
        }
    }

    public void ChangeCash(int cashDelta)
    {
        CashAmount += cashDelta;

        if (CashAmount < 0)
        {
            CashAmount = 0;
        }
    }

    public void SetCash(int newCashAmount)
    {
        CashAmount = Mathf.Max(0, newCashAmount);
    }

    public void AddWaitTurns(int turnCount)
    {
        TurnsToWait += Mathf.Max(0, turnCount);
    }

    public void QueueDelayedMovement(MovementInstruction movementInstruction)
    {
        if (movementInstruction == null)
        {
            PendingMovement = new MovementInstruction();
            return;
        }

        PendingMovement = movementInstruction.Clone();
    }

    public void ClearDelayedMovement()
    {
        PendingMovement = new MovementInstruction();
    }

    private void ApplyDependentFlagRules(PlayerFlagType flagType, bool value)
    {
        if (flagType == PlayerFlagType.HasPhone && value == false)
        {
            HasWorkingPhone = false;
        }

        if (flagType == PlayerFlagType.HasWorkingPhone && value == true)
        {
            HasPhone = true;
        }

        if (flagType == PlayerFlagType.IsInShelter && value == true)
        {
            HasShelter = true;
            HasPlaceToSleep = true;
        }

        if (flagType == PlayerFlagType.HasShelter && value == true)
        {
            HasPlaceToSleep = true;
        }
    }
}

public enum PlayerFlagType
{
    HasCar,
    HasId,
    HasJob,
    HasShelter,
    HasAddiction,
    HasDriversLicense,
    HasHealthInsurance,
    HasCriminalRecord,
    HasBeenEvictedInThePast,
    HasCleanClothes,
    IsLiterate,
    HasGedOrDiploma,
    HasPhone,
    HasBackpack,
    IsInRelationship,
    IsVeteran,
    HasBirthCertificate,
    HasSocialSecurityCard,
    HasCaseworker,
    HasWorkingPhone,
    HasPlaceToSleep,
    HasSafePlaceDuringDay,
    IsHealthy,
    HasBeenToJail,
    IsInShelter,
    NeedsMedication,
    HasMedication
}

public enum PlayerCheckType
{
    HasCar,
    HasId,
    HasJob,
    HasShelter,
    HasAddiction,
    HasDriversLicense,
    HasHealthInsurance,
    HasCriminalRecord,
    HasBeenEvictedInThePast,
    HasCleanClothes,
    IsLiterate,
    HasGedOrDiploma,
    HasPhone,
    HasBackpack,
    IsInRelationship,
    IsVeteran,
    HasBirthCertificate,
    HasSocialSecurityCard,
    HasCaseworker,
    HasWorkingPhone,
    HasPlaceToSleep,
    HasSafePlaceDuringDay,
    IsHealthy,
    HasBeenToJail,
    IsInShelter,
    NeedsMedication,
    HasMedication,

    HasMoney,
    Under25,
    NoAddictions,
    NeverBeenEvicted,
    NeverBeenToJail
}