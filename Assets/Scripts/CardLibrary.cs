using System.Collections.Generic;

public static class EscapeHomelessnessCardLibrary
{
    public static List<Card> CreateStopCards()
    {
        return new List<Card>
        {
            RelativeCard(
                "Removed From Shelter For Fighting",
                "You are removed from shelter for fighting. Move back 3 spaces.",
                CardType.Stop,
                -3,
                new PlayerFlagChange(PlayerFlagType.HasShelter, false),
                new PlayerFlagChange(PlayerFlagType.IsInShelter, false),
                new PlayerFlagChange(PlayerFlagType.HasPlaceToSleep, false)),

            RelativeCard(
                "Go To The ER",
                "You need to go to the ER for fighting. Move ahead 1 space.",
                CardType.Stop,
                1,
                new PlayerFlagChange(PlayerFlagType.IsHealthy, false)),

            DelayedCard(
                "Phone Out Of Minutes",
                "Your phone is out of minutes. Stay where you are, then move to the nearest pink.",
                CardType.Stop,
                1,
                NearestType(BoardSpaceType.Community, MovementTiming.AfterWaiting),
                new CurrencyChange(),
                new PlayerFlagChange(PlayerFlagType.HasWorkingPhone, false)),

            DelayedCard(
                "Free Donut",
                "You get a free donut! Wait here for a turn then move ahead 1 space.",
                CardType.Stop,
                1,
                Relative(1, MovementTiming.AfterWaiting)),

            TypeMoveCard(
                "COVID",
                "You have COVID. Move back to a pink space.",
                CardType.Stop,
                PreviousType(BoardSpaceType.Community),
                new PlayerFlagChange(PlayerFlagType.IsHealthy, false)),

            DelayedCard(
                "Tent Stolen",
                "Someone stole your tent. Stay here for a turn then move ahead 1 space.",
                CardType.Stop,
                1,
                Relative(1, MovementTiming.AfterWaiting),
                new CurrencyChange(),
                new PlayerFlagChange(PlayerFlagType.HasPlaceToSleep, false)),

            RelativeCard(
                "Lost Bus Pass",
                "You lost your bus pass. Go back 2 spaces.",
                CardType.Stop,
                -2),

            RelativeCard(
                "Wind Chill Warning",
                "There is a wind chill warning today. Move back 2 spaces.",
                CardType.Stop,
                -2),

            RelativeCard(
                "Caught With Drugs",
                "You are caught with drugs. Move back 3 spaces.",
                CardType.Stop,
                -3,
                new PlayerFlagChange(PlayerFlagType.HasAddiction, true),
                new PlayerFlagChange(PlayerFlagType.HasCriminalRecord, true)),

            RelativeCard(
                "Backpack Stolen",
                "Someone stole your backpack. Move back 4 spaces.",
                CardType.Stop,
                -4,
                new PlayerFlagChange(PlayerFlagType.HasBackpack, false)),

            RelativeCard(
                "Ran Out Of Food",
                "You ran out of food. Move back 2 spaces.",
                CardType.Stop,
                -2),

            DelayedCard(
                "Spent Money On Beer",
                "You spent your $5 on beer. Stay here then move ahead 1 space.",
                CardType.Stop,
                1,
                Relative(1, MovementTiming.AfterWaiting),
                new CurrencyChange(CurrencyChangeMode.Add, -5),
                new PlayerFlagChange(PlayerFlagType.HasAddiction, true)),

            DelayedCard(
                "Beat Up Last Night",
                "You were beat up last night. Stay here one turn then move ahead 2 spaces.",
                CardType.Stop,
                1,
                Relative(2, MovementTiming.AfterWaiting),
                new CurrencyChange(),
                new PlayerFlagChange(PlayerFlagType.IsHealthy, false)),

            DelayedCard(
                "Shelter Full Tonight",
                "The shelter is full tonight. Wait here then move ahead 1 space.",
                CardType.Stop,
                1,
                Relative(1, MovementTiming.AfterWaiting),
                new CurrencyChange(),
                new PlayerFlagChange(PlayerFlagType.IsInShelter, false),
                new PlayerFlagChange(PlayerFlagType.HasShelter, false)),

            RelativeCard(
                "Birthday",
                "It’s your birthday and no one cares. Move back 1 space.",
                CardType.Stop,
                -1),

            DelayedCard(
                "Shelter Closing",
                "Wait. The shelter is closing. Stay here then move ahead 1 space.",
                CardType.Stop,
                1,
                Relative(1, MovementTiming.AfterWaiting),
                new CurrencyChange(),
                new PlayerFlagChange(PlayerFlagType.IsInShelter, false),
                new PlayerFlagChange(PlayerFlagType.HasShelter, false)),

            TypeMoveCard(
                "Money Stolen",
                "Someone stole your money. Move back to a yellow space.",
                CardType.Stop,
                PreviousType(BoardSpaceType.Question),
                new CurrencyChange(CurrencyChangeMode.Set, 0)),

            TypeMoveCard(
                "Can’t Afford ID",
                "You can’t afford an ID. Move back to a yellow space.",
                CardType.Stop,
                PreviousType(BoardSpaceType.Question),
                new PlayerFlagChange(PlayerFlagType.HasId, false)),

            RelativeCard(
                "Broke Your Phone",
                "You broke your phone. Go back 2 spaces.",
                CardType.Stop,
                -2,
                new PlayerFlagChange(PlayerFlagType.HasPhone, false),
                new PlayerFlagChange(PlayerFlagType.HasWorkingPhone, false)),

            TypeMoveCard(
                "Lost Your Paperwork",
                "You lost your paperwork. Move back 2 yellow spaces.",
                CardType.Stop,
                PreviousType(BoardSpaceType.Question, 2),
                new PlayerFlagChange(PlayerFlagType.HasId, false),
                new PlayerFlagChange(PlayerFlagType.HasBirthCertificate, false),
                new PlayerFlagChange(PlayerFlagType.HasSocialSecurityCard, false)),

            RelativeCard(
                "No Address For Applications",
                "You don’t have an address for applications. Move back 1 space.",
                CardType.Stop,
                -1),

            TypeMoveCard(
                "Sick With Fever",
                "You are sick with fever. Move back to a pink space.",
                CardType.Stop,
                PreviousType(BoardSpaceType.Community),
                new PlayerFlagChange(PlayerFlagType.IsHealthy, false)),

            RelativeCard(
                "No Coat In The Cold",
                "It’s 0° outside and you have no coat. Move back 2 spaces.",
                CardType.Stop,
                -2),

            RelativeCard(
                "No Shade In The Heat",
                "It’s 100° out and there is no shade. Move back 2 spaces.",
                CardType.Stop,
                -2),

            RelativeCard(
                "Ran Out Of Medicine",
                "You ran out of your medicine. Move back 2 spaces.",
                CardType.Stop,
                -2,
                new PlayerFlagChange(PlayerFlagType.HasMedication, false)),

            RelativeCard(
                "Headache And Dehydrated",
                "You have a headache, you are dehydrated. Move back 2 spaces.",
                CardType.Stop,
                -2,
                new PlayerFlagChange(PlayerFlagType.IsHealthy, false)),

            DelayedCard(
                "Broke Up With Your Partner",
                "You broke up with your partner. Stay here then move to the nearest pink.",
                CardType.Stop,
                1,
                NearestType(BoardSpaceType.Community, MovementTiming.AfterWaiting),
                new CurrencyChange(),
                new PlayerFlagChange(PlayerFlagType.IsInRelationship, false)),

            RelativeCard(
                "Warrant For Arrest",
                "There is a warrant for your arrest. Move back 3 spaces.",
                CardType.Stop,
                -3,
                new PlayerFlagChange(PlayerFlagType.HasCriminalRecord, true)),

            DelayedCard(
                "Gambled All Your Money",
                "You gambled all your money. Stay here then move ahead 1 space.",
                CardType.Stop,
                1,
                Relative(1, MovementTiming.AfterWaiting),
                new CurrencyChange(CurrencyChangeMode.Set, 0))
        };
    }

