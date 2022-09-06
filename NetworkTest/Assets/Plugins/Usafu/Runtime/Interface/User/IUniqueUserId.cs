using System;

namespace FishingCactus.User
{
    public interface IUniqueUserId : IEquatable< IUniqueUserId >
    {
        bool IsValid { get; }
    }
}