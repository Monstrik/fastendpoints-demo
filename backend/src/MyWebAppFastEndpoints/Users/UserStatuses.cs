public static class UserStatuses
{
    public const string Default = "🟢 Available";

    public static readonly HashSet<string> Allowed =
    [
        "🟢 Available",
        "🔴 Busy",
        "🔕 Do not disturb",
        "🎯 Focused",
        "⛔ Blocked",
        "🍽️ Eating",
        "🔨 In progress",
        "👀 Reviewing",
        "🧪 Testing",
        "🎨 Designing",
        "🔍 Researching",
        "📝 Documenting",
        "⏳ Waiting for input",
        "🏠 Working from home",
        "🏢 In office",
        "✈️ Traveling",
        "🚶 Stepped away",
        "🐢 Delayed response",
        "🌴 Out of office (OOO)",
        "🏖️ On vacation",
        "🤒 Sick leave",
        "🤝 Waiting on another team",
        "🧑‍💼 Waiting on customer",
        "⚫ Offline",
        "🆕 Onboarding",
        "📟 On-call"
    ];
}