    public static List<Card> CreateCommunityCards()
    {
        return new List<Card>
        {
            RelativeCard(
                "Looked Suspicious",
                "A police officer thought you looked suspicious. Move ahead 3 spaces.",
                CardType.Community,
                3),

            RelativeCard(
                "Bus On Time",
                "The bus was on time! Move ahead 3 spaces.",
                CardType.Community,
                3),

            RelativeCard(
                "Food Shelf Closed",
                "The food shelf is closed today. Move back 3 spaces.",
                CardType.Community,
                -3),

            RelativeCard(
                "Employment Center Has No Jobs",
                "The employment center has no jobs. Go back 2 spaces.",
                CardType.Community,
                -2),

            RelativeCard(
                "Landlord Will Not Rent",
                "The landlord will not rent to you. Move back 3 spaces.",
                CardType.Community,
                -3),

            RelativeCard(
                "Found $5",
                "You found $5! Buy a meal. Move ahead 3 spaces.",
                CardType.Community,
                3),

            RelativeCard(
                "Arrested For Loitering",
                "Police arrest you for loitering. Go back 2 spaces.",
                CardType.Community,
                -2,
                new PlayerFlagChange(PlayerFlagType.HasCriminalRecord, true),
                new PlayerFlagChange(PlayerFlagType.HasBeenToJail, true)),

            RelativeCard(
                "Church Group Helped",
                "A church group helped you out. Move ahead 2 spaces.",
                CardType.Community,
                2),

            RelativeCard(
                "Community Center Closed",
                "The community center is closed. Move back 2 spaces.",
                CardType.Community,
                -2),

            RelativeCard(
                "Fight Nearby",
                "There is a fight nearby. Move back 2 spaces.",
                CardType.Community,
                -2),

            RelativeCard(
                "Bought A Bus Pass",
                "You saved up enough money to buy a bus pass. Move ahead 3 spaces.",
                CardType.Community,
                3),

            RelativeCard(
                "Caseworker Has A Lead",
                "Your caseworker has a lead. Move ahead 2 spaces.",
                CardType.Community,
                2,
                new PlayerFlagChange(PlayerFlagType.HasCaseworker, true)),

            RelativeCard(
                "Free Food From Business",
                "A local business gives free food. Move ahead 2 spaces.",
                CardType.Community,
                2),

            RelativeCard(
                "You Got A Job",
                "You got a job! Move ahead 3 spaces.",
                CardType.Community,
                3,
                new PlayerFlagChange(PlayerFlagType.HasJob, true)),

            RelativeCard(
                "Friend Lets You Stay",
                "A friend lets you stay. Move ahead 2 spaces.",
                CardType.Community,
                2,
                new PlayerFlagChange(PlayerFlagType.HasPlaceToSleep, true)),

            RelativeCard(
                "Shelter Was Full",
                "The shelter was full. Go back 1 space.",
                CardType.Community,
                -1,
                new PlayerFlagChange(PlayerFlagType.IsInShelter, false),
                new PlayerFlagChange(PlayerFlagType.HasShelter, false)),

            RelativeCard(
                "Program Accepted You",
                "A program accepted you. Move ahead 2 spaces.",
                CardType.Community,
                2),

            RelativeCard(
                "Volunteered And Got A Meal",
                "You volunteered and got a meal. Move ahead 2 spaces.",
                CardType.Community,
                2),

            RelativeCard(
                "Food Pantry Open",
                "The food pantry is open! Move ahead 3 spaces.",
                CardType.Community,
                3),

            RelativeCard(
                "Sick And Lost Work",
                "You got sick and lost work. Move back 2 spaces.",
                CardType.Community,
                -2,
                new PlayerFlagChange(PlayerFlagType.IsHealthy, false)),

            RelativeCard(
                "Place To Stay Tonight",
                "You found a place to stay for the night. Move ahead 2 spaces.",
                CardType.Community,
                2,
                new PlayerFlagChange(PlayerFlagType.HasPlaceToSleep, true)),

            RelativeCard(
                "Lost Food Card",
                "You lost your food card. Move back 1 space.",
                CardType.Community,
                -1),

            RelativeCard(
                "Bus Broke Down",
                "The bus broke down. You are late. Move back 2 spaces.",
                CardType.Community,
                -2),

            RelativeCard(
                "You Got Your ID",
                "You got your ID! Move ahead 3 spaces.",
                CardType.Community,
                3,
                new PlayerFlagChange(PlayerFlagType.HasId, true)),

            RelativeCard(
                "Job Interview Went Well",
                "The job interview went well. Move ahead 3 spaces.",
                CardType.Community,
                3)
        };
    }

    public static List<Card> CreateQuestionCards()
    {
        return new List<Card>
        {
            PositiveConditionCard(
                "No Addictions Check",
                "If you have no addictions. Move ahead 3 spaces.",
                PlayerCheckType.NoAddictions,
                3),

            PositiveConditionCard(
                "Car Check",
                "If you have a car, move ahead 3 spaces.",
                PlayerCheckType.HasCar,
                3),

            PositiveConditionCard(
                "Cash Check",
                "If you have cash. Move ahead 1 space.",
                PlayerCheckType.HasMoney,
                1),

            PositiveConditionCard(
                "Never Been To Jail Check",
                "If you have never been to jail. Move ahead 3 spaces.",
                PlayerCheckType.NeverBeenToJail,
                3),

            PositiveConditionCard(
                "Working Phone Check",
                "If you have a working phone. Move ahead 3 spaces.",
                PlayerCheckType.HasWorkingPhone,
                3),

            PositiveConditionCard(
                "Clean Clothes Check",
                "If you have clean clothes. Move ahead 1 space.",
                PlayerCheckType.HasCleanClothes,
                1),

            PositiveConditionCard(
                "Safe Place During Day Check",
                "If you have a safe place during the day. Move ahead 2 spaces.",
                PlayerCheckType.HasSafePlaceDuringDay,
                2),

            PositiveConditionCard(
                "Never Been Evicted Check",
                "If you have never been evicted. Move ahead 2 spaces.",
                PlayerCheckType.NeverBeenEvicted,
                2),

            PositiveConditionCard(
                "Student Check",
                "If you are a student. Move ahead 2 spaces.",
                PlayerCheckType.IsStudent,
                2),

            PositiveConditionCard(
                "Church Attendance Check",
                "If you are in church 4–5 times a week. Move ahead 3 spaces.",
                PlayerCheckType.AttendsChurchFrequently,
                3),

            PositiveConditionCard(
                "Healthy Check",
                "If you are healthy. Move ahead 1 space.",
                PlayerCheckType.IsHealthy,
                1),

            PositiveConditionCard(
                "Job Check",
                "If you have a job. Move ahead 2 spaces.",
                PlayerCheckType.HasJob,
                2),

            PositiveConditionCard(
                "Can Read Check",
                "If you can read. Move ahead 2 spaces.",
                PlayerCheckType.IsLiterate,
                2),

            PositiveConditionCard(
                "Place To Sleep Check",
                "If you have a place to sleep. Move ahead 2 spaces.",
                PlayerCheckType.HasPlaceToSleep,
                2),

            PositiveConditionCard(
                "Birth Certificate Check",
                "If you have a birth certificate. Move ahead 1 space.",
                PlayerCheckType.HasBirthCertificate,
                1),

            PositiveConditionCard(
                "Shelter Check",
                "If you are in a shelter. Move ahead 1 space.",
                PlayerCheckType.IsInShelter,
                1),

            PositiveConditionCard(
                "Social Security Card Check",
                "If you have a social security card. Move ahead 2 spaces.",
                PlayerCheckType.HasSocialSecurityCard,
                2),

            PositiveConditionCard(
                "GED Or Diploma Check",
                "If you have a GED or Diploma. Move ahead 3 spaces.",
                PlayerCheckType.HasGedOrDiploma,
                3),

            PositiveConditionCard(
                "Caseworker Check",
                "If you have a caseworker. Move ahead 1 space.",
                PlayerCheckType.HasCaseworker,
                1)
        };
    }

    public static List<Card> CreateGoCards()
    {
        return new List<Card>
        {
            RelativeCard(
                "Got Into The Shelter",
                "You got into the shelter! Move ahead 3 spaces.",
                CardType.Go,
                3,
                new PlayerFlagChange(PlayerFlagType.HasShelter, true),
                new PlayerFlagChange(PlayerFlagType.IsInShelter, true),
                new PlayerFlagChange(PlayerFlagType.HasPlaceToSleep, true)),

            RelativeCard(
                "Able To Get Clean",
                "You were able to get clean. Move ahead 2 spaces.",
                CardType.Go,
                2,
                new PlayerFlagChange(PlayerFlagType.HasCleanClothes, true)),

            RelativeCard(
                "Friend Bought Gas",
                "Your friend bought gas for the car. Move ahead 2 spaces.",
                CardType.Go,
                2),

            RelativeCard(
                "Out Of The Hospital",
                "You are out of the hospital. Move ahead 1 space.",
                CardType.Go,
                1,
                new PlayerFlagChange(PlayerFlagType.IsHealthy, true)),

            RelativeCard(
                "Got Health Insurance",
                "You got health insurance. Move ahead 3 spaces.",
                CardType.Go,
                3,
                new PlayerFlagChange(PlayerFlagType.HasHealthInsurance, true)),

            RelativeCard(
                "Refilled Medicine",
                "You refilled your medicine. Move ahead 2 spaces.",
                CardType.Go,
                2,
                new PlayerFlagChange(PlayerFlagType.HasMedication, true)),

            RelativeCard(
                "Two Good Meals",
                "You ate 2 good meals today. Move ahead 2 spaces.",
                CardType.Go,
                2),

            RelativeCard(
                "Shelter Opening Early",
                "The shelter is opening early today. Move ahead 1 space.",
                CardType.Go,
                1,
                new PlayerFlagChange(PlayerFlagType.HasShelter, true),
                new PlayerFlagChange(PlayerFlagType.IsInShelter, true),
                new PlayerFlagChange(PlayerFlagType.HasPlaceToSleep, true)),

            RelativeCard(
                "Sober For A Month",
                "You have been sober for a month. Move ahead 3 spaces.",
                CardType.Go,
                3,
                new PlayerFlagChange(PlayerFlagType.HasAddiction, false)),

            RelativeCard(
                "Housing Application",
                "You sent in an application for housing. Move ahead 2 spaces.",
                CardType.Go,
                2),

            TypeMoveCard(
                "Found A Car",
                "You found a car you can afford. Move ahead to the next green space.",
                CardType.Go,
                NextType(BoardSpaceType.Go),
                new PlayerFlagChange(PlayerFlagType.HasCar, true)),

            RelativeCard(
                "New Apartments Available",
                "2 new apartments are available for you to look at. Move ahead 2 spaces.",
                CardType.Go,
                2),

            TypeMoveCard(
                "Car Is Fixed",
                "Your car is fixed. Move ahead to the next green space.",
                CardType.Go,
                NextType(BoardSpaceType.Go),
                new PlayerFlagChange(PlayerFlagType.HasCar, true)),

            RelativeCard(
                "Got A $15/hr Job",
                "You got a $15/hr job. Move ahead 3 spaces.",
                CardType.Go,
                3,
                new PlayerFlagChange(PlayerFlagType.HasJob, true)),

            RelativeCard(
                "Slept In A Real Bed",
                "You slept in a real bed last night. Move ahead 2 spaces.",
                CardType.Go,
                2,
                new PlayerFlagChange(PlayerFlagType.HasPlaceToSleep, true)),

            RelativeCard(
                "Tax Return",
                "You got a tax return. Move ahead 2 spaces.",
                CardType.Go,
                2,
                currencyChange: new CurrencyChange(CurrencyChangeMode.Add, 100)),

            RelativeCard(
                "Met With Caseworker",
                "You met with your caseworker. Move ahead 2 spaces.",
                CardType.Go,
                2,
                new PlayerFlagChange(PlayerFlagType.HasCaseworker, true)),

            RelativeCard(
                "Ate Happy Today",
                "You ate happy today. Move ahead 1 space.",
                CardType.Go,
                1),

            RelativeCard(
                "Started Psychiatrist Meds",
                "You started meds from your psychiatrist. Move ahead 2 spaces.",
                CardType.Go,
                2,
                new PlayerFlagChange(PlayerFlagType.HasMedication, true)),

            RelativeCard(
                "Hired At McDonald’s",
                "You were hired at McDonald's. Move ahead 3 spaces.",
                CardType.Go,
                3,
                new PlayerFlagChange(PlayerFlagType.HasJob, true)),

            RelativeCard(
                "Enough Money For Supplies",
                "You have enough money for supplies today. Move ahead 2 spaces.",
                CardType.Go,
                2,
                currencyChange: new CurrencyChange(CurrencyChangeMode.Add, 10)),

            RelativeCard(
                "ID Came In The Mail",
                "Your ID came in the mail. Move ahead 2 spaces.",
                CardType.Go,
                2,
                new PlayerFlagChange(PlayerFlagType.HasId, true)),

            RelativeCard(
                "Went To Treatment",
                "You went to treatment this morning. Move ahead 3 spaces.",
                CardType.Go,
                3,
                new PlayerFlagChange(PlayerFlagType.HasAddiction, false)),

            RelativeCard(
                "New Backpack With Supplies",
                "You got a new backpack with supplies from a street outreach worker. Move ahead 2 spaces.",
                CardType.Go,
                2,
                new PlayerFlagChange(PlayerFlagType.HasBackpack, true)),

            RelativeCard(
                "Got To Shower",
                "You got to shower today. Move ahead 2 spaces.",
                CardType.Go,
                2,
                new PlayerFlagChange(PlayerFlagType.HasCleanClothes, true)),

            RelativeCard(
                "Friend Took You To Dinner",
                "A friend took you to dinner. Move ahead 1 space.",
                CardType.Go,
                1),

            RelativeCard(
                "New Work Boots",
                "You got new work boots. Move ahead 3 spaces.",
                CardType.Go,
                3),

            RelativeCard(
                "New Phone",
                "You got a new phone. Move ahead 2 spaces.",
                CardType.Go,
                2,
                new PlayerFlagChange(PlayerFlagType.HasPhone, true),
                new PlayerFlagChange(PlayerFlagType.HasWorkingPhone, true)),

            RelativeCard(
                "Good Weather",
                "The weather is good! Move ahead 1 space.",
                CardType.Go,
                1)
        };
    }

    private static Card RelativeCard(
        string title,
        string rulesText,
        CardType cardType,
        int spaces,
        params PlayerFlagChange[] flagChanges)
    {
        return RelativeCard(title, rulesText, cardType, spaces, new CurrencyChange(), flagChanges);
    }

    private static Card RelativeCard(
        string title,
        string rulesText,
        CardType cardType,
        int spaces,
        CurrencyChange currencyChange,
        params PlayerFlagChange[] flagChanges)
    {
        return new Card(
            title,
            rulesText,
            cardType,
            new List<CardOutcome>
            {
                new CardOutcome(
                    rulesText,
                    new List<PlayerCondition>(),
                    Relative(spaces),
                    0,
                    currencyChange ?? new CurrencyChange(),
                    new List<PlayerFlagChange>(flagChanges ?? new PlayerFlagChange[0]))
            });
    }

    private static Card DelayedCard(
        string title,
        string rulesText,
        CardType cardType,
        int waitTurns,
        MovementInstruction delayedMovement,
        CurrencyChange currencyChange = null,
        params PlayerFlagChange[] flagChanges)
    {
        return new Card(
            title,
            rulesText,
            cardType,
            new List<CardOutcome>
            {
                new CardOutcome(
                    rulesText,
                    new List<PlayerCondition>(),
                    delayedMovement,
                    waitTurns,
                    currencyChange ?? new CurrencyChange(),
                    new List<PlayerFlagChange>(flagChanges ?? new PlayerFlagChange[0]))
            });
    }

    private static Card TypeMoveCard(
        string title,
        string rulesText,
        CardType cardType,
        MovementInstruction movementInstruction,
        params PlayerFlagChange[] flagChanges)
    {
        return TypeMoveCard(title, rulesText, cardType, movementInstruction, new CurrencyChange(), flagChanges);
    }

    private static Card TypeMoveCard(
        string title,
        string rulesText,
        CardType cardType,
        MovementInstruction movementInstruction,
        CurrencyChange currencyChange,
        params PlayerFlagChange[] flagChanges)
    {
        return new Card(
            title,
            rulesText,
            cardType,
            new List<CardOutcome>
            {
                new CardOutcome(
                    rulesText,
                    new List<PlayerCondition>(),
                    movementInstruction,
                    0,
                    currencyChange ?? new CurrencyChange(),
                    new List<PlayerFlagChange>(flagChanges ?? new PlayerFlagChange[0]))
            });
    }

    private static Card PositiveConditionCard(
        string title,
        string rulesText,
        PlayerCheckType checkType,
        int spacesToMove)
    {
        return new Card(
            title,
            rulesText,
            CardType.Question,
            new List<CardOutcome>
            {
                new CardOutcome(
                    "Condition met. Move ahead " + spacesToMove + " space(s).",
                    new List<PlayerCondition>
                    {
                        new PlayerCondition(checkType, true)
                    },
                    Relative(spacesToMove),
                    0,
                    new CurrencyChange(),
                    new List<PlayerFlagChange>()),

                new CardOutcome(
                    "Condition not met. Stay where you are.",
                    new List<PlayerCondition>(),
                    new MovementInstruction(),
                    0,
                    new CurrencyChange(),
                    new List<PlayerFlagChange>(),
                    true)
            });
    }

    private static MovementInstruction Relative(int spaces, MovementTiming movementTiming = MovementTiming.Immediate)
    {
        return new MovementInstruction
        {
            MovementMode = MovementMode.RelativeSpaces,
            SpaceCount = spaces,
            MovementTiming = movementTiming,
            TargetSpaceType = BoardSpaceType.Start
        };
    }

    private static MovementInstruction NextType(BoardSpaceType targetSpaceType, int occurrences = 1, MovementTiming movementTiming = MovementTiming.Immediate)
    {
        return new MovementInstruction
        {
            MovementMode = MovementMode.NextSpaceOfType,
            TargetSpaceType = targetSpaceType,
            SpaceCount = occurrences,
            MovementTiming = movementTiming
        };
    }

    private static MovementInstruction PreviousType(BoardSpaceType targetSpaceType, int occurrences = 1, MovementTiming movementTiming = MovementTiming.Immediate)
    {
        return new MovementInstruction
        {
            MovementMode = MovementMode.PreviousSpaceOfType,
            TargetSpaceType = targetSpaceType,
            SpaceCount = occurrences,
            MovementTiming = movementTiming
        };
    }

    private static MovementInstruction NearestType(BoardSpaceType targetSpaceType, MovementTiming movementTiming = MovementTiming.Immediate)
    {
        return new MovementInstruction
        {
            MovementMode = MovementMode.NearestSpaceOfType,
            TargetSpaceType = targetSpaceType,
            SpaceCount = 1,
            MovementTiming = movementTiming
        };
    }
}
